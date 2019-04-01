
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
using System.Net;
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
            List<byte> cacheCmd = new List<byte>();
            IPEndPoint iPEndPoint = _socketXml.RemoteEndPoint as IPEndPoint;
            while (Connected)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    // 读取包头
                    ReceiveData(buffer, 0, 4, _socketXml);

                    if (buffer[0] == Xml_Start[0] && buffer[1] == Xml_Start[1] && buffer[2] == Xml_Start[2] && buffer[3] == Xml_Start[3])
                    {
                        // 读取头结构
                        ReceiveData(buffer, 4, 4, _socketXml);
                        byte[] lenArr = new byte[4];
                        Buffer.BlockCopy(buffer, 4, lenArr, 0, 4);
                        Array.Reverse(lenArr);
                        int len = BitConverter.ToInt32(lenArr, 0);
                        // 读取剩下的数据
                        ReceiveData(buffer, 8, len + 4, _socketXml);
                        byte[] data = new byte[len + 12];
                        Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                        if (!CheckXMLCommand(data))
                            continue;
                        //offset += recBytes;
                        // 从尾部查找0x00,Xml反序列化的时候不能正确识别截止符,因此需要将截止符去掉
                        for (int i = data.Length - 5; i >= 0; i--)
                        {
                            if (data[i] == 0x00)
                                len--;
                            else
                                break;
                        }
                        byte[] xmlData = new byte[len];
                        Buffer.BlockCopy(data, 8, xmlData, 0, len);
                        Request request = (Request)XmlWrapper.DeserializeObject<Request>(xmlData);
                        ProcessXml(request);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SocketException || ex is ObjectDisposedException)
                    {
                        CloseConnect(iPEndPoint);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 读取指定长度的数据到数组
        /// </summary>
        /// <param name="recvBuffer">接收数据缓冲区</param>
        /// <param name="offset">缓冲区的偏移</param>
        /// <param name="bytesToRead">要读取的字节数</param>
        /// <param name="socket">要接收数据的套接字</param>
        private void ReceiveData(byte[] recvBuffer, int offset, int bytesToRead, Socket socket)
        {
            //当前已接收到的字节数
            int totalRecvLen = 0;
            //循环接收数据，确保接收完指定字节数
            while (totalRecvLen < bytesToRead)
            {
                int recvLen = socket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen, SocketFlags.None);
                if (recvLen <= 0)
                {
                    //远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候此处不会抛异常而是立即返回0，
                    //为避免出现此情况将导致该函数死循环，此处直接抛SocketException异常
                    //10054:远程主机强迫关闭了一个现有连接
                    throw new SocketException(10054);
                }
                totalRecvLen += recvLen;
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
                        AnalysisCmdAudioMode(request, ref reply);
                        break;
                    case CMD_ANTENNACONTROL:
                        break;
                    case CMD_REFERENCEMODE:
                        break;
                    case CMD_RFMODE:
                        AnalysisCmdRfMode(request, ref reply);
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
                        if (ClientList.All(c => !c.IsSendAudio) && ClientList.All(c => !c.IsSendDF)
                            && ClientList.All(c => !c.IsSendIQ) && ClientList.All(c => !c.IsSendCW)
                            && ClientList.All(c => !c.IsSendSpectrum))
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
                        _dispatcher.Invoke(new Action(() => ScanRangeList.Clear()));
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
                            {
                                _attAuto = true;
                                Attenuation = -1;
                            }
                            else
                                _attAuto = false;
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
                            if (!_attAuto)
                                Attenuation = attVal;
                            else
                                Attenuation = -1;
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
                    case "eIFPanMode":
                        EIFPan_Mode mode = EIFPan_Mode.IFPAN_MODE_CLRWRITE;
                        if (Enum.TryParse(para.Value, out mode))
                        {
                            if (mode != _fftMode)
                            {
                                lock (_lockfftData)
                                {
                                    _fftCount = 0;
                                    _lastFFTSendTime = DateTime.Now;
                                    _fftData = null;
                                    _fftMode = mode;
                                }
                            }
                        }
                        else
                        {
                            string msg = string.Format("No eIFPanMode found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eAntPreAmp":
                        EState state = EState.STATE_OFF;
                        if (Enum.TryParse(para.Value, out state))
                        {
                            AntPreAmp = state == EState.STATE_ON;
                        }
                        else
                        {
                            string msg = string.Format("No eAntPreAmp found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                        }
                        break;
                    case "eBlockAveragingSelect":// 默认永远为测量时间模式
                    case "iBlockAveragingCycles":// 默认永远为测量时间模式
                    case "eDfAlt":
                    case "eWindowType":
                    case "eDfPanSelectivity":
                    case "iAttHoldTime"://暂时不处理
                    case "eIFPanSelectivity":
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
                            MeasureTime = time / 1000000d;
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
                        {
                            ClientList.Where(i => i.IsSendCW).ToList().ForEach(i => i.IsSendITU = enabled);
                            CheckSwitch();
                        }
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
                        bool isUse = false;
                        if (bool.TryParse(para.Value, out isUse))
                            UseAutoBandwidthLimits = isUse;
                        break;
                    case "iLowerBandwidthLimit":
                        int lower = 0;
                        if (int.TryParse(para.Value, out lower))
                            LowerBandwidthLimit = lower / 1000d;
                        break;
                    case "iUpperBandwidthLimit":
                        int upper = 0;
                        if (int.TryParse(para.Value, out upper))
                            UpperBandwidthLimit = upper / 1000d;
                        break;
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
                            {
                                _gainAuto = true;
                                Gain = -100;
                            }
                            else
                                _gainAuto = false;
                        }
                        break;
                    case "iGainValue":
                        int gain = 0;
                        if (int.TryParse(para.Value, out gain))
                        {
                            if (!_gainAuto)
                                Gain = gain;
                            else
                                Gain = -100;
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
                        if (ClientList.Where(i => i.Address.Address.ToString().Equals(ip)).Count() == 0)
                        {
                            string msg = string.Format("No zIP found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "iPort":
                        int.TryParse(para.Value, out port);
                        if (ClientList.Where(i => i.Address.Port.Equals(port)).Count() == 0)
                        {
                            string msg = string.Format("No iPort found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                }
            }

            var client = _clientList.Where(i => i.Address.Address.ToString().Equals(ip) && i.Address.Port.Equals(port) && !i.IsXml).ToList();
            switch (eTraceTag)
            {
                case ETraceTag.TRACETAG_AUDIO:
                    client.ForEach(c => c.IsSendAudio = true);
                    break;
                case ETraceTag.TRACETAG_DF:
                    client.ForEach(c => c.IsSendDF = true);
                    break;
                case ETraceTag.TRACETAG_IF:
                    client.ForEach(c => c.IsSendIQ = true);
                    break;
                case ETraceTag.TRACETAG_CWAVE:
                    client.ForEach(c => c.IsSendCW = true);
                    break;
                case ETraceTag.TRACETAG_IFPAN:
                    client.ForEach(c => c.IsSendSpectrum = true);
                    break;
                case ETraceTag.TRACETAG_PSCAN:
                    client.ForEach(c => c.IsSendPScan = true);
                    break;
                default:
                    break;
            }

            if ((DfMode == EDfMode.DFMODE_FFM || DfMode == EDfMode.DFMODE_SCAN || DfMode == EDfMode.DFMODE_SEARCH) && client.Count(i => i.IsSendDF) > 0)
            {
                IsRunning = true;
            }
            CheckSwitch();
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
                        if (ClientList.Count(i => i.Address.Address.ToString().Equals(ip)) == 0)
                        {
                            string msg = string.Format("No zIP found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                    case "iPort":
                        int.TryParse(para.Value, out port);
                        if (ClientList.Count(i => i.Address.Port.Equals(port)) == 0)
                        {
                            string msg = string.Format("No iPort found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        break;
                }
            }


            var client = _clientList.Where(i => i.Address.Address.ToString().Equals(ip) && i.Address.Port.Equals(port) && !i.IsXml).ToList();
            switch (eTraceTag)
            {
                case ETraceTag.TRACETAG_AUDIO:
                    client.ForEach(c => c.IsSendAudio = false);
                    break;
                case ETraceTag.TRACETAG_DF:
                    client.ForEach(c => c.IsSendDF = false);
                    _lastSendCnt = 0;
                    _hopIndexScanDF = 0;
                    _scanRangeCnt = 0;
                    break;
                case ETraceTag.TRACETAG_IF:
                    client.ForEach(c => c.IsSendIQ = false);
                    break;
                case ETraceTag.TRACETAG_CWAVE:
                    client.ForEach(c => c.IsSendCW = false);
                    break;
                case ETraceTag.TRACETAG_IFPAN:
                    client.ForEach(c => c.IsSendSpectrum = false);
                    break;
                case ETraceTag.TRACETAG_PSCAN:
                    client.ForEach(c => c.IsSendPScan = false);
                    break;
                default:
                    break;
            }
            //if (DfMode == EDfMode.DFMODE_FFM && !Client.IsSendDF)
            //{
            //    IsRunning = false;
            //}
            CheckSwitch();
        }

        private void CheckSwitch()
        {
            AudioSwitch = ClientList.FirstOrDefault(i => i.IsSendAudio) != null;
            CWSwitch = ClientList.FirstOrDefault(i => i.IsSendCW) != null;
            DFSwitch = ClientList.FirstOrDefault(i => i.IsSendDF) != null;
            IQSwitch = ClientList.FirstOrDefault(i => i.IsSendIQ) != null;
            ITUSwitch = ClientList.FirstOrDefault(i => i.IsSendITU) != null;
            ScanSwitch = ClientList.FirstOrDefault(i => i.IsSendPScan) != null;
            SpectrumSwitch = ClientList.FirstOrDefault(i => i.IsSendSpectrum) != null;
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
                        _dispatcher.Invoke(new Action(() => ScanRangeList.Remove(info)));
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
            info.NumHops = GetNumHops(info.StartFrequency, info.StopFrequency, info.Step, info.Span);
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
            info.NumHops = GetNumHops(info.StartFrequency, info.StopFrequency, info.Step, info.Span);

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

        private void AnalysisCmdRfMode(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "eRfMode":
                        ERf_Mode mode = ERf_Mode.RFMODE_NORMAL;
                        if (!Enum.TryParse(para.Value, out mode))
                        {
                            string msg = string.Format("No eRfMode found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        RfMode = mode;
                        break;
                    default:
                        break;
                }
            }
        }

        private void AnalysisCmdAudioMode(Request request, ref Reply reply)
        {
            foreach (var para in request.Command.Params)
            {
                switch (para.Name)
                {
                    case "eAudioMode":
                        EAudioMode mode = EAudioMode.AUDIO_MODE_OFF;
                        if (!Enum.TryParse(para.Value, out mode))
                        {
                            string msg = string.Format("No eAudioMode found with the name \"{0}\"", para.Value);
                            UpdateReply(ref reply, "10520", msg, para.Name, para.Value);
                            return;
                        }
                        AudioMode = mode;
                        break;
                    default:
                        break;
                }
            }
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

        private int GetNumHops(double start, double stop, double step, double span)
        {
            double span1 = ((stop - start) * 1000);
            int total = (int)(span1 / step) + 1;
            double span2 = span > span1 ? span1 : span;
            int sendlen = (int)(span2 / step - 1);
            int hops = total / sendlen;
            int num = total % sendlen;
            if (num > 0)
                hops++;
            return hops;
        }
    }
}
