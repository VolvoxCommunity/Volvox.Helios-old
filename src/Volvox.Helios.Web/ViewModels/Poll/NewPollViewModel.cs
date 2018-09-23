using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.PollModule
{
    public class NewPollViewModel
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        public SelectList Channels { get; set; }

        [Required]
        [DisplayName("Channel")]
        public ulong ChannelId { get; set; }

        [Required]
        public string GuildId { get; set; }

        // Number of option text boxes to display in form.
        [Required]
        public int TotalOptions { get; set; }
    } 
}
