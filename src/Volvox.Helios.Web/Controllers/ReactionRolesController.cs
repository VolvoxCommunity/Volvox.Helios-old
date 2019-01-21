using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volvox.Helios.Domain.Module;
using Volvox.Helios.Service.Discord.Guild;
using Volvox.Helios.Service.Discord.UserGuild;
using Volvox.Helios.Service.EntityService;
using Volvox.Helios.Web.Filters;
using Volvox.Helios.Web.ViewModels.ReactionRoles;

namespace Volvox.Helios.Web.Controllers
{
    [Authorize]
    [Route("/reactionroles/{guildId}")]
    [IsUserGuildAdminFilter]
    public class ReactionRolesController : Controller
    {
        [HttpGet("edit")]
        public async Task<IActionResult> Edit(ulong guildId,
            [FromQuery]long? reactionRoleMessageId,
            [FromServices]IEntityService<ReactionRolesMessage> messageService,
            [FromServices]IDiscordGuildService guildService)
        {
            ReactionRolesMessage message = null;

            if (reactionRoleMessageId.HasValue)
                message = (await messageService.Get(msg => msg.Id == reactionRoleMessageId.Value,
                    msg => msg.RollMappings))
                    .FirstOrDefault();

            if (message is null)
            {
                message = new ReactionRolesMessage
                {
                    GuildId = guildId,
                    RollMappings = new List<ReactionRolesEmoteMapping>()
                };
            }

            var channels = await guildService.GetChannels(guildId);
            var roles = await guildService.GetRoles(guildId);
            var emojis = await guildService.GetEmojis(guildId);

            var vm = new ReactionRolesEditViewModel
            {
                Id = message.Id,
                Message = message.Message,
                GuildId = guildId,
                ChannelId = message.ChannelId,
                MessageId = message.MessageId,
                Channels = new SelectList(channels, "Id", "Name"),
                Emojis = new SelectList(emojis, "Id", "Name"),
                Title = message.Title,
                RollMappings = message.RollMappings?.Select(rm => new ReactionRolesEmoteMappingViewModel
                {
                    Id = rm.Id,
                    RoleId = rm.RoleId,
                    EmoteId = rm.EmoteId,
                    GuildId = guildId,
                    ReactionRoleMessageId = message.Id
                }).ToList() ?? new List<ReactionRolesEmoteMappingViewModel>()
            };

            return View(vm);
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(ulong guildId,
            ReactionRolesEditViewModel vm,
            [FromServices]IEntityService<ReactionRolesMessage> messageService)
        {
            if (vm.Id != default(long))
            {
                var existingMessage = (await messageService.Get(msg => msg.Id == vm.Id,
                    msg => msg.RollMappings))
                    .FirstOrDefault();

                if (existingMessage != null)
                {
                    existingMessage.Message = vm.Message;
                    existingMessage.ChannelId = vm.ChannelId;
                    existingMessage.Title = vm.Title;

                    existingMessage.RollMappings = vm.RollMappings.Select(rm => new ReactionRolesEmoteMapping
                    {
                        RoleId = rm.RoleId,
                        EmoteId = rm.EmoteId,
                        GuildId = guildId
                    }).ToList();

                    await messageService.Update(existingMessage);
                    return RedirectToAction("ReactionRolesSettings", "Settings");
                }
            }

            var message = new ReactionRolesMessage
            {
                Message = vm.Message,
                GuildId = guildId,
                ChannelId = vm.ChannelId,
                Title = vm.Title
            };

            if (vm.RollMappings != null)
            {
                message.RollMappings = vm.RollMappings.Select(rm => new ReactionRolesEmoteMapping
                {
                    RoleId = rm.RoleId,
                    EmoteId = rm.EmoteId,
                    GuildId = guildId
                }).ToList();
            }

            await messageService.Create(message);
            return RedirectToAction("ReactionRolesSettings", "Settings");
        }
    }
}