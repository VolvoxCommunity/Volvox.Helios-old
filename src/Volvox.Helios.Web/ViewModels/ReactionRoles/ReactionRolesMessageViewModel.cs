using System.Collections.Generic;

namespace Volvox.Helios.Web.ViewModels.ReactionRoles
{
    public class ReactionRolesMessageViewModel
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public ulong? MessageId { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }

        public List<ReactionRolesEmoteMappingViewModel> RollMappings { get; set; }
    }
}