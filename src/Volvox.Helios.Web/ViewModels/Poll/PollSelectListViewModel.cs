using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Volvox.Helios.Web.ViewModels.Poll
{
    public class PollSelectListViewModel
    {
        public string Title { get; set; }

        public ulong MessageId { get; set; }

        public ulong ChannelId { get; set; }

        // So can store both ids in selectlist
        public string ConcattedIds
        {
            get
            {
                return string.Format("{0}-{1}", MessageId, ChannelId);
            }
        }
    }
}
