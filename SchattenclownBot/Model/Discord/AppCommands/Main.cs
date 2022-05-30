using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.CommandsNext.Converters;

using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;
using SchattenclownBot.Model.Discord.ChoiceProvider;
using SchattenclownBot.Model.Discord.Main;
using DisCatSharp.EventArgs;

namespace SchattenclownBot.Model.Discord.AppCommands
{
    /// <summary>
    /// The AppCommands.
    /// </summary>
    internal class Main : ApplicationCommandsModule
    {
        /// <summary>
        /// Send the help of this bot.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        [SlashCommand("help", "Schattenclown Help")]
        public static async Task HelpAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = "Help",
                Description = "This is the command help for the Schattenclown Bot",
                Color = DiscordColor.Purple
            };
            discordEmbedBuilder.AddField(new DiscordEmbedField("/level", "Shows your level!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/leaderboard", "Shows the levelsystem!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/timer", "Set´s a timer!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/mytimers", "Look up your timers!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/alarmclock", "Set an alarm for a spesific time!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/myalarms", "Look up your alarms!"));
            discordEmbedBuilder.AddField(new DiscordEmbedField("/invite", "Send´s an invite link!"));
            discordEmbedBuilder.WithAuthor("Schattenclown help");
            discordEmbedBuilder.WithFooter("(✿◠‿◠) thanks for using me");
            discordEmbedBuilder.WithTimestamp(DateTime.Now);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("alarmclock", "Set an alarm for a spesific time!")]
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

        [SlashCommand("myalarms", "Look up your alarms!")]
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
                    discordEmbedBuilder.AddField(new DiscordEmbedField($"{scAlarmClock.NotificationTime}", $"Alarm with ID {scAlarmClock.DBEntryID}"));
                }
            }

            if (noTimers)
                discordEmbedBuilder.Title = "No alarms set!";

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("timer", "Set a timer!")]
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

        [SlashCommand("mytimers", "Look up your timers!")]
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
                    discordEmbedBuilder.AddField(new DiscordEmbedField($"{scTimer.NotificationTime}", $"Timer with ID {scTimer.DBEntryID}"));
                }
            }
            if (noTimers)
                discordEmbedBuilder.Title = "No timers set!";

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("leaderboard", "Look up the leaderboard!")]
        public static async Task Leaderboard(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<DcUserLevelSystem> dcUserLevelSystemList = DcUserLevelSystem.Read(interactionContext.Guild.Id);

            List<DcUserLevelSystem> dcUserLevelSystemListSorted = dcUserLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            dcUserLevelSystemListSorted.Reverse();

            int top15 = 0;

            string liststring = "```css\n";
            foreach (var dcLevelSystem in dcUserLevelSystemListSorted)
            {
                var discordUser = await Bot.Client.GetUserAsync(dcLevelSystem.MemberId, true);

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

        [SlashCommand("Level", "Look up your level!")]
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

            var discordUser = await Bot.Client.GetUserAsync(interactionContext.Member.Id);
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
                xp = $"{modXp}/1000xp ";
            }
            else
            {
                totalLevel = 0;
                modXp = 0;
                level = $"Level {totalLevel}";
                xp = $"{modXp}/1000xp ";
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
        [SlashCommand("invite", "Invite $chattenclown")]
        public static async Task InviteAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bot_invite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(bot_invite.AbsoluteUri));
        }

        /// <summary>
        /// Generates an Invite link.
        /// </summary>
        /// <param name="interactionContext">The ic.</param>
        /// <returns>A Task.</returns>
        [SlashCommand("Poke", "Poke user!")]
        public static async Task Poke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordMember discordMember = discordUser as DiscordMember;
            await PokeAsync(interactionContext, null, discordMember, false, 2, false);
        }

        [SlashCommand("ForcePoke", "Poke user! Forcefully!")]
        public static async Task ForcePoke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordMember discordMember = discordUser as DiscordMember;
            await PokeAsync(interactionContext, null, discordMember, false, 2, true);
        }

        [ContextMenu(ApplicationCommandType.User, "Poke user!", true)]
        public static async Task AppsPoke(ContextMenuContext contextMenuContext)
        {
            await PokeAsync(null, contextMenuContext, contextMenuContext.TargetMember, true, 2, false);
        }

        [ContextMenu(ApplicationCommandType.User, "Poke user! Forcefully!", true)]
        public static async Task AppsForcePoke(ContextMenuContext contextMenuContext)
        {
            await PokeAsync(null, contextMenuContext, contextMenuContext.TargetMember, true, 2, true);
        }

        public static async Task PokeAsync(InteractionContext interactionContext, ContextMenuContext contextMenuContext, DiscordMember discordTargetMember, bool deleteResponseAsync, int pokeAmount, bool force)
        {
            if (interactionContext != null)
                await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            else
                await contextMenuContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Poke {discordTargetMember.DisplayName}"
            };

            if (interactionContext != null)
                discordEmbedBuilder.WithFooter($"Requested by {interactionContext.Member.DisplayName}", interactionContext.Member.AvatarUrl);
            else
                discordEmbedBuilder.WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl);

            if (interactionContext != null)
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            else
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

            bool rightToMove = false;
            List<DiscordRole> discordRoleList = new();

            if (interactionContext != null)
                discordRoleList = interactionContext.Member.Roles.ToList();
            else
                discordRoleList = contextMenuContext.Member.Roles.ToList();

            foreach (DiscordRole discordRoleItem in discordRoleList)
            {
                if (discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers))
                    rightToMove = true;
            }

            bool desktopHasValue = false;
            bool webHasValue = false;
            bool mobileHasValue = false;
            bool presenceWasNull = false;

            if (discordTargetMember.Presence != null)
            {
                desktopHasValue = discordTargetMember.Presence.ClientStatus.Desktop.HasValue;
                webHasValue = discordTargetMember.Presence.ClientStatus.Web.HasValue;
                mobileHasValue = discordTargetMember.Presence.ClientStatus.Mobile.HasValue;
            }
            else
                presenceWasNull = true;

            if (discordTargetMember.VoiceState != null && rightToMove && (force || presenceWasNull || ((desktopHasValue || webHasValue) && !mobileHasValue)))
            {
                DiscordChannel currentChannel = default;
                DiscordChannel tempCategory = default;
                DiscordChannel tempChannel2 = default;
                DiscordChannel tempChannel1 = default;

                try
                {
                    DiscordEmoji discordEmoji = DiscordEmoji.FromName(Bot.Client, ":no_entry_sign:");

                    if (interactionContext != null)
                    {
                        tempCategory = interactionContext.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                        tempChannel1 = interactionContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                        tempChannel2 = interactionContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                    }
                    else
                    {
                        tempCategory = contextMenuContext.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                        tempChannel1 = contextMenuContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                        tempChannel2 = contextMenuContext.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while creating the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    currentChannel = discordTargetMember.VoiceState.Channel;

                    for (int i = 0; i < pokeAmount; i++)
                    {
                        await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel1);
                        await Task.Delay(250);
                        await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel2);
                        await Task.Delay(250);
                    }
                    await discordTargetMember.ModifyAsync(x => x.VoiceChannel = tempChannel1);
                    await Task.Delay(250);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await discordTargetMember.ModifyAsync(x => x.VoiceChannel = currentChannel);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel2.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel1.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempCategory.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }
            }
            else if (discordTargetMember.VoiceState == null)
            {
                discordEmbedBuilder.Description = "User is not connected!";
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (!rightToMove)
            {
                discordEmbedBuilder.Description = "Your not allowed to use that!";
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (mobileHasValue)
            {
                string description = "Their phone will explode STOP!\n";

                DiscordEmoji discordEmoji_white_check_mark = DiscordEmoji.FromName(Bot.Client, ":white_check_mark:");
                DiscordEmoji discordEmojiCheck_x = DiscordEmoji.FromName(Bot.Client, ":x:");

                if (discordTargetMember.Presence.ClientStatus.Desktop.HasValue)
                    description += discordEmoji_white_check_mark + " Dektop" + "\n";
                else
                    description += discordEmojiCheck_x + " Dektop" + "\n";

                if (discordTargetMember.Presence.ClientStatus.Web.HasValue)
                    description += discordEmoji_white_check_mark + " Web" + "\n";
                else
                    description += discordEmojiCheck_x + " Web" + "\n";

                if (discordTargetMember.Presence.ClientStatus.Mobile.HasValue)
                    description += discordEmoji_white_check_mark + " Mobile";
                else
                    description += discordEmojiCheck_x + " Mobile";

                discordEmbedBuilder.Description = description;

                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                else
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            if (deleteResponseAsync)
            {
                for (int i = 3; i > 0; i--)
                {
                    discordEmbedBuilder.AddField(new DiscordEmbedField("This message will be deleted in", $"{i} Secounds"));
                    await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(1000);
                    discordEmbedBuilder.RemoveFieldAt(0);
                }

                await contextMenuContext.DeleteResponseAsync();
            }
        }

        /// <summary>
        /// Gets the user's avatar & banner.
        /// </summary>
        /// <param name="contextMenuContext">The contextmenu context.</param>
        [ContextMenu(ApplicationCommandType.User, "Get avatar & banner!")]
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

        [ContextMenu(ApplicationCommandType.User, "Give Rating!")]
        public static async Task GiveRating(ContextMenuContext contextMenuContext)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
            discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
            discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
            discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

            var comps = new DiscordSelectComponent("give_rating", "Select a Rating!", discordSelectComponentOptionList);

            await contextMenuContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(comps).WithContent($"Give <@{contextMenuContext.TargetMember.Id}> a Rating!"));
        }

        [SlashCommand("GiveRating", "Give an User a rating!")]
        public static async Task GiveRating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordMember discordTargetMember = discordUser as DiscordMember;

            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
            discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
            discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
            discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

            var comps = new DiscordSelectComponent("give_rating", "Select a Rating!", discordSelectComponentOptionList);

            await interactionContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(comps).WithContent($"Give <@{discordTargetMember.Id}> a Rating!"));
        }

        public static async Task Discord_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            int rating = 0;
            DiscordMember discordMember = e.User as DiscordMember;
            DiscordMember discordTargetMember = e.Message.MentionedUsers[0].ConvertToMember(e.Guild).Result;
            switch (e.Values[0])
            {
                case "rating_1":
                    rating = 1;
                    break;
                case "rating_2":
                    rating = 2;
                    break;
                case "rating_3":
                    rating = 3;
                    break;
                case "rating_4":
                    rating = 4;
                    break;
                case "rating_5":
                    rating = 5;
                    break;
            }

            bool foundTargetMemberInDB = false;
            bool memberIsFlagged91 = false;
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();

            if (e.Guild.Id == 928930967140331590)
            {
                DiscordRole discordRole = e.Guild.GetRole(980071522427363368);
                if (discordMember.Roles.Contains(discordRole))
                    memberIsFlagged91 = true;
            }

            if (memberIsFlagged91)
            {
                discordEmbedBuilder.Title = "Rating";
                discordEmbedBuilder.Description = $"U are Flagged +91 u cant vote!";
            }
            else if (discordMember.Id == discordTargetMember.Id)
            {
                discordEmbedBuilder.Title = "Rating";
                discordEmbedBuilder.Description = $"Nonono we dont do this around here! CHEATER!";
            }
            else
            {
                DcSympathieSystem dcSympathieSystemObj = new DcSympathieSystem
                {
                    VotingUserID = discordTargetMember.Id,
                    VotedUserID = discordTargetMember.Id,
                    GuildID = e.Guild.Id,
                    VoteRating = rating,
                };

                List<DcSympathieSystem> dcSympathieSystemsList = DcSympathieSystem.ReadAll(e.Guild.Id);

                foreach (DcSympathieSystem dcSympathieSystemItem in dcSympathieSystemsList)
                {
                    if (dcSympathieSystemItem.VotingUserID == dcSympathieSystemObj.VotingUserID && dcSympathieSystemItem.VotedUserID == dcSympathieSystemObj.VotedUserID)
                        foundTargetMemberInDB = true;
                }

                if (!foundTargetMemberInDB)
                    DcSympathieSystem.Add(dcSympathieSystemObj);
                else if (foundTargetMemberInDB)
                    DcSympathieSystem.Change(dcSympathieSystemObj);

                discordEmbedBuilder.Title = "Rating";
                discordEmbedBuilder.Description = $"You gave {discordTargetMember.Mention} the Rating {rating}";
            }

            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("RatingSetup", "Set up the roles for the Ratingsystem!", false)]
        public static async Task RatingSetup(InteractionContext interactionContext, [ChoiceProvider(typeof(VoteRatingChoiceProvider))][Option("Vote", "Setup")] string voteRating, [Option("Role", "@...")] DiscordRole discordRole)
        {
            bool found = DcSympathieSystem.CheckRoleInfoExists(interactionContext.Guild.Id, Convert.ToInt32(voteRating));

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Setting Role!"));

            DcSympathieSystem dcSympathieSystemObj = new DcSympathieSystem
            {
                GuildID = interactionContext.Guild.Id
            };
            dcSympathieSystemObj.RoleInfo = new();

            switch (Convert.ToInt32(voteRating))
            {
                case 1:
                    dcSympathieSystemObj.RoleInfo.RatingOne = discordRole.Id;
                    break;
                case 2:
                    dcSympathieSystemObj.RoleInfo.RatingTwo = discordRole.Id;
                    break;
                case 3:
                    dcSympathieSystemObj.RoleInfo.RatingThree = discordRole.Id;
                    break;
                case 4:
                    dcSympathieSystemObj.RoleInfo.RatingFour = discordRole.Id;
                    break;
                case 5:
                    dcSympathieSystemObj.RoleInfo.RatingFive = discordRole.Id;
                    break;
                default:
                    break;
            }
            if (!found)
                DcSympathieSystem.AddRoleInfo(dcSympathieSystemObj);
            if (found)
                DcSympathieSystem.ChangeRoleInfo(dcSympathieSystemObj);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{discordRole.Id} set for {voteRating}"));
        }
        [SlashCommand("Showrating", "Shows the rating of an user!")]
        public static async Task Showrating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            string description = "```\n";
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            for (int i = 1; i < 6; i++)
            {
                description += $"Rating with {i}: {DcSympathieSystem.GetUserRatings(interactionContext.Guild.Id, discordUser.Id, i)}\n";
            }
            description += "```";
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Votes for {discordUser.Username}",
                Color = DiscordColor.Purple,
                Description = description
            };

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
    }
}
