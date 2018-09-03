using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Modules.DiscordFacing.Framework;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.DiscordFacing
{
    public class DiscordFacingManager : Module
    {
        private DiscordSocketClient Client { get; set; }

        private IList<ICommand> Modules { get; }

        public DiscordFacingManager(ILogger<IModule> logger, IList<ICommand> modules, IDiscordSettings settings, IConfiguration config) : base(settings, logger, config)
        {
            Modules = modules;
        }
        
        public override Task Init(DiscordSocketClient client)
        {
            Client = client;
            Client.MessageReceived += HandleCommandAsync;
            
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage emitted)
        {
            if (!(emitted is SocketUserMessage message) || emitted.Channel is IDMChannel || !message.Content.StartsWith("h-") /*placeholder prefix */) return;
            var context = new DiscordFacingContext(message, Client, "h-");

            foreach (var module in Modules)
            {
                try
                {
                    await module.TryTrigger(context);
                }
                catch (TriggerFailException e)
                {
                    continue;
                }
                catch (Exception e)
                {
                    Logger.LogError(e.ToString());
                    var embed = new EmbedBuilder()
                        .WithColor(new Color(255, 0, 0))
                        .WithTitle("Well, this is embarrassing...")
                        .WithDescription(
                            $"Something went ***very*** wrong trying to run that command. \n```\n{e.Message}\n```")
                        .Build();
                    await context.Channel.SendMessageAsync("", false, embed);
                }
            }
        }
    }
}