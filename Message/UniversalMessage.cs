/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:28
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
        public IService FromService { get; private set; }
        public IService ToService { get; set; }
        public string Author { get; private set; }
        public string Text { get; private set; }
        public DateTime Time { get; private set; }
        public bool IsHaveAttachments { get; private set; }

        protected UniversalMessage(IService fromService, string msgAuthor, string msgText, DateTime msgDateTime)
        {
            this.FromService = fromService;
            this.Author = msgAuthor;
            this.Time = msgDateTime;
            this.Text = msgText;
        }

        public static string MakeUpArray(IService makeUpService, UniversalMessage[] msgs)
        {
            StringBuilder sb = new StringBuilder();
            foreach(UniversalMessage msg in msgs)
            {
                sb.AppendLine(makeUpService.MakeUp(msg));
            }
            return sb.ToString();
        }
    }
}
