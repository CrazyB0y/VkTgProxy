/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:41
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace VkTgProxy
{
    public class TgService : QueryService
    {
        private readonly ServiceManager _proxyService;
        private TelegramBotClient _tgBotClient;
        private long _lastMessageId = 0;
        private ChatId _chatId;

        public TgService(ServiceManager proxyService)
        {
            _proxyService = proxyService;
            _lastMessageId = Convert.ToInt64(ProgramSettings.GetStoredData("TG_LAST_MESSAGE_ID", "0"));
        }

        public override bool IsServiceOnline()
        {
            return (_chatId != null && _tgBotClient.IsReceiving);
        }

        public override string GetServiceName() { return "Telegram"; }

        public override void Exit()
        {
            ProgramSettings.SetStoredData("TG_LAST_MESSAGE_ID", _lastMessageId.ToString());
            if (_tgBotClient != null && _tgBotClient.IsReceiving)
                _tgBotClient.StopReceiving();
        }

        public override void Send(string preparedMessage)
        {
            if (ProgramSettings.SM_SEND_IN_MILK) return;
            //Send already prepared message
            _tgBotClient.SendTextMessageAsync(
                _chatId,
                preparedMessage,
                ParseMode.Html
            );
        }

        public override void Send(UniversalMessage[] msgs)
        {
            //Send one raw message in universal format
            this.Send(UniversalMessage.MakeUpArray(this,msgs));
        }

        public override void Send(UniversalMessage msg)
        {
            //Send one raw message in universal format
            this.Send(MakeUp(msg));
        }
  
        public override string MakeUp(UniversalMessage msg)
        {
            string msgTemplate = "<b>{0}</b> (via {1}) - {2}\r\n{3}";
            return String.Format(msgTemplate,
                msg.Author,
                msg.FromService.GetServiceName(),
                msg.Time.ToString(),
                msg.Text
            );
        }

        private bool _repairProxyNeedSignature = false;
        private bool _repairWrongProxySignature = false;

        public override void Repair()
        {
            if (_repairProxyNeedSignature)
            {
                if (!ProgramSettings.PROXY_FORCE_TG)
                {
                    Utils.WriteLineWClr("[TG API] [REPAIR] Warning! Your operator trying to replace certs or blocking telegram!", ConsoleColor.DarkYellow);
                    Utils.WriteLineWClr("[TG API] [REPAIR] Recommend use proxy or VPN for protect your traffic", ConsoleColor.DarkYellow);
                    Utils.WriteLineWClr("[TG API] [REPAIR] Telegram don't work on this network, switching to proxy...", ConsoleColor.DarkYellow);
                    ProgramSettings.PROXY_FORCE_TG = true; //This is temp enable proxy
                }
                else
                {
                    Utils.WriteLineWClr("[TG API] [ERROR] FuckDaRKN? Your operator block proxy or replace certs!", ConsoleColor.DarkRed);
                    Utils.WriteLineWClr("[TG API] [ERROR] Use another proxy or activate VPN connection", ConsoleColor.DarkRed);
                    Program.TerminateProxy(new Exception("Proxy automatic switching error, AuthenticationException still accuring"));
                }
            }
            else if(_repairWrongProxySignature)
            {
                Utils.WriteLineWClr("[TG API] [ERROR] Proxy refuse connections. Please choose another proxy.", ConsoleColor.DarkRed);
                Program.TerminateProxy(new Exception("Proxy connection error, ProxyException accuring"));
            }
        }

        public override bool Start()
        {
            //Open proxy for telegram
            if (ProgramSettings.PROXY_FORCE_TG)
            {
                //Show message
                Utils.WriteLine("[TG API] Using proxy for connect to telegram...");
                Utils.WriteLine("[TG API] Proxy settings - {0}:{1} ({2}@{3})",
                    ProgramSettings.PROXY_HOST, ProgramSettings.PROXY_PORT,
                    ProgramSettings.PROXY_USER, ProgramSettings.PROXY_PASS);
                //Load proxy settings
                var settings = new SocksSharp.Proxy.ProxySettings();
                settings.SetHost(ProgramSettings.PROXY_HOST);
                settings.SetPort(ProgramSettings.PROXY_PORT);
                settings.SetCredential(ProgramSettings.PROXY_USER, ProgramSettings.PROXY_PASS);
                //Proxy connect
                var proxyClientHandler = new SocksSharp.ProxyClientHandler<SocksSharp.Proxy.Socks5>(settings);
                var httpClient = new System.Net.Http.HttpClient(proxyClientHandler);
                _tgBotClient = new TelegramBotClient(ProgramSettings.TG_BOT_KEY, httpClient);
            }
            else
            {
                Utils.WriteLine("[TG API] Using direct connect to telegram...");
                _tgBotClient = new TelegramBotClient(ProgramSettings.TG_BOT_KEY);
            }
            //Authorize in telegram
            Utils.WriteLine("[TG API] Using token: {0}", ProgramSettings.TG_BOT_KEY);
            User currentTgBotInfo = null;
            try
            {
                currentTgBotInfo = _tgBotClient.GetMeAsync().Result;
            }
            catch (AggregateException ex)
            {
                //Checking what error type
                ex.Handle(x =>
                {
                    if (x is WebException ||
                        x is System.Net.Http.HttpRequestException ||
                        x is System.Security.Authentication.AuthenticationException)
                    {
                        _repairProxyNeedSignature = true;
                        _repairWrongProxySignature = false;
                        return true;
                    }
                    if (x is SocksSharp.Proxy.ProxyException ||
                        x is System.Net.Sockets.SocketException)
                    {
                        _repairWrongProxySignature = true;
                        _repairProxyNeedSignature = false;
                        return true;
                    }
                    _repairProxyNeedSignature = false;
                    _repairWrongProxySignature = false;
                    return false;
                });
            }
            //Checking is auth succesful
            if (currentTgBotInfo == null)
            {
                Utils.WriteLineWClr("[TG API] [ERROR] Auth in telegram failed. Check BotToken.", ConsoleColor.Red);
                return false;
            }
            Utils.WriteLine("[TG API] Succesful authorized as {0} {1} ({2}).",
                currentTgBotInfo.FirstName, currentTgBotInfo.LastName, currentTgBotInfo.Username);
            //Accure chat id 
            _chatId = new ChatId(ProgramSettings.TG_CHAT_ID);
            //Starting thread for reciving data
            _tgBotClient.OnMessage += _tgBotClient_OnMessage;
            _tgBotClient.StartReceiving(new UpdateType[] { UpdateType.MessageUpdate });
            return true;
        }

        private void _tgBotClient_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            //Load message in var
            var message = messageEventArgs.Message;
            if (message == null) return;
            //Check last message id
            if ((_lastMessageId >= message.MessageId) ||
                    (message.Type != MessageType.TextMessage)) return;
            //Save last id
            _lastMessageId = message.MessageId;
            //Emergency shutdown
            if (message.Text == "SHUTDOWN_1595_NOW")
            {
                string terminateMsg = "Emergency shutdown from Telegram";
                Utils.WriteLineWClr("[PROXY STOP] " + terminateMsg, ConsoleColor.Red);
                Program.TerminateProxy(new Exception(terminateMsg));
            }
            //Make universal message and send
            UniversalMessage arrivedMsg = new UniversalMessage(this, message);
            //OLD DATA Utils.WriteLine("[TG API] Message ID arrived {0}", message.MessageId);
            this._proxyService.SendToAnother(arrivedMsg);
        }
    }
}
