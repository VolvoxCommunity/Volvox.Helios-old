using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.DiscordFacing;

namespace Volvox.Helios.Core.Modules.Common
{
    public class StringTrigger : ITrigger
    {
        private readonly string prefix;

        public StringTrigger(string prefix)
        {
            this.prefix = prefix;
        }

        public bool Valid(DiscordFacingContext context)
        {
            if (context.Message.Content.StartsWith(prefix))
            {
                return true;
            }

        return false;        }
    }
}