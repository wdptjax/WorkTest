using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODM20181102TJ01
{
    /// <summary>
    /// 测向体制
    /// </summary>
    public enum EDfAlt
    {
        DFALT_WATSONWATT = 0,
        DFALT_CORRELATION = 1,
        DFALT_SUPERRESOLUTION = 2,
        DFALT_VECTORMATCHING = 3,
        MAX_DFALT
    }

    /// <summary>
    /// 天线极化方式
    /// </summary>
    public enum EAntPol
    {
        /// <summary>
        /// 圆形左极化
        /// </summary>
        ANTPOL_LEFT = 0,    /* circular left polarisation */
        /// <summary>
        /// 圆形右极化
        /// </summary>
        ANTPOL_RIGHT = 1,    /* circular right polarisation */
        /// <summary>
        /// 垂直极化
        /// </summary>
        ANTPOL_VERT = 2,    /* linear vertical polarisation */
        /// <summary>
        /// 水平极化
        /// </summary>
        ANTPOL_HOR = 3,    /* linear horizontal polarisation */
        /// <summary>
        /// 默认，极化方式自动
        /// </summary>
        ANTPOL_AUTO = 4,    /* automatic or no polarisation switching */
        MAX_ANTPOL
    }

    /// <summary>
    /// 输出数据类型
    /// </summary>
    public enum EDataOut
    {
        AUDIO_DATA = 0xC0,   /* data of NF signal                  */
        FFM_DATA,                             /* DF results at fixed frequency      */
        GPS_DATA,                   /* geographical coordinates und absolute time   */
        IF_SPECTRUM_DATA,           /* spectrum data of the intermediate frequency  */
        SCAN_DATA,                  /* DF result of an amount of channels           */
        TDMA_DATA,                  /* DF results of the scanned TDMA channels      */
        TEST_DATA,                  /* test results, in graph form                  */
        TEST_STRING,                /* test result, in textual form                 */
        TEST_VALUES,                /* test results, in plain structure form        */
        SEARCH_DATA,                /* DF results at fixed frequency during search  */
        FFM_SR_DATA,                /* FFM superresolution data                     */
        SR_SPECTRUM_DATA,           /* SR spectrum data                             */
        MAX_DATA_OUT
    }

    /// <summary>
    /// 任务需要的功能类别
    /// </summary>
    public enum EDataMode
    {
        MODE_FFM,
        MODE_SEARCH,
        MODE_SCAN,
        MODE_TDMA,
        MODE_CALIB,
        MODE_DIAG,
        /// <summary>
        /// 任务停止
        /// </summary>
        MODE_STANDBY
    }

    #region 参数枚举

    /// <summary>
    /// 测向模式
    /// </summary>
    public enum EAverageMode
    {
        Norm,
        Cont,
        Gate
    }

    public enum EReadMode
    {
        RM_CONT,
        RM_RAISING,
        RM_RAISING_FALLING,
        RM_FALLING,
        RM_RAISING_CONT,
        RM_FALLING_CONT,
        RM_RAISING_FALLING_CONT
    }

    public enum ESampleMode
    {
        SAMPLE_MODE_HF_100KHZ,
        SAMPLE_MODE_HF_200KHZ,
        SAMPLE_MODE_HF_500KHZ,
        SAMPLE_MODE_HF_1MHZ,
        SAMPLE_MODE_HFD_1MHZ,
        SAMPLE_MODE_HFD_2MHZ,
        SAMPLE_MODE_HFD_5MHZ,
        SAMPLE_MODE_HFD_10MHZ,
        SAMPLE_MODE_VUHF_100KHZ,
        SAMPLE_MODE_VUHF_200KHZ,
        SAMPLE_MODE_VUHF_500KHZ,
        SAMPLE_MODE_VUHF_1MHZ,
        SAMPLE_MODE_VUHF_2MHZ,
        SAMPLE_MODE_VUHF_5MHZ,
        SAMPLE_MODE_VUHF_10MHZ,
        SAMPLE_MODE_VLF_100KHZ,
        SAMPLE_MODE_VLF_200KHZ,
        SAMPLE_MODE_VLF_500KHZ,
        SAMPLE_MODE_GSM
    }

    /// <summary>
    /// 解调模式
    /// </summary>
    public enum EAfDemod
    {
        DEMOD_AM,
        DEMOD_FM,
        DEMOD_USB,
        DEMOD_LSB,
        DEMOD_CW
    }

    #endregion 参数枚举
}
