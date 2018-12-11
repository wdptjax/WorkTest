
/*********************************************************************************************
 *	
 * 文件名称:    XmlSim.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-29 15:32:36
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceSim.Device
{
    public partial class DDF550Device
    {
        private static readonly byte[] Xml_Start = new byte[] { 0xB1, 0xC2, 0xD3, 0xE4 };
        private static readonly byte[] Xml_End = new byte[] { 0xE4, 0xD3, 0xC2, 0xB1 };

        /// <summary>
        /// Xml数据接收
        /// </summary>
        private void DataReceiveXml()
        {
            int recBytes = 0;
            List<byte> cacheCmd = new List<byte>();
            while (Connected)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    //recBytes = _cmdSocket.Receive(_cmdReply, offset, _cmdReply.Length - offset, SocketFlags.None);
                    recBytes = _socketXml.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (recBytes == 0)
                    {
                        CloseConnect();
                        break;
                    }

                    if (UnPackedXMLCommand(buffer.Take(recBytes).ToArray(), ref cacheCmd))
                    {
                        if (!CheckXMLCommand(cacheCmd.ToArray()))
                            continue;
                        //offset += recBytes;
                        int len = cacheCmd.Count;
                        byte[] data = cacheCmd.Skip(8).Take(len - 13).ToArray();
                        cacheCmd.Clear();
                        Request request = (Request)XmlWrapper.DeserializeObject<Request>(data.ToArray());
                        ProcessXml(request);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is ObjectDisposedException)
                    {
                        CloseConnect();
                        break;
                    }
                }

            }
        }

        private void DataSendXml(Reply reply)
        {
            byte[] buffer = XmlWrapper.SerializeObject(reply);
            byte[] cmd = PackedXMLCommand(buffer);
            WriteRecvData(cmd);
        }

        private void ProcessXml(Request request)
        {
            Reply reply = new Reply();
            reply.Id = request.Id;
            reply.Type = request.Type;
            reply.Command.Name = request.Command.Name;
            if (request.Type == "set")
            {
                switch (request.Command.Name)
                {
                    case CMD_DFMODE:
                        AnalysisCmdSetDfMode(request, ref reply);
                        break;
                    case CMD_MEASURESETTINGSFFM:
                        AnalysisCmdSetMeasureSettingsFFM(request, ref reply);
                        break;
                    case CMD_MEASURESETTINGSPSCAN:
                        AnalysisCmdSetMeasureSettingsPScan(request, ref reply);
                        break;
                    case CMD_RXSETTINGS:
                        AnalysisCmdSetRxSettings(request, ref reply);
                        break;
                    case CMD_ITU:
                        AnalysisCmdSetITU(request, ref reply);
                        break;
                    case CMD_SELCALL:
                        break;
                    case CMD_DEMODULATIONSETTINGS:
                        AnalysisCmdSetDemodulationSettings(request, ref reply);
                        break;
                    case CMD_AUDIOMODE:
                        break;
                    case CMD_ANTENNACONTROL:
                        break;
                    case CMD_REFERENCEMODE:
                        break;
                    case CMD_RFMODE:
                        break;
                    case CMD_IFMODE:
                        break;
                    case CMD_TRACEENABLE:
                        AnalysisCmdSetTraceEnable(request, ref reply);
                        break;
                    case CMD_TRACEDISABLE:
                        AnalysisCmdSetTraceDisable(request, ref reply);
                        break;
                    case CMD_TRACEDELETE:
                        break;
                    case CMD_TRACEDELETEINACTIVE:
                        if (!Client.IsSendAudio && !Client.IsSendDF && !Client.IsSendIQ && !Client.IsSendCW && !Client.IsSendSpectrum)
                        {
                            IsRunning = false;
                        }
                        break;
                    case CMD_TRACEFLAGDISABLE:
                        break;
                    case CMD_TRACEFLAGENABLE:
                        break;
                    case CMD_INITIATE:
                        if (DfMode == EDfMode.DFMODE_RX || DfMode == EDfMode.DFMODE_RXPSCAN)
                        {
                            IsRunning = true;
                        }
                        break;
                    case CMD_ABORT:
                        break;
                    case CMD_SCANRANGE:
                        AnalysisCmdScanRange(request, ref reply);
                        break;
                    case CMD_SCANRANGEADD:
                        AnalysisCmdScanRangeAdd(request, ref reply);
                        break;
                    case CMD_SCANRANGEDELETE:
                        AnalysisCmdScanRangeDelete(request, ref reply);
                        break;
                    case CMD_SCANRANGEDELETEALL:
                        _dispatcher.BeginInvoke(new Action(() => ScanRangeList.Clear()));
                        RunningScanRange = null;
                        break;
                    case CMD_SCANRANGENEXT:
                    case INCORRECT_COMMAND:
                    default:
                        reply.Id = "0";
                        reply.Type = "set";
                        reply.Command.Name = INCORRECT_COMMAND;
                        break;
                }
            }
            else
            {

            }
            DataSendXml(reply);
        }

        private void AnalysisCmdSetDfMode(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                if (para.Name == "eOperationMode")
                {
                    EDfMode mode = EDfMode.DFMODE_RX;
                    if (!Enum.TryParse(para.Value, out mode))
                    {

                        string msg = string.Format("No eOperationMode found with the name \"{0}\"", para.Value);
                        UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        continue;
                    }
                    DfMode = mode;
                }
                else
                {
                    reply.Command.RtnCode = "10516";
                    string msg = "Unknown command received";
                    UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                    continue;
                }
            }
        }

        private void AnalysisCmdSetMeasureSettingsFFM(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iFrequency":
                        double freq = 0d;
                        if (double.TryParse(para.Value, out freq))
                        {
                            Frequency = freq / 1000000;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iFrequency with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eAvgMode":
                        EAverage_Mode avgMode = EAverage_Mode.DFSQU_NORM;
                        if (Enum.TryParse(para.Value, out avgMode))
                        {
                            DFindMode = avgMode;
                        }
                        else
                        {
                            string msg = string.Format("No eAvgMode found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eDfPanStep":
                        EDfPan_Step dfPanStep = EDfPan_Step.DFPAN_STEP_100KHZ;
                        if (Enum.TryParse(para.Value, out dfPanStep))
                        {
                            DfPanStep = ((ulong)dfPanStep) / 100000d;
                        }
                        else
                        {
                            string msg = string.Format("No eDfPanStep found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "iBlockAveragingTime":
                        int measureTime = 100;
                        if (int.TryParse(para.Value, out measureTime))
                        {
                            IntegrationTime = measureTime;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iBlockAveragingTime with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "iThreshold":
                        int threshold = 0;
                        if (int.TryParse(para.Value, out threshold))
                        {
                            LevelThreshold = threshold;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iThreshold with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eAntPol":
                        EAnt_Pol antPol = EAnt_Pol.POL_VERTICAL;
                        if (Enum.TryParse(para.Value, out antPol))
                        {
                            AntPol = antPol;
                        }
                        else
                        {
                            string msg = string.Format("No eAntPol found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eSpan":
                        ESpan span = ESpan.IFPAN_FREQ_RANGE_100;
                        if (Enum.TryParse(para.Value, out span))
                        {
                            SpectrumSpan = (int)span;
                        }
                        else
                        {
                            string msg = string.Format("No eSpan found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eAttSelect":
                        EAtt_Select attSelect = EAtt_Select.ATT_AUTO;
                        if (Enum.TryParse(para.Value, out attSelect))
                        {
                            if (attSelect == EAtt_Select.ATT_AUTO)
                                Attenuation = -1;
                        }
                        else
                        {
                            string msg = string.Format("No eAttSelect found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "iAttValue":
                        int attVal = -1;
                        if (int.TryParse(para.Value, out attVal))
                        {
                            if (Attenuation != -1)
                                Attenuation = attVal;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iAttValue with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eIFPanStep":
                        EIFPan_Step ifPanStep = EIFPan_Step.IFPAN_STEP_100KHZ;
                        if (Enum.TryParse(para.Value, out ifPanStep))
                        {
                            if (ifPanStep == EIFPan_Step.IFPAN_STEP_AUTO)
                                ifPanStep = GetAutoIfBandWidth();
                            IfPanStep = ((ulong)ifPanStep) / 100000d;
                        }
                        else
                        {
                            string msg = string.Format("No eIFPanStep found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eBlockAveragingSelect":// 默认永远为测量时间模式
                    case "iBlockAveragingCycles":// 默认永远为测量时间模式
                    case "eAntPreAmp"://不用处理
                    case "eDfAlt":
                    case "eWindowType":
                    case "eDfPanSelectivity":
                    case "iAttHoldTime"://暂时不处理
                    case "eIFPanSelectivity":
                    case "eIFPanMode":
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdSetMeasureSettingsPScan(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iFreqBegin":
                        double freqStart = 0d;
                        if (double.TryParse(para.Value, out freqStart))
                        {
                            StartFrequency = freqStart / 1000000;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iFreqBegin with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "iFreqEnd":
                        double freqStop = 0d;
                        if (double.TryParse(para.Value, out freqStop))
                        {
                            StopFrequency = freqStop / 1000000;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iFreqEnd with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eStep":
                        EPScan_Step step = EPScan_Step.PSCAN_STEP_200;
                        if (Enum.TryParse(para.Value, out step))
                        {
                            Step = (uint)step / 1000d;
                        }
                        else
                        {
                            string msg = string.Format("No eStep found with the name  \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdSetRxSettings(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iMeasureTime":
                        double time = 100000;
                        if (double.TryParse(para.Value, out time))
                        {
                            MeasureTime = time / 1000000;
                        }
                        else
                        {
                            string msg = string.Format("Incorrect number format of iFreqBegin with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eMeasureMode":
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdSetITU(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "bEnableMeasurement":
                        bool enabled = false;
                        if (bool.TryParse(para.Value, out enabled))
                            Client.IsSendITU = enabled;
                        break;
                    case "eBwMeasurementMode":
                        EMeasureMode mode = EMeasureMode.MEASUREMODE_XDB;
                        if (Enum.TryParse(para.Value, out mode))
                            ItuMeasureMode = mode;
                        break;
                    case "iConfigBwXdB":
                        int xdb = 0;
                        if (int.TryParse(para.Value, out xdb))
                            XdbBandWidth = xdb / 10d;
                        break;
                    case "iConfigBwBeta":
                        int beta = 0;
                        if (int.TryParse(para.Value, out beta))
                            BetaBandWidth = beta / 10d;
                        break;
                    case "bUseAutoBandwidthLimits":
                    case "iLowerBandwidthLimit":
                    case "iUpperBandwidthLimit":
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdSetDemodulationSettings(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "eDemodulation":
                        EDemodulation mode = EDemodulation.MOD_AM;
                        if (Enum.TryParse(para.Value, out mode))
                            DemMode = mode;
                        break;
                    case "iAfFrequency":
                        long freq = 101700000;
                        if (long.TryParse(para.Value, out freq))
                            DemFrequency = freq / 1000000d;
                        break;
                    case "eAfBandwidth":
                        EAf_BandWidth bw = EAf_BandWidth.BW_120;
                        if (Enum.TryParse(para.Value, out bw))
                            DemBandWidth = (uint)bw / 1000d;
                        break;
                    case "iAfThreshold":
                        int threshold = 0;
                        if (int.TryParse(para.Value, out threshold))
                            SquelchThreshold = threshold;
                        break;
                    case "bUseAfThreshold":
                        bool isUse = false;
                        if (bool.TryParse(para.Value, out isUse))
                            IsUseSquelch = isUse;
                        break;
                    case "eLevelIndicator":
                        ELevel_Indicatir detector = ELevel_Indicatir.LEVEL_INDICATOR_FAST;
                        if (Enum.TryParse(para.Value, out detector))
                            Detector = detector;
                        break;
                    case "eGainSelect":
                        EGain_Control ctrl = EGain_Control.GAIN_AUTO;
                        if (Enum.TryParse(para.Value, out ctrl))
                        {
                            if (ctrl == EGain_Control.GAIN_AUTO)
                                Gain = -100;
                        }
                        break;
                    case "iGainValue":
                        int gain = 0;
                        if (int.TryParse(para.Value, out gain))
                        {
                            if (Gain > -100)
                                Gain = gain;
                        }
                        break;
                    case "eGainTiming":
                    case "bStereoDecoder":
                    case "iBfoFrequency":
                    case "iPassbandFrequency":
                    case "bAfc":
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdSetTraceEnable(Request request, ref Reply reply)
        {
            string ip = "";
            int port = 0;
            ETraceTag eTraceTag = ETraceTag.TRACETAG_AUDIO;
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "eTraceTag":
                        if (!Enum.TryParse(para.Value, out eTraceTag))
                        {
                            string msg = string.Format("No eTraceTag found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "zIP":
                        ip = para.Value;
                        if (!Client.AddressData.Equals(ip))
                        {
                            string msg = string.Format("No zIP found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "iPort":
                        int.TryParse(para.Value, out port);
                        if (port != Client.PortData)
                        {
                            string msg = string.Format("No iPort found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                }
            }

            switch (eTraceTag)
            {
                case ETraceTag.TRACETAG_AUDIO:
                    Client.IsSendAudio = true;
                    break;
                case ETraceTag.TRACETAG_DF:
                    Client.IsSendDF = true;
                    break;
                case ETraceTag.TRACETAG_IF:
                    Client.IsSendIQ = true;
                    break;
                case ETraceTag.TRACETAG_CWAVE:
                    Client.IsSendCW = true;
                    break;
                case ETraceTag.TRACETAG_IFPAN:
                    Client.IsSendSpectrum = true;
                    break;
                case ETraceTag.TRACETAG_PSCAN:
                    Client.IsSendPScan = true;
                    break;
                default:
                    break;
            }

            if ((DfMode == EDfMode.DFMODE_FFM || DfMode == EDfMode.DFMODE_SCAN || DfMode == EDfMode.DFMODE_SEARCH) && Client.IsSendDF)
            {
                IsRunning = true;
            }
        }

        private void AnalysisCmdSetTraceDisable(Request request, ref Reply reply)
        {
            string ip = "";
            int port = 0;
            ETraceTag eTraceTag = ETraceTag.TRACETAG_AUDIO;
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "eTraceTag":
                        if (!Enum.TryParse(para.Value, out eTraceTag))
                        {
                            string msg = string.Format("No eTraceTag found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "zIP":
                        ip = para.Value;
                        if (!Client.AddressData.Equals(ip))
                        {
                            string msg = string.Format("No zIP found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "iPort":
                        int.TryParse(para.Value, out port);
                        if (port != Client.PortData)
                        {
                            string msg = string.Format("No iPort found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                }
            }


            switch (eTraceTag)
            {
                case ETraceTag.TRACETAG_AUDIO:
                    Client.IsSendAudio = false;
                    break;
                case ETraceTag.TRACETAG_DF:
                    Client.IsSendDF = false;
                    _lastSendCnt = 0;
                    _hopIndexScanDF = 0;
                    _scanRangeCnt = 0;
                    break;
                case ETraceTag.TRACETAG_IF:
                    Client.IsSendIQ = false;
                    break;
                case ETraceTag.TRACETAG_CWAVE:
                    Client.IsSendCW = false;
                    break;
                case ETraceTag.TRACETAG_IFPAN:
                    Client.IsSendSpectrum = false;
                    break;
                case ETraceTag.TRACETAG_PSCAN:
                    Client.IsSendPScan = false;
                    break;
                default:
                    break;
            }
            //if (DfMode == EDfMode.DFMODE_FFM && !Client.IsSendDF)
            //{
            //    IsRunning = false;
            //}
        }

        private void AnalysisCmdScanRangeDelete(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iScanRangeId":
                        int id = 0;
                        if (!int.TryParse(para.Value, out id))
                        {
                            string msg = string.Format("Incorrect number format of iScanRangeId with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        var info = ScanRangeList.FirstOrDefault(i => i.ID == id);
                        if (info == null)
                        {
                            string msg = string.Format("No iScanRangeId found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        _dispatcher.BeginInvoke(new Action(() => ScanRangeList.Remove(info)));
                        if (info.ID == RunningScanRange.ID)
                            RunningScanRange = null;
                        break;
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdScanRangeAdd(Request request, ref Reply reply)
        {
            ulong start = 0;
            ulong stop = 0;
            ESpan spanEnum = ESpan.IFPAN_FREQ_RANGE_100;
            EDfPan_Step stepEnum = EDfPan_Step.DFPAN_STEP_1000KHZ;
            double startFreq = 0;
            double stopFreq = 0;
            double span = 0;
            double step = 0;
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iFreqBegin":
                        if (!ulong.TryParse(para.Value, out start))
                        {
                            string msg = string.Format("Incorrect number format of iFreqBegin with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        startFreq = start / 1000000d;
                        break;
                    case "iFreqEnd":
                        if (!ulong.TryParse(para.Value, out stop))
                        {
                            string msg = string.Format("Incorrect number format of iFreqEnd with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        stopFreq = stop / 1000000d;
                        break;
                    case "eSpan":
                        if (!Enum.TryParse(para.Value, out spanEnum))
                        {
                            string msg = string.Format("No eSpan found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        span = ((int)spanEnum);
                        break;
                    case "eDfPanStep":
                        if (!Enum.TryParse(para.Value, out stepEnum))
                        {
                            string msg = string.Format("No eDfPanStep found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        step = ((int)stepEnum) / 100000d;
                        break;
                    default:
                        break;
                }
            }

            int id = 0;
            if (_scanRangeList.Count == 0)
                id = 0;
            else
            {
                id = _scanRangeList.Max(i => i.ID) + 1;
            }
            ScanRangeInfo info = new ScanRangeInfo();
            info.ID = id;
            info.StartFrequency = startFreq;
            info.StopFrequency = stopFreq;
            info.Span = span;
            info.Step = step;
            info.NumHops = GetNumHops(info.StartFrequency, info.StopFrequency, info.Step);
            _dispatcher.Invoke(new Action(() => ScanRangeList.Add(info)));

            Param param1 = new Param();
            param1.Name = "iFreqBegin";
            param1.Value = start.ToString();
            Param param2 = new Param();
            param2.Name = "iFreqEnd";
            param2.Value = stop.ToString();
            Param param3 = new Param();
            param3.Name = "eSpan";
            param3.Value = spanEnum.ToString();
            Param param4 = new Param();
            param4.Name = "eDfPanStep";
            param4.Value = stepEnum.ToString();
            Param param5 = new Param();
            param5.Name = "iScanRangeId";
            param5.Value = info.ID.ToString();
            Param param6 = new Param();
            param6.Name = "iNumHops";
            param6.Value = info.NumHops.ToString();


            reply.Command.Params.Add(param1);
            reply.Command.Params.Add(param2);
            reply.Command.Params.Add(param3);
            reply.Command.Params.Add(param4);
            reply.Command.Params.Add(param5);
            reply.Command.Params.Add(param6);
        }

        private void AnalysisCmdScanRange(Request request, ref Reply reply)
        {
            ulong start = 0;
            ulong stop = 0;
            ESpan spanEnum = ESpan.IFPAN_FREQ_RANGE_100;
            EDfPan_Step stepEnum = EDfPan_Step.DFPAN_STEP_1000KHZ;
            bool startChanged = false;
            bool stopChanged = false;
            bool spanChanged = false;
            bool stepChanged = false;
            double startFreq = 0;
            double stopFreq = 0;
            double span = 0;
            double step = 0;
            ScanRangeInfo info = null;
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "iScanRangeId":
                        int id = 0;
                        if (!int.TryParse(para.Value, out id))
                        {
                            string msg = string.Format("Incorrect number format of iScanRangeId with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        info = ScanRangeList.FirstOrDefault(i => i.ID == id);
                        if (info == null)
                        {
                            string msg = string.Format("No iScanRangeId found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "iFreqBegin":
                        if (!ulong.TryParse(para.Value, out start))
                        {
                            string msg = string.Format("Incorrect number format of iFreqBegin with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        startFreq = start / 1000000d;
                        startChanged = true;
                        break;
                    case "iFreqEnd":
                        if (!ulong.TryParse(para.Value, out stop))
                        {
                            string msg = string.Format("Incorrect number format of iFreqEnd with the value \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        stopFreq = stop / 1000000d;
                        stopChanged = true;
                        break;
                    case "eSpan":
                        if (!Enum.TryParse(para.Value, out spanEnum))
                        {
                            string msg = string.Format("No eSpan found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        span = ((int)spanEnum);
                        spanChanged = true;
                        break;
                    case "eDfPanStep":
                        if (!Enum.TryParse(para.Value, out stepEnum))
                        {
                            string msg = string.Format("No eDfPanStep found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        step = ((int)stepEnum) / 100000d;
                        stepChanged = true;
                        break;
                    default:
                        break;
                }
            }
            if (startChanged)
                info.StartFrequency = start;
            if (stopChanged)
                info.StopFrequency = stop;
            if (spanChanged)
                info.Span = span;
            if (stepChanged)
                info.Step = step;
            info.NumHops = GetNumHops(info.StartFrequency, info.StopFrequency, info.Step);

            Param param5 = new Param();
            param5.Name = "iScanRangeId";
            param5.Value = info.ID.ToString();
            Param param6 = new Param();
            param6.Name = "iNumHops";
            param6.Value = info.NumHops.ToString();


            reply.Command.Params.Add(param5);
            reply.Command.Params.Add(param6);

            RunningScanRange = info;
        }

        private void UpdateReply(ref Reply reply, string rtnCode, string rtnMessage, string paramName, string paramValue)
        {
            reply.Command.RtnCode = rtnCode;
            reply.Command.RtnMessage = rtnMessage;
            reply.Command.Params.Add(new Param() { Name = paramName, Value = paramValue });
        }


        private EIFPan_Step GetAutoIfBandWidth()
        {
            ESpan span = (ESpan)((int)_spectrumSpan);
            switch (span)
            {
                case ESpan.IFPAN_FREQ_RANGE_100:
                    return EIFPan_Step.IFPAN_STEP_62P5HZ;
                case ESpan.IFPAN_FREQ_RANGE_200:
                    return EIFPan_Step.IFPAN_STEP_125HZ;
                case ESpan.IFPAN_FREQ_RANGE_500:
                    return EIFPan_Step.IFPAN_STEP_312P5HZ;
                case ESpan.IFPAN_FREQ_RANGE_1000:
                    return EIFPan_Step.IFPAN_STEP_625HZ;
                case ESpan.IFPAN_FREQ_RANGE_2000:
                    return EIFPan_Step.IFPAN_STEP_1P25KHZ;
                case ESpan.IFPAN_FREQ_RANGE_5000:
                    return EIFPan_Step.IFPAN_STEP_3P125KHZ;
                case ESpan.IFPAN_FREQ_RANGE_10000:
                    return EIFPan_Step.IFPAN_STEP_6P25KHZ;
                case ESpan.IFPAN_FREQ_RANGE_20000:
                    return EIFPan_Step.IFPAN_STEP_12P5KHZ;
                case ESpan.IFPAN_FREQ_RANGE_40000:
                    return EIFPan_Step.IFPAN_STEP_25KHZ;
                case ESpan.IFPAN_FREQ_RANGE_80000:
                    return EIFPan_Step.IFPAN_STEP_50KHZ;
            }
            return EIFPan_Step.IFPAN_STEP_6P25KHZ;
        }

        //装包
        private byte[] PackedXMLCommand(byte[] cmd)
        {
            List<byte> arr = new List<byte>();
            arr.AddRange(Xml_Start);
            int len = cmd.Length + 1;
            byte[] lenArr = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenArr);
            arr.AddRange(lenArr);
            arr.AddRange(cmd);
            arr.Add(0x00);//添加终止符\0
            arr.AddRange(Xml_End);
            return arr.ToArray();
        }

        //解包
        private bool UnPackedXMLCommand(byte[] data, ref List<byte> cacheCmd)
        {
            cacheCmd.AddRange(data);
            byte[] buffer = cacheCmd.ToArray();
            cacheCmd.Clear();
            var find1 = buffer.IndexesOf(0, Xml_Start);
            var find2 = buffer.IndexesOf(0, Xml_End);
            int count1 = find1.Count();
            int count2 = find2.Count();
            if (count1 > 0 && count2 > 0)
            {
                //有头有尾,找第一个符合条件的
                int index1 = find1.First();
                int index2 = find2.FirstOrDefault(i => i > index1);//如果找不到符合条件的,则返回0
                if (index2 > 0)
                {
                    //尾在头后,正常
                    cacheCmd.AddRange(buffer.Skip(index1).Take(index2 + Xml_End.Length - index1));
                    return true;
                }
                else
                {
                    //尾在头前
                    cacheCmd.AddRange(data.Skip(index1));
                    return false;
                }
            }
            else if (count1 == 0 && count2 > 0)
            {
                //没头有尾
                return false;
            }
            else if (count1 > 0 && count2 == 0)
            {
                //有头没尾
                int index1 = find1.First();
                cacheCmd.AddRange(data.Skip(index1));
                return false;
            }
            else
            {
                //没头没尾
                return false;
            }
        }

        //校验
        private bool CheckXMLCommand(byte[] data)
        {
            byte[] lenData = new byte[4];
            Buffer.BlockCopy(data, 4, lenData, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenData);
            int len = BitConverter.ToInt32(lenData, 0);
            return len == data.Length - 12;
        }

        private int GetNumHops(double start, double stop, double step)
        {
            int count = (int)(((stop - start) * 1000) / step) + 1;
            int hops = count / (int)SCANDF_MAXCNT;
            int num = count % (int)SCANDF_MAXCNT;
            if (num > 0)
                hops++;
            return hops;
        }
    }
}
