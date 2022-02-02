using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.Interaction
{
    /// <summary>
    /// The slash commands.
    /// </summary>
    internal class Slash : ApplicationCommandsModule
    {
        /// <summary>
        /// Send the help of this bot.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        [SlashCommand("help", "Schattenclown Help", true)]
        public static async Task HelpAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = "Help",
                Description = "This is the command help for the Schattenclown Bot",
                Color = DiscordColor.Purple
            };
            discordEmbedBuilder.AddField("/level", "Shows your level!");
            discordEmbedBuilder.AddField("/leaderboard", "Shows the levelsystem!");
            discordEmbedBuilder.AddField("/timer", "Set´s a timer!");
            discordEmbedBuilder.AddField("/mytimers", "Look up your timers!");
            discordEmbedBuilder.AddField("/alarmclock", "Set an alarm for a spesific time!");
            discordEmbedBuilder.AddField("/myalarms", "Look up your alarms!");
            discordEmbedBuilder.AddField("/invite", "Send´s an invite link!");
            discordEmbedBuilder.WithAuthor("Schattenclown help");
            discordEmbedBuilder.WithFooter("(✿◠‿◠) thanks for using me");
            discordEmbedBuilder.WithTimestamp(DateTime.Now);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("alarmclock", "Set an alarm for a spesific time!", true)]
        public static async Task AlarmClock(InteractionContext interactionContext, [Option("hourofday", "0-23")] double hour, [Option("minuteofday", "0-59")] double minute)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating alarm..."));

            if (!TimeFormat(hour, minute))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            DateTime alarm = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), 0);

            if (alarm < DateTime.Now)
                alarm = alarm.AddDays(1);

            ScAlarmClock scAlarmClock = new ScAlarmClock
            {
                ChannelId = interactionContext.Channel.Id,
                MemberId = interactionContext.Member.Id,
                NotificationTime = alarm
            };
            ScAlarmClock.Add(scAlarmClock);
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Alarm set for {scAlarmClock.NotificationTime}!"));
        }
        [SlashCommand("myalarms", "Look up your alarms!", true)]
        public static async Task AlarmClockLookup(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<ScAlarmClock> lstScAlarmClocks = DB_ScAlarmClocks.ReadAll();
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Your alarms",
                Color = DiscordColor.Purple,
                Description = $"<@{interactionContext.Member.Id}>"
            };
            bool noTimers = true;
            foreach (var scAlarmClock in lstScAlarmClocks)
            {
                if (scAlarmClock.MemberId == interactionContext.Member.Id)
                {
                    noTimers = false;
                    discordEmbedBuilder.AddField($"{scAlarmClock.NotificationTime}", $"Alarm with ID {scAlarmClock.DBEntryID}");
                }
            }
            if (noTimers)
                discordEmbedBuilder.Title = "No alarms set!";

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("timer", "Set a timer!", true)]
        public static async Task Timer(InteractionContext interactionContext, [Option("hours", "0-23")] double hour, [Option("minutes", "0-59")] double minute)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating timer..."));

            if (!TimeFormat(hour, minute))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            ScTimer scTimer = new ScTimer
            {
                ChannelId = interactionContext.Channel.Id,
                MemberId = interactionContext.Member.Id,
                NotificationTime = dateTimeNow.AddHours(hour).AddMinutes(minute)
            };
            ScTimer.Add(scTimer);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timer set for {scTimer.NotificationTime}!"));
        }

        [SlashCommand("mytimers", "Look up your timers!", true)]
        public static async Task TimerLookup(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<ScTimer> lstScTimers = DB_ScTimers.ReadAll();
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Your timers",
                Color = DiscordColor.Purple,
                Description = $"<@{interactionContext.Member.Id}>"
            };
            bool noTimers = true;
            foreach (var scTimer in lstScTimers)
            {
                if (scTimer.MemberId == interactionContext.Member.Id)
                {
                    noTimers = false;
                    discordEmbedBuilder.AddField($"{scTimer.NotificationTime}", $"Timer with ID {scTimer.DBEntryID}");
                }
            }
            if (noTimers)
                discordEmbedBuilder.Title = "No timers set!";

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("leaderboard", "Look up the leaderboard!", true)]
        public static async Task Leaderboard(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<DcUserLevelSystem> dcUserLevelSystemList = DcUserLevelSystem.Read(interactionContext.Guild.Id);

            List<DcUserLevelSystem> dcUserLevelSystemListSorted = dcUserLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            dcUserLevelSystemListSorted.Reverse();

            int top15 = 0;

            string liststring = "```css\n" +
                                "{365/24:60}[Username]\n\n";
            foreach (var dcLevelSystem in dcUserLevelSystemListSorted)
            {
                var discordUser = await Discord.DiscordBot.Client.GetUserAsync(dcLevelSystem.MemberId, true);

                DateTime date1 = new DateTime(1969, 4, 20, 4, 20, 0);
                DateTime date2 = new DateTime(1969, 4, 20, 4, 20, 0).AddMinutes(dcLevelSystem.OnlineTicks);
                TimeSpan timeSpan = date2 - date1;

                liststring += "{" + $"{timeSpan,9:ddd\\/hh\\:mm}" + "}" + $"[{discordUser.Username}]\n";
                top15++;
                if (top15 == 20)
                    break;
            }
            liststring += "\n```";
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();
            discordEmbedBuilder.Title = "LevelSystem";
            discordEmbedBuilder.Description = liststring;
            discordEmbedBuilder.Color = DiscordColor.Purple;

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("Level", "Look up your level!", true)]
        public static async Task Level(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<DcUserLevelSystem> dcUserLevelSystemList = DcUserLevelSystem.Read(interactionContext.Guild.Id);
            List<DcUserLevelSystem> dcUserLevelSystemListSorted = dcUserLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            dcUserLevelSystemListSorted.Reverse();

            string uriString = "https://quickchart.io/chart/render/zm-483cf019-58bf-423e-bd2c-514d8f9b2ff6?data1=";

            int totalXp = 0;
            int totalLevel, modXp;
            string rank = "N/A";
            string level, xp;

            var discordUser = await Discord.DiscordBot.Client.GetUserAsync(interactionContext.Member.Id);
            string username = discordUser.Username;

            foreach (var dcUserLevelSystemItem in dcUserLevelSystemListSorted)
            {
                if (dcUserLevelSystemItem.MemberId == interactionContext.Member.Id)
                {
                    rank = (dcUserLevelSystemListSorted.IndexOf(dcUserLevelSystemItem) + 1).ToString();
                    totalXp = dcUserLevelSystemItem.OnlineTicks * 125 / 60;
                    break;
                }
            }

            if (totalXp > 0)
            {
                totalLevel = totalXp / 1000;
                modXp = totalXp % 1000;
                level = $"Level {totalLevel}";
                xp = $"{modXp}/1000xp";
            }
            else
            {
                totalLevel = 0;
                modXp = 0;
                level = $"Level {totalLevel}";
                xp = $"{modXp}/1000xp";
            }

            string xppad = xp.PadLeft(11, ' ');
            string levelpad = level.PadLeft(8, ' ');
            string temp = xppad + levelpad;
            string temppad = ("<@" + interactionContext.Member.Id + ">").PadRight(38, ' ') + temp;
            //string temppad = ("1").PadRight(38, ' ') + temp;

            //uriString += $"{level} {username} {xp,50}";
            uriString += $"{modXp}";

            Uri uri = new Uri(uriString);
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();
            discordEmbedBuilder.WithImageUrl(uri.AbsoluteUri);
            discordEmbedBuilder.WithTitle(temp);
            discordEmbedBuilder.WithDescription("<@" + interactionContext.Member.Id + ">");
            discordEmbedBuilder.WithFooter("Rank #" + rank);
            discordEmbedBuilder.Color = DiscordColor.Purple;

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        /// Checks if the given hour and minute are usable to make a datetime object out of them.
        /// Returns true if the given arguments are usable.
        /// Returns false if the hour or the minute are not usable.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <returns>A bool.</returns>
        public static bool TimeFormat(double hour, double minute)
        {
            bool hourformatisright = false;
            bool minuteformatisright = false;

            for (int i = 0; i < 24; i++)
            {
                if (hour == i)
                    hourformatisright = true;
            }
            if (!hourformatisright)
                return false;

            for (int i = 0; i < 60; i++)
            {
                if (minute == i)
                    minuteformatisright = true;
            }
            if (!minuteformatisright)
                return false;

            return true;
        }

        /// <summary>
        /// Generates an Invite link.
        /// </summary>
        /// <param name="interactionContext">The ic.</param>
        /// <returns>A Task.</returns>
        [SlashCommand("invite", "Invite ListforgeNotify", true)]
        public static async Task InviteAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bot_invite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(bot_invite.AbsoluteUri));
        }

        [ContextMenu(ApplicationCommandType.User, "Poke a user!", true)]
        public static async Task Poke(ContextMenuContext contextMenuContext)
        {
            await contextMenuContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Poke {contextMenuContext.TargetMember.DisplayName}"
            }.
            WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl);
            await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

            var rolesList = contextMenuContext.Member.Roles.ToList();
            bool rightToMove = false;

            foreach (var roleItem in rolesList)
            {
                if (roleItem.Permissions.HasPermission(Permissions.MoveMembers))
                    rightToMove = true;
            }

            const int taskDelayShort = 400;
            const int taskDelayLong = 2000;

            if (contextMenuContext.TargetMember.VoiceState != null && rightToMove)
            {
                DiscordChannel currentChannel;
                DiscordChannel tempCategory = default;
                DiscordChannel tempChannel2 = default;
                DiscordChannel tempChannel1 = default;

                try
                {
                    tempCategory = contextMenuContext.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                    tempChannel1 = contextMenuContext.Guild.CreateVoiceChannelAsync($"{contextMenuContext.TargetMember.VoiceState.User.Id}", tempCategory).Result;
                    tempChannel2 = contextMenuContext.Guild.CreateVoiceChannelAsync($"{contextMenuContext.TargetMember.VoiceState.User.Id}", tempCategory).Result;
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while creating the channels!";
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(taskDelayLong);
                }

                try
                {
                    currentChannel = contextMenuContext.TargetMember.VoiceState.Channel;

                    for (int i = 0; i < 2; i++)
                    {
                        await contextMenuContext.TargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel1);
                        await Task.Delay(taskDelayShort);
                        await contextMenuContext.TargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel2);
                        await Task.Delay(taskDelayShort);
                    }
                    await contextMenuContext.TargetMember.ModifyAsync(x => x.VoiceChannel = currentChannel);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(taskDelayLong);
                }

                try
                {
                    await tempCategory.DeleteAsync();
                    await tempChannel1.DeleteAsync();
                    await tempChannel2.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(taskDelayLong);
                }
                await contextMenuContext.DeleteResponseAsync();
            }
            else if (contextMenuContext.TargetMember.VoiceState == null)
            {
                discordEmbedBuilder.Description = "User is not connected!";
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                await Task.Delay(taskDelayLong);
                await contextMenuContext.DeleteResponseAsync();
            }
            else if (!rightToMove)
            {
                discordEmbedBuilder.Description = "Your not allowed to use that!";
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                await Task.Delay(taskDelayLong);
                await contextMenuContext.DeleteResponseAsync();
            }
        }

        /// <summary>
        /// Gets the user's avatar & banner.
        /// </summary>
        /// <param name="contextMenuContext">The contextmenu context.</param>
        [ContextMenu(ApplicationCommandType.User, "Get avatar & banner")]
        public static async Task GetUserBannerAsync(ContextMenuContext contextMenuContext)
        {
            var user = await contextMenuContext.Client.GetUserAsync(contextMenuContext.TargetUser.Id, true);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Avatar & Banner of {user.Username}",
                ImageUrl = user.BannerHash != null ? user.BannerUrl : null
            }.
            WithThumbnail(user.AvatarUrl).
            WithColor(user.BannerColor ?? DiscordColor.Purple).
            WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl).
            WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
            await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
    }
}
