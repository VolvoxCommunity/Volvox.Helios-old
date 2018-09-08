using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.Models
{
    public class StreamAnnouncerChannelSettingsModel
    {
        [Required]
        [DisplayName("Enable channel")]
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        [Required]
        [DisplayName("Remove messages on conclusion")]
        [DefaultValue(false)]
        public bool RemoveMessages { get; set; }
    }
}
