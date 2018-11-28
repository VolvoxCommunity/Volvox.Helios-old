using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Volvox.Helios.Core.Modules.Command.Framework;
using Volvox.Helios.Core.Utilities;

namespace Volvox.Helios.Core.Modules.Command.Commands
{
    public class AboutCommand : ICommand
    {
        public async Task TryTrigger(CommandContext context)
        {
            if (!context.Message.Content.StartsWith($"{context.GivenPrefix}about")) throw new TriggerFailException();
            await Execute(context);
        }

        public async Task Execute(CommandContext context)
        {
            if (!( context.Channel is SocketGuildChannel channel )) return;

            // Your args will always start at 1; the command itself is argument zero.
            var flag = context.Message.Content.Split(' ')[1];

            var embed = new EmbedBuilder()
                .WithColor(EmbedColors.LogoColor)
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Helios"));

            var version = Assembly.GetEntryAssembly().GetName().Version;

            switch (flag)
            {
                case "-g":
                case "-guild":
                    embed.WithTitle($"Welcome to {channel.Guild.Name}!")
                        .WithImageUrl(channel.Guild.IconUrl)
                        .AddField("Owner", $"{channel.Guild.Owner.Username} ({channel.Guild.Owner.Mention})")
                        .AddField("User Count", channel.Guild.MemberCount)
                        .AddField("Voice Region", channel.Guild.VoiceRegionId, true);
                    break;
                default:
                    embed.WithTitle("So you want to hear my life story, ey?")
                        .AddField("Bot Version", $"v{version.Major}.{version.Minor}.{version.Build}")
                        .AddField("Bloat Factor", $"{GC.GetTotalMemory(false) / 1048576}MB", true)
                        .AddField("Server Count", context.Client.Guilds.Count, true)
                        .WithFooter(
                            new EmbedFooterBuilder().WithText(
                                "psst, you can use -g with this command to get server information!"));
                    break;
            }

            await context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}