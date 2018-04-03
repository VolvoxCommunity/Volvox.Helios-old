using System;
using System.Threading;
using System.Threading.Tasks;
using Volvox.Helios.Core.Bot;
using Volvox.Helios.Web.HostedServices.Common;

namespace Volvox.Helios.Web.HostedServices.Bot
{
    public class BotHostedService : BackgroundService
    {
        private readonly IBot _bot;

        public BotHostedService(IBot bot)
        {
            _bot = bot;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(async () => await _bot.Stop());
            
            await _bot.Start();
        }
    }
}