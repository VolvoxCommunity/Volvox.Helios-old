using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Volvox.Helios.Web.ViewModels.Poll
{
    public class ViewPollsViewModel
    {
        [Required]
        [DisplayName("Polls")]
        public SelectList Polls { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Options")]
        public List<OptionModel> Options { get; set; }
    }
}
