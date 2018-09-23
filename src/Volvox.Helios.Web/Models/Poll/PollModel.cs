using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Poll
{
    public class PollModel
    {
        public string Title { get; set; }

        public List<OptionModel> Options {get;set;}
    }
}
