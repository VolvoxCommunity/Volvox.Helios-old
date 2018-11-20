namespace Volvox.Helios.Domain.Discord
{
    public class Guild
    {
        public Guild()
        {
            Details = new GuildDetails();
        }

        public ulong Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string ImageUrl { get; set; }

        /// <summary>
        ///     Guild specific details. NOT populated from Discord API.
        /// </summary>
        public GuildDetails Details { get; }
    }
}