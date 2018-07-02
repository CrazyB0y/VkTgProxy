/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:18
 */
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VkTgProxy
{
	public class IniFile
	{
        private readonly string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        public void IniWriteValue(string Section, string Key, int Value)
        {
            IniWriteValue(Section, Key, Value.ToString());
        }

        public void IniWriteValue(string Section, string Key, bool Value)
        {
            IniWriteValue(Section, Key, Value.ToString());
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            /*int i = */GetPrivateProfileString(Section, Key, "", temp,
                                            255, this.path);
            return temp.ToString();
        }

        /// <summary>
        /// Read string value with default one
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string IniReadStringValue(string Section, string Key, string defaultValue)
        {
            string stringValue = IniReadValue(Section, Key);
            if(stringValue.Length > 0)
            {
                return stringValue;
            }
            else
            {
                IniWriteValue(Section, Key, defaultValue.ToString());
                return defaultValue;
            }
        }

        /// <summary>
        /// Read Bool value from INI file
        /// If read fail return default value
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool IniReadBoolValue(string Section, string Key, bool defaultValue = true)
        {
            string stringValue = IniReadValue(Section, Key);
            try
            {
                bool returnValue = Convert.ToBoolean(stringValue);
                return returnValue;
            }
            catch
            {
                IniWriteValue(Section, Key, defaultValue.ToString());
                return defaultValue;
            }
        }

        /// <summary>
        /// Read Int value from INI file
        /// If read fail return default value
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int IniReadIntValue(string Section, string Key, int defaultValue = 0)
        {
            string stringValue = IniReadValue(Section, Key);
            try
            {
                int returnValue = Convert.ToInt32(stringValue);
                return returnValue;
            }
            catch
            {
                IniWriteValue(Section, Key, defaultValue.ToString());
                return defaultValue;
            }
        }

        /// <summary>
        /// Check is INI have some data
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public bool IniIsHaveValue(string Section, string Key)
        {
            return (IniReadValue(Section, Key).Length > 0);
        }

        /// <summary>
        /// Create new INI data if INI dont have that
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        public bool IniCreateIfDontHave(string Section, string Key, string Default)
        {
            if (!IniIsHaveValue(Section, Key))
            {
                IniWriteValue(Section, Key, Default);
                return true;
            }
            return false;
        }
	}
}
