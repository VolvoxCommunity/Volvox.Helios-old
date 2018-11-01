using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Bot.Connector;
using Volvox.Helios.Core.Bot.Reliability;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Bot
{
    /// <summary>
    ///     Discord bot.
    /// </summary>
    public class Bot : IBot
    {
        private const long VolvoxGuildId = 468467000344313866;
        private const long VolvoxGuildLogsChannelId = 507373438051287050;

        /// <summary>
        ///     Discord bot.
        /// </summary>
        /// <param name="modules">List of modules for the bot.</param>
        /// <param name="settings">Settings used to connect to Discord.</param>
        /// <param name="logger">Application logger.</param>
        public Bot(IList<IModule> modules, IDiscordSettings settings, ILogger<Bot> logger, DiscordSocketClient client)
        {
            Modules = modules;
            Logger = logger;
            Client = client;

            Client.Log += Log;

            // Log when the bot is disconnected.
            Client.Disconnected += exception =>
            {
                Logger.LogCritical("Bot has been disconnected!");

                return Task.CompletedTask;
            };

            // Set bot game.
            Client.Ready += () =>
            {
                Task.Run(async () =>
                {
                    for (;;)
                    {
                        var memberCount = Client.Guilds.Sum(guild => guild.MemberCount);

                        await Client.SetGameAsync($"volvox.tech | with {Client.Guilds.Count} servers & {memberCount} members");
                        await Task.Delay(TimeSpan.FromMinutes(15));
                    }
                });

                return Task.CompletedTask;
            };

            // Announce to Volvox when the bot joins a guild.
            Client.JoinedGuild += async guild =>
            {
                await Client.GetGuild(VolvoxGuildId).GetTextChannel(VolvoxGuildLogsChannelId)
                    .SendMessageAsync($"Joined Guild: {guild.Name} [{guild.MemberCount} Members]");
            };

            // Announce to Volvox when the bot leaves a guild.
            Client.LeftGuild += async guild =>
            {
                await Client.GetGuild(VolvoxGuildId).GetTextChannel(VolvoxGuildLogsChannelId)
                    .SendMessageAsync($"Left Guild: {guild.Name} [{guild.MemberCount} Members]");
            };

            // Add reliability service.
            _ = new ReliabilityService(Client, logger);

            Connector = new BotConnector(settings, Client);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize all of modules available to the bot.
        /// </summary>
        public async Task InitModules()
        {
            foreach (var module in Modules)
            {
                Logger.LogInformation($"Initializing {module.GetType().Name}");
                await module.Init(Client);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Start the bot.
        /// </summary>
        public async Task Start()
        {
            await InitModules();

            await Connector.Connect();

            await Task.Delay(Timeout.Infinite);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Stop the bot.
        /// </summary>
        public async Task Stop()
        {
            Logger.LogInformation("Disconnecting bot naturally");

            await Connector.Disconnect();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get a list of the guilds the bot is in.
        /// </summary>
        /// <returns>List of the guilds the bot is in.</returns>
        public IReadOnlyCollection<SocketGuild> GetGuilds()
        {
            return Client.Guilds;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns true if the specified guild is in the bot and false otherwise.
        /// </summary>
        /// <returns>Returns true if the specified guild is in the bot and false otherwise.</returns>
        public bool IsBotInGuild(ulong guildId)
        {
            return GetGuilds().Any(g => g.Id == guildId);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get the bots role in the hierarchy of the specified guild.
        /// </summary>
        /// <param name="guildId">Id of the guild to get the hierarchy from.</param>
        /// <returns>Bots role position.</returns>
        public int GetBotRoleHierarchy(ulong guildId)
        {
            var hierarchy = GetGuilds()?.FirstOrDefault(g => g.Id == guildId)?.CurrentUser.Hierarchy;

            return hierarchy ?? 0;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Log an event.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Logger.LogCritical(message.Message);
                    break;
                case LogSeverity.Error:
                    Logger.LogError(message.Message);
                    break;
                case LogSeverity.Warning:
                    Logger.LogWarning(message.Message);
                    break;
                case LogSeverity.Info:
                    Logger.LogInformation(message.Message);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Logger.LogTrace(message.Message);
                    break;
                default:
                    Logger.LogInformation(message.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Client for the bot.
        /// </summary>
        public DiscordSocketClient Client { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Connector that the bot uses to connect to Discord.
        /// </summary>
        public IBotConnector Connector { get; }

        /// <inheritdoc />
        /// <summary>
        ///     List of modules for the bot.
        /// </summary>
        public IList<IModule> Modules { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Application logger.
        /// </summary>
        public ILogger<Bot> Logger { get; }
    }
}