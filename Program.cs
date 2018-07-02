/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:10
 */
using System;
using System.Runtime.InteropServices;

namespace VkTgProxy
{
	class Program
	{
		internal static readonly string APP_PATH = System.AppDomain.CurrentDomain.BaseDirectory; //App directory
		internal static readonly string PROXY_VERSION = "1.0.0 Alpha"; //Current version
        internal static ServiceManager proxyS; //Proxy
		
		private static bool isProxyWork = true; //Is wrapper need work
		
		//For catching exit event
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
		
		public static void Main(string[] args)
		{
            //Logo and license, console styles
            Utils.ConfigureConsole();
            //Loading wrapper settings
            ProgramSettings.Init();            
            //Take handler for close console and exceptions
            handler = new ConsoleEventDelegate(ApplicationKillCallback);
            SetConsoleCtrlHandler(handler, true);
            AppDomain.CurrentDomain.ProcessExit += (o, e) => { TerminateProxy(new Exception("Shuting down system by user interaction...")); };
            AppDomain.CurrentDomain.UnhandledException += ApplicationExceptionCallback;
            //Just start networking in other threads
#if DEBUG
            proxyS = new ServiceManager();
            proxyS.Start();
#else
            try
            {
                proxyS = new ServiceManager();
				proxyS.Start();
            }
            catch(Exception ex)
            {
                Utils.WriteLineWClr("[PROXY EXCEPTION] Taking error while running ProxyManager!", ConsoleColor.Red);
                TerminateProxy(ex);
            }
#endif
            //Take user command
            while (isProxyWork)
            {
                //DEBUGING string readedFromConsole = Utils.ReadLine();
                //DEBUGING if (readedFromConsole.Length < 2) continue;
                //TODO: Make commands system
               System.Threading.Thread.Sleep(10000);
            }
            Utils.WriteLineWClr("\r\n[APPLICATION EXECUTION STOPPED] {0}", ConsoleColor.DarkYellow, DateTime.Now.ToString());
            Utils.WriteLineWClr("Press any key for close console...", ConsoleColor.DarkGray);
            Console.ReadKey(true);
		}
		

        public static void TerminateProxy(Exception arrivedException)
        {
            //TODO: MAKE RIGHT EXCEPTION ACCURING
            Exception currentWorkException = arrivedException;
            short exNumber = 1;
            string fatalLogFileName = Program.APP_PATH + "FATAL_ERROR_LOGS.log";
            System.IO.File.Delete(fatalLogFileName);
            while (true)
            {
                //Check is ex is exists
                if (currentWorkException.InnerException == null) break;
                //Taking data from current ex
                var message = currentWorkException.Message;
                var stackTrace = currentWorkException.StackTrace;
                var targetSite = currentWorkException.TargetSite;
                //Draw data to console and file
                string exLog = "[Exception #" + exNumber.ToString() + "]\r\nMessage: " + message + 
                    "\r\nTarget: " + targetSite + "\r\nStackTrace: " + stackTrace;
                Utils.WriteLineWClr("[TERMINATING PROXY] {0}", ConsoleColor.Red, exLog);
                System.IO.File.AppendAllText(fatalLogFileName, exLog);
                //Take deep in ex
                currentWorkException = currentWorkException.InnerException;
                exNumber++;
            }
            //Kill server
            if (proxyS.GetIsWorking())
           	{
                proxyS.Exit();
            }
            //Disable commands and others interactions
            isProxyWork = false;
            Environment.ExitCode = 2;
        }

        private static bool ApplicationKillCallback(int eventType)
        {
            if (eventType == 2)
            {
                TerminateProxy(new Exception("Shuting down system by by user interaction. Window closed - kill imminent."));
            }
            return false;
        }

        private static void ApplicationExceptionCallback(object sender, UnhandledExceptionEventArgs e)
        {
            //Catching unhandled expections from app
            if (e.IsTerminating)
            {
                Utils.WriteLineWClr("[UNHANDLED EXCEPTION] [FATAL]: {0}", 
                    ConsoleColor.Red, ((Exception)e.ExceptionObject).Message);
                TerminateProxy(((Exception)e.ExceptionObject));
            }
            else
            {
                Utils.WriteLineWClr("[UNHANDLED EXCEPTION] [NON-FATAL]: {0}", 
                                    ConsoleColor.Red, ((Exception)e.ExceptionObject).Message);
            }
        }
	}
}