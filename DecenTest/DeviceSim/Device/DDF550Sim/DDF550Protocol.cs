using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDF550Sim
{
    #region Xml枚举

    //eAF_BANDWIDTH
    /// <summary>
    /// 3.1 解调带宽
    /// </summary>
    public enum EDemoBandWidth
    {
        /// <summary>
        /// 100 Hz
        /// </summary>
        BW_0P1,
        /// <summary>
        /// 150 Hz
        /// </summary>
        BW_0P15,
        /// <summary>
        /// 300 Hz
        /// </summary>
        BW_0P3,
        /// <summary>
        /// 600 Hz
        /// </summary>
        BW_0P6,
        /// <summary>
        /// 1 kHz
        /// </summary>
        BW_1,
        /// <summary>
        /// 1.5 kHz
        /// </summary>
        BW_1P5,
        /// <summary>
        /// 2.1 kHz
        /// </summary>
        BW_2P1,
        /// <summary>
        /// 2.4 kHz
        /// </summary>
        BW_2P4,
        /// <summary>
        /// 2.7 kHz
        /// </summary>
        BW_2P7,
        /// <summary>
        /// 3.1 kHz
        /// </summary>
        BW_3P1,
        /// <summary>
        /// 4 kHz
        /// </summary>
        BW_4,
        /// <summary>
        /// 4.8 kHz
        /// </summary>
        BW_4P8,
        /// <summary>
        /// 6 kHz
        /// </summary>
        BW_6,
        /// <summary>
        /// 8.333 kHz
        /// </summary>
        BW_8P333,
        /// <summary>
        /// 9 kHz
        /// </summary>
        BW_9,
        /// <summary>
        /// 12 kHz
        /// </summary>
        BW_12,
        /// <summary>
        /// 15 kHz
        /// </summary>
        BW_15,
        /// <summary>
        /// 25 kHz
        /// </summary>
        BW_25,
        /// <summary>
        /// 30 kHz
        /// </summary>
        BW_30,
        /// <summary>
        /// 50 kHz
        /// </summary>
        BW_50,
        /// <summary>
        /// 75 kHz
        /// </summary>
        BW_75,
        /// <summary>
        /// 120 kHz
        /// </summary>
        BW_120,
        /// <summary>
        /// 150 kHz
        /// </summary>
        BW_150,
        /// <summary>
        /// 250 kHz
        /// </summary>
        BW_250,
        /// <summary>
        /// 300 kHz
        /// </summary>
        BW_300,
        /// <summary>
        /// 500 kHz
        /// </summary>
        BW_500,
        /// <summary>
        /// 800 kHz
        /// </summary>
        BW_800,
        /// <summary>
        /// 1 MHz
        /// </summary>
        BW_1000,
        /// <summary>
        /// 1.25 MHz
        /// </summary>
        BW_1250,
        /// <summary>
        /// 1.5 MHz
        /// </summary>
        BW_1500,
        /// <summary>
        /// 2 MHz
        /// </summary>
        BW_2000,
        /// <summary>
        /// 5 MHz
        /// </summary>
        BW_5000,
        /// <summary>
        /// 8 MHz
        /// </summary>
        BW_8000,
        /// <summary>
        /// 10 MHz
        /// </summary>
        BW_10000,
        /// <summary>
        /// 12.5 MHz
        /// </summary>
        BW_12500,
        /// <summary>
        /// 15 MHz
        /// </summary>
        BW_15000,
        /// <summary>
        /// 20 MHz
        /// </summary>
        BW_20000,
    }

    /// <summary>
    /// 3.2 音频过滤模式
    /// </summary>
    public enum EAudioFilterMode
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
    /// 3.3 输出IF (X61)和视频(X62)的模拟信号类型。
    /// Type of analog signal at outputs IF (X61) and Video (X62).
    /// </summary>
    public enum EAnalogVideoOutputMode
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
    /// 3.4 天线测试运行:输出电平指示值或不输出电平指示值，天线测试散热器开或关
    /// Test operation of antenna: output of level indications or not, antenna test radiator on or off.
    /// </summary>
    public enum EAntennaLevelSwitch
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
    /// 3.5 天线控制模式
    /// </summary>
    public enum EAntennaControl
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
    /// 3.6 天线极化方式
    /// </summary>
    public enum EAntennaPolarizationType
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
    /// 3.7 衰减模式
    /// </summary>
    public enum EAttenuatorMode
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
    /// 3.8 音频格式
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
    /// 3.9 AUX控制模式
    /// </summary>
    public enum EAuxControl
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
    /// 3.10 取平均值模式?
    /// </summary>
    public enum EAveragingMode
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
    /// 3.11 Blanking input.
    /// </summary>
    public enum EBlankingInput
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
    /// 3.12 Blanking mode.
    /// </summary>
    public enum EBlankingMode
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
    /// 3.13 Polarity of Blanking signal.
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
    /// 3.14 平均值取值模式
    /// </summary>
    public enum EAveragingSelectType
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
    /// 3.15 Modulation of calibration generator.
    /// </summary>
    public enum ECalibrationModulation
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
    /// 3.16 Output connector for calibration signal.
    /// NOTE 1: In case of connected antenna, input connector(s) for antenna signal(s) and output connector for calibration signal must be in same connector group.
    /// NOTE 2: With DDF1GTX, both calibration outputs are equivalent, but selection must be done with command CalibrationGenerator.
    /// NOTE 3: Mind that some connectors for HF range are available only if the corresponding option is installed:
    /// </summary>
    public enum ECalibrationOutputConnector
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
    /// 3.17 Position of antenna signal switch.
    /// </summary>
    public enum EAntennaSignalSwitch
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


    #endregion Xml枚举
    #region 结构体
    #endregion 结构体
}
