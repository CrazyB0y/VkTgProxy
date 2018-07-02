/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:31
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
        public UniversalMessage(IService fromVkSrv, VkNet.Model.Message vkMessage, string authorName)
            : this(fromVkSrv, authorName, vkMessage.Body, vkMessage.Date.Value)
        {

        }
    }
}
