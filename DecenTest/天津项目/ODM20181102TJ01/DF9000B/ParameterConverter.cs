using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tracker800.Server.Parameters;
using Tracker800.Server.Contract;

namespace ODM20181102TJ01.DF9000B
{
    /// <summary>
    /// 参数转换 0xE=>Tracker800
    /// </summary>
    class ParameterConverter : IParameterConverter
    {
        // 当前任务打开的功能
        private EDataMode _dataMode = EDataMode.MODE_FFM;

        /// <summary>
        /// 当前任务打开的功能
        /// </summary>
        public EDataMode DataMode
        {
            get { return _dataMode; }
            set { _dataMode = value; }
        }

        /// <summary>
        /// 参数转换
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="parameterTable"></param>
        public void ParamsConverter(IParameter parameters, ref ParameterTable parameterTable)
        {
            if (parameters == null || parameterTable == null)
            {
                return;
            }
            try
            {
                switch (DataMode)
                {
                    case EDataMode.MODE_FFM:
                        FfmParamsConverter((FfmParams)parameters, ref parameterTable);
                        break;
                    case EDataMode.MODE_SCAN:
                        ScanParamsConverter((ScanParams)parameters, ref parameterTable);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                // TODO: 异常处理
            }

        }
        /// <summary>
        /// 将FFM的参数解析为Tracker800参数
        /// </summary>
        /// <param name="ffmParams"></param>
        /// <param name="parameterTable"></param>
        private void FfmParamsConverter(FfmParams ffmParams, ref ParameterTable parameterTable)
        {
            CommParamsConverter(ffmParams.CommParams, ref parameterTable);
        }

        /// <summary>
        /// 将扫描参数解析为Tracker800参数
        /// </summary>
        /// <param name="scanParams"></param>
        /// <param name="parameterTable"></param>
        private void ScanParamsConverter(ScanParams scanParams, ref ParameterTable parameterTable)
        {
            if (parameterTable == null || parameterTable.Items == null || scanParams == null)
            {
                return;
            }
            CommParamsConverter(scanParams.CommParams, ref parameterTable);
            // 起始频率
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.StartFrequency))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.StartFrequency);
                decimal value = new decimal(scanParams.StartFreq / 1000000d);
                CheckMaxMinDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 结束频率
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.StopFrequency))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.StopFrequency);
                decimal value = new decimal(scanParams.StopFreq / 1000000d);
                CheckMaxMinDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 步进
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.StepFrequency))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.StepFrequency);
                decimal value = new decimal(scanParams.StepWidth / 1000d);
                CheckEnumDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
        }

        /// <summary>
        /// 将CommParams解析为Tracker800参数
        /// </summary>
        /// <param name="commParams"></param>
        /// <param name="parameterTable"></param>
        private void CommParamsConverter(CommParams commParams, ref ParameterTable parameterTable)
        {
            if (parameterTable == null || parameterTable.Items == null || commParams == null)
            {
                return;
            }
            // 测向带宽
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.DFBandWidth))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.DFBandWidth);
                decimal value = new decimal(commParams.DfBandWdith / 1000d);
                CheckEnumDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 测向模式 Tracker800中对应Normal/Feebleness/Gate
            if (parameterTable.Items.Exists(i => i.Name == "DFindMode"))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == "DFindMode");
                EAverageMode mode = (EAverageMode)commParams.AverageMode;
                ConvertDFindMode(mode, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 电平门限
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.LevelThreshold))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.LevelThreshold);
                decimal value = new decimal(commParams.Threshold);
                CheckMaxMinDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 天线极化方式 Tracker800中对应枚举PolarityType，只有垂直水平两个
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.PolarityType))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.PolarityType);
                EAntPol value = (EAntPol)commParams.AntPol;
                ConvertPolarityType(value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 测向体制 Tracker800中对应枚举DFindMethod,CI-相关干涉仪，PHD-相位差，SSE-空间谱
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.DFindMethod))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.DFindMethod);
                EDfAlt value = (EDfAlt)commParams.DfMethod;
                ConvertDfMethod(value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            //解调模式
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.DemMode))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.DemMode);
                EAfDemod value = (EAfDemod)commParams.AfDemod;
                ConvertDemoMode(value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 静噪门限
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.SquelchThreshold))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.SquelchThreshold);
                decimal value = new decimal(commParams.AfThreshold);
                CheckMaxMinDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 解调带宽
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.IFBandWidth))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.IFBandWidth);
                decimal value = new decimal(commParams.AfBandWidth / 1000d);
                CheckEnumDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
            // 测向质量门限
            if (parameterTable.Items.Exists(i => i.Name == ParameterNames.QualityThreshold))
            {
                PropertyItem item = parameterTable.Items.FirstOrDefault(i => i.Name == ParameterNames.QualityThreshold);
                decimal value = new decimal(commParams.QualityThreshold);
                CheckMaxMinDecimalValue(ref value, ref item);
                //设置完以后要检查脚本约束
                parameterTable.InnerCheckRestrict(parameterTable.ScriptObject, parameterTable.RestrictMethod, item.Name, item.Value);
            }
        }

        /// <summary>
        /// 检查枚举值并设置值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="item"></param>
        private void CheckEnumDecimalValue(ref decimal value, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            string enumStr = item.EnumString;
            if (string.IsNullOrEmpty(enumStr))
            {
                // TODO: 异常处理
                return;
            }
            string[] split = enumStr.Substring(1).Split('|');
            if (split == null || split.Length == 0)
            {
                // TODO: 异常处理
                return;
            }
            var decList = split.Select(s => decimal.Parse(s)).ToList();
            decList.Sort();// 按从小到大排序
            if (!decList.Contains(value))
            {
                // TODO: 如果在序列中不包含当前设置的值，需要将值修改为最近的一个枚举值，然后设置下去，这个做法是否要改？
                decimal old = value;
                value = decList.FirstOrDefault(i => old <= i);//找到大于等于old的第一个值
            }
            item.Value = value;
        }

        /// <summary>
        /// 检查最大最小值并设置值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="item"></param>
        private void CheckMaxMinDecimalValue(ref decimal value, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            decimal max = new decimal(item.MaxValue);
            decimal min = new decimal(item.MinValue);
            // TODO: 超量程的处理需要斟酌！
            //现在是大于最大值或者小于最小值都会将当前值修改为最大值或者最小值
            value = value > max ? max : (value < min ? min : value);
            item.Value = value;
        }

        /// <summary>
        /// 转换测向模式
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="item"></param>
        private void ConvertDFindMode(EAverageMode mode, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            DFindMode dFindMode = DFindMode.Normal;
            switch (mode)
            {
                case EAverageMode.Norm:
                    dFindMode = DFindMode.Normal;
                    break;
                case EAverageMode.Gate:
                    dFindMode = DFindMode.Gate;
                    break;
                case EAverageMode.Cont:
                    dFindMode = DFindMode.Feebleness;
                    break;
                default:
                    return;
            }
            item.Value = dFindMode.ToString();
        }

        /// <summary>
        /// 转换天线极化方式
        /// Tracker800中对应枚举PolarityType，只有垂直水平两个
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="item"></param>
        private void ConvertPolarityType(EAntPol mode, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            PolarityType polarityType = PolarityType.Vertical;
            switch (mode)
            {
                case EAntPol.ANTPOL_VERT:
                    polarityType = PolarityType.Vertical;
                    break;
                case EAntPol.ANTPOL_HOR:
                    polarityType = PolarityType.Horizontal;
                    break;
                default:
                    return;
            }
            item.Value = polarityType.ToString();
        }

        /// <summary>
        /// 转换测向体制
        /// Tracker800中对应枚举DFindMethod,CI-相关干涉仪，PHD-相位差，SSE-空间谱
        /// </summary>
        /// <param name="value"></param>
        /// <param name="item"></param>
        private void ConvertDfMethod(EDfAlt value, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            DFindMethod method = DFindMethod.CI;
            switch (value)
            {
                case EDfAlt.DFALT_CORRELATION://相关干涉仪
                    method = DFindMethod.CI;
                    break;
                case EDfAlt.DFALT_VECTORMATCHING://矢量匹配--这个是不是相位差？
                    // TODO: 这里不确定
                    method = DFindMethod.PHD;
                    break;
                case EDfAlt.DFALT_SUPERRESOLUTION://超分辨率
                case EDfAlt.DFALT_WATSONWATT://沃特森-瓦特
                default:
                    return; 
            }
            item.Value = method.ToString();
        }

        /// <summary>
        /// 转换解调模式
        /// </summary>
        /// <param name="value"></param>
        /// <param name="item"></param>
        private void ConvertDemoMode(EAfDemod value, ref PropertyItem item)
        {
            if (item == null)
            {
                return;
            }
            DemoduMode demoMode = DemoduMode.AM;
            switch (value)
            {
                case EAfDemod.DEMOD_AM:
                    demoMode = DemoduMode.AM;
                    break;
                case EAfDemod.DEMOD_FM:
                    demoMode = DemoduMode.FM;
                    break;
                case EAfDemod.DEMOD_USB:
                    demoMode = DemoduMode.USB;
                    break;
                case EAfDemod.DEMOD_LSB:
                    demoMode = DemoduMode.LSB;
                    break;
                case EAfDemod.DEMOD_CW:
                    demoMode = DemoduMode.CW;
                    break;
                default:
                    return;
            }
            item.Value = demoMode.ToString();
        }

    }
}
