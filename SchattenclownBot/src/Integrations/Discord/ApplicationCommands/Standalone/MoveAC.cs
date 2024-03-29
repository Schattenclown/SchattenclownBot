﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands.Standalone
{
    public class MoveAC : ApplicationCommandsModule
    {
        [SlashCommand("Move", "MassMove the whole channel your in to a different one!")]
        public async Task MoveAsync(InteractionContext interactionContext, [Option("Channel", "#..."), ChannelTypes(ChannelType.Voice)] DiscordChannel discordTargetChannel)
        {
            List<DiscordRole> discordPermissions = interactionContext.Member.Roles.ToList();
            bool rightToMove = false;

            foreach (DiscordRole dummy in discordPermissions.Where(discordRoleItem => discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers)))
            {
                rightToMove = true;
            }

            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (!rightToMove)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You don´t have Permission!"));
            }
            else
            {
                // ReSharper disable HeuristicUnreachableCode
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (interactionContext.Member.VoiceState.Channel != null)
                {
                    DiscordChannel source = interactionContext.Member.VoiceState.Channel;

                    IReadOnlyList<DiscordMember> members = source.Users;
                    foreach (DiscordMember member in members)
                    {
                        await member.PlaceInAsync(discordTargetChannel);
                    }

                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
                }
                else
                {
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("U are not connected!"));
                }
                // ReSharper restore HeuristicUnreachableCode
            }
        }
    }
}