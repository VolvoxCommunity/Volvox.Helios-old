using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volvox.Helios.Core.Modules.Common;
using Volvox.Helios.Core.Utilities;
using Volvox.Helios.Domain.ModuleSettings;
using Volvox.Helios.Service.ModuleSettings;

namespace Volvox.Helios.Core.Modules.Leveling
{
    public class LevelingModule : Module
    {
        private readonly IModuleSettingsService<LevelingSettings> _levelingSettings;

        public LevelingModule(IDiscordSettings discordSettings,
            ILogger<IModule> logger,
            IConfiguration config,
            IModuleSettingsService<LevelingSettings> levelingSettings)
            : base(discordSettings, logger, config)
        {
            _levelingSettings = levelingSettings;
        }

        public override async Task<bool> IsEnabledForGuild(ulong guildId)
        {
            var guildSettings = await _levelingSettings.GetSettingsByGuild(guildId);
            return guildSettings?.Enabled ?? false;
        }

        public override Task Init(DiscordSocketClient client)
        {
            client.MessageReceived += OnMessageReceived;

            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (!( await ShouldProceedWithMessage(message) ))
                return;

            // Cool things go here
        }

        private async Task<bool> ShouldProceedWithMessage(SocketMessage message)
        {
            if (message.Source != MessageSource.User)
                return false;

            if (message.Channel is SocketGuildChannel guildChannel)
            {
                var isEnabled = await IsEnabledForGuild(guildChannel.Guild.Id);
                if (!isEnabled)
                    return false;
            }

            return true;
        }
    }
}