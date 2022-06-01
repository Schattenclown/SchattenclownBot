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
        /// Set an Alarmclock per command.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        /// <param name="hour">The Houre of the Alarm in the Future.</param>
        /// <param name="minute">The Minute of the Alarm in the Future.</param>
        /// <returns></returns>
        [SlashCommand("SetAlarm", "Set an alarm for a spesific time!")]
        public static async Task AlarmClock(InteractionContext interactionContext, [Option("hourofday", "0-23")] double hour, [Option("minuteofday", "0-59")] double minute)
        {
            //Create a Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating alarm..."));
            
            //Check if the Give Timeformat is Valid.
            if (!TimeFormat(hour, minute))
            {
                //Tell the User that the Timeformat is not valid and return.
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            //Create a DateTime Variable if the Timeformat was Valid.
            DateTime dateTimeNow = DateTime.Now;
            DateTime alarm = new(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), 0);

            //Check if the Alarm has a Time for Tommorow if it is in the Past already Today.
            if (alarm < DateTime.Now)
                alarm = alarm.AddDays(1);

            //Create an AlarmObject and add it to the Database.
            BotAlarmClock botAlarmClock = new()
            {
                ChannelId = interactionContext.Channel.Id,
                MemberId = interactionContext.Member.Id,
                NotificationTime = alarm
            };
            BotAlarmClock.Add(botAlarmClock);

            //Let the User know that the Alarm was set Succsefully.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Alarm set for {botAlarmClock.NotificationTime}!"));
        }

        /// <summary>
        /// To look up what Alarams have been set.
        /// </summary>
        /// <param name="interactionContext"></param>
        /// <returns></returns>
        [SlashCommand("MyAlarms", "Look up your alarms!")]
        public static async Task AlarmClockLookup(InteractionContext interactionContext)
        {
            //Create an Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            //Create a List where all Alarms will be Listed if there are any set.
            List<BotAlarmClock> botAlarmClockList = DB_BotAlarmClocks.ReadAll();

            //Create an Embed.
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = "Your alarms",
                Color = DiscordColor.Purple,
                Description = $"<@{interactionContext.Member.Id}>"
            };

            //Switch to check if there are any Timers at all.
            bool noTimers = true;

            //Search for any Alarms that match the Alarmcreator and Requesting User.
            foreach (var botAlarmClockItem in botAlarmClockList)
            {
                //Check if the Alarmcreator and the Requesting User are the same.
                if (botAlarmClockItem.MemberId == interactionContext.Member.Id)
                {
                    //Set the swtich to false because at leaset one Alarm was found.
                    noTimers = false;
                    //Add an field to the Embed with the Alarm that was found.
                    discordEmbedBuilder.AddField(new DiscordEmbedField($"{botAlarmClockItem.NotificationTime}", $"Alarm with ID {botAlarmClockItem.DBEntryID}"));
                }
            }

            //Set the Title so the User knows no Alarms for him where found.
            if (noTimers)
                discordEmbedBuilder.Title = "No alarms set!";

            //Edit the Responce and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        /// Set an Timer per Command.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="hour">The Houre of the Alarm in the Future.</param>
        /// <param name="minute">The Minute of the Alarm in the Future.</param>
        /// <returns></returns>
        [SlashCommand("SetTimer", "Set a timer!")]
        public static async Task Timer(InteractionContext interactionContext, [Option("hours", "0-23")] double hour, [Option("minutes", "0-59")] double minute)
        {
            //Create a Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating timer..."));

            //Check if the Give Timeformat is Valid.
            if (!TimeFormat(hour, minute))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            //Create an TimerObject and add it to the Database.
            DateTime dateTimeNow = DateTime.Now;
            BotTimer botTimer = new()
            {
                ChannelId = interactionContext.Channel.Id,
                MemberId = interactionContext.Member.Id,
                NotificationTime = dateTimeNow.AddHours(hour).AddMinutes(minute)
            };
            BotTimer.Add(botTimer);

            //Edit the Responce and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timer set for {botTimer.NotificationTime}!"));
        }

        /// <summary>
        /// To look up what Timers have been set.
        /// </summary>
        /// <param name="interactionContext"></param>
        /// <returns></returns>
        [SlashCommand("MyTimers", "Look up your timers!")]
        public static async Task TimerLookup(InteractionContext interactionContext)
        {
            //Create a Reponse.
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            //Create an List with all Timers that where found in the Database.
            List<BotTimer> botTimerList = DB_BotTimer.ReadAll();

            //Create an Embed.
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = "Your timers",
                Color = DiscordColor.Purple,
                Description = $"<@{interactionContext.Member.Id}>"
            };

            //Switch to check if any Timers where set at all.
            bool noTimers = true;

            //Search for any Timers that match the Timercreator and Requesting User.
            foreach (var botTimerItem in botTimerList)
            {
                //Check if the Timercreater and the Requesting User are the same.
                if (botTimerItem.MemberId == interactionContext.Member.Id)
                {
                    //Set the swtich to false because at leaset one Timer was found.
                    noTimers = false;
                    //Add an field to the Embed with the Timer that was found.
                    discordEmbedBuilder.AddField(new DiscordEmbedField($"{botTimerItem.NotificationTime}", $"Timer with ID {botTimerItem.DBEntryID}"));
                }
            }

            //Set the Title so the User knows no Timers for him where found.
            if (noTimers)
                discordEmbedBuilder.Title = "No timers set!";

            //Edit the Responce and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        /// Shows the Leaderboard.
        /// </summary>
        /// <param name="interactionContext"></param>
        /// <returns></returns>
        [SlashCommand("Leaderboard", "Look up the leaderboard for connectiontime!")]
        public static async Task Leaderboard(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<UserLevelSystem> userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);

            List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            userLevelSystemListSorted.Reverse();

            int top30 = 0;
            int totalXp = 0;
            int totalLevel, modXp;
            string level, xp;

            string liststring = "```css\n";
            foreach (var userLevelSystemItem in userLevelSystemListSorted)
            {
                var discordUser = await Bot.Client.GetUserAsync(userLevelSystemItem.MemberId, true);

                DateTime date1 = new(1969, 4, 20, 4, 20, 0);
                DateTime date2 = new DateTime(1969, 4, 20, 4, 20, 0).AddMinutes(userLevelSystemItem.OnlineTicks);
                TimeSpan timeSpan = date2 - date1;

                totalXp = userLevelSystemItem.OnlineTicks * 125 / 60;

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


                liststring += "{" + $"{timeSpan,9:ddd\\/hh\\:mm}" + "}" + $" Level {totalLevel,4} " + $"[{discordUser.Username}]\n";
                top30++;
                if (top30 == 30)
                    break;
            }
            liststring += "\n```";
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = "LevelSystem",
                Description = liststring,
                Color = DiscordColor.Purple
            };

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("MyLevel", "Look up your level!")]
        public static async Task Level(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            List<UserLevelSystem> userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
            List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            userLevelSystemListSorted.Reverse();

            string uriString = "https://quickchart.io/chart/render/zm-483cf019-58bf-423e-bd2c-514d8f9b2ff6?data1=";

            int totalXp = 0;
            int totalLevel, modXp;
            string rank = "N/A";
            string level, xp;

            var discordUser = await Bot.Client.GetUserAsync(interactionContext.Member.Id);
            string username = discordUser.Username;

            foreach (var userLevelSystemItem in userLevelSystemListSorted)
            {
                if (userLevelSystemItem.MemberId == interactionContext.Member.Id)
                {
                    rank = (userLevelSystemListSorted.IndexOf(userLevelSystemItem) + 1).ToString();
                    totalXp = userLevelSystemItem.OnlineTicks * 125 / 60;
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

            Uri uri = new(uriString);
            DiscordEmbedBuilder discordEmbedBuilder = new();
            discordEmbedBuilder.WithTitle(temp);
            discordEmbedBuilder.WithDescription("<@" + interactionContext.Member.Id + ">");
            discordEmbedBuilder.WithFooter("Rank #" + rank);
            discordEmbedBuilder.WithImageUrl(uri.AbsoluteUri);
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

        [SlashCommand("Poke", "Poke user!")]
        public static async Task Poke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

            DiscordSelectComponent discordSelectComponent = new("force", "Select a method!", discordSelectComponentOptionList);

            await interactionContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(discordSelectComponent).WithContent($"Poke user <@{discordUser.Id}>!"));
        }

        [ContextMenu(ApplicationCommandType.User, "Poke user!", true)]
        public static async Task AppsPoke(ContextMenuContext contextMenuContext)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

            DiscordSelectComponent discordSelectComponent = new DiscordSelectComponent("force", "Select a method!", discordSelectComponentOptionList);

            await contextMenuContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(discordSelectComponent).WithContent($"Poke user <@{contextMenuContext.TargetMember.Id}>!"));
        }

        [SlashCommand("GiveRating", "Give an User a rating!")]
        public static async Task GiveRating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
            discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
            discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
            discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

            DiscordSelectComponent discordSelectComponent = new("give_rating", "Select a Rating!", discordSelectComponentOptionList);

            await interactionContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(discordSelectComponent).WithContent($"Give <@{discordUser.Id}> a Rating!"));
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

            DiscordSelectComponent discordSelectComponent = new("give_rating", "Select a Rating!", discordSelectComponentOptionList);

            await contextMenuContext.CreateResponseAsync(DisCatSharp.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddComponents(discordSelectComponent).WithContent($"Give <@{contextMenuContext.TargetMember.Id}> a Rating!"));
        }

        public static async Task Discord_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs compnentInteractionCreateEventArgs)
        {
            switch (compnentInteractionCreateEventArgs.Values[0])
            {
                case "rating_1":
                    await VoteRatingAsync(compnentInteractionCreateEventArgs, 1);
                    break;
                case "rating_2":
                    await VoteRatingAsync(compnentInteractionCreateEventArgs, 2);
                    break;
                case "rating_3":
                    await VoteRatingAsync(compnentInteractionCreateEventArgs, 3);
                    break;
                case "rating_4":
                    await VoteRatingAsync(compnentInteractionCreateEventArgs, 4);
                    break;
                case "rating_5":
                    await VoteRatingAsync(compnentInteractionCreateEventArgs, 5);
                    break;
                case "light":
                    await PokeAsync(compnentInteractionCreateEventArgs, false, 2, false);
                    break;
                case "hard":
                    await PokeAsync(compnentInteractionCreateEventArgs, false, 2, true);
                    break;
            }
        }

        public static async Task PokeAsync(ComponentInteractionCreateEventArgs compnentInteractionCreateEventArgs, bool deleteResponseAsync, int pokeAmount, bool force)
        {
            DiscordMember discordMember = compnentInteractionCreateEventArgs.User as DiscordMember;
            DiscordMember discordTargetMember = compnentInteractionCreateEventArgs.Message.MentionedUsers[0].ConvertToMember(compnentInteractionCreateEventArgs.Guild).Result;

            await compnentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Poke {discordTargetMember.DisplayName}"
            };

            discordEmbedBuilder.WithFooter($"Requested by {compnentInteractionCreateEventArgs.User.Username}", discordMember.AvatarUrl);

            await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

            bool rightToMove = false;
            List<DiscordRole> discordRoleList = new();

            discordRoleList = discordMember.Roles.ToList();

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

                    tempCategory = compnentInteractionCreateEventArgs.Interaction.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                    tempChannel1 = compnentInteractionCreateEventArgs.Interaction.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                    tempChannel2 = compnentInteractionCreateEventArgs.Interaction.Guild.CreateVoiceChannelAsync(discordEmoji, tempCategory).Result;
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while creating the channels!";
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
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
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await discordTargetMember.ModifyAsync(x => x.VoiceChannel = currentChannel);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel2.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempChannel1.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    await tempCategory.DeleteAsync();
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }
            }
            else if (discordTargetMember.VoiceState == null)
            {
                discordEmbedBuilder.Description = "User is not connected!";
                await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (!rightToMove)
            {
                discordEmbedBuilder.Description = "Your not allowed to use that!";
                await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
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

                await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            if (deleteResponseAsync)
            {
                for (int i = 3; i > 0; i--)
                {
                    discordEmbedBuilder.AddField(new DiscordEmbedField("This message will be deleted in", $"{i} Secounds"));
                    await compnentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(1000);
                    discordEmbedBuilder.RemoveFieldAt(0);
                }

                await compnentInteractionCreateEventArgs.Interaction.DeleteOriginalResponseAsync();
            }
        }

        public static async Task VoteRatingAsync(ComponentInteractionCreateEventArgs compnentInteractionCreateEventArgs, int rating)
        {
            DiscordMember discordMember = compnentInteractionCreateEventArgs.User as DiscordMember;
            DiscordMember discordTargetMember = compnentInteractionCreateEventArgs.Message.MentionedUsers[0].ConvertToMember(compnentInteractionCreateEventArgs.Guild).Result;

            bool foundTargetMemberInDB = false;
            bool memberIsFlagged91 = false;
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();

            if (compnentInteractionCreateEventArgs.Guild.Id == 928930967140331590)
            {
                DiscordRole discordRole = compnentInteractionCreateEventArgs.Guild.GetRole(980071522427363368);
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
                SympathySystem sympathySystemObj = new SympathySystem
                {
                    VotingUserID = discordTargetMember.Id,
                    VotedUserID = discordTargetMember.Id,
                    GuildID = compnentInteractionCreateEventArgs.Guild.Id,
                    VoteRating = rating,
                };

                List<SympathySystem> sympathySystemsList = SympathySystem.ReadAll(compnentInteractionCreateEventArgs.Guild.Id);

                foreach (SympathySystem sympathySystemItem in sympathySystemsList)
                {
                    if (sympathySystemItem.VotingUserID == sympathySystemObj.VotingUserID && sympathySystemItem.VotedUserID == sympathySystemObj.VotedUserID)
                        foundTargetMemberInDB = true;
                }

                if (!foundTargetMemberInDB)
                    SympathySystem.Add(sympathySystemObj);
                else if (foundTargetMemberInDB)
                    SympathySystem.Change(sympathySystemObj);

                discordEmbedBuilder.Title = "Rating";
                discordEmbedBuilder.Description = $"You gave {discordTargetMember.Mention} the Rating {rating}";
            }

            await compnentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("RatingSetup", "Set up the roles for the Ratingsystem!", false)]
        public static async Task RatingSetup(InteractionContext interactionContext, [ChoiceProvider(typeof(RatingSetupChoiceProvider))][Option("Vote", "Setup")] string voteRating, [Option("Role", "@...")] DiscordRole discordRole)
        {
            bool found = SympathySystem.CheckRoleInfoExists(interactionContext.Guild.Id, Convert.ToInt32(voteRating));

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Setting Role!"));

            SympathySystem sympathySystemObj = new SympathySystem
            {
                GuildID = interactionContext.Guild.Id
            };
            sympathySystemObj.RoleInfo = new();

            switch (Convert.ToInt32(voteRating))
            {
                case 1:
                    sympathySystemObj.RoleInfo.RatingOne = discordRole.Id;
                    break;
                case 2:
                    sympathySystemObj.RoleInfo.RatingTwo = discordRole.Id;
                    break;
                case 3:
                    sympathySystemObj.RoleInfo.RatingThree = discordRole.Id;
                    break;
                case 4:
                    sympathySystemObj.RoleInfo.RatingFour = discordRole.Id;
                    break;
                case 5:
                    sympathySystemObj.RoleInfo.RatingFive = discordRole.Id;
                    break;
                default:
                    break;
            }
            if (!found)
                SympathySystem.AddRoleInfo(sympathySystemObj);
            if (found)
                SympathySystem.ChangeRoleInfo(sympathySystemObj);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{discordRole.Id} set for {voteRating}"));
        }

        [SlashCommand("ShowRating", "Shows the rating of an user!")]
        public static async Task Showrating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            string description = "```\n";
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            for (int i = 1; i < 6; i++)
            {
                description += $"Rating with {i}: {SympathySystem.GetUserRatings(interactionContext.Guild.Id, discordUser.Id, i)}\n";
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

        [SlashCommand("Invite", "Invite $chattenclown")]
        public static async Task InviteAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bot_invite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(bot_invite.AbsoluteUri));
        }
    }
}
