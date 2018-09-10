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
    /// <summary>
    ///     Manager for Discord message commands.
    /// </summary>
    public class CommandManager : Module
    {
        /// <summary>
        ///     Manager for Discord message commands.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="modules">List of Command modules to be used by the manager.</param>
        /// <param name="settings">Discord settings.</param>
        /// <param name="config">Configuration.</param>
        public CommandManager(ILogger<IModule> logger, IList<ICommand> modules, IDiscordSettings settings,
            IConfiguration config) : base(settings, logger, config)
        {
            Modules = modules;
        }

        private DiscordSocketClient Client { get; set; }

        private IList<ICommand> Modules { get; }

        /// <summary>
        ///     Initialize the manager by binding to the MessageReceived event.
        /// </summary>
        /// <param name="client">Client to subscribe to.</param>
        public override Task Init(DiscordSocketClient client)
        {
            Client = client;
            Client.MessageReceived += HandleCommand;

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handle the MessageReceived event.
        /// </summary>
        /// <param name="emitted">Message that was sent.</param>
        private async Task HandleCommand(SocketMessage emitted)
        {
            if (!( emitted is SocketUserMessage message ) || emitted.Channel is IDMChannel ||
                !message.Content.StartsWith("h-") || message.Author.IsBot) return;

            var context = new CommandContext(message, Client, "h-");

            foreach (var module in Modules)
                try
                {
                    await module.TryTrigger(context);
                }
                catch (TriggerFailException e)
                {
                    Logger.LogDebug($"Command Manager: Trigger fail exception occurred {e.Message}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Command Manager: Error occurred {e.Message}");

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