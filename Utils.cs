/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:18
 */
using System;

namespace VkTgProxy
{
    public static class Utils
    {
        public static void ConfigureConsole()
        {
            Console.Title = "TgProxy (" + Program.PROXY_VERSION + ")";
            Console.Beep(2500, 75);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\r\n [VKontakte to Telegram Proxy]\r\n" +
            @" Software written by Cr1zyB0y (aka Anton Zhuchkov)" + "\r\n" +
            @" Use for contact me: https://t.me/cr1zyb0y" + "\r\n" +
            @" For license information open LICENSE.TXT file " + "\r\n" +
            @" Proxy version is " + Program.PROXY_VERSION + "\r\n\r\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public static string ReadLine()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(" > ");
            Console.ForegroundColor = ConsoleColor.Gray;
            string value = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            return value;
        }

        public static void WriteLine(string line, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(line, arg);
        }

        public static void WriteLineWClr(string line, ConsoleColor color, params object[] arg)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(line, arg);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
    }
}
