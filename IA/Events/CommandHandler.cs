﻿using IA.SDK;
using IA.SDK.Events;
using IA.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA.Events
{
    public class CommandHandlerBuilder
    {
        CommandHandler commandHandler = null;

        public CommandHandlerBuilder()
        {
            commandHandler = new CommandHandler(Bot.instance.Events);
        }
        public CommandHandlerBuilder(EventSystem eventSystem)
        {
            commandHandler = new CommandHandler(eventSystem);
        }

        public CommandHandlerBuilder AddCommand(ICommandEvent cmd)
        {
            commandHandler.AddCommand(cmd);
            return this;
        }

        public CommandHandlerBuilder AddModule(IModule module)
        {
            commandHandler.AddModule(module);
            return this;
        }

        public CommandHandlerBuilder SetOwner(ulong owner)
        {
            commandHandler.IsPrivate = true;
            commandHandler.Owner = owner;
            return this;
        }

        public CommandHandlerBuilder DisposeInSeconds(int seconds)
        {
            commandHandler.ShouldBeDisposed = true;
            commandHandler.timeDisposed = DateTime.Now.AddSeconds(seconds);
            return this;
        }

        public CommandHandlerBuilder AddPrefix(string value)
        {
            if (!commandHandler.Prefixes.ContainsKey(value))
            {
                commandHandler.Prefixes.Add(value, new PrefixInstance(value, false, false));
            }
            return this;
        }

        public CommandHandler Build()
        {
            return commandHandler;
        }
    }

    public class CommandHandler : ICommandHandler
    {
        public bool IsPrivate { get; set; } = false;
        public bool ShouldBeDisposed { get; set; } = false;

        public ulong Owner { get; set; } = 0;

        public DateTime TimeCreated = DateTime.Now;
        internal DateTime timeDisposed;

        internal EventSystem eventSystem;

        public Dictionary<string, PrefixInstance> Prefixes = new Dictionary<string, PrefixInstance>();

        internal Dictionary<string, string> aliases = new Dictionary<string, string>();

        public Dictionary<string, IModule> Modules = new Dictionary<string, IModule>();
        public Dictionary<string, ICommandEvent> Commands = new Dictionary<string, ICommandEvent>();

        public CommandHandler(EventSystem eventSystem)
        {
            this.eventSystem = eventSystem;
        }

        public bool ShouldDispose()
        {
            return (DateTime.Now > timeDisposed);
        }

        public async Task CheckAsync(IDiscordMessage msg)
        {
            if (IsPrivate)
            {
                if (msg.Author.Id == Owner)
                {
                    foreach (PrefixInstance prefix in Prefixes.Values)
                    {
                        if (await TryRunCommandAsync(msg, prefix))
                        {
                            break;
                        }
                    }
                }
                return;
            }

            foreach (PrefixInstance prefix in Prefixes.Values)
            {
                if (await TryRunCommandAsync(msg, prefix))
                {
                    break;
                }
            }
        }

        public void AddCommand(ICommandEvent cmd)
        {
            foreach (string a in cmd.Aliases)
            {
                aliases.Add(a, cmd.Name.ToLower());
            }
            Commands.Add(cmd.Name.ToLower(), cmd);
        }

        public void AddModule(IModule module)
        {
            foreach (ICommandEvent c in module.Events)
            {
                AddCommand(c);
            }
            Modules.Add(module.Name.ToLower(), module);
        }

        public async Task<bool> TryRunCommandAsync(IDiscordMessage msg, PrefixInstance prefix)
        {
            string identifier = await prefix.GetForGuildAsync(msg.Guild.Id);
            string message = msg.Content.ToLower();

            if (msg.Content.StartsWith(identifier))
            {
                string command = message
                    .Substring(identifier.Length)
                    .Split(' ')
                    .First();

                command = (aliases.ContainsKey(command)) ? aliases[command] : command;

                ICommandEvent eventInstance = GetCommandEvent(command);

                if (eventInstance == null)
                {
                    return false;
                }

                if (GetUserAccessibility(msg) >= eventInstance.Accessibility)
                {
                    if (await eventInstance.IsEnabled(msg.Channel.Id) || prefix.ForceCommandExecution && GetUserAccessibility(msg) >= EventAccessibility.DEVELOPERONLY)
                    {
                        Task.Run(() => eventInstance.Check(msg, this, identifier));
                        return true;
                    }
                }
                else
                {
                    await eventSystem.OnCommandDone(msg, eventInstance, false);
                }
            }
            return false;
        }

        public EventAccessibility GetUserAccessibility(IDiscordMessage e)
        {
            if (e.Channel == null) return EventAccessibility.PUBLIC;

            if (eventSystem.Developers.Contains(e.Author.Id)) return EventAccessibility.DEVELOPERONLY;
            if (e.Author.HasPermissions(e.Channel, DiscordGuildPermission.ManageRoles)) return EventAccessibility.ADMINONLY;
            return EventAccessibility.PUBLIC;
        }
        public ICommandEvent GetCommandEvent(string value)
        {
            if(Commands.ContainsKey(value))
            {
                return Commands[value];
            }
            return null;
        }

        public void RequestDispose()
        {
            if (Owner != 0)
            {
                eventSystem.DisposePrivateCommandHandler(Owner);
                return;
            }
            else
            {
                if(eventSystem.CommandHandler == this)
                {
                    Log.Warning("you just asked to dispose the standard command handler??");
                }
                else
                {
                    eventSystem.DisposeCommandHandler(this);
                }
            }
        }
    }
}