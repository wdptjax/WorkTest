/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\Script.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    无
 * 
 * 备    注:		MR3000A系列接收机参数约束
 *                                            
*********************************************************************************************/

using System;
using System.Collections.Generic;

[Serializable]
public class PropertyObjectScript
{
    public List<object[]> RestrictCheck(Dictionary<string, object> allValues, string changedName, object changedValue, string featureType)
    {
        var retValues = new List<object[]>();
        retValues.Add(new object[] { string.Empty });

        RFModeConstraint(allValues, changedName, changedValue, ref retValues);
        AttenuationControlConstraint(changedName, changedValue, ref retValues);
        // DetectModeConstraint(changedName, changedValue, ref retValues);
        IQAndAudioConstraint(changedName, changedValue, ref retValues);

        switch (featureType.ToLower())
        {
            case "scan":
                HFVHFSegmentConstraint(changedName, allValues, changedValue, ref retValues);
                ScanSegmentConstraint(allValues, ref retValues);
                ScanModeConstraint(changedName, allValues, changedValue, ref retValues);
                break;
            case "fastscan":
                retValues.Add(new object[] { "Value", "StartFrequency", 20.0d });
                retValues.Add(new object[] { "Value", "StopFrequency", 8000.0d });
                retValues.Add(new object[] { "EnumString", "StepFrequency", "|25", "|25kHz" });
                retValues.Add(new object[] { "Value", "StepFrequency", 25 });
                retValues.Add(new object[] { "ReadOnly", "StartFrequency", true });
                retValues.Add(new object[] { "ReadOnly", "StopFrequency", true });
                retValues.Add(new object[] { "ReadOnly", "StepFrequency", true });
                break;
            case "ifmultichannel":
                retValues.Add(new object[] { "EnumString", "SpectrumSpan", "|40000", "|40MHz" });
                retValues.Add(new object[] { "Value", "SpectrumSpan", 40000.0d });
                retValues.Add(new object[] { "Browsable", "MaxChanCount", false });
                break;
            case "gsmrscan":
                retValues.Add(new object[] { "EnumString", "StepFrequency", "|25", "|25kHz" });
                retValues.Add(new object[] { "Value", "StepFrequency", 25 });
                break;
            default:
                break;
        }

        return retValues;
    }

    private void RFModeConstraint(Dictionary<string, object> allValues, string changedName, object changedValue, ref List<object[]> retValues)
    {
        if (changedName.ToLower().Equals("rfmode"))
        {
            if (changedValue.ToString().ToLower().Equals("lowdistort"))
            {
                retValues.Add(new object[] { "ValueRange", "RFAttenuation", 20, 0 });
                foreach (var item in allValues)
                {
                    try
                    {
                        if (item.Key.ToLower().Equals("rfattenuation"))
                        {
                            var value = int.Parse(item.Value.ToString());
                            if (value > 20)
                            {
                                retValues.Add(new object[] { "Value", "RFAttenuation", 20 });
                            }
                            else
                            {
                                retValues.Add(new object[] { "Value", "RFAttenuation", value });
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        retValues[0] = new object[] { ex.Message };
                        return;
                    }
                }
            }
            else
            {
                retValues.Add(new object[] { "ValueRange", "RFAttenuation", 30, 0 });
            }
        }
    }

    private void AttenuationControlConstraint(string changedName, object changedValue, ref List<object[]> retValues)
    {
        if (changedName.ToLower().Equals("autoattenuation"))
        {
            if (changedValue.ToString().ToLower().Equals("agc"))
            {
                retValues.Add(new object[] { "Browsable", "RFAttenuation", false });
                retValues.Add(new object[] { "Browsable", "IFAttenuation", false });
            }
            else
            {
                retValues.Add(new object[] { "Browsable", "RFAttenuation", true });
                retValues.Add(new object[] { "Browsable", "IFAttenuation", true });
            }
        }
    }

    private void DetectModeConstraint(string changedName, object changedValue, ref List<object[]> retValues)
    {
        if (changedName.ToLower().Equals("detector"))
        {
            if (changedValue.ToString().ToLower().Equals("fast"))
            {
                retValues.Add(new object[] { "Browsable", "MeasureTime", false });
            }
            else
            {
                retValues.Add(new object[] { "Browsable", "MeasureTime", true });
            }
        }
    }

    private void HFVHFSegmentConstraint(string changedName, Dictionary<string, object> allValues, object changedValue, ref List<object[]> retValues)
    {
        var startFrequency = -99999.9d;
        var stopFrequency = -99999.9d;
        var antennaAvilable = false;
        var antenna = string.Empty;
        foreach (var item in allValues)
        {
            if (item.Key.ToLower().Equals("startfrequency"))
            {
                startFrequency = double.Parse(item.Value.ToString());
            }
            if (item.Key.ToLower().Equals("stopfrequency"))
            {
                stopFrequency = double.Parse(item.Value.ToString());
            }
            if (item.Key.ToLower().Equals("antennaid") || item.Key.ToLower().Equals("polaritytype"))
            {
                antennaAvilable = true;
                antenna = item.Key;
            }
        }

        if (changedName.ToLower().Equals("startfrequency"))
        {
            if (startFrequency >= 30)  // 调整到VHF
            {
                if (stopFrequency < 30) // 结束频率也至少从40MHz开始
                {
                    retValues.Add(new object[] { "Value", "StopFrequency", 30 });
                }
                if (antennaAvilable)
                {
                    retValues.Add(new object[] { "Browsable", antenna, true });
                }
            }
            else
            {   // 调整到HF

                if (stopFrequency > 30)
                {
                    retValues.Add(new object[] { "Value", "StopFrequency", 30 });
                }
                if (antennaAvilable)
                {
                    retValues.Add(new object[] { "Browsable", antenna, false });
                }
            }
        }

        if (changedName.ToLower().Equals("stopfrequency"))
        {
            if (stopFrequency >= 30)        // 调整到VHF
            {
                if (startFrequency < 30) // 开始频率也至少从40MHz开始
                {
                    retValues.Add(new object[] { "Value", "StartFrequency", 30 });
                }
                if (antennaAvilable)
                {
                    retValues.Add(new object[] { "Browsable", antenna, true });
                }
            }
            else
            {   // 调整到HF

                if (startFrequency > 30)
                {
                    retValues.Add(new object[] { "Value", "StartFrequency", 30 });
                }
                if (antennaAvilable)
                {
                    retValues.Add(new object[] { "Browsable", antenna, false });
                }
            }
        }
    }

    private void ScanModeConstraint(string changedName, Dictionary<string, object> allValues, object changedValue, ref List<object[]> retValues)
    {
        if (!changedName.ToLower().Equals("scanmode"))
        {
            return;
        }
        if (changedValue == null)
        {
            retValues[0] = new object[] { "扫描模式不能为空" };
            return;
        }

        var steps = new double[] { 3.125, 6.25, 12.5, 25, 50, 100, 200, 500 };
        var valueString = changedValue.ToString();
        if (valueString.ToLower().Equals("pscan"))
        {
            retValues.Add(new object[] { "IsSelectOnly", "StepFrequency", true });
            retValues.Add(new object[] { "ReadOnly", "IFBandWidth", true });
            retValues.Add(new object[] { "Browsable", "IFBandWidth", false });

            foreach (var item in allValues)
            {
                if (item.Key.ToLower() == "stepfrequency")
                {
                    double value = double.Parse(item.Value.ToString());
                    bool legal = false;
                    double min = Math.Abs(value - steps[0]);
                    int idx = 0;
                    for (int i = 0; i < steps.Length; ++i)
                    {
                        double diff = Math.Abs(value - steps[i]);
                        if (diff == 0.0)
                        {
                            legal = true;
                            break;
                        }
                        if (diff <= min)
                        {
                            min = diff;
                            idx = i;
                        }
                    }
                    if (!legal)
                    {
                        retValues.Add(new object[] { "Value", "StepFrequency", steps[idx] });
                    }
                    break;
                }
            }
        }
        else if (valueString.ToLower().Equals("fscan"))
        {
            retValues.Add(new object[] { "IsSelectOnly", "StepFrequency", false });
            retValues.Add(new object[] { "ReadOnly", "IFBandWidth", false });
            retValues.Add(new object[] { "Browsable", "IFBandWidth", true });
        }
    }

    private void ScanSegmentConstraint(Dictionary<string, object> allValues, ref List<object[]> retValues)
    {
        var startFrequency = 99999.9d;
        var stopFrequency = -99999.9d;
        var stepFrequency = 999999.9d;

        foreach (var item in allValues)
        {
            try
            {
                if (item.Key.ToLower().Equals("startfrequency"))
                {
                    startFrequency = double.Parse(item.Value.ToString());
                }
                if (item.Key.ToLower().Equals("stopfrequency"))
                {
                    stopFrequency = double.Parse(item.Value.ToString());
                }
                if (item.Key.ToLower().Equals("stepfrequency"))
                {
                    stepFrequency = double.Parse(item.Value.ToString());
                }
            }
            catch
            {
                retValues[0] = new object[] { "起始频率/结束频率/步进设置不符合要求" };
                return;
            }
        }

        if (startFrequency >= stopFrequency || Math.Abs(stopFrequency - startFrequency) * 1000.0d < stepFrequency)
        {
            retValues[0] = new object[] { "起始频率/结束频率/步进设置不符合要求" };
        }
    }

    private void IQAndAudioConstraint(string changedName, object changedValue, ref List<object[]> retValues)
    {
        if (changedName.ToLower().Equals("iqswitch") && bool.Parse(changedValue.ToString()))
        {
            retValues.Add(new object[] { "Value", "AudioSwitch", false });
            retValues.Add(new object[] { "Value", "ITUSwitch", false });
        }
        if (changedName.ToLower().Equals("audioswitch") && bool.Parse(changedValue.ToString()))
        {
            retValues.Add(new object[] { "Value", "IQSwitch", false });
        }
        if (changedName.ToLower().Equals("ituswitch") && bool.Parse(changedValue.ToString()))
        {
            retValues.Add(new object[] { "Value", "IQSwitch", false });
        }
    }
}
