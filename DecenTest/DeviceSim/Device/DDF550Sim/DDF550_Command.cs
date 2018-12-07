
/*********************************************************************************************
 *	
 * 文件名称:    DDF550_TraceTagEnabled.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-15 14:55:00
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceSim.Device
{
    public partial class DDF550Device
    {
        #region Command定义

        /// <summary>
        /// 未知的Command
        /// </summary>
        private const string INCORRECT_COMMAND = "CouldNotParseCommandError";

        // 通用设置
        private const string CMD_DFMODE = "DfMode";
        private const string CMD_MEASURESETTINGSFFM = "MeasureSettingsFFM";

        private const string CMD_ITU = "ITU";
        private const string CMD_SELCALL = "SelCall";
        private const string CMD_DEMODULATIONSETTINGS = "DemodulationSettings";
        private const string CMD_AUDIOMODE = "AudioMode";
        private const string CMD_REFERENCEMODE = "ReferenceMode";
        private const string CMD_RFMODE = "RfMode";
        private const string CMD_IFMODE = "IfMode";

        // 数据启用/停用
        private const string CMD_TRACEENABLE = "TraceEnable";
        private const string CMD_TRACEDISABLE = "TraceDisable";
        private const string CMD_TRACEDELETE = "TraceDelete";
        private const string CMD_TRACEDELETEINACTIVE = "TraceDeleteInactive";

        // TraceFlag设置
        private const string CMD_TRACEFLAGDISABLE = "TraceFlagDisable";
        private const string CMD_TRACEFLAGENABLE = "TraceFlagEnable";

        // Rx或RxPScan模式下的设置
        private const string CMD_INITIATE = "Initiate";
        private const string CMD_ABORT = "Abort";
        private const string CMD_MEASURESETTINGSPSCAN = "MeasureSettingsPScan";
        private const string CMD_RXSETTINGS = "RxSettings";

        // 天线设置
        private const string CMD_ANTENNACONTROL = "AntennaControl";
        private const string CMD_ANTENNASETUP = "AntennaSetup";

        //扫描测向
        private const string CMD_SCANRANGE = "ScanRange";
        private const string CMD_SCANRANGEADD = "ScanRangeAdd";
        private const string CMD_SCANRANGEDELETE = "ScanRangeDelete";
        private const string CMD_SCANRANGEDELETEALL = "ScanRangeDeleteAll";
        private const string CMD_SCANRANGENEXT = "ScanRangeNext";

        #endregion Command定义

    }
}
