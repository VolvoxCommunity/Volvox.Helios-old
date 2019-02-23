using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.BackgroundJobs;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.CleanChat
{
    public class CleanChatModule : Module
    {
        private readonly IJobService _jobService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModuleSettingsService<CleanChatSettings> _settingsService;

        public CleanChatModule(IDiscordSettings discordSettings, ILogger<IModule> logger, IConfiguration config,
            IModuleSettingsService<CleanChatSettings> settingsService, IJobService jobService,
            IServiceScopeFactory scopeFactory) : base(discordSettings, logger, config)
        {
            _settingsService = settingsService;
            _jobService = jobService;
            _scopeFactory = scopeFactory;
        }

        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var settings = await _settingsService.GetSettingsByGuild(guildId);

            return settings != null && settings.Enabled;
        }

        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += MessageReceived;

            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Channel is ITextChannel textChannel && !message.Author.IsBot)
            {
                var settings = await _settingsService.GetSettingsByGuild(textChannel.GuildId, x => x.Channels);

                if (await IsEnabledForGuild(textChannel.GuildId) &&
                    settings.Channels.Any(c => c.Id == message.Channel.Id))
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var cleanChatJob = scope.ServiceProvider.GetRequiredService<CleanChatJob>();

                        _jobService.ScheduleJob(() =>
                                cleanChatJob.DeleteMessage(textChannel.GuildId, textChannel.Id, message.Id),
                            TimeSpan.FromMinutes(settings.MessageDuration));
                    }
            }
        }
    }
}