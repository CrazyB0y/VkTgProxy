/*
 * Written by Cr1zyB0y (aka Anton Zhuchkov) 
 * https://t.me/cr1zyb0y
 * 24.04.2018 - 22:22
 */
using System;
using System.Collections.Generic;

namespace VkTgProxy
{
	public class ServiceManager
	{
        private readonly List<IService> _services;
        private bool _isStarted = false;

        public ServiceManager()
		{
            _services = new List<IService>
            {
                new VkService(this),
                new TgService(this)
            };
        }

		public void Start()
		{
			if(_isStarted) return;
			_isStarted = true; //Block all others starts
            //Start proxy services
            foreach(var service in _services)
            {
                if (!StartService(service))
                {
                    this.Exit();
                    Program.TerminateProxy(new Exception("Service " + service.GetServiceName() + 
                        " starting fail..."));
                }
            }
        }

        private bool StartService(IService service)
        {
            //Try start services 3 times
            for(int _startAttempts = 0; _startAttempts < 4; _startAttempts++)
            {
                Utils.WriteLineWClr("[ServiceManager] Starting {0} service... ({1})",
                    ConsoleColor.Gray, service.GetServiceName(), _startAttempts);
                if (service.Start())
                {
                    //Service is succesful started
                    Utils.WriteLineWClr("[ServiceManager] Service {0} started!",
                        ConsoleColor.DarkGreen, service.GetServiceName());
                    return true;
                }
                else
                {
                    //If last trying return false
                    if (_startAttempts == 3) return false;
                    //Trying to repair service and start it again
                    Utils.WriteLineWClr("[ServiceManager] Repair {0} service...",
                        ConsoleColor.DarkYellow, service.GetServiceName());
                    service.Repair();
                    Utils.WriteLineWClr("[ServiceManager] Repair {0} service complete!",
                        ConsoleColor.DarkGreen, service.GetServiceName());
                }
            }
            return false;
        }

        public void SendToAnother(UniversalMessage msg)
        {
            //Take service from message
            IService serviceFrom = msg.FromService;
            //Taking all service one by one
            foreach (var serviceTo in _services)
            {
                if(serviceFrom != serviceTo)
                {
                    Utils.WriteLineWClr("[ServiceManager] [From {0} to {1}] {2} <{3}> {4}",
                        ConsoleColor.DarkGray, serviceFrom.GetServiceName(),
                        serviceTo.GetServiceName(), msg.Time, msg.Author, msg.Text);
                    msg.ToService = serviceTo;
                    ((QueryService)serviceTo).SendToQueue(msg); //TODO: replace KOSTYLU
                }
            }
        }

        public bool GetIsWorking()
		{
			return _isStarted;
		}

		public void Exit()
		{
			_isStarted = false;
            //Stop proxy services
            foreach (var service in _services)
            {
                Utils.WriteLineWClr("[ServiceManager] Stopping {0} service...",
                    ConsoleColor.Gray, service.GetServiceName());
                service.Exit();
                Utils.WriteLineWClr("[ServiceManager] Service {0} stopped!",
                    ConsoleColor.Gray, service.GetServiceName());
            }
        }
	}
}
