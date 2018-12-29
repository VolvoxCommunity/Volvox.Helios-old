using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.ModerationModule.PunishmentService;
using Volvox.Helios.Core.Modules.ModerationModule.UserWarningsService;
using Volvox.Helios.Core.Modules.ModerationModule.WarningService;
using Volvox.Helios.Core.Services.MessageService;
using Volvox.Helios.Domain.Module.ModerationModule;
using Volvox.Helios.Domain.Module.ModerationModule.Common;
using Volvox.Helios.Domain.ModuleSettings;

namespace Volvox.Helios.Core.Modules.ModerationModule.ViolationService
{
    public class ViolationService : IViolationService
    {
        private readonly IMessageService _messageService;

        private readonly IPunishmentService _punishmentService;

        private readonly IWarningService _warningService;

        private readonly IUserWarningsService _userWarningService;

        public ViolationService(IMessageService messageService, IPunishmentService punishmentService,
            IWarningService warningService, IUserWarningsService userWarningService)
        {
            _messageService = messageService;

            _punishmentService = punishmentService;

            _warningService = warningService;

            _userWarningService = userWarningService;
        }

        public async Task HandleViolation(ModerationSettings moderationSettings, SocketMessage message, WarningType warningType)
        {
            var user = message.Author as SocketGuildUser;

            await message.DeleteAsync();

            await _messageService.Post(message.Channel.Id, $"Message by <@{user.Id}> deleted\nReason: {warningType}");

            // Get user entry from db, this is where a users warnings etc are stored.
            var userData = await _userWarningService.GetUser(user.Id, user.Guild.Id, u => u.Warnings, u => u.ActivePunishments);

            await AddWarning(moderationSettings, user, userData, warningType);
            
            await ApplyPunishments(moderationSettings, message, userData, warningType);
        }

        private async Task AddWarning(ModerationSettings moderationSettings, SocketGuildUser user, UserWarnings userData, WarningType warningType)
        {
            // Add warning to database.
            var newWarning = await _warningService.AddWarning(moderationSettings, user, warningType);

            // Update cached version.
            userData.Warnings.Add(newWarning);
        }

        private async Task ApplyPunishments(ModerationSettings moderationSettings, SocketMessage message, UserWarnings userData, WarningType warningType)
        {
            var user = message.Author as SocketGuildUser;

            var punishments = GetPunishmentsToApply(moderationSettings, userData, warningType);

            await _punishmentService.ApplyPunishments(moderationSettings, message.Channel.Id, punishments, user);
        }

        private List<Punishment> GetPunishmentsToApply(ModerationSettings moderationSettings, UserWarnings userData, WarningType warningType)

        {
            // Get all warnings that haven't expired.
            var userWarnings = userData.Warnings.Where(x => x.WarningExpires > DateTimeOffset.Now);

            // Count warnings of violation type.
            var specificWarningCount = userWarnings.Count(x => x.WarningType == warningType);

            // Count total number of warnings.
            var allWarningsCount = userWarnings.Count();

            var punishments = new List<Punishment>();

            // Global punishments
            punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == WarningType.General && x.WarningThreshold == allWarningsCount));

            // Punishments for specific type. I.E. profanity violation.
            punishments.AddRange(moderationSettings.Punishments.Where(x => x.WarningType == warningType && x.WarningThreshold == specificWarningCount));

            return punishments;
        }

    }
}
