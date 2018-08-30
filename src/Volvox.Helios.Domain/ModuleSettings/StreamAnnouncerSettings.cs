namespace Volvox.Helios.Domain.ModuleSettings
{
    public class StreamAnnouncerSettings : ModuleSettings
    {
        public ulong AnnouncementChannelId { get; set; }

        // Determines if stream announcement messages should be removed on stream conclusion
        public bool RemoveMessages { get; set; }
    }
}