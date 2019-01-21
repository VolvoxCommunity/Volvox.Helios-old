namespace Volvox.Helios.Domain.Discord
{
    public class Emoji
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public Role[] Roles { get; set; }
        public User User { get; set; }
        public bool RequiresColons { get; set; }
        public bool Managed { get; set; }
        public bool Animated { get; set; }
    }
}