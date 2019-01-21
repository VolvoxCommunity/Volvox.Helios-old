using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.ReactionRoles
{
    public class ReactionRolesEditViewModel
    {
        public long Id { get; set; }
        public bool IsEdit => Id != default(long);
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong? MessageId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public SelectList Channels { get; set; }
        public SelectList Emojis { get; set; }
        public List<ReactionRolesEmoteMappingViewModel> RollMappings { get; set; }
    }
}