using System.Threading.Tasks;
using Volvox.Helios.Core.Modules.DiscordFacing;

namespace Volvox.Helios.Core.Modules.Common
{
    public class TriggerableModuleDecorator : IModule
    {
        private readonly IModule module;

        public TriggerableModuleDecorator(ITrigger trigger, IModule module)
        {
            this.Trigger = trigger;
            this.module = module;
        }

        public ITrigger Trigger { get; }

        public void Enable()
        {
            module.Enable();
        }

        public void Disable()
        {
            this.module.Disable();
        }

        public async Task InvokeAsync(DiscordFacingContext discordFacingContext)
        {
            if (this.Trigger.Valid(discordFacingContext))
            {
                await this.module.InvokeAsync(discordFacingContext);
            }
        }
    }
}