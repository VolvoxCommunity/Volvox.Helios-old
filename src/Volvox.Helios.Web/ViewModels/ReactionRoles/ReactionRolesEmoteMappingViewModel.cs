namespace Volvox.Helios.Web.ViewModels.ReactionRoles
{
    public class ReactionRolesEmoteMappingViewModel
    {
        public long Id { get; set; }
        public long ReactionRoleMessageId { get; set; }
        public ulong GuildId { get; set; }
        public ulong EmoteId { get; set; }
        public ulong RoleId { get; set; }
    }
}