/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:37
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VkTgProxy
{
    public abstract class QueryService : IService
    {
        public abstract string GetServiceName();
        public abstract bool Start();
        public abstract bool IsServiceOnline();
        public abstract string MakeUp(UniversalMessage msg);
        public abstract void Repair();
        public abstract void Exit();
        public abstract void Send(UniversalMessage msg);
        public abstract void Send(UniversalMessage[] msgs);
        public abstract void Send(string preparedMessage);

        private readonly Queue<UniversalMessage> _messagesQueue;
        private readonly Timer _sendTimer;

        private const int _rndLeftPos = 3500;
        private const int _rndRightPos = 5500;
        private const int _rndCenterPos = (_rndLeftPos + _rndRightPos) / 2;

        protected QueryService()
        {
            //Create queue
            _messagesQueue = new Queue<UniversalMessage>();
            //Making timers and others
            _sendTimer = new Timer(QueueSendArrived,null, _rndCenterPos, _rndCenterPos);
        }

        private void QueueSendArrived(object state)
        {
            //Check is we have messages
            if (_messagesQueue.Count > 0 && this.IsServiceOnline())
            {
                //Take messages without attachments
                List<UniversalMessage> msgs = new List<UniversalMessage>();
                bool isHaveAttachments = !_messagesQueue.Peek().IsHaveAttachments;
                while (!isHaveAttachments)
                {
                    isHaveAttachments = !_messagesQueue.Peek().IsHaveAttachments;
                    if (!isHaveAttachments)
                    {
                        msgs.Add(_messagesQueue.Dequeue());
                    }
                }
                //Check is we have many text messages, or only one
                if (msgs.Count > 1)
                {
                    //Send all text messages
                    this.Send(msgs.ToArray());
                }
                else
                {
                    //Send only one message
                    this.Send(_messagesQueue.Dequeue());
                }
            }
            //Change timer value
            _sendTimer.Change(0,new Random().Next(_rndLeftPos, _rndRightPos));
        }

        public void SendToQueue(UniversalMessage msg)
        {
            _messagesQueue.Enqueue(msg);
        }
    }
}
