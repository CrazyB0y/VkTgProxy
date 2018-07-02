/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:44
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace VkTgProxy
{
    public class VkService : QueryService
    {
        private readonly ServiceManager _proxyService;
        private readonly VkApi _vkAPI;
        private readonly Dictionary<long, string> _userNameByIdCache;
        private readonly long _chatPeedId = 2000000000;
        private readonly Thread _pollVKMsgThread;
        private long _lastMessageId = 0;
        private bool _vkAPIActive = false;

        public VkService(ServiceManager proxyService)
        {
            _proxyService = proxyService;
            _userNameByIdCache = new Dictionary<long, string>();
            _vkAPI = new VkApi(NLog.LogManager.GetLogger("VKAPI"));
            _lastMessageId = Convert.ToInt64(ProgramSettings.GetStoredData("VK_LAST_MESSAGE_ID","0"));
            _chatPeedId += ProgramSettings.VK_CHAT_ID;
            _pollVKMsgThread = new Thread(PollUnreadMsgInVk)
            {
                IsBackground = true,
                Name = nameof(_pollVKMsgThread)
            };
        }

        public override bool IsServiceOnline()
        {
            return (_vkAPI.UserId != null && _vkAPI.IsAuthorized && _vkAPIActive);
        }

        public override string GetServiceName() { return "VK"; }

        public override void Exit()
        {
            ProgramSettings.SetStoredData("VK_LAST_MESSAGE_ID", _lastMessageId.ToString());
            _pollVKMsgThread.Abort();
        }

        public override void Send(string preparedMessage)
        {
            if (ProgramSettings.SM_SEND_IN_MILK) return;
            //Send already prepared message
            _vkAPI.Messages.Send(new MessagesSendParams
            {
                PeerId = _chatPeedId,
                Message = preparedMessage
            });
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
            string msgTemplate = " >> {0} (via {1}) - {2}\r\n{3}";
            return String.Format(msgTemplate,
                UnicodeFormatting('\u0331', msg.Author), //0331 or 035f
                msg.FromService.GetServiceName(),
                msg.Time.ToString(),
                msg.Text
            );
        }

        private string UnicodeFormatting(char unicodeChar, string typicalStr)
        {
            List<char> newStr = new List<char>();
            foreach (char oldSymb in typicalStr)
            {
                newStr.Add(oldSymb);
                if (oldSymb != ' ') newStr.Add(unicodeChar);
            }
            string formatted = new String(newStr.ToArray());
            return formatted.Remove(formatted.Length - 1);
        }

        public override void Repair()
        {

        }

        public override bool Start()
        {
            //GO VK GO!
            _vkAPI.Authorize(new ApiAuthParams
            {
                AccessToken = ProgramSettings.VK_ACCESS_TOKEN
            });
            //Checking is auth succesful
            Utils.WriteLine("[VK API] Using token: {0}", _vkAPI.Token);
            long? userId = _vkAPI.UserId;
            if (userId == null)
            {
                Utils.WriteLineWClr("[VK API] [ERROR] Auth in vk.com failed. Check AccessToken.", ConsoleColor.Red);
                return false;
            }
            //Get data about user profile
            var currentVkProfile = _vkAPI.Account.GetProfileInfo();
            Utils.WriteLine("[VK API] Succesful authorized as {0} {1} ({2}).",
                currentVkProfile.FirstName, currentVkProfile.LastName, currentVkProfile.ScreenName);
            //Starting thread for reciving data
            _pollVKMsgThread.Start();
            _vkAPIActive = true;
            return true;
        }

        private void PollUnreadMsgInVk()
        {
            while (_proxyService.GetIsWorking())
            {
                //Get messages from specified chat
                var msgs = _vkAPI.Messages.GetHistory(new MessagesGetHistoryParams
                {
                    PeerId = _chatPeedId
                });
                //Read only unreaded messages
                if (msgs.Unread > 0)
                {
                    Utils.WriteLine("[VK API] Arrived unreaded {0} messages.", msgs.Unread);
                    List<long> markAsReaded = new List<long>();
                    foreach (var message in msgs.Messages.Reverse())
                    {
                        //Skip wrong messages
                        if (_lastMessageId >= message.Id.Value) continue;
                        if (message.ReadState != VkNet.Enums.MessageReadState.Unreaded) continue;
                        //Save last messageId
                        _lastMessageId = message.Id.Value;
                        //Mark message as readed
                        markAsReaded.Add(message.Id.Value);
                        //Check is have text
                        if (message.Body.Length < 0) continue;
                        //Create universal message
                        string authorName = GetUserNameById(message.FromId.Value);
                        UniversalMessage arrivedMsg = new UniversalMessage(this, message, authorName);
                        //Send to another services
                        this._proxyService.SendToAnother(arrivedMsg);
                    }
                    _vkAPI.Messages.MarkAsRead(markAsReaded, _chatPeedId.ToString());
                }
                Thread.Sleep(5000);
            }
        }

        private string GetUserNameById(long id)
        {
            if(_userNameByIdCache.ContainsKey(id))
            {
                return _userNameByIdCache[id];
            }
            else
            {
                var authorId = new long[] { id };
                var authorProfile = _vkAPI.Users.Get(authorId);
                string userName = authorProfile[0].FirstName + " " + authorProfile[0].LastName;
                _userNameByIdCache.Add(id, userName);
                return userName;
            }
        }
    }
}
