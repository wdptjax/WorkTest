using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Utils
    {
        /// <summary>
        /// 计算扫描的总点数
        /// </summary>
        /// <param name="startFrequency">起始频率 MHz</param>
        /// <param name="stopFrequency">结束频率 MHz</param>
        /// <param name="stepFrequency">步进 kHz</param>
        /// <returns></returns>
        public static int GetTotalCount(double startFrequency, double stopFrequency, double stepFrequency)
        {
            decimal start = new decimal(startFrequency);
            decimal stop = new decimal(stopFrequency);
            decimal step = new decimal(stepFrequency / 1000.0d);
            int total = decimal.ToInt32((stop - start) / step) + 1;
            return total;
        }

        /// <summary>
        /// 从配置文件读取天线配置
        /// </summary>
        /// <param name="antennaConfigName"></param>
        /// <returns></returns>
        public static List<AntennaInfo> GetAntennaInfos(string antennaConfigName) 
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + Define.ANTENNA_CONFIGPATH;

            IniFiles iniFiles = new IniFiles(path + antennaConfigName);

            StringCollection sections = new StringCollection();
            iniFiles.ReadSections(sections);
            if (sections.Count == 0)
            {
                return null;
            }
            List<AntennaInfo> antennas = new List<AntennaInfo>();
            foreach (string name in sections)
            {
                AntennaInfo antenna = new AntennaInfo();
                antenna.Name = name;
                antenna.Index = iniFiles.ReadInteger(name, Define.ANTENNA_INDEX, -1);
                antenna.ControlCode = iniFiles.ReadString(name, Define.ANTENNA_CONTROLCODE, "");
                antenna.AntType = (AntennaType)iniFiles.ReadInteger(name, Define.ANTENNA_ANTTYPE, 0);
                antenna.PolarityType = (PolarityType)iniFiles.ReadInteger(name, Define.ANTENNA_POLARITYTYPE, 0);
                double db = 0;
                string str = iniFiles.ReadString(name, Define.ANTENNA_STARTFREQUENCY, "0");
                double.TryParse(str, out db);
                antenna.StartFrequency = db;
                str = iniFiles.ReadString(name, Define.ANTENNA_STOPFREQUENCY, "0");
                double.TryParse(str, out db);
                antenna.StopFrequency = db;
                antenna.FactorFile = iniFiles.ReadString(name, Define.ANTENNA_FACTORFILE, "");
                antenna.Description = iniFiles.ReadString(name, Define.ANTENNA_DESCRIPTION, "").TrimEnd('\0');
                antennas.Add(antenna);
            }
            return antennas;
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="ident"></param>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public static string ReadIniFiles(string section, string ident,string defaultValue, string configFileName)
        {
            try
            {

                string path = AppDomain.CurrentDomain.BaseDirectory + Define.ANTENNA_CONFIGPATH;
                IniFiles iniFiles = new IniFiles(path + configFileName);

                string value = iniFiles.ReadString(section, ident, defaultValue);
                return value;
            }
            catch (Exception ex)
            {
                //throw ex;
                return defaultValue;
            }
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="ident"></param>
        /// <param name="defaultValue"></param>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public static int ReadIniFiles(string section, string ident, int defaultValue, string configFileName)
        {
            try
            {

                string path = AppDomain.CurrentDomain.BaseDirectory + Define.ANTENNA_CONFIGPATH;
                IniFiles iniFiles = new IniFiles(path + configFileName);

                int value = iniFiles.ReadInteger(section, ident, defaultValue);
                return value;
            }
            catch (Exception ex)
            {
                //throw ex;
                return defaultValue;
            }
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="ident"></param>
        /// <param name="defaultValue"></param>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public static bool ReadIniFiles(string section, string ident, bool defaultValue, string configFileName)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + Define.ANTENNA_CONFIGPATH;
                IniFiles iniFiles = new IniFiles(path + configFileName);

                bool value = iniFiles.ReadBool(section, ident, defaultValue);
                return value;
            }
            catch (Exception ex)
            {
                //throw ex;
                return defaultValue;
            }
        }
    }
}
