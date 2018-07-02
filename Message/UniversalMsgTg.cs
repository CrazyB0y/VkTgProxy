/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:30
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkTgProxy
{
    public partial class UniversalMessage
    {
        public UniversalMessage(IService fromTelegramSrv, Telegram.Bot.Types.Message telegramMessage)
            : this(fromTelegramSrv,
                  ((telegramMessage.From.FirstName + " " + telegramMessage.From.LastName).Trim()),
                  telegramMessage.Text,
                  telegramMessage.Date.AddHours(3))
        {

        }
    }
}
