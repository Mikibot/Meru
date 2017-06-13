﻿using Discord;
using IA.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA.SDK
{
    class RuntimeAudioChannel : IDiscordAudioChannel
    {
        IVoiceChannel audio;

        public RuntimeAudioChannel(IVoiceChannel a)
        {
            audio = a;
        }

        public IDiscordGuild Guild
        {
            get
            {
                return new RuntimeGuild(audio.Guild);
            }
        }

        public ulong Id
        {
            get
            {
                return audio.Id;
            }
        }

        public string Name
        {
            get
            {
                return audio.Name;
            }
        }

        public async Task<IDiscordAudioClient> ConnectAsync()
        {
            return new RuntimeAudioClient(await audio.ConnectAsync());
        }

        public async Task<IEnumerable<IDiscordUser>> GetUsersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
