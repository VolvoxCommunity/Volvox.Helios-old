namespace Volvox.Helios.Domain.Discord
{
    public class Channel
    {
        public ulong Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public ulong GuildId { get; set; }
    }
}