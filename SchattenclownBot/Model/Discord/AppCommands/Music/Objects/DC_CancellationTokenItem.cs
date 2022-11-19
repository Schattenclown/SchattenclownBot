using System.Threading;
using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

internal class DC_CancellationTokenItem
{
    internal DiscordGuild DiscordGuild { get; set; }
    internal CancellationTokenSource CancellationTokenSource { get; set; }

    internal DC_CancellationTokenItem(DiscordGuild discordGuild, CancellationTokenSource cancellationTokenSource)
    {
        DiscordGuild = discordGuild;
        CancellationTokenSource = cancellationTokenSource;
    }

    internal DC_CancellationTokenItem()
    {
    }
}