namespace UltiDev.Framework
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    public class IniFileHelper
    {
        private string defaultSectionName;
        private string iniFilePath;

        public IniFileHelper(string iniFilePath) : this(iniFilePath, null)
        {
        }

        public IniFileHelper(string iniFilePath, string defaultSectionName)
        {
            if (string.IsNullOrEmpty(defaultSectionName))
            {
                this.defaultSectionName = "Misc Settings";
            }
            else
            {
                this.defaultSectionName = defaultSectionName;
            }
            if (string.IsNullOrEmpty(iniFilePath))
            {
                throw new ArgumentNullException("iniFilePath");
            }
            this.iniFilePath = iniFilePath;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        private static extern uint GetPrivateProfileString(string sectionName, string settingName, string settingDefaultValue, StringBuilder returnedString, int nSize, string iniFilePath);
        public string GetSetting(string settingName)
        {
            return this.GetSetting(settingName, null);
        }

        public string GetSetting(string settingName, string defaultValue)
        {
            return this.GetSetting(this.defaultSectionName, settingName, defaultValue);
        }

        public string GetSetting(string sectionName, string settingName, string defaultValue)
        {
            StringBuilder returnedString = new StringBuilder(0x4000);
            GetPrivateProfileString(sectionName, settingName, defaultValue, returnedString, returnedString.Capacity, this.iniFilePath);
            return returnedString.ToString();
        }

        public bool SetSetting(string settingName, string settingValue)
        {
            return this.SetSetting(null, settingName, settingValue);
        }

        public bool SetSetting(string sectionName, string settingName, string settingValue)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }
            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = this.defaultSectionName;
            }
            if (settingValue == null)
            {
                settingValue = string.Empty;
            }
            return WritePrivateProfileString(sectionName, settingName, settingValue, this.iniFilePath);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern bool WritePrivateProfileString(string sectionName, string settingName, string settingValue, string initFilePath);

        public string this[string settingName]
        {
            get
            {
                return this.GetSetting(settingName);
            }
            set
            {
                this.SetSetting(settingName, value);
            }
        }
    }
}

