namespace Volvox.Helios.Domain.ModuleSettings
{
    public class UserEventSettings : ModuleSettings
    {
        public bool EnableUserLeftEvent { get; set; }

        public ulong UserLeftEventChannelId { get; set; }
    }
}