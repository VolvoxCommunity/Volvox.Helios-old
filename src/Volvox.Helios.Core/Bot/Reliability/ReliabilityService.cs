using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Volvox.Helios.Core.Bot.Reliability
{
    public class ReliabilityService
    {
        private readonly DiscordSocketClient _discord;
        private readonly ILogger _logger;
        private CancellationTokenSource _cts;

        public ReliabilityService(DiscordSocketClient discord, ILogger logger)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _logger = logger;

            _discord.Connected += ConnectedAsync;
            _discord.Disconnected += DisconnectedAsync;
        }

        private Task ConnectedAsync()
        {
            _logger.LogInformation("Client reconnected, resetting cancel tokens...");

            // Bot connected so cancel the task.
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        public Task DisconnectedAsync(Exception _e)
        {
            _logger.LogError($"Client disconnected with error: {_e.Message}");

            Task.Delay(TimeSpan.FromSeconds(15), _cts.Token).ContinueWith(_ => { ResetClient(); });

            return Task.CompletedTask;
        }

        private void ResetClient()
        {
            // Client reconnected, no need to reset
            if (_discord.ConnectionState == ConnectionState.Connected) return;

            _logger.LogInformation("Attempting to reset the client");

            var connect = _discord.StartAsync();

            if (connect.IsCompletedSuccessfully)
                _logger.LogInformation("Client reset successfully!");
            else
                _logger.LogCritical("Client did not reset successfully");
        }
    }
}