using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Poll
{
    public class OptionModel
    {
        public string Option { get; set; }

        public int VoteCount { get; set; }
    }
}
