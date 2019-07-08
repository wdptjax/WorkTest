using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Common
{
    public static class AntennaHelper
    {

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
    }
}
