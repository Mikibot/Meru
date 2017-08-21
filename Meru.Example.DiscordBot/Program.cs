﻿using System;
using System.Threading.Tasks;
using Meru.Commands;
using Meru.Common;
using Meru.Providers.Discord;

namespace Meru.Example.DiscordBot
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Bot bot = new Bot();

            bot.AddProvider(
                new DiscordBotProvider(
                    new DiscordProviderConfigurations()
                    {
                        Token = "MzQ5MjA1MjEyMTU5ODAzMzky.DHyGNQ.cns1SIH2yV6OxZhWFGiPq8blTPk",
                        ShardCount = 1
                    }));

            bot.AddPlugin(
                new CommandsPlugin(bot, new CommandProcessorConfiguration()
                {
                   AutoSearchForCommands  = true,
                   DefaultPrefix = ">"
                }));

            await bot.StartAsync();
            await Task.Delay(-1);
        }
    }
}