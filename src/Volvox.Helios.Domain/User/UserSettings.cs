using System.ComponentModel.DataAnnotations;

namespace Volvox.Helios.Domain.User
{
    public class UserSettings
    {
        [Key]
        public ulong UserId { get; set; }

        public Themes Theme { get; set; }
    }

    public enum Themes
    {
        Light,
        Dark
    }
}