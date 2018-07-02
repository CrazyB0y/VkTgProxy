/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:33
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkTgProxy
{
    public interface IService
    {
        string GetServiceName();
        bool IsServiceOnline();
        string MakeUp(UniversalMessage msg);
        void Send(UniversalMessage msg);
        void Send(UniversalMessage[] msgs);
        void Send(string preparedMessage);
        bool Start();
        void Repair();
        void Exit();
    }
}
