
/*********************************************************************************************
 *	
 * 文件名称:    DDF550Protocol.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-14 9:10:01
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DeviceSim.Device
{

    #region Xml枚举

    /// <summary>
    /// 3.1 Enum: eAF_BANDWIDTH 解调带宽
    /// Demodulation bandwidth.
    /// </summary>
    public enum EAf_BandWidth : uint
    {
        /// <summary>
        /// 100 Hz
        /// </summary>
        BW_0P1 = 100,
        /// <summary>
        /// 150 Hz
        /// </summary>
        BW_0P15 = 150,
        /// <summary>
        /// 300 Hz
        /// </summary>
        BW_0P3 = 300,
        /// <summary>
        /// 600 Hz
        /// </summary>
        BW_0P6 = 600,
        /// <summary>
        /// 1 kHz
        /// </summary>
        BW_1 = 1000,
        /// <summary>
        /// 1.5 kHz
        /// </summary>
        BW_1P5 = 1500,
        /// <summary>
        /// 2.1 kHz
        /// </summary>
        BW_2P1 = 2100,
        /// <summary>
        /// 2.4 kHz
        /// </summary>
        BW_2P4 = 2400,
        /// <summary>
        /// 2.7 kHz
        /// </summary>
        BW_2P7 = 2700,
        /// <summary>
        /// 3.1 kHz
        /// </summary>
        BW_3P1 = 3100,
        /// <summary>
        /// 4 kHz
        /// </summary>
        BW_4 = 4000,
        /// <summary>
        /// 4.8 kHz
        /// </summary>
        BW_4P8 = 4800,
        /// <summary>
        /// 6 kHz
        /// </summary>
        BW_6 = 6000,
        /// <summary>
        /// 8.333 kHz
        /// </summary>
        BW_8P333 = 8333,
        /// <summary>
        /// 9 kHz
        /// </summary>
        BW_9 = 9000,
        /// <summary>
        /// 12 kHz
        /// </summary>
        BW_12 = 12000,
        /// <summary>
        /// 15 kHz
        /// </summary>
        BW_15 = 15000,
        /// <summary>
        /// 25 kHz
        /// </summary>
        BW_25 = 25000,
        /// <summary>
        /// 30 kHz
        /// </summary>
        BW_30 = 30000,
        /// <summary>
        /// 50 kHz
        /// </summary>
        BW_50 = 50000,
        /// <summary>
        /// 75 kHz
        /// </summary>
        BW_75 = 75000,
        /// <summary>
        /// 120 kHz
        /// </summary>
        BW_120 = 120000,
        /// <summary>
        /// 150 kHz
        /// </summary>
        BW_150 = 150000,
        /// <summary>
        /// 250 kHz
        /// </summary>
        BW_250 = 250000,
        /// <summary>
        /// 300 kHz
        /// </summary>
        BW_300 = 300000,
        /// <summary>
        /// 500 kHz
        /// </summary>
        BW_500 = 500000,
        /// <summary>
        /// 800 kHz
        /// </summary>
        BW_800 = 800000,
        /// <summary>
        /// 1 MHz
        /// </summary>
        BW_1000 = 1000000,
        /// <summary>
        /// 1.25 MHz
        /// </summary>
        BW_1250 = 1250000,
        /// <summary>
        /// 1.5 MHz
        /// </summary>
        BW_1500 = 1500000,
        /// <summary>
        /// 2 MHz
        /// </summary>
        BW_2000 = 2000000,
        /// <summary>
        /// 5 MHz
        /// </summary>
        BW_5000 = 5000000,
        /// <summary>
        /// 8 MHz
        /// </summary>
        BW_8000 = 8000000,
        /// <summary>
        /// 10 MHz
        /// </summary>
        BW_10000 = 10000000,
        /// <summary>
        /// 12.5 MHz
        /// </summary>
        BW_12500 = 12500000,
        /// <summary>
        /// 15 MHz
        /// </summary>
        BW_15000 = 15000000,
        /// <summary>
        /// 20 MHz
        /// </summary>
        BW_20000 = 20000000,
    }

    /// <summary>
    /// 3.2 Enum: eAF_FILTER_MODE 音频过滤模式
    /// Audio filter mode.
    /// </summary>
    public enum EAf_Filter_Mode
    {
        /// <summary>
        /// Off: no filter function active.
        /// </summary>
        AF_FILTER_OFF,
        /// <summary>
        /// Notch filter: automatic elimination of interference signals.
        /// </summary>
        AF_FILTER_NOTCH,
        /// <summary>
        /// Noise reduction filter.
        /// </summary>
        AF_FILTER_NR,
        /// <summary>
        /// Bandpass filter 300 Hz to 3.3 kHz (telephone channel).
        /// </summary>
        AF_FILTER_BP,
        /// <summary>
        /// High deemphasis (time constant 25 µs).
        /// </summary>
        AF_FILTER_DEEMPHASIS_25,
        /// <summary>
        /// European FM radio deemphasis (time constant 50 µs).
        /// </summary>
        AF_FILTER_DEEMPHASIS_50,
        /// <summary>
        /// USA FM radio deemphasis (time constant 75 µs).
        /// </summary>
        AF_FILTER_DEEMPHASIS_75,
        /// <summary>
        /// FM Radio-telephone deemphasis (time constant 750 µs).
        /// </summary>
        AF_FILTER_DEEMPHASIS_750,
    }

    /// <summary>
    /// 3.3 Enum: eANALOG_VIDEO_OUTPUT 输出IF (X61)和视频(X62)的模拟信号类型。
    /// Type of analog signal at outputs IF (X61) and Video (X62).
    /// </summary>
    public enum EAnalog_Video_Output
    {
        /// <summary>
        /// IF signal: in-phase component on X61, quadrature component on X62.
        /// </summary>
        VIDEO_OUTPUT_IF,
        /// <summary>
        ///  Demodulated signal in negative form: AM on X61, FM on X62.
        /// </summary>
        VIDEO_OUTPUT_VIDEO_NEG,
        /// <summary>
        /// Demodulated signal in positive form: AM on X61, FM on X62.
        /// </summary>
        VIDEO_OUTPUT_VIDEO_POS,
    }

    /// <summary>
    /// 3.4 Enum: eANTLEVELSWITCH 天线测试运行:输出电平指示值或不输出电平指示值，天线测试散热器开或关
    /// Test operation of antenna: output of level indications or not, antenna test radiator on or off.
    /// </summary>
    public enum EAnt_Level_Switch
    {
        /// <summary>
        /// Normal DF or Rx operation: no output of antenna level indications.
        /// </summary>
        ANTLEVEL_OFF,
        /// <summary>
        /// Antenna radiator test operation: output of level indications, antenna test radiator not available or switched off (test signal from outside).
        /// </summary>
        ANTLEVEL_ON_EMITTER_OFF,
        /// <summary>
        /// Ditto, test radiator on and emitting test signal.
        /// </summary>
        ANTLEVEL_ON_EMITTER_ON,
    }

    /// <summary>
    /// 3.5 Enum: eANT_CTRL_MODE 天线控制模式
    /// </summary>
    public enum EAnt_Ctrl_Mode
    {
        /// <summary>
        /// Manual.
        /// </summary>
        ANT_CTRL_MODE_MANUAL,
        /// <summary>
        /// Auto.
        /// </summary>
        ANT_CTRL_MODE_AUTO,
    }

    /// <summary>
    /// 3.6 Enum: eANT_POL 天线极化方式
    /// Polarization type of antenna.
    /// </summary>
    public enum EAnt_Pol
    {
        /// <summary>
        /// 垂直极化
        /// Linear, vertical.
        /// </summary>
        POL_VERTICAL,
        /// <summary>
        /// 水平极化
        /// Linear, horizontal.
        /// </summary>
        POL_HORIZONTAL,
        /// <summary>
        /// 圆极化逆时针
        /// Circular, left-hand (counter-clockwise).
        /// </summary>
        POL_LEFT,
        /// <summary>
        /// 圆极化顺时针
        /// Circular, right-hand (clockwise).
        /// </summary>
        POL_RIGHT,
    }

    /// <summary>
    /// 3.7 Enum: eATT_SELECT 衰减模式
    /// Selection mode for attenuator.
    /// </summary>
    public enum EAtt_Select
    {
        /// <summary>
        /// Automatic.
        /// </summary>
        ATT_AUTO,
        /// <summary>
        /// Manual.
        /// </summary>
        ATT_MANUAL,
    }

    /// <summary>
    /// 3.8 Enum: eAUDIO_MODE 音频格式
    /// All available Audio modes (data formats of issued audio data).
    /// </summary>
    public enum EAudioMode
    {
        /// <summary>
        /// Audio mode 0: no useful data available (signal level below threshold).
        /// </summary>
        AUDIO_MODE_OFF,
        /// <summary>
        /// Mode 1: sampling rate 32 kHz, length of samples 16 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_32KHZ_16BIT_STEREO,
        /// <summary>
        /// Mode 2: rate 32 kHz, length 16 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_32KHZ_16BIT_MONO,
        /// <summary>
        /// Mode 3: rate 32 kHz, length 8 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_32KHZ_8BIT_STEREO,
        /// <summary>
        /// Mode 4: rate 32 kHz, length 8 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_32KHZ_8BIT_MONO,
        /// <summary>
        /// Mode 5: rate 16 kHz, length 16 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_16KHZ_16BIT_STEREO,
        /// <summary>
        /// Mode 6: rate 16 kHz, length 16 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_16KHZ_16BIT_MONO,
        /// <summary>
        /// Mode 7: rate 16 kHz, length 8 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_16KHZ_8BIT_STEREO,
        /// <summary>
        /// Mode 8: rate 16 kHz, length 8 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_16KHZ_8BIT_MONO,
        /// <summary>
        /// Mode 9: rate 8 kHz, length 16 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_8KHZ_16BIT_STEREO,
        /// <summary>
        /// Mode 10: rate 8 kHz, length 16 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_8KHZ_16BIT_MONO,
        /// <summary>
        /// Mode 11: rate 8 kHz, length 8 bit, 2 channels.
        /// </summary>
        AUDIO_MODE_8KHZ_8BIT_STEREO,
        /// <summary>
        /// Mode 12: rate 8 kHz, length 8 bit, 1 channel.
        /// </summary>
        AUDIO_MODE_8KHZ_8BIT_MONO,
    }

    /// <summary>
    /// 3.9 Enum: eAUX_CTRL_MODE AUX控制模式
    /// AUX control mode.
    /// </summary>
    public enum EAux_Ctrl_Mode
    {
        /// <summary>
        /// MANUAL  Manual mode: output value specified with command.
        /// </summary>
        AUX_CTRL_MODE_MANUAL,
        /// <summary>
        /// FREQ Frequency mode: output current Rx frequency value.
        /// </summary>
        AUX_CTRL_MODE_FREQ,
        /// <summary>
        /// ANTENNA Antenna mode: output value of "AntennaSetup".
        /// </summary>
        AUX_CTRL_MODE_ANTENNA,
    }

    /// <summary>
    /// 3.10 Enum: eAVERAGE_MODE 取平均值模式?
    /// Averaging mode.
    /// </summary>
    public enum EAverage_Mode
    {
        /// <summary>
        /// CONT    Continuous Mode: no signal level threshold exists.
        /// </summary>
        DFSQU_OFF,
        /// <summary>
        /// GATE    Gated Mode: averaging resumed after "inactive" period.
        /// </summary>
        DFSQU_GATE,
        /// <summary>
        /// NORM    Normal Mode: averaging restarts after "inactive" period.
        /// </summary>
        DFSQU_NORM,
    }

    /// <summary>
    /// 3.11 Enum: eBLANKING_INPUT 消隐信号输入?(视频使用)
    /// Blanking input.
    /// </summary>
    public enum EBlanking_Input
    {
        /// <summary>
        /// Blanking signal taken from Auxiliary input (connector X17 AUX, pin 9 BLANKING) (also default value).
        /// </summary>
        BLANKING_INPUT_AUX,
        /// <summary>
        /// Blanking signal taken from Trigger input (connector X44 TRIGGER).
        /// </summary>
        BLANKING_INPUT_TRIGGER,
    }

    /// <summary>
    /// 3.12 Enum: eBLANKING_MODE 消隐模式
    /// Blanking mode.
    /// </summary>
    public enum EBlanking_Mode
    {
        /// <summary>
        /// Blanking signal (input) is ignored.
        /// </summary>
        BLANKING_MODE_OFF,
        /// <summary>
        /// During averaging phase, with blanking signal active antenna signals are ignored (no contribution to averaging) (also default value).
        /// </summary>
        BLANKING_MODE_SUSPEND,
        /// <summary>
        ///  During averaging phase, with blanking signal active only status bit in DFPScan:DF status is set, but DF results and level generated (averaging continued).
        /// </summary>
        BLANKING_MODE_STATUS,
    }

    /// <summary>
    /// 3.13 Enum: eBLANKING_POLARITY 消隐信号的极性
    /// Polarity of Blanking signal.
    /// </summary>
    public enum EBlankingPolarity
    {
        /// <summary>
        /// Blanking signal is active low (also default value).
        /// </summary>
        BLANKING_POLARITY_LOW,
        /// <summary>
        /// Blanking signal is active high.
        /// </summary>
        BLANKING_POLARITY_HIGH,
    }

    /// <summary>
    /// 3.14 Enum: eBLOCK_AVERAGING_SELECT 平均值取值模式
    /// Indication type for duration of averaging block.
    /// </summary>
    public enum EBlock_Averaging_Select
    {
        /// <summary>
        /// 按次数取值
        /// Number of cycles.
        /// </summary>
        BLOCK_AVERAGING_SELECT_CYCLES,
        /// <summary>
        /// 按时间(ms)取值
        /// Time duration [ms].
        /// </summary>
        BLOCK_AVERAGING_SELECT_TIME,
    }

    /// <summary>
    /// 3.15 Enum: eCALMOD 校准发电机的调制。
    /// Modulation of calibration generator.
    /// </summary>
    public enum ECalMod
    {
        /// <summary>
        /// Calibration generator off.
        /// </summary>
        CALMOD_CWOFF,
        /// <summary>
        /// CW (continuous wave: no modulation).
        /// </summary>
        CALMOD_CWON,
        /// <summary>
        /// Comb spectrum 10 kHz, 10 spectral lines.
        /// </summary>
        CALMOD_F10K10L,
        /// <summary>
        /// 1 kHz, 100 lines.
        /// </summary>
        CALMOD_F1K100L,
        /// <summary>
        /// 20 kHz, 10 lines.
        /// </summary>
        CALMOD_F20K10L,
        /// <summary>
        /// 2 kHz, 100 lines.
        /// </summary>
        CALMOD_F2K100L,
        /// <summary>
        /// 50 kHz, 10 lines.
        /// </summary>
        CALMOD_F50K10L,
        /// <summary>
        /// 5 kHz, 100 lines.
        /// </summary>
        CALMOD_F5K100L,
        /// <summary>
        /// 100 kHz, 10 lines.
        /// </summary>
        CALMOD_F100K10L,
        /// <summary>
        /// 10 kHz, 100 lines.
        /// </summary>
        CALMOD_F10K100L,
        /// <summary>
        /// 200 kHz, 10 lines.
        /// </summary>
        CALMOD_F200K10L,
        /// <summary>
        /// 20 kHz, 100 lines.
        /// </summary>
        CALMOD_F20K100L,
        /// <summary>
        /// 500 kHz, 10 lines.
        /// </summary>
        CALMOD_F500K10L,
        /// <summary>
        /// 50 kHz, 100 lines.
        /// </summary>
        CALMOD_F50K100L,
        /// <summary>
        /// 1 MHz, 10 lines.
        /// </summary>
        CALMOD_F1M10L,
        /// <summary>
        /// 100 kHz, 100 lines.
        /// </summary>
        CALMOD_F100K100L,
        /// <summary>
        /// 2 MHz, 10 lines.
        /// </summary>
        CALMOD_F2M10L,
        /// <summary>
        /// 200 kHz, 100 lines.
        /// </summary>
        CALMOD_F200K100L,
        /// <summary>
        /// 4 MHz, 10 lines.
        /// </summary>
        CALMOD_F4M10L,
        /// <summary>
        /// 400 kHz, 100 lines.
        /// </summary>
        CALMOD_F400K100L,
        /// <summary>
        /// 800 kHz, 100 lines.
        /// </summary>
        CALMOD_F800K100L,
        /// <summary>
        /// 10 MHz, 10 lines.
        /// </summary>
        CALMOD_F10M10L,
        /// <summary>
        /// 1 MHz, 100 lines.
        /// </summary>
        CALMOD_F1M100L,
        /// <summary>
        /// 
        /// </summary>
        CALMOD_EXT,
    }

    /// <summary>
    /// 3.16 Enum: eCALOUT 用于校准信号的输出连接器。
    /// Output connector for calibration signal.
    /// NOTE 1: In case of connected antenna, input connector(s) for antenna signal(s) and output connector for calibration signal must be in same connector group.
    /// NOTE 2: With DDF1GTX, both calibration outputs are equivalent, but selection must be done with command CalibrationGenerator.
    /// NOTE 3: Mind that some connectors for HF range are available only if the corresponding option is installed:
    /// </summary>
    public enum ECalOut
    {
        /// <summary>
        /// No signal output.
        /// </summary>
        CALOUT_NONE,
        /// <summary>
        /// Calibration HF (see above notes 2 and 3).
        /// </summary>
        CALOUT_HF,
        /// <summary>
        /// Calibration V/UHF.
        /// </summary>
        CALOUT_VUHF,
        /// <summary>
        /// Calibration U/SHF.
        /// </summary>
        CALOUT_USHF,
    }

    /// <summary>
    /// 3.17 Enum: eCAL_SWITCH 天线信号输入开关
    /// Position of antenna signal switch.
    /// </summary>
    public enum ECal_Switch
    {
        /// <summary>
        /// Get input signal from antenna elements (normal operation).
        /// </summary>
        ANTCAL_RECEIVE,
        /// <summary>
        /// Get input signal from calibration generator (perform calibration).
        /// </summary>
        ANTCAL_CALIBRATE,
    }

    /// <summary>
    /// 3.18 Enum: eCLOCK_ORIGIN 设置时间同步的模式
    /// Origin of timing the data clock has been set to on latest setting. 
    /// </summary>
    public enum EClock_Origin
    {
        /// <summary>
        /// 通过命令[DateAndTime]手动更新时间
        /// Timing data set manually (command DateAndTime).
        /// </summary>
        CLOCK_ORIGIN_MANUAL,
        /// <summary>
        /// 通过自身的时钟更新时间
        /// Timing data set from internal RTC (Real Time Clock, battery-buffered, setting only with power-up).
        /// </summary>
        CLOCK_ORIGIN_BACKUP,
        /// <summary>
        /// 通过接收GPS信息更新时间
        /// Timing data set from external GPS receiver (or option R&S DDFx-IGT, Integrated GPS Module) to maintain continuity of clock time in periods of power-down, (command LocationAndTimeSource:eLocTimeSource=LOC_TIME_SRC_GPS).
        /// </summary>
        CLOCK_ORIGIN_GPS,
        /// <summary>
        /// 通过网络更新时间
        /// Timing data set from external NTP (Network Time Protocol) server (command NTP).
        /// </summary>
        CLOCK_ORIGIN_NTP
    }

    /// <summary>
    /// 3.19 Enum: eCLOCK_START 系统时钟的启动模式
    /// Mode of system clock start.
    /// </summary>
    public enum EClock_Start
    {
        /// <summary>
        /// Clock is started immediately upon setting (any pulse at PPS input ignored).
        /// </summary>
        CLOCK_START_AUTO,
        /// <summary>
        /// Clock is only started on pulse detected at PPS input (clock remains halted if such pulse missing).
        /// </summary>
        CLOCK_START_EXTERNAL
    }

    /// <summary>
    /// 3.20 Enum: eCOMPASS_CODE 所有可能的罗盘类型。
    /// All possible compass types. 
    /// </summary>
    public enum ECompass_Code
    {
        /// <summary>
        /// No known type of compass.
        /// </summary>
        COMPASS_UNDEFINED,
        /// <summary>
        /// User defined type.
        /// </summary>
        COMPASS_USER,
        /// <summary>
        /// R&S GH150 antenna compass.
        /// </summary>
        COMPASS_GH150,
        /// <summary>
        /// NMEA compass (e.g. vehicle compass).
        /// </summary>
        COMPASS_NMEA,
        /// <summary>
        /// Software compass (command SwCompassHeading).
        /// </summary>
        COMPASS_SW,
        /// <summary>
        /// GPS compass.
        /// </summary>
        COMPASS_GPS,
        /// <summary>
        /// Compass values received via UDP.
        /// </summary>
        COMPASS_UDP_NMEA,
    }

    /// <summary>
    /// 3.21 Enum: eDDCE_BW DDCE带宽
    /// DDCE (DDC/Digital Down Converter Signal Extraction) bandwidth.
    /// </summary>
    public enum EDDCE_Bw
    {
        /// <summary>
        /// 100 Hz	100 Hz.
        /// </summary>
        DDCE_BW_100,
        /// <summary>
        /// 150 Hz	150 Hz.
        /// </summary>
        DDCE_BW_150,
        /// <summary>
        /// 300 Hz	300 Hz.
        /// </summary>
        DDCE_BW_300,
        /// <summary>
        /// 600 Hz	600 Hz.
        /// </summary>
        DDCE_BW_600,
        /// <summary>
        /// 1 kHz	1 kHz.
        /// </summary>
        DDCE_BW_1000,
        /// <summary>
        /// 1.5 kHz	1.5 kHz.
        /// </summary>
        DDCE_BW_1500,
        /// <summary>
        /// 2.1 kHz	2.1 kHz.
        /// </summary>
        DDCE_BW_2100,
        /// <summary>
        /// 2.4 kHz	2.4 kHz.
        /// </summary>
        DDCE_BW_2400,
        /// <summary>
        /// 2.7 kHz	2.7 kHz.
        /// </summary>
        DDCE_BW_2700,
        /// <summary>
        /// 3.1 kHz	3.1 kHz.
        /// </summary>
        DDCE_BW_3100,
        /// <summary>
        /// 4 kHz	4 kHz.
        /// </summary>
        DDCE_BW_4000,
        /// <summary>
        /// 4.8 kHz	4.8 kHz.
        /// </summary>
        DDCE_BW_4800,
        /// <summary>
        /// 6 kHz	6 kHz.
        /// </summary>
        DDCE_BW_6000,
        /// <summary>
        /// 9 kHz	9 kHz.
        /// </summary>
        DDCE_BW_9000,
        /// <summary>
        /// 12 kHz	12 kHz.
        /// </summary>
        DDCE_BW_12000,
        /// <summary>
        /// 15 kHz	15 kHz.
        /// </summary>
        DDCE_BW_15000,
        /// <summary>
        /// 30 kHz	30 kHz.
        /// </summary>
        DDCE_BW_30000,
        /// <summary>
        /// 50 kHz	50 kHz.
        /// </summary>
        DDCE_BW_50000,
        /// <summary>
        /// 120 kHz	120 kHz.
        /// </summary>
        DDCE_BW_120000,
        /// <summary>
        /// 150 kHz	150 kHz.
        /// </summary>
        DDCE_BW_150000,
        /// <summary>
        /// 250 kHz	250 kHz.
        /// </summary>
        DDCE_BW_250000,
        /// <summary>
        /// 300 kHz	300 kHz.
        /// </summary>
        DDCE_BW_300000,
    }

    /// <summary>
    /// 3.22 Enum: eDDCE_REMOTE_MODE DDCE远程模式
    /// DDCE (DDC/Digital Down Converter Signal Extraction) remote mode.
    /// </summary>
    public enum EDDCE_Remote_Mode
    {
        /// <summary>
        /// Stop transfer via remote interface.
        /// </summary>
        DDCE_REMOTE_MODE_OFF,
        /// <summary>
        /// Digital IF, AMMOS format 16 bit I and 16 bit Q
        /// </summary>
        DDCE_REMOTE_MODE_SHORT,
        /// <summary>
        /// Digital IF, AMMOS format 32 bit I and 32 bit Q
        /// </summary>
        DDCE_REMOTE_MODE_LONG
    }

    /// <summary>
    /// 3.23 Enum: eDDCE_STATE DDCE状态
    /// DDCE (DDC/Digital Down Converter Signal Extraction) state.
    /// </summary>
    public enum EDDCE_State
    {
        /// <summary>
        /// Deactivate selected DDCE or remove binding to Short-Time Synthesizer.
        /// </summary>
        DDCE_STATE_OFF,
        /// <summary>
        /// Activate selected DDCE.
        /// </summary>
        DDCE_STATE_ON,
        /// <summary>
        /// Bind selected DDCE to Short-Time Synthesizer.
        /// </summary>
        DDCE_STATE_STIF
    }

    /// <summary>
    /// 3.24 Enum: eDECLINATION_SOURCE 磁北与真北的偏差模式
    /// Source of indication for declination (deviation of magnetic north vs. true [geographic] north; m. N. assumed to the right of g. N.).
    /// </summary>
    public enum EDeclination_Source
    {
        /// <summary>
        /// No declination value incorporated.
        /// </summary>
        DECL_SOUR_NO,
        /// <summary>
        /// Declination indication has been specified manually; entered value used.
        /// </summary>
        DECL_SOUR_MAN,
        /// <summary>
        /// Declination indication from GPS receiver; manual value not used (but kept until next evocation of manual mode).
        /// </summary>
        DECL_SOUR_GPS
    }

    /// <summary>
    /// 3.25 Enum: eDEMODULATION 解调模式
    /// Demodulation mode.
    /// </summary>
    public enum EDemodulation
    {
        /// <summary>
        /// FM (frequency modulation).
        /// </summary>
        MOD_FM,
        /// <summary>
        /// AM (amplitude modulation).
        /// </summary>
        MOD_AM,
        /// <summary>
        /// Pulse.
        /// </summary>
        MOD_PULS,
        /// <summary>
        /// PM (pulse modulation).
        /// </summary>
        MOD_PM,
        /// <summary>
        /// I/Q (in-phase and quadrature).
        /// </summary>
        MOD_IQ,
        /// <summary>
        /// ISB (independent side band).
        /// </summary>
        MOD_ISB,
        /// <summary>
        /// CW (continuous wave).
        /// </summary>
        MOD_CW,
        /// <summary>
        /// USB (upper side band).
        /// </summary>
        MOD_USB,
        /// <summary>
        /// LSB (lower side band).
        /// </summary>
        MOD_LSB,
        /// <summary>
        /// TV (television).
        /// </summary>
        MOD_TV,
    }

    /// <summary>
    /// 3.26 Enum: eDEVICE_INFO 当前连接的DDF单元的设备类型
    /// Type of device parameters of currently connected DDF unit. 
    /// </summary>
    public enum EDevice_Info
    {
        /// <summary>
        /// Minimum and maximum frequency of DDF.
        /// </summary>
        DEV_INFO_FREQUENCY_MIN_MAX,
        /// <summary>
        /// Minimum and maximum frequency of HF limit.
        /// </summary>
        DEV_INFO_HF_LIMIT_MIN_MAX,
        /// <summary>
        /// Minimum and maximum channel count for normal DF.
        /// </summary>
        DEV_INFO_DF_CHANNELCOUNT_MIN_MAX,
        /// <summary>
        /// Minimum and maximum channel count for super-resolution DF.
        /// </summary>
        DEV_INFO_SR_CHANNELCOUNT_MIN_MAX,
        /// <summary>
        /// List of all supported spans.
        /// </summary>
        DEV_INFO_SPAN_LIST,
        /// <summary>
        /// List of all supported spans for DDCE option.
        /// </summary>
        DEV_INFO_DDCE_SPAN_LIST,
        /// <summary>
        /// List of all supported spans for HRP option.
        /// </summary>
        DEV_INFO_HRP_SPAN_LIST,
        /// <summary>
        /// List of all supported spans for ST option.
        /// </summary>
        DEV_INFO_ST_SPAN_LIST,
        /// <summary>
        /// List of all supported DFPan steps.
        /// </summary>
        DEV_INFO_DFPAN_STEP_LIST,
        /// <summary>
        /// List of all supported IFPan steps.
        /// </summary>
        DEV_INFO_IFPAN_STEP_LIST,
        /// <summary>
        /// List of connectors for calibration signal (output) (eCALOUT).
        /// </summary>
        DEV_INFO_CALOUT_LIST,
        /// <summary>
        /// List of connectors for V/U/SHF signal (input) (eRF_INPUT).
        /// </summary>
        DEV_INFO_RF_INPUT_LIST,
        /// <summary>
        /// List of connectors for HF signal (input) (eHF_INPUT).
        /// </summary>
        DEV_INFO_HF_INPUT_LIST,
        /// <summary>
        /// List of all possible DF paths for Rx antenna VHF/UHF.
        /// </summary>
        DEV_INFO_RF_PATH_LIST,
        /// <summary>
        /// List of all possible DF paths for Rx antenna HF.
        /// </summary>
        DEV_INFO_HF_PATH_LIST,
    }

    /// <summary>
    /// 3.27 Enum: eDFMODE 测向模式
    /// DF Mode.
    /// </summary>
    public enum EDfMode
    {
        /// <summary>
        /// FFM (Fixed Frequency Mode).
        /// </summary>
        DFMODE_FFM,
        /// <summary>
        /// Scan.
        /// </summary>
        DFMODE_SCAN,
        /// <summary>
        /// Search.
        /// </summary>
        DFMODE_SEARCH,
        /// <summary>
        /// Rx (Receive only, no DF).
        /// </summary>
        DFMODE_RX,
        /// <summary>
        /// Rx Panorama Scan.
        /// </summary>
        DFMODE_RXPSCAN,
    }

    /// <summary>
    /// 3.28 Enum: eDFPAN_SELECTIVITY
    /// Selectivity of DF panorama.
    /// </summary>
    public enum EDfPan_Selectivity
    {
        /// <summary>
        /// Select automatically.
        /// </summary>
        DFPAN_SELECTIVITY_AUTO,
        /// <summary>
        /// Normal (factor 1, i.e. no prolongation).
        /// </summary>
        DFPAN_SELECTIVITY_NORMAL,
        /// <summary>
        /// Narrow (factor 2).
        /// </summary>
        DFPAN_SELECTIVITY_NARROW,
        /// <summary>
        ///  Sharp (factor 4).
        /// </summary>
        DFPAN_SELECTIVITY_SHARP,
    }

    /// <summary>
    /// 3.29 Enum: eDFPAN_STEP 测向信道带宽(0.01Hz
    /// DF channel spacing.
    /// </summary>
    public enum EDfPan_Step : ulong
    {
        /// <summary>
        /// 12.5 Hz	12.5 Hz.
        /// </summary>
        DFPAN_STEP_12P5HZ = 1250,
        /// <summary>
        /// 20 Hz	20 Hz.
        /// </summary>
        DFPAN_STEP_20HZ = 2000,
        /// <summary>
        /// 25 Hz	25 Hz.
        /// </summary>
        DFPAN_STEP_25HZ = 2500,
        /// <summary>
        /// 31.25 Hz	31.25 Hz.
        /// </summary>
        DFPAN_STEP_31P25HZ = 3125,
        /// <summary>
        /// 50 Hz	50 Hz.
        /// </summary>
        DFPAN_STEP_50HZ = 5000,
        /// <summary>
        /// 62.5 Hz	62.5 Hz.
        /// </summary>
        DFPAN_STEP_62P5HZ = 6250,
        /// <summary>
        /// 100 Hz	100 Hz.
        /// </summary>
        DFPAN_STEP_100HZ = 10000,
        /// <summary>
        /// 125 Hz	125 Hz.
        /// </summary>
        DFPAN_STEP_125HZ = 12500,
        /// <summary>
        /// 200 Hz	200 Hz.
        /// </summary>
        DFPAN_STEP_200HZ = 20000,
        /// <summary>
        /// 250 Hz	250 Hz.
        /// </summary>
        DFPAN_STEP_250HZ = 25000,
        /// <summary>
        /// 312.5 Hz	312.5 Hz.
        /// </summary>
        DFPAN_STEP_312P5HZ = 31250,
        /// <summary>
        /// 500 Hz	500 Hz.
        /// </summary>
        DFPAN_STEP_500HZ = 50000,
        /// <summary>
        /// 625 Hz	625 Hz.
        /// </summary>
        DFPAN_STEP_625HZ = 62500,
        /// <summary>
        /// 1 kHz	1 kHz.
        /// </summary>
        DFPAN_STEP_1KHZ = 100000,
        /// <summary>
        /// 1.25 kHz	1.25 kHz.
        /// </summary>
        DFPAN_STEP_1P25KHZ = 125000,
        /// <summary>
        /// 2 kHz	2 kHz.
        /// </summary>
        DFPAN_STEP_2KHZ = 200000,
        /// <summary>
        /// 2.5 kHz	2.5 kHz.
        /// </summary>
        DFPAN_STEP_2P5KHZ = 250000,
        /// <summary>
        /// 3.125 kHz	3.125 kHz.
        /// </summary>
        DFPAN_STEP_3P125KHZ = 312500,
        /// <summary>
        /// 5 kHz	5 kHz.
        /// </summary>
        DFPAN_STEP_5KHZ = 500000,
        /// <summary>
        /// 6.25 kHz	6.25 kHz.
        /// </summary>
        DFPAN_STEP_6P25KHZ = 625000,
        /// <summary>
        /// 8.333 kHz	8.333 kHz.
        /// </summary>
        DFPAN_STEP_8P333KHZ = 833300,
        /// <summary>
        /// 10 kHz	10 kHz.
        /// </summary>
        DFPAN_STEP_10KHZ = 1000000,
        /// <summary>
        /// 12.5 kHz	12.5 kHz.
        /// </summary>
        DFPAN_STEP_12P5KHZ = 1250000,
        /// <summary>
        /// 20 kHz	20 kHz.
        /// </summary>
        DFPAN_STEP_20KHZ = 2000000,
        /// <summary>
        /// 25 kHz	25 kHz.
        /// </summary>
        DFPAN_STEP_25KHZ = 2500000,
        /// <summary>
        /// 50 kHz	50 kHz.
        /// </summary>
        DFPAN_STEP_50KHZ = 5000000,
        /// <summary>
        /// 100 kHz	100 kHz.
        /// </summary>
        DFPAN_STEP_100KHZ = 10000000,
        /// <summary>
        /// 200 kHz	200 kHz.
        /// </summary>
        DFPAN_STEP_200KHZ = 20000000,
        /// <summary>
        /// 500 kHz	500 kHz.
        /// </summary>
        DFPAN_STEP_500KHZ = 50000000,
        /// <summary>
        /// 1 MHz	1 MHz.
        /// </summary>
        DFPAN_STEP_1000KHZ = 100000000,
        /// <summary>
        /// 2 MHz	2 MHz.
        /// </summary>
        DFPAN_STEP_2000KHZ = 200000000,
    }

    /// <summary>
    /// 3.30 Enum: eDF_ALT 测向体制?不确定
    /// DF evaluation principle.
    /// </summary>
    public enum EDf_Alt
    {
        /// <summary>
        /// Select automatically according to antenna type.
        /// </summary>
        DFALT_AUTO,
        /// <summary>
        /// Watson-Watt.
        /// </summary>
        DFALT_WATSONWATT,
        /// <summary>
        /// Correlation.
        /// </summary>
        DFALT_CORRELATION,
        /// <summary>
        /// Super-resolution.
        /// </summary>
        DFALT_SUPERRESOLUTION,
        /// <summary>
        /// Vector matching.
        /// </summary>
        DFALT_VECTORMATCHING,
    }

    /// <summary>
    /// 3.31 Enum: eDF_METHOD
    /// </summary>
    public enum EDf_Method
    {
        /// <summary>
        /// Watson-Watt.
        /// </summary>
        DF_WW,
        /// <summary>
        /// Correlation, 5 antenna elements.
        /// </summary>
        DF_COR_5,
        /// <summary>
        /// Correlation, 6 elements.
        /// </summary>
        DF_COR_6,
        /// <summary>
        /// Correlation, 8 elements.
        /// </summary>
        DF_COR_8,
        /// <summary>
        /// Correlation, 8 elements, omniphase correction.
        /// </summary>
        DF_COR_8_OMNI,
        /// <summary>
        /// Correlation, 9 elements.
        /// </summary>
        DF_COR_9,
        /// <summary>
        /// Correlation, 9 elements as one antenna base.
        /// </summary>
        DF_COR_9_ONE_BASE,
        /// <summary>
        /// Super-resolution, 5 elements.
        /// </summary>
        DF_SR_5,
        /// <summary>
        /// Super-resolution, 8 elements.
        /// </summary>
        DF_SR_8,
        /// <summary>
        /// Super-resolution, 9 elements.
        /// </summary>
        DF_SR_9,
        /// <summary>
        /// Super-resolution, 9 elements, antenna ADD011SR.
        /// </summary>
        DF_SR_9_011SR,
        /// <summary>
        /// Vector matching, 2 elements.
        /// </summary>
        DF_VM_2,
        /// <summary>
        /// Vector matching, 6 elements.
        /// </summary>
        DF_VM_6,
        /// <summary>
        /// Vector matching, 6 antenna sectors.
        /// </summary>
        DF_VM_6_SECTOR,
        /// <summary>
        /// Vector matching, 8 elements.
        /// </summary>
        DF_VM_8,
        /// <summary>
        /// Vector matching, 9 elements.
        /// </summary>
        DF_VM_9,
    }

    /// <summary>
    /// 3.32 Enum: eDIRECTION 精度和维度的方向?
    /// Direction of geographical latitude and longitude
    /// </summary>
    public enum EDirection
    {
        /// <summary>
        /// North(latitude).
        /// </summary>
        DIRECTION_NORTH,
        /// <summary>
        /// East (longitude).
        /// </summary>
        DIRECTION_EAST,
        /// <summary>
        /// South (latitude).
        /// </summary>
        DIRECTION_SOUTH,
        /// <summary>
        /// West (longitude).
        /// </summary>
        DIRECTION_WEST,
    }

    /// <summary>
    /// 3.33 Enum: eDISPLAY_VARIANTS VDPan(视频全景)数据跟踪状态和显示变量。
    /// VDPan (video panorama) data trace state and display variants. 
    /// </summary>
    public enum EDisplay_Variants
    {
        /// <summary>
        /// VDPan data trace off (no demodulated data).
        /// </summary>
        DISPLAY_VARIANTS_OFF,
        /// <summary>
        /// IF panorama (level) data.
        /// </summary>
        DISPLAY_VARIANTS_IF_PAN,
        /// <summary>
        /// Demodulated data: AM.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_AM,
        /// <summary>
        /// Demodulated data: FM.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_FM,
        /// <summary>
        /// Demodulated data: I/Q.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_IQ,
        /// <summary>
        /// Demodulated data, squared: AM.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_AM_SQUARE,
        /// <summary>
        /// Demodulated data, squared: FM.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_FM_SQUARE,
        /// <summary>
        /// Demodulated data, squared: I/Q.
        /// </summary>
        DISPLAY_VARIANTS_VIDEO_PAN_IQ_SQUARE,
    }

    /// <summary>
    /// 3.34 Enum: eFPGA FPGA的ID
    /// ID of FPGA.
    /// </summary>
    public enum EFPGA
    {
        /// <summary>
        /// FPGA A (also default).
        /// </summary>
        FPGA_A,
        /// <summary>
        /// FPGA B.
        /// </summary>
        FPGA_B,
        /// <summary>
        /// FPGA C.
        /// </summary>
        FPGA_C,
        /// <summary>
        /// FPGA D.
        /// </summary>
        FPGA_D,
    }

    /// <summary>
    /// 3.35 Enum: eGAIN_CONTROL 增益模式
    /// Mode of gain control for demodulation path.
    /// </summary>
    public enum EGain_Control
    {
        /// <summary>
        /// AGC (automatic gain control).
        /// </summary>
        GAIN_AUTO,
        /// <summary>
        /// MGC (manual gain control).
        /// </summary>
        GAIN_MANUAL,
    }

    /// <summary>
    /// 3.36 Enum: eGAIN_TIMING
    /// AGC gain timing characteristics.
    /// </summary>
    public enum EGain_Timing
    {
        /// <summary>
        /// Fast.
        /// </summary>
        GC_FAST,
        /// <summary>
        /// Default.
        /// </summary>
        GC_DEFAULT,
        /// <summary>
        /// Slow.
        /// </summary>
        GC_SLOW,
    }

    /// <summary>
    /// 3.37 Enum: eGPS_ANT_STATUS GPS天线状态
    /// GPS antenna (error) status of option R&S DDFx-IGT, Integrated GPS Module).
    /// </summary>
    public enum EGPS_Ant_Status
    {
        /// <summary>
        /// No GPS antenna error detected.
        /// </summary>
        GPS_ANT_STATUS_NO_ERROR,
        /// <summary>
        /// GPS antenna connector unconnected (no GPS antenna detected).
        /// </summary>
        GPS_ANT_STATUS_OPEN,
        /// <summary>
        /// GPS antenna connector shorted.
        /// </summary>
        GPS_ANT_STATUS_SHORT,
    }

    /// <summary>
    /// 3.38 Enum: eGPS_ANT_TYPE GPS天线类型(无源/有源)
    /// Type of GPS antenna connected to option R&S DDFx-IGT, Integrated GPS Module.
    /// </summary>
    public enum EGPS_Ant_Type
    {
        /// <summary>
        /// Active antenna.
        /// </summary>
        GPS_ANT_ACTIVE,
        /// <summary>
        /// Passive antenna.
        /// </summary>
        GPS_ANT_PASSIVE,
    }

    /// <summary>
    /// 3.39 Enum: eGPS_CODE 连接到X15或X16的GPS接收器类型。
    /// Type of GPS receiver connected to X15 or X16. 
    /// </summary>
    public enum EGPS_Code
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        GPS_UNDEFINED,
        /// <summary>
        /// Not used.
        /// </summary>
        GPS_USER,
        /// <summary>
        /// Not used.
        /// </summary>
        GPS_GINA,
        /// <summary>
        /// Conventional GPS receiver.
        /// </summary>
        GPS_NMEA,
        /// <summary>
        /// LEA (option R&S DDFx-IGT, Integrated GPS Module).
        /// </summary>
        GPS_LEA,
        /// <summary>
        /// Not used.
        /// </summary>
        GPS_TSIP,
        /// <summary>
        /// LEA6.
        /// </summary>
        GPS_LEA6,
        /// <summary>
        /// LEA6T.
        /// </summary>
        GPS_LEA6T,
        /// <summary>
        /// LEAM8.
        /// </summary>
        GPS_LEAM8,
        /// <summary>
        /// LEAM8T.
        /// </summary>
        GPS_LEAM8T,
        /// <summary>
        /// GPS NMEA sentences received (from NMEA UDP port: command NmeaUdpPort).
        /// </summary>
        GPS_UDP_NMEA,
    }

    /// <summary>
    /// 3.40 Enum: eGPS_EDGE
    /// Edge of PPS (GPS one-second pulse) to be used.
    /// </summary>
    public enum EGPS_Edge
    {
        /// <summary>
        /// Rising edge.
        /// </summary>
        EDGE_RAISING
    }

    /// <summary>
    /// 3.41 Enum: eGPS_ERROR GPS错误
    /// Error status of option R&S DDFx-IGT, Integrated GPS Module. 
    /// </summary>
    public enum EGPS_Error
    {
        /// <summary>
        /// No GPS error detected.
        /// </summary>
        _GPS_NO_ERROR,
        /// <summary>
        /// No GPS antenna detected (antenna connector unconnected).
        /// </summary>
        _GPS_ANTENNA_OPEN,
        /// <summary>
        /// GPS antenna connector shorted.
        /// </summary>
        _GPS_ANTENNA_SHORT,
        /// <summary>
        /// Fixed position entered does not match current position detected within averaging accuracy.
        /// </summary>
        _GPS_POS_MISMATCH,
        /// <summary>
        /// Unknown GPS error.
        /// </summary>
        _GPS_UNKNOWN_ERROR,
    }

    /// <summary>
    /// 3.42 Enum: eGPS_OPMODE GPS工作模式
    /// Operation mode of option R&S DDFx-IGT, Integrated GPS Module (for GET and SET operation). 
    /// </summary>
    public enum EGPS_OpMode
    {
        /// <summary>
        /// Free Run.
        /// </summary>
        GPS_OPMODE_FREE_RUN,
        /// <summary>
        /// Averaging.
        /// </summary>
        GPS_OPMODE_AVERAGING,
        /// <summary>
        /// Fixed Location.
        /// </summary>
        GPS_OPMODE_FIXED,
    }

    /// <summary>
    /// 3.43 Enum: eGPS_OPMODE_STATUS GPS工作模式状态
    /// Operation mode status of option R&S DDFx-IGT, Integrated GPS Module (for GET operation only). 
    /// </summary>
    public enum EGPS_OpMode_Status
    {
        /// <summary>
        /// No GPS connected.
        /// </summary>
        GPS_OPMODE_STAT_NO_GPS,
        /// <summary>
        /// Free Run.
        /// </summary>
        GPS_OPMODE_STAT_FREE_RUN,
        /// <summary>
        /// Averaging.
        /// </summary>
        GPS_OPMODE_STAT_AVERAGING,
        /// <summary>
        /// Fixed Location.
        /// </summary>
        GPS_OPMODE_STAT_FIXED,
    }

    /// <summary>
    /// 3.44 Enum: eGPS_RESET GPS复位方式
    /// Type of Reset of GPS receiver.
    /// </summary>
    public enum EGPS_Reset
    {
        /// <summary>
        /// Cold Reset: Discard: calculated position (P), almanac data (A), UTC time (U), satellites in view (S); try to locate satellites and to recalculate position.
        /// </summary>
        GPS_RESET_COLD,
        /// <summary>
        /// Warm Reset: Keep: P, A, U; discard: S; try to locate satellites and to recalculate position.
        /// </summary>
        GPS_RESET_WARM,
        /// <summary>
        /// Hot Reset: Keep: P, A, U, S; try to recalculate position.
        /// </summary>
        GPS_RESET_HOT,
    }

    /// <summary>
    /// 3.45 Enum: eHEADING_TYPE 
    /// Heading type of compass.
    /// </summary>
    public enum EHeading_Type
    {
        /// <summary>
        /// No heading value available.
        /// </summary>
        HEADING_TYPE_UNDEFINED,
        /// <summary>
        /// Heading unknown.
        /// </summary>
        HEADING_TYPE_UNKNOWN,
        /// <summary>
        /// Reference is compass north marker.
        /// </summary>
        HEADING_TYPE_COMPASS,
        /// <summary>
        /// Reference is magnetic north.
        /// </summary>
        HEADING_TYPE_MAGNETIC,
        /// <summary>
        /// Reference is true (geographic) north.
        /// </summary>
        HEADING_TYPE_TRUE,
        /// <summary>
        /// Heading value unusable.
        /// </summary>
        HEADING_TYPE_UNUSABLE,
        /// <summary>
        /// Heading value derived from GPS data.
        /// </summary>
        HEADING_TYPE_TRACK,
        /// <summary>
        /// Heading value bad because movement is too slow (GPS compass only).
        /// </summary>
        HEADING_TYPE_TRACK_SLOW,
    }

    /// <summary>
    /// 3.46 Enum: eHF_INPUT HF天线输入
    /// Antenna signal input connector group for HF.
    /// </summary>
    public enum EHF_Input
    {
        /// <summary>
        /// HF  HF (see above note 2).
        /// </summary>
        HF_INPUT_HF1,
        /// <summary>
        /// HF/V/U/SHF  HF/V/U/SHF.
        /// </summary>
        HF_INPUT_HF2,
    }

    /// <summary>
    /// 3.47 Enum: eHRPAN_MODE
    /// HRPAN (High-Resolution Panorama) mode.
    /// </summary>
    public enum EHRPan_Mode
    {
        /// <summary>
        /// OFF Deactivate data stream.
        /// </summary>
        HRPAN_OFF,
        /// <summary>
        /// SHORT Activate data stream and set mode to 16 bit format.
        /// </summary>
        HRPAN_16BIT,
    }

    /// <summary>
    /// 3.48 Enum: eHRPAN_STEP
    /// </summary>
    public enum EHRPan_Step
    {
        /// <summary>
        /// 0.39 Hz	0.39 Hz.
        /// </summary>
        HRPAN_STEP_0P39HZ,
        /// <summary>
        /// 0.77 Hz	0.77 Hz.
        /// </summary>
        HRPAN_STEP_0P77HZ,
        /// <summary>
        /// 1.53 Hz	1.53 Hz.
        /// </summary>
        HRPAN_STEP_1P53HZ,
        /// <summary>
        /// 3.06 Hz	3.06 Hz.
        /// </summary>
        HRPAN_STEP_3P06HZ,
        /// <summary>
        /// 6.11 Hz	6.11 Hz.
        /// </summary>
        HRPAN_STEP_6P11HZ,
        /// <summary>
        /// 12.21 Hz	12.21 Hz.
        /// </summary>
        HRPAN_STEP_12P21HZ,
        /// <summary>
        /// 24.42 Hz	24.42 Hz.
        /// </summary>
        HRPAN_STEP_24P42HZ,
        /// <summary>
        /// 48.83 Hz	48.83 Hz.
        /// </summary>
        HRPAN_STEP_48P83HZ,
        /// <summary>
        /// 97.66 Hz	97.66 Hz.
        /// </summary>
        HRPAN_STEP_97P66HZ,
        /// <summary>
        /// 195.32 Hz	195.32 Hz.
        /// </summary>
        HRPAN_STEP_195P32HZ,
        /// <summary>
        /// 390.63 Hz	390.63 Hz.
        /// </summary>
        HRPAN_STEP_390P63HZ,
        /// <summary>
        /// 781.25 Hz	781.25 Hz.
        /// </summary>
        HRPAN_STEP_781P25HZ,
        /// <summary>
        /// 1.5625 kHz	1.5625 kHz.
        /// </summary>
        HRPAN_STEP_1562P5HZ,
        /// <summary>
        /// 3.125 kHz	3.125 kHz.
        /// </summary>
        HRPAN_STEP_3125HZ,
        /// <summary>
        /// 6.25 kHz	6.25 kHz.
        /// </summary>
        HRPAN_STEP_6250HZ,
        /// <summary>
        /// 12.5 kHz	12.5 kHz.
        /// </summary>
        HRPAN_STEP_12500HZ,
    }

    /// <summary>
    /// 3.49 Enum: eHW_STATUS 硬件模块状态
    /// Status of hardware module (peripheral device: antenna, compass, ...). 
    /// </summary>
    public enum EHW_Status
    {
        /// <summary>
        /// All right.
        /// </summary>
        HW_STATUS_OK,
        /// <summary>
        /// Disconnected.
        /// </summary>
        HW_STATUS_DISCONNECTED,
        /// <summary>
        /// Transitional state
        /// </summary>
        HW_STATUS_CONNECT_PENDING,
    }

    /// <summary>
    /// 3.50 Enum: eHW_TYPE
    /// Type of hardware module (peripheral device: antenna, compass, ...).
    /// </summary>
    public enum EHW_Type
    {
        /// <summary>
        /// DF antenna.
        /// </summary>
        HW_DF_ANTENNA,
        /// <summary>
        /// Internal HW module.
        /// </summary>
        HW_BOARD,
        /// <summary>
        /// Signal converting device.
        /// </summary>
        HW_CONVERTER,
        /// <summary>
        /// Compass.
        /// </summary>
        HW_COMPASS,
        /// <summary>
        /// GPS receiver.
        /// </summary>
        HW_GPS,
        /// <summary>
        /// Processing device.
        /// </summary>
        HW_EBD,
        /// <summary>
        /// CPLD (Complex Programmable Logical Device).
        /// </summary>
        HW_CPLD,
        /// <summary>
        /// Miscellaneous (other hardware).
        /// </summary>
        HW_MISC,
        /// <summary>
        /// Rx antenna.
        /// </summary>
        HW_RX_ANTENNA,
    }

    /// <summary>
    /// 3.51 Enum: eIFPAN_MODE
    /// </summary>
    public enum EIFPan_Mode
    {
        /// <summary>
        /// Display plain data (no averaging or holding).
        /// </summary>
        IFPAN_MODE_CLRWRITE,
        /// <summary>
        /// Min Hold.
        /// </summary>
        IFPAN_MODE_MINHOLD,
        /// <summary>
        /// Max Hold.
        /// </summary>
        IFPAN_MODE_MAXHOLD,
        /// <summary>
        /// Average.
        /// </summary>
        IFPAN_MODE_AVERAGE,
    }

    /// <summary>
    /// 3.52 Enum: eIFPAN_SELECTIVITY
    /// </summary>
    public enum EIFPan_Selectivity
    {
        /// <summary>
        /// Select automatically.
        /// </summary>
        IFPAN_SELECTIVITY_AUTO,
        /// <summary>
        /// Normal.
        /// </summary>
        IFPAN_SELECTIVITY_NORMAL,
        /// <summary>
        /// Narrow.
        /// </summary>
        IFPAN_SELECTIVITY_NARROW,
        /// <summary>
        /// Sharp.
        /// </summary>
        IFPAN_SELECTIVITY_SHARP,
    }

    /// <summary>
    /// 3.53 Enum: eIFPAN_STEP 中频带宽(*0.01Hz)
    /// </summary>
    public enum EIFPan_Step : long
    {
        /// <summary>
        /// auto    Select automatically depending on span.
        /// </summary>
        IFPAN_STEP_AUTO = -100000,
        /// <summary>
        /// 31.25 Hz	31.25 Hz.
        /// </summary>
        IFPAN_STEP_31P25HZ = 3125,
        /// <summary>
        /// 50 Hz	50 Hz.
        /// </summary>
        IFPAN_STEP_50HZ = 5000,
        /// <summary>
        /// 62.5 Hz	62.5 Hz.
        /// </summary>
        IFPAN_STEP_62P5HZ = 6250,
        /// <summary>
        /// 100 Hz	100 Hz.
        /// </summary>
        IFPAN_STEP_100HZ = 10000,
        /// <summary>
        /// 125 Hz	125 Hz.
        /// </summary>
        IFPAN_STEP_125HZ = 12500,
        /// <summary>
        /// 200 Hz	200 Hz.
        /// </summary>
        IFPAN_STEP_200HZ = 20000,
        /// <summary>
        /// 250 Hz	250 Hz.
        /// </summary>
        IFPAN_STEP_250HZ = 25000,
        /// <summary>
        /// 312.5 Hz	312.5 Hz.
        /// </summary>
        IFPAN_STEP_312P5HZ = 31250,
        /// <summary>
        /// 500 Hz	500 Hz.
        /// </summary>
        IFPAN_STEP_500HZ = 50000,
        /// <summary>
        /// 625 Hz	625 Hz.
        /// </summary>
        IFPAN_STEP_625HZ = 62500,
        /// <summary>
        /// 1 kHz	1 kHz.
        /// </summary>
        IFPAN_STEP_1KHZ = 100000,
        /// <summary>
        /// 1.25 kHz	1.25 kHz.
        /// </summary>
        IFPAN_STEP_1P25KHZ = 125000,
        /// <summary>
        /// 2 kHz	2 kHz.
        /// </summary>
        IFPAN_STEP_2KHZ = 200000,
        /// <summary>
        /// 2.5 kHz	2.5 kHz.
        /// </summary>
        IFPAN_STEP_2P5KHZ = 250000,
        /// <summary>
        /// 3.125 kHz	3.125 kHz.
        /// </summary>
        IFPAN_STEP_3P125KHZ = 312500,
        /// <summary>
        /// 5 kHz	5 kHz.
        /// </summary>
        IFPAN_STEP_5KHZ = 500000,
        /// <summary>
        /// 6.25 kHz	6.25 kHz.
        /// </summary>
        IFPAN_STEP_6P25KHZ = 625000,
        /// <summary>
        /// 8.333 kHz	8.333 kHz.
        /// </summary>
        IFPAN_STEP_8P333KHZ = 833300,
        /// <summary>
        /// 10 kHz	10 kHz.
        /// </summary>
        IFPAN_STEP_10KHZ = 1000000,
        /// <summary>
        /// 12.5 kHz	12.5 kHz.
        /// </summary>
        IFPAN_STEP_12P5KHZ = 1250000,
        /// <summary>
        /// 20 kHz	20 kHz.
        /// </summary>
        IFPAN_STEP_20KHZ = 2000000,
        /// <summary>
        /// 25 kHz	25 kHz.
        /// </summary>
        IFPAN_STEP_25KHZ = 2500000,
        /// <summary>
        /// 50 kHz	50 kHz.
        /// </summary>
        IFPAN_STEP_50KHZ = 5000000,
        /// <summary>
        /// 100 kHz	100 kHz.
        /// </summary>
        IFPAN_STEP_100KHZ = 10000000,
        /// <summary>
        /// 200 kHz	200 kHz.
        /// </summary>
        IFPAN_STEP_200KHZ = 20000000,
        /// <summary>
        /// 500 kHz	500 kHz.
        /// </summary>
        IFPAN_STEP_500KHZ = 50000000,
        /// <summary>
        /// 1 MHz	1 MHz.
        /// </summary>
        IFPAN_STEP_1000KHZ = 100000000,
        /// <summary>
        /// 2 MHz	2 MHz.
        /// </summary>
        IFPAN_STEP_2000KHZ = 200000000,
    }

    /// <summary>
    /// 3.54 Enum: eIF_MODE IQ数据格式
    /// IF (I/Q data) data trace state and data format.
    /// </summary>
    public enum EIF_Mode
    {
        /// <summary>
        /// Both IF data traces off (no demodulated I/Q data).
        /// </summary>
        IF_OFF,
        /// <summary>
        /// Data trace IF, 16 bit per data value.
        /// </summary>
        IF_16BIT,
        /// <summary>
        /// Data trace IF, 32 bit per data value.
        /// </summary>
        IF_32BIT,
        /// <summary>
        /// Data trace AMMOS IF, 16 bit per data value.
        /// </summary>
        IF_16BIT_AMMOS,
        /// <summary>
        /// Data trace AMMOS IF, 32 bit per data value.
        /// </summary>
        IF_32BIT_AMMOS,
    }

    /// <summary>
    /// 3.55 Enum: eINPUT_RANGE
    /// Type of input connector to use. If a type describes more than one connector, the specific one is determined by additional parameters eHF_INPUT and eRF_INPUT . 
    /// </summary>
    public enum EInput_Range
    {
        /// <summary>
        /// Undefined input.
        /// </summary>
        INPUT_UNDEFINED,
        /// <summary>
        /// Use HF connector; specific connector selected by eHF_INPUT.
        /// </summary>
        INPUT_HF,
        /// <summary>
        /// Use VUHF connector; specific connector selected by eRF_INPUT.
        /// </summary>
        INPUT_VUHF,
    }

    /// <summary>
    /// 3.56 Enum: eLEVEL_INDICATOR
    /// ITU measurements level detector characteristics (option R&S DDFx-IM, ITU Measurement Software, required).
    /// </summary>
    public enum ELevel_Indicatir
    {
        /// <summary>
        /// Measure average value of momentary amplitudes.
        /// </summary>
        LEVEL_INDICATOR_AVG,
        /// <summary>
        /// Extract peak value of momentary amplitudes.
        /// </summary>
        LEVEL_INDICATOR_PEAK,
        /// <summary>
        /// Fix current value at moment of readout query.
        /// </summary>
        LEVEL_INDICATOR_FAST,
        /// <summary>
        /// Measure RMS value of momentary amplitudes.
        /// </summary>
        LEVEL_INDICATOR_RMS,
    }

    /// <summary>
    /// 3.57 Enum: eLOC_TIME_SOURCE 位置和时间的数据来源
    /// Source of location and timing data in use.
    /// </summary>
    public enum ELoc_Time_Source
    {
        /// <summary>
        /// Manual: neutral state.
        /// </summary>
        LOC_TIME_SRC_MANUAL,
        /// <summary>
        /// GPS: 1st NMEA sentence "GPRMC" arriving after having entered this state will be evaluated.
        /// </summary>
        LOC_TIME_SRC_GPS,
    }

    /// <summary>
    /// 3.58 Enum: eMEASUREMODE 用于带宽测量的带宽类型(需要R&S DDFx-IM选项，ITU测量软件)。
    /// Type of bandwidth for bandwidth measurement in use (option R&S DDFx-IM, ITU Measurement Software, required).
    /// </summary>
    public enum EMeasureMode
    {
        /// <summary>
        /// X dB bandwidth.
        /// </summary>
        MEASUREMODE_XDB,
        /// <summary>
        /// Beta % bandwidth.
        /// </summary>
        MEASUREMODE_BETA,
    }

    /// <summary>
    /// 3.59 Enum: eMEASUREMODECP
    /// Measuring mode with ITU measurements (option R&S DDFx-IM, ITU Measurement Software, required).
    /// </summary>
    public enum EMeasureModeCP
    {
        /// <summary>
        /// Continuous Measuring Mode.
        /// </summary>
        MEASUREMODECP_CONT,
        /// <summary>
        /// Periodic Measuring Mode.
        /// </summary>
        MEASUREMODECP_PER,
    }

    /// <summary>
    /// 3.60 Enum: eOUT_OF_RANGE
    /// Status of test point.
    /// </summary>
    public enum EOut_Of_Range
    {
        /// <summary>
        /// Test point value inside tolerance range.
        /// </summary>
        LIMIT_IN,
        /// <summary>
        /// Test point value below tolerance range.
        /// </summary>
        LIMIT_LOWER,
        /// <summary>
        /// Test point value above tolerance range.
        /// </summary>
        LIMIT_UPPER,
    }

    /// <summary>
    /// 3.61 Enum: ePSCAN_STEP 全景扫描步进
    /// Range setting for RxPSCan (Rx Panorama Scan).
    /// </summary>
    public enum EPScan_Step : uint
    {
        /// <summary>
        /// 100 Hz	100 Hz.
        /// </summary>
        PSCAN_STEP_0P1 = 100,
        /// <summary>
        /// 125 Hz	125 Hz.
        /// </summary>
        PSCAN_STEP_0P125 = 125,
        /// <summary>
        /// 200 Hz	200 Hz.
        /// </summary>
        PSCAN_STEP_0P2 = 200,
        /// <summary>
        /// 250 Hz	250 Hz.
        /// </summary>
        PSCAN_STEP_0P25 = 250,
        /// <summary>
        /// 500 Hz	500 Hz.
        /// </summary>
        PSCAN_STEP_0P5 = 500,
        /// <summary>
        /// 625 Hz	625 Hz.
        /// </summary>
        PSCAN_STEP_0P625 = 625,
        /// <summary>
        /// 1 kHz	1 kHz.
        /// </summary>
        PSCAN_STEP_1 = 1000,
        /// <summary>
        /// 1.25 kHz	1.25 kHz.
        /// </summary>
        PSCAN_STEP_1P25 = 1250,
        /// <summary>
        /// 2 kHz	2 kHz.
        /// </summary>
        PSCAN_STEP_2 = 2000,
        /// <summary>
        /// 2.5 kHz	2.5 kHz.
        /// </summary>
        PSCAN_STEP_2P5 = 2500,
        /// <summary>
        /// 3.125 kHz	3.125 kHz.
        /// </summary>
        PSCAN_STEP_3P125 = 3215,
        /// <summary>
        /// 5 kHz	5 kHz.
        /// </summary>
        PSCAN_STEP_5 = 5000,
        /// <summary>
        /// 6.25 kHz	6.25 kHz.
        /// </summary>
        PSCAN_STEP_6P25 = 6250,
        /// <summary>
        /// 8.333 kHz	8.333 kHz.
        /// </summary>
        PSCAN_STEP_8P333 = 8333,
        /// <summary>
        /// 10 kHz	10 kHz.
        /// </summary>
        PSCAN_STEP_10 = 10000,
        /// <summary>
        /// 12.5 kHz	12.5 kHz.
        /// </summary>
        PSCAN_STEP_12P5 = 12500,
        /// <summary>
        /// 20 kHz	20 kHz.
        /// </summary>
        PSCAN_STEP_20 = 20000,
        /// <summary>
        /// 25 kHz	25 kHz.
        /// </summary>
        PSCAN_STEP_25 = 25000,
        /// <summary>
        /// 50 kHz	50 kHz.
        /// </summary>
        PSCAN_STEP_50 = 50000,
        /// <summary>
        /// 100 kHz	100 kHz.
        /// </summary>
        PSCAN_STEP_100 = 100000,
        /// <summary>
        /// 200 kHz	200 kHz.
        /// </summary>
        PSCAN_STEP_200 = 200000,
        /// <summary>
        /// 500 kHz	500 kHz.
        /// </summary>
        PSCAN_STEP_500 = 500000,
        /// <summary>
        /// 1 MHz	1 MHz.
        /// </summary>
        PSCAN_STEP_1000 = 1000000,
        /// <summary>
        /// 2 MHz	2 MHz.
        /// </summary>
        PSCAN_STEP_2000 = 2000000,
    }

    /// <summary>
    /// 3.62 Enum: eREFERENCE_MODE
    /// System reference (10 MHz reference frequency) mode.
    /// </summary>
    public enum EReference_Mode
    {
        /// <summary>
        /// System reference external (supplied to REF IN connector).
        /// </summary>
        REFERENCE_MODE_EXTERNAL,
        /// <summary>
        /// System reference internal (generated by internal OCXO).
        /// </summary>
        REFERENCE_MODE_INTERNAL,
    }

    /// <summary>
    /// 3.63 Enum: eREFERENCE_SYNCH 同步状态
    /// System reference (10 MHz reference frequency) state of synchronization.
    /// </summary>
    public enum EReference_Synch
    {
        /// <summary>
        /// Internally synchronized.
        /// </summary>
        REFERENCE_SYNCH_INTERNAL,
        /// <summary>
        /// Synchronized to external reference (X41 REF IN).
        /// </summary>
        REFERENCE_SYNCH_EXTERNAL,
        /// <summary>
        /// Not locked to PPS (GPS one-second pulse, X43 GPS PPS).
        /// </summary>
        REFERENCE_SYNCH_PPS_UNLOCK,
        /// <summary>
        /// Locked to PPS.
        /// </summary>
        REFERENCE_SYNCH_PPS_LOCK,
    }

    /// <summary>
    /// 3.64 Enum: eRESET_TYPE 重置类型
    /// Type of reset (see command Reset for explanation).
    /// </summary>
    public enum EReset_Type
    {
        /// <summary>
        /// Reset R&S DDFx settings to factory defaults, do not perform reboot.
        /// </summary>
        RESET_SETTINGS,
        /// <summary>
        /// Warm reset: perform reboot, do not set to factory defaults (also default value).
        /// </summary>
        RESET_WARM,
        /// <summary>
        /// Cold reset: perform reboot, set to factory defaults.
        /// </summary>
        RESET_COLD,
    }

    /// <summary>
    /// 3.65 Enum: eRESULT 自检结果
    /// Self test result.
    /// </summary>
    public enum EResult
    {
        /// <summary>
        /// Self test successful: all tests passed.
        /// </summary>
        RESULT_GO,
        /// <summary>
        /// Self test unsuccessful: at least one test point failed.
        /// </summary>
        RESULT_NOGO,
    }

    /// <summary>
    /// 3.66 Enum: eRF_INPUT
    /// Antenna signal input connector group for VHF/UHF/SHF.
    /// </summary> 
    public enum ERF_Input
    {
        /// <summary>
        /// V/UHF   V/UHF.
        /// </summary>
        RF_INPUT_VUSHF1,
        /// <summary>
        /// HF/V/U/SHF  HF/V/U/SHF.
        /// </summary>
        RF_INPUT_VUSHF2,
    }

    /// <summary>
    /// 3.67 Enum: eRF_MODE 射频模式
    /// Preselection mode.
    /// </summary>
    public enum ERf_Mode
    {
        /// <summary>
        /// Normal.
        /// </summary>
        RFMODE_NORMAL,
        /// <summary>
        /// Low noise.
        /// </summary>
        RFMODE_LOW_NOISE,
        /// <summary>
        /// Low distortion.
        /// </summary>
        RFMODE_LOW_DISTORTION,
    }

    /// <summary>
    /// 3.68 Enum: eRX_PATH
    /// DF path an RX antenna is connected to. 
    /// </summary>
    public enum ERx_Paht
    {
        /// <summary>
        /// DF1 DF path 1.
        /// </summary>
        RX_PATH_DF1,
        /// <summary>
        /// DF2 DF path 2.
        /// </summary>
        RX_PATH_DF2,
        /// <summary>
        /// DF3 DF path 3.
        /// </summary>
        RX_PATH_DF3,
    }

    /// <summary>
    /// 3.69 Enum: eSELECTORFLAG
    /// Selector flags: subset of data that mass data output is to be enabled for. Find more detailed information in R&S DDFx system manuals. 
    /// </summary>
    public enum ESelectorFlag
    {
        /// <summary>
        /// Level.
        /// </summary>
        SELFLAG_LEVEL,
        /// <summary>
        /// Frequency offset.
        /// </summary>
        SELFLAG_OFFSET,
        /// <summary>
        /// Field strength.
        /// </summary>
        SELFLAG_FSTRENGTH,
        /// <summary>
        /// Amplitude swing, average.
        /// </summary>
        SELFLAG_AM,
        /// <summary>
        /// Amplitude swing, positive.
        /// </summary>
        SELFLAG_AM_POS,
        /// <summary>
        /// Amplitude swing, negative.
        /// </summary>
        SELFLAG_AM_NEG,
        /// <summary>
        /// Frequency deviation, average.
        /// </summary>
        SELFLAG_FM,
        /// <summary>
        /// Frequency deviation, positive.
        /// </summary>
        SELFLAG_FM_POS,
        /// <summary>
        /// Frequency deviation, negative.
        /// </summary>
        SELFLAG_FM_NEG,
        /// <summary>
        /// Phase swing.
        /// </summary>
        SELFLAG_PM,
        /// <summary>
        /// Bandwidth.
        /// </summary>
        SELFLAG_BANDWIDTH,
        /// <summary>
        /// DF level.
        /// </summary>
        SELFLAG_DF_LEVEL,
        /// <summary>
        /// Azimuth.
        /// </summary>
        SELFLAG_AZIMUTH,
        /// <summary>
        /// DF quality.
        /// </summary>
        SELFLAG_DF_QUALITY,
        /// <summary>
        /// DF field strength.
        /// </summary>
        SELFLAG_DF_FSTRENGTH,
        /// <summary>
        /// DF level (continuous).
        /// </summary>
        SELFLAG_DF_LEVEL_CONT,
        /// <summary>
        /// Channel.
        /// </summary>
        SELFLAG_CHANNEL,
        /// <summary>
        /// Lower 32 bit of the frequency.
        /// </summary>
        SELFLAG_FREQ_LOW,
        /// <summary>
        /// DF omniphase.
        /// </summary>
        SELFLAG_DF_OMNIPHASE,
        /// <summary>
        /// Elevation.
        /// </summary>
        SELFLAG_ELEVATION,
        /// <summary>
        /// DF channel status.
        /// </summary>
        SELFLAG_DF_CHANNEL_STATUS,
        /// <summary>
        /// Base counter.
        /// </summary>
        SELFLAG_BASECOUNTER,
        /// <summary>
        /// Upper 32 bit of the frequency.
        /// </summary>
        SELFLAG_FREQ_HIGH,
        /// <summary>
        /// Swap endianness.
        /// </summary>
        SELFLAG_SWAP,
        /// <summary>
        /// Show only values above squelch.
        /// </summary>
        SELFLAG_SIGNAL_GREATER_SQUELCH,
        /// <summary>
        /// Optional Header.
        /// </summary>
        SELFLAG_OPTIONAL_HEADER,
    }

    /// <summary>
    /// 3.70 Enum: eSELFTEST 自检类型
    /// Type of self test.
    /// </summary>
    public enum ESelfTest
    {
        /// <summary>
        /// Short self test.
        /// </summary>
        SELFTEST_SHORT,
        /// <summary>
        /// Long self test.
        /// </summary>
        SELFTEST_LONG,
    }

    /// <summary>
    /// 3.71 Enum: eSIGP_DATATYPE 信号处理数据的类型
    /// Type of signal processing data.
    /// NOTE: Item is used for output of interim results from signal processing and therefore is subject to future enhancement.
    /// </summary>
    public enum ESigp_DataType
    {
        /// <summary>
        /// Averaged data.
        /// </summary>
        SIGP_DATA_AVG
    }

    /// <summary>
    /// 3.72 Enum: eSPAN 频谱带宽(KHz)
    /// Realtime bandwidth (frequency span).
    /// </summary>
    public enum ESpan : UInt32
    {
        /// <summary>
        /// 100 kHz	100 kHz.
        /// </summary>
        IFPAN_FREQ_RANGE_100 = 100,
        /// <summary>
        /// 200 kHz	200 kHz.
        /// </summary>
        IFPAN_FREQ_RANGE_200 = 200,
        /// <summary>
        /// 500 kHz	500 kHz.
        /// </summary>
        IFPAN_FREQ_RANGE_500 = 500,
        /// <summary>
        /// 1 MHz	1 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_1000 = 1000,
        /// <summary>
        /// 2 MHz	2 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_2000 = 2000,
        /// <summary>
        /// 5 MHz	5 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_5000 = 5000,
        /// <summary>
        /// 10 MHz	10 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_10000 = 10000,
        /// <summary>
        /// 20 MHz	20 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_20000 = 20000,
        /// <summary>
        /// 40 MHz	40 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_40000 = 40000,
        /// <summary>
        /// 80 MHz	80 MHz.
        /// </summary>
        IFPAN_FREQ_RANGE_80000 = 80000,
    }

    /// <summary>
    /// 3.73 Enum: eSTATE 双极开关状态(天线前置放大器等)。
    /// Bipolar switching states (antenna preamplifier etc.).
    /// </summary>
    public enum EState
    {
        /// <summary>
        /// Switched off (i.e. disabled) (unless noted otherwise).
        /// </summary>
        STATE_OFF,
        /// <summary>
        /// Switched on (i.e. enabled).
        /// </summary>
        STATE_ON,
    }

    /// <summary>
    /// 3.74 Enum: eSTD_STEP 步进
    /// Short-Time Detector spectral step size.
    /// </summary>
    public enum EStd_Step
    {
        /// <summary>
        /// 390.63 Hz	390.63 Hz.
        /// </summary>
        STD_STEP_390P63HZ,
        /// <summary>
        /// 781.25 Hz	781.25 Hz.
        /// </summary>
        STD_STEP_781P25HZ,
        /// <summary>
        /// 1.5625 kHz	1.5625 kHz.
        /// </summary>
        STD_STEP_1562P5HZ,
        /// <summary>
        /// 3.125 kHz	3.125 kHz.
        /// </summary>
        STD_STEP_3125HZ,
        /// <summary>
        /// 6.25 kHz	6.25 kHz.
        /// </summary>
        STD_STEP_6250HZ,
        /// <summary>
        /// 12.5 kHz	12.5 kHz.
        /// </summary>
        STD_STEP_12500HZ,
        /// <summary>
        /// 25 kHz	25 kHz.
        /// </summary>
        STD_STEP_25000HZ,
        /// <summary>
        /// 50 kHz	50 kHz.
        /// </summary>
        STD_STEP_50000HZ,
        /// <summary>
        /// 100 kHz	100 kHz.
        /// </summary>
        STD_STEP_100000HZ,
    }

    /// <summary>
    /// 3.75 Enum: eSTS_BW 带宽
    /// Short-Time Synthesizer bandwidth.
    /// </summary>
    public enum ESts_BW
    {
        /// <summary>
        /// 30 kHz	30 kHz.
        /// </summary>
        STS_BW_30000,
        /// <summary>
        /// 300 kHz	300 kHz.
        /// </summary>
        STS_BW_300000,
    }

    /// <summary>
    /// 3.76 Enum: eTRACETAG
    /// Trace tags: set of data that mass data output is to be enabled for. Find more detailed information in R&S DDFx system manual. 
    /// </summary>
    public enum ETraceTag
    {
        /// <summary>
        /// Audio: digital audio signal.
        /// </summary>
        TRACETAG_AUDIO,
        /// <summary>
        /// IFPan: spectrum (panorama) of the IF signal.
        /// </summary>
        TRACETAG_IFPAN,
        /// <summary>
        /// DF: Direction finding massdata (DFPScan).
        /// </summary>
        TRACETAG_DF,
        /// <summary>
        /// GPS_COMPASS: GPS and compass data.
        /// </summary>
        TRACETAG_GPS_COMPASS,
        /// <summary>
        /// ANT_LEVEL: antenna level data.
        /// </summary>
        TRACETAG_ANT_LEVEL,
        /// <summary>
        /// SelCall: SelCal analysis data.
        /// </summary>
        TRACETAG_SEL_CALL,
        /// <summary>
        /// CW: data from measurements (triggered manually or periodically).
        /// </summary>
        TRACETAG_CWAVE,
        /// <summary>
        /// IF: IF signal (I/Q data, unregulated).
        /// </summary>
        TRACETAG_IF,
        /// <summary>
        /// VIDEO: Video data.
        /// </summary>
        TRACETAG_VIDEO,
        /// <summary>
        /// VDPan: Video panorama data.
        /// </summary>
        TRACETAG_VIDEOPAN,
        /// <summary>
        /// PScan: Panorama Scan level data.
        /// </summary>
        TRACETAG_PSCAN,
        /// <summary>
        /// SignalProcessing.
        /// </summary>
        TRACETAG_SIGP,
        TRACETAG_DEBUG,
        /// <summary>
        /// AMMOS I/Q Data.
        /// </summary>
        TRACETAG_AMMOS_IF,
        /// <summary>
        /// HRPAN: High-Resolution Panorama.
        /// </summary>
        TRACETAG_HRPAN,
        /// <summary>
        /// DDCE: AMMOS DDC data.
        /// </summary>
        TRACETAG_DDCE,
        /// <summary>
        /// STD: AMMOS Burst Emission List data.
        /// </summary>
        TRACETAG_STD,
    }

    /// <summary>
    /// 3.77 Enum: eTRIGGER_MEASUREMODE 触发式测量工作模式
    /// Operating mode of triggered measuring mode.
    /// </summary>
    public enum ETrigger_MeasureMode
    {
        /// <summary>
        /// Single measurement per trigger event.
        /// </summary>
        DFTRIGGERMEASMODE_SINGLE,
        /// <summary>
        /// Continuous measuring.
        /// </summary>
        DFTRIGGERMEASMODE_CONT,
    }

    /// <summary>
    /// 3.78 Enum: eTRIGGER_MODE 触发测量模式状态。
    /// State of triggered measuring mode. 
    /// </summary>
    public enum ETrigger_Mode
    {
        /// <summary>
        /// Disabled: no triggered measuring.
        /// </summary>
        DFTRIGGERMODE_DISABLED,
        /// <summary>
        /// Enabled: trigger event on external connector starts a measurement.
        /// </summary>
        DFTRIGGERMODE_EXTERN,
        /// <summary>
        /// Synchronous Scan: measurement start is controlled by scan range settings
        /// </summary>
        DFTRIGGERMODE_TIMESYNC,
    }

    /// <summary>
    /// 3.79 Enum: eTRIGGER_SOURCE 触发测量模式的触发源
    /// Trigger source for triggered measuring mode.
    /// </summary>
    public enum ETrigger_Source
    {
        /// <summary>
        /// X44 External trigger: connector X44 TRIGGER.
        /// </summary>
        DFTRIGGERSOURCE_EXT_TRIG_IN_1,
        /// <summary>
        /// X17 GSM: connector X17 AUX, pin 8 GSM_STROBE.
        /// </summary>
        DFTRIGGERSOURCE_GSM_STROBE,
        /// <summary>
        /// 
        /// </summary>
        DFTRIGGERSOURCE_TRIGGER,
        /// <summary>
        /// X43 PPS (GPS one-second pulse): connector X43 GPS PPS.
        /// </summary>
        DFTRIGGERSOURCE_GPS_PPS,
    }

    /// <summary>
    /// 3.80 Enum: eUART
    /// UART (Universal Asynchronous Receiver/Transmitter) for individual connector.
    /// </summary>
    public enum EUART
    {
        /// <summary>
        /// X15 (GPS)	X15 (GPS).
        /// </summary>
        UART_GPS,
        /// <summary>
        /// X16 (Compass)	X16 (Compass).
        /// </summary>
        UART_COMPASS,
    }

    /// <summary>
    /// 3.81 Enum: eVIDEO_MODE 视频格式
    /// VIDEO data trace state and data format.
    /// </summary>
    public enum EVideo_Mode
    {
        /// <summary>
        /// VIDEO data trace off (no demodulated data).
        /// </summary>
        VIDEO_OFF,
        /// <summary>
        /// VIDEO data trace on, 16 bit per data value.
        /// </summary>
        VIDEO_16BIT,
        /// <summary>
        /// VIDEO data trace on, 32 bit per data value.
        /// </summary>
        VIDEO_32BIT,
    }

    /// <summary>
    /// 3.82 Enum: eWINDOW_TYPE FFT窗函数类型
    /// Type (function) of FFT window.
    /// </summary>
    public enum EWindow_Type
    {
        /// <summary>
        /// (for internal use only)
        /// </summary>
        DF_WINDOW_TYPE_TEST,
        /// <summary>
        /// Window rectangle(i.e.no window).
        /// </summary>
        DF_WINDOW_TYPE_RECTANGLE,
        /// <summary>
        /// Window Hamming.
        /// </summary>
        DF_WINDOW_TYPE_HAMMING,
        /// <summary>
        /// Window Blackman-Harris.
        /// </summary>
        DF_WINDOW_TYPE_BLACKMAN_HARRIS,
        /// <summary>
        /// Window Hann.
        /// </summary>
        DF_WINDOW_TYPE_HANN,
        /// <summary>
        /// Window Blackman.
        /// </summary>
        DF_WINDOW_TYPE_BLACKMAN,
        /// <summary>
        /// Window Nuttal.
        /// </summary>
        DF_WINDOW_TYPE_NUTTAL,
        /// <summary>
        /// Window Blackman-Nuttal.
        /// </summary>
        DF_WINDOW_TYPE_BLACKMAN_NUTTAL,
    }

    #endregion Xml枚举

    [Flags]
    public enum TAGS
    {
        FSCAN = 101,
        MSCAN = 201,
        AUDIO = 401,
        IFPAN = 501,
        CW = 801,
        IF = 901,
        VIDEO = 1001,
        VDPAN = 1101,
        PSCAN = 1201,
        SELECall = 1301,
        DFPAN = 1401,
        PIFPAN = 1601,
        GPSCompass = 1801,
        AntLevel = 1901,
        DFPScan = 5301,
        SIGP = 5501,
        HRPAN = 5601,
        LAST_TAG
    };

    [Flags]
    public enum FLAGS : uint
    {
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        LEVEL = 0x1,
        /// <summary>
        /// Hz
        /// </summary>
        OFFSET = 0x2,
        /// <summary>
        /// 1/10 dBμV/m
        /// </summary>
        FSTRENGTH = 0x4,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM = 0x8,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM_POS = 0x10,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM_NEG = 0x20,
        /// <summary>
        /// Hz
        /// </summary>
        FM = 0x40,
        /// <summary>
        /// Hz
        /// </summary>
        FM_POS = 0x80,
        /// <summary>
        /// Hz
        /// </summary>
        FM_NEG = 0x100,
        /// <summary>
        /// 1/100 rad
        /// </summary>
        PM = 0x200,
        /// <summary>
        /// Hz
        /// </summary>
        BANDWIDTH = 0x400,
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        DF_LEVEL = 0x800,
        /// <summary>
        /// 1/10 °
        /// </summary>
        AZIMUTH = 0x1000,
        /// <summary>
        /// 1/10 %
        /// </summary>
        DF_QUALITY = 0x2000,
        /// <summary>
        /// 1/10 dBμV/m
        /// </summary>
        DF_FSTRENGTH = 0x4000,
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        DF_LEVEL_CONT = 0x8000,
        CHANNEL = 0x00010000,
        FREQLOW = 0x00020000,
        /// <summary>
        /// 1/10 °
        /// </summary>
        ELEVATION = 0x00040000,
        DF_CHANNEL_STATUS = 0x80000,
        /// <summary>
        /// 1/10 °
        /// </summary>
        DF_OMNIPHASE = 0x00100000,
        FREQHIGH = 0x00200000,
        BANDWIDTH_CENTER = 0x00400000,
        FREQ_OFFSET_REL = 0x00800000,
        PRIVATE = 0x10000000,
        SWAP = 0x20000000,              // swap ON means: do NOT swap (for little endian machines)
        SIGNAL_GREATER_SQUELCH = 0x40000000,
        OPTIONAL_HEADER = 0x80000000
    };
}
