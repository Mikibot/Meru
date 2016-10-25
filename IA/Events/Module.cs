﻿using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA.Events
{
    public class Module
    {
        public ModuleInformation defaultInfo;

        Dictionary<ulong, bool> enabled = new Dictionary<ulong, bool>();

        bool isInstalled = false;

        public Module(string name, bool enabled = true)
        {
            defaultInfo = new ModuleInformation();
            defaultInfo.name = name;
            defaultInfo.enabled = enabled;
        }       
        public Module(Action<ModuleInformation> info)
        {
            info.Invoke(defaultInfo);
        }

        public string GetState()
        {
            return defaultInfo.name + ": " + "ACTIVE";
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public Task Install(Bot bot)
        {
            if(defaultInfo.messageEvent != null)
            {
                bot.Client.MessageReceived += Client_MessageReceived;
            }

            isInstalled = true;
            return Task.CompletedTask;
        }

        public Task Uninstall(Bot bot)
        {
            if (!isInstalled)
            {
                return Task.CompletedTask;
            }

            if(defaultInfo.messageEvent != null)
            {
                bot.Client.MessageReceived -= Client_MessageReceived;
            }

            isInstalled = false;
            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(IMessage message)
        {
            await defaultInfo.messageEvent(message, null);
        }
    }
}
