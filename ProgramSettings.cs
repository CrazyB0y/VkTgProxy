/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:20
 */
using System;

namespace VkTgProxy
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public static class ProgramSettings
	{
		private static IniFile ifile;

        internal static string VK_APP_ID { get; private set; }
        internal static string VK_ACCESS_TOKEN { get; private set; }
        internal static long VK_CHAT_ID { get; private set; }

        internal static string TG_BOT_KEY { get; private set; }
        internal static long TG_CHAT_ID { get; private set; }
  
        internal static string PROXY_HOST { get; private set; }
        internal static int PROXY_PORT { get; private set; }
        internal static string PROXY_USER { get; private set; }
        internal static string PROXY_PASS { get; private set; }
        internal static bool PROXY_FORCE_TG { get; set; }
        internal static bool PROXY_FORCE_VK { get; set; }

        internal static bool SM_SEND_IN_MILK { get; set; }

        internal static bool Init()
        {
            try
            {
                //Load INI file
                ifile = new IniFile(Program.APP_PATH + @"proxy.ini");
                //Init all varibles 
                VK_APP_ID = "";
                VK_ACCESS_TOKEN = "";
                VK_CHAT_ID = 0;
                TG_BOT_KEY = "";
                TG_CHAT_ID = 0;
                PROXY_HOST = "";
                PROXY_PORT = 0;
                PROXY_USER = "";
                PROXY_PASS = "";
                PROXY_FORCE_TG = false;
                PROXY_FORCE_VK = false;
                SM_SEND_IN_MILK = false;
                //Load settings
                PrepareSettings();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private static void PrepareSettings()
        {
            //Accure vk appId - 6460395
            if (ifile.IniIsHaveValue("VK", "APP_ID"))
                VK_APP_ID = ifile.IniReadValue("VK", "APP_ID");
            //Checking key
            while (true)
            {
                //Check is key right
                if ((VK_APP_ID.Length > 3)) //TODO HERE CHECK THROUGH TG
                {
                    ifile.IniWriteValue("VK", "APP_ID", VK_APP_ID);
                    break;
                }
                else
                {
                    Utils.WriteLineWClr("[SETTINGS MISSING] ERROR_100 - Wrong VK AppId provided",
                        ConsoleColor.Red);
                }
                //Take key
                Utils.WriteLineWClr(" Please provide VK AppId: ", ConsoleColor.DarkGreen);
                VK_APP_ID = Utils.ReadLine();
            }
            //Accure access token
            if (ifile.IniIsHaveValue("VK", "ACCESS_TOKEN"))
                VK_ACCESS_TOKEN = ifile.IniReadValue("VK", "ACCESS_TOKEN");
            while (true)
            {
                //Check is key right
                if ((VK_ACCESS_TOKEN.Length > 10))
                {
                    ifile.IniWriteValue("VK", "ACCESS_TOKEN", VK_ACCESS_TOKEN);
                    break;
                }
                else
                {
                    Utils.WriteLineWClr("[SETTINGS MISSING] ERROR_110 - Wrong access token provided",
                        ConsoleColor.Red);
                }
                //Take key
                Utils.WriteLineWClr(" Now we open VK authorization page in your browser...", ConsoleColor.White);
                Utils.WriteLineWClr(" After authorization please, copy accessToken from browser URL field to program", ConsoleColor.White);
                System.Threading.Thread.Sleep(4000);
                System.Diagnostics.Process.Start(@"https://" + @"oauth.vk.com/authorize?client_id=" + 
                    VK_APP_ID + "&display=mobile&scope=messages,offline&response_type=token&v=5.74");
                Utils.WriteLineWClr(" Please provide access key: ", ConsoleColor.DarkGreen);
                VK_ACCESS_TOKEN = Utils.ReadLine();
            }
            //Accure chat id for VK
            if (ifile.IniIsHaveValue("VK", "CHAT_ID"))
                VK_CHAT_ID = Convert.ToInt64(ifile.IniReadValue("VK", "CHAT_ID"));
            while (true)
            {
                //Check is key right
                if ((VK_CHAT_ID > 0))
                {
                    ifile.IniWriteValue("VK", "CHAT_ID", VK_CHAT_ID.ToString());
                    break;
                }
                else
                {
                    Utils.WriteLineWClr("[SETTINGS MISSING] ERROR_120 - Wrong VK ChatId provided",
                        ConsoleColor.Red);
                }
                //Take key
                Utils.WriteLineWClr(" Please provide VK ChatId: ", ConsoleColor.DarkGreen);
                try
                {
                    VK_CHAT_ID = Convert.ToInt64(Utils.ReadLine());
                }
                catch
                {
                    VK_CHAT_ID = 0;
                }
            }
            //Take telegram bot api key
            if (ifile.IniIsHaveValue("TELEGRAM", "BOT_KEY"))
                TG_BOT_KEY = ifile.IniReadValue("TELEGRAM", "BOT_KEY");
            //Checking key
            while (true)
            {
                //Check is key right
                if ((TG_BOT_KEY.Length > 20)) //TODO HERE CHECK THROUGH TG
                {
                    ifile.IniWriteValue("TELEGRAM", "BOT_KEY", TG_BOT_KEY);
                    break;
                }
                else
                {
                    Utils.WriteLineWClr("[SETTINGS MISSING] ERROR_130 - Wrong access key provided",
                        ConsoleColor.Red);
                }
                //Take key
                Utils.WriteLineWClr(" Please provide telegram access key: ", ConsoleColor.DarkGreen);
                TG_BOT_KEY = Utils.ReadLine();
            }
            //Accure chat id for TG
            if (ifile.IniIsHaveValue("TELEGRAM", "CHAT_ID"))
                TG_CHAT_ID = Convert.ToInt64(ifile.IniReadValue("TELEGRAM", "CHAT_ID"));
            while (true)
            {
                //Check is key right
                if ((TG_CHAT_ID != 0))
                {
                    ifile.IniWriteValue("TELEGRAM", "CHAT_ID", TG_CHAT_ID.ToString());
                    break;
                }
                else
                {
                    Utils.WriteLineWClr("[SETTINGS MISSING] ERROR_140 - Wrong Telegram ChatId provided",
                        ConsoleColor.Red);
                }
                //Take key
                Utils.WriteLineWClr(" Please provide Telegram ChatId: ", ConsoleColor.DarkGreen);
                try
                {
                    TG_CHAT_ID = Convert.ToInt64(Utils.ReadLine());
                }
                catch
                {
                    TG_CHAT_ID = 0;
                }
            }
            //Get Telegram Proxy params
            PROXY_FORCE_TG = Convert.ToBoolean(ifile.IniReadBoolValue("PROXY", "FORCE_TELEGRAM", false));
            PROXY_FORCE_VK = Convert.ToBoolean(ifile.IniReadBoolValue("PROXY", "FORCE_VK", false));
            PROXY_PORT = ifile.IniReadIntValue("PROXY", "PORT", 3306);
            PROXY_HOST = ifile.IniReadStringValue("PROXY", "HOST", "127.0.0.1");
            PROXY_USER = ifile.IniReadStringValue("PROXY", "USER", "ProxyUser");
            PROXY_PASS = ifile.IniReadStringValue("PROXY", "PASS", "ProxyPass");
            //Service manager settings
            SM_SEND_IN_MILK = Convert.ToBoolean(ifile.IniReadBoolValue("SERVICE_MANAGER", "ENABLE_SEND_IN_MILK", false));
        }

        public static string GetStoredData(string dataKey, string defaultValue)
        {
            if (ifile.IniIsHaveValue("STORED_DATA", dataKey))
                return ifile.IniReadValue("STORED_DATA", dataKey);
            else
                return defaultValue;
        }
        public static void SetStoredData(string dataKey, string value)
        {
            ifile.IniWriteValue("STORED_DATA", dataKey, value);
        }
    }
}
