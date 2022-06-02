using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.ChoiceProvider;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.AppCommands;

/// <summary>
///     The AppCommands.
/// </summary>
internal class Main : ApplicationCommandsModule
{
    /// <summary>
    ///     Set an Alarm clock per command.
    /// </summary>
    /// <param name="interactionContext">The interaction context.</param>
    /// <param name="hour">The Hour of the Alarm in the Future.</param>
    /// <param name="minute">The Minute of the Alarm in the Future.</param>
    /// <returns></returns>
    [SlashCommand("SetAlarm", "Set an alarm for a specific time!")]
    public static async Task AlarmClock(InteractionContext interactionContext, [Option("hourofday", "0-23")] double hour, [Option("minuteofday", "0-59")] double minute)
    {
        //Create a Response.
        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating alarm..."));

        //Check if the given Time format is Valid.
        if (!TimeFormat(hour, minute))
        {
            //Tell the User that the Time format is not valid and return.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Wrong format for hour or minute!"));
            return;
        }

        //Create a DateTime Variable if the Time format was Valid.
        var dateTimeNow = DateTime.Now;
        DateTime alarm = new(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), 0);

        //Check if the Alarm is a Time for Tomorrow, if it is in the Past already Today.
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

        //Let the User know that the Alarm was set Successfully.
        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Alarm set for {botAlarmClock.NotificationTime}!"));
    }

    /// <summary>
    ///     To look up what Alarm´s have been set.
    /// </summary>
    /// <param name="interactionContext"></param>
    /// <returns></returns>
    [SlashCommand("MyAlarms", "Look up your alarms!")]
    public static async Task AlarmClockLookup(InteractionContext interactionContext)
    {
        //Create an Response.
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        //Create a List where all Alarms will be Listed if there are any set.
        var botAlarmClockList = DB_BotAlarmClocks.ReadAll();

        //Create an Embed.
        DiscordEmbedBuilder discordEmbedBuilder = new()
        {
            Title = "Your alarms",
            Color = DiscordColor.Purple,
            Description = $"<@{interactionContext.Member.Id}>"
        };

        //Switch to check if there are any Timers at all.
        var noTimers = true;

        //Search for any Alarms that match the Alarm creator and Requesting User.
        foreach (var botAlarmClockItem in botAlarmClockList.Where(botAlarmClockItem => botAlarmClockItem.MemberId == interactionContext.Member.Id))
        {
            //Set the switch to false because at least one Alarm was found.
            noTimers = false;
            //Add an field to the Embed with the Alarm that was found.
            discordEmbedBuilder.AddField(new DiscordEmbedField($"{botAlarmClockItem.NotificationTime}", $"Alarm with ID {botAlarmClockItem.DBEntryID}"));
        }

        //Set the Title so the User knows no Alarms for him where found.
        if (noTimers)
            discordEmbedBuilder.Title = "No alarms set!";

        //Edit the Response and add the Embed.
        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Set an Timer per Command.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <param name="hour">The Hour of the Alarm in the Future.</param>
    /// <param name="minute">The Minute of the Alarm in the Future.</param>
    /// <returns></returns>
    [SlashCommand("SetTimer", "Set a timer!")]
    public static async Task Timer(InteractionContext interactionContext, [Option("hours", "0-23")] double hour, [Option("minutes", "0-59")] double minute)
    {
        //Create a Response.
        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating timer..."));

        //Check if the Give Time format is Valid.
        if (!TimeFormat(hour, minute))
        {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Wrong format for hour or minute!"));
            return;
        }

        //Create an TimerObject and add it to the Database.
        var dateTimeNow = DateTime.Now;
        BotTimer botTimer = new()
        {
            ChannelId = interactionContext.Channel.Id,
            MemberId = interactionContext.Member.Id,
            NotificationTime = dateTimeNow.AddHours(hour).AddMinutes(minute)
        };
        BotTimer.Add(botTimer);

        //Edit the Response and add the Embed.
        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timer set for {botTimer.NotificationTime}!"));
    }

    /// <summary>
    ///     To look up what Timers have been set.
    /// </summary>
    /// <param name="interactionContext"></param>
    /// <returns></returns>
    [SlashCommand("MyTimers", "Look up your timers!")]
    public static async Task TimerLookup(InteractionContext interactionContext)
    {
        //Create a Response.
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        //Create an List with all Timers that where found in the Database.
        var botTimerList = DB_BotTimer.ReadAll();

        //Create an Embed.
        DiscordEmbedBuilder discordEmbedBuilder = new()
        {
            Title = "Your timers",
            Color = DiscordColor.Purple,
            Description = $"<@{interactionContext.Member.Id}>"
        };

        //Switch to check if any Timers where set at all.
        var noTimers = true;

        //Search for any Timers that match the Timer creator and Requesting User.
        foreach (var botTimerItem in botTimerList.Where(botTimerItem => botTimerItem.MemberId == interactionContext.Member.Id))
        {
            //Set the switch to false because at least one Timer was found.
            noTimers = false;
            //Add an field to the Embed with the Timer that was found.
            discordEmbedBuilder.AddField(new DiscordEmbedField($"{botTimerItem.NotificationTime}", $"Timer with ID {botTimerItem.DBEntryID}"));
        }

        //Set the Title so the User knows no Timers for him where found.
        if (noTimers)
            discordEmbedBuilder.Title = "No timers set!";

        //Edit the Response and add the Embed.
        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Shows the leaderboard.
    /// </summary>
    /// <param name="interactionContext"></param>
    /// <returns></returns>
    [SlashCommand("Leaderboard", "Look up the leaderboard for connection time!")]
    public static async Task Leaderboard(InteractionContext interactionContext)
    {
        //Create an Response.
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        DiscordMember discordMemberObj = null;

        //Create List where all users are listed.
        var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);

        //Order the list by online ticks.
        var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
        userLevelSystemListSorted.Reverse();

        var top30 = 0;
        var leaderboardString = "```css\n";

        var discordMemberList = Bot.Client.GetGuildAsync(interactionContext.Guild.Id).Result.Members.Values.ToList();

        //Create the Leaderboard string
        foreach (var userLevelSystemItem in userLevelSystemListSorted)
        {
            foreach (var discordMemberItem in discordMemberList.Where(discordMemberItem => discordMemberItem.Id == userLevelSystemItem.MemberId))
            {
                discordMemberObj = discordMemberItem;
            }

            if (discordMemberObj != null)
            {
                DateTime date1 = new(1969, 4, 20, 4, 20, 0);
                var date2 = new DateTime(1969, 4, 20, 4, 20, 0).AddMinutes(userLevelSystemItem.OnlineTicks);
                var timeSpan = date2 - date1;
                
                var calculatedLevel = UserLevelSystem.CalculateLevel(userLevelSystemItem.OnlineTicks);
                var calculatedXpOverCurrentLevel = UserLevelSystem.CalculateXpOverCurrentLevel(userLevelSystemItem.OnlineTicks);

                leaderboardString += "{" + $"{timeSpan,9:ddd\\/hh\\:mm}" + "}" + $" Level {calculatedLevel,3} {userLevelSystemItem.OnlineTicks,7} {calculatedXpOverCurrentLevel,6} [{discordMemberObj.DisplayName}]\n";
                top30++;
                if (top30 == 30)
                    break;

                discordMemberObj = null;
            }
        }

        leaderboardString += "\n```";
        /*DiscordEmbedBuilder discordEmbedBuilder = new()
        {
            Title = "LevelSystem",
            Description = leaderboardString,
            Color = DiscordColor.Purple
        };*/

        //await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(leaderboardString));
    }

    /// <summary>
    ///     Command to view ur connection time Level.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <returns></returns>
    [SlashCommand("Level", "Look up your level!")]
    public static async Task Level(InteractionContext interactionContext)
    {
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
        var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
        userLevelSystemListSorted.Reverse();
        const string apiKey = "zm-7c07552d-7ed5-42b6-910c-fc8a92082bc5";
        var uriString = $"https://quickchart.io/chart/render/{apiKey}?data1=";
        
        var rank = "N/A";
        int xp = 0, calculatedXpOverCurrentLevel = 0, calculatedXpSpanToReachNextLevel = 0, level = 0;
        

        var discordUser = await Bot.Client.GetUserAsync(interactionContext.Member.Id);

        foreach (var userLevelSystemItem in userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == interactionContext.Member.Id))
        {
            rank = (userLevelSystemListSorted.IndexOf(userLevelSystemItem) + 1).ToString();
            calculatedXpOverCurrentLevel = UserLevelSystem.CalculateXpOverCurrentLevel(userLevelSystemItem.OnlineTicks);
            calculatedXpSpanToReachNextLevel = UserLevelSystem.CalculateXpSpanToReachNextLevel(userLevelSystemItem.OnlineTicks);
            xp = userLevelSystemItem.OnlineTicks;
            level = UserLevelSystem.CalculateLevel(userLevelSystemItem.OnlineTicks);
            break;
        }

        var xpString = $"{calculatedXpOverCurrentLevel} / {calculatedXpSpanToReachNextLevel} XP ";
        var levelString = $"Level {level}";

        var xpPadLeft = xpString.PadLeft(11, ' ');
        var levelPadLeft = levelString.PadLeft(8, ' ');
        var temp = xpPadLeft + levelPadLeft;

        uriString += $"{calculatedXpOverCurrentLevel}&data2={calculatedXpSpanToReachNextLevel}";

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
    ///     Checks if the given hour and minute are usable to make a datetime object out of them.
    ///     Returns true if the given arguments are usable.
    ///     Returns false if the hour or the minute are not usable.
    /// </summary>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    /// <returns>A bool.</returns>
    public static bool TimeFormat(double hour, double minute)
    {
        var hourformatisright = false;
        var minuteformatisright = false;

        for (var i = 0; i < 24; i++)
            if (hour == i)
                hourformatisright = true;
        if (!hourformatisright)
            return false;

        for (var i = 0; i < 60; i++)
            if (minute == i)
                minuteformatisright = true;

        return minuteformatisright;
    }

    /// <summary>
    ///     Poke an User per command.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <param name="discordUser">the discordUser</param>
    /// <returns></returns>
    [SlashCommand("Poke", "Poke user!")]
    public static async Task Poke(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
    {
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

        DiscordSelectComponent discordSelectComponent = new("force", "Select a method!", discordSelectComponentOptionList);

        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke user <@{discordUser.Id}>!"));
    }

    /// <summary>
    ///     Poke an User per contextmenu.
    /// </summary>
    /// <param name="contextMenuContext">The contextMenuContext</param>
    /// <returns></returns>
    [ContextMenu(ApplicationCommandType.User, "Poke user!")]
    public static async Task ContextMenuPoke(ContextMenuContext contextMenuContext)
    {
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

        var discordSelectComponent = new DiscordSelectComponent("force", "Select a method!", discordSelectComponentOptionList);

        await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke user <@{contextMenuContext.TargetMember.Id}>!"));
    }

    /// <summary>
    ///     Command to give an User a Rating.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <param name="discordUser">The discordUser</param>
    /// <returns></returns>
    [SlashCommand("GiveRating", "Give an User a rating!")]
    public static async Task GiveRating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
    {
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
        discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
        discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
        discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

        DiscordSelectComponent discordSelectComponent = new("give_rating", "Select a Rating!", discordSelectComponentOptionList);

        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Give <@{discordUser.Id}> a Rating!"));
    }

    /// <summary>
    ///     Poke an User with the Context Menu.
    /// </summary>
    /// <param name="contextMenuContext">The contextMenuContext</param>
    /// <returns></returns>
    [ContextMenu(ApplicationCommandType.User, "Give Rating!")]
    public static async Task GiveRating(ContextMenuContext contextMenuContext)
    {
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
        discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
        discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
        discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

        DiscordSelectComponent discordSelectComponent = new("give_rating", "Select a Rating!", discordSelectComponentOptionList);

        await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Give <@{contextMenuContext.TargetMember.Id}> a Rating!"));
    }

    public static async Task Discord_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs)
    {
        switch (componentInteractionCreateEventArgs.Values[0])
        {
            case "rating_1":
                await VoteRatingAsync(componentInteractionCreateEventArgs, 1);
                break;
            case "rating_2":
                await VoteRatingAsync(componentInteractionCreateEventArgs, 2);
                break;
            case "rating_3":
                await VoteRatingAsync(componentInteractionCreateEventArgs, 3);
                break;
            case "rating_4":
                await VoteRatingAsync(componentInteractionCreateEventArgs, 4);
                break;
            case "rating_5":
                await VoteRatingAsync(componentInteractionCreateEventArgs, 5);
                break;
            case "light":
                await PokeAsync(componentInteractionCreateEventArgs, false, 2, false);
                break;
            case "hard":
                await PokeAsync(componentInteractionCreateEventArgs, false, 2, true);
                break;
        }
    }

    /// <summary>
    ///     The Poke function.
    /// </summary>
    /// <param name="componentInteractionCreateEventArgs">The componentInteractionCreateEventArgs.</param>
    /// <param name="deleteResponseAsync">If the response should be Deleted after the poke action.</param>
    /// <param name="pokeAmount">The amount the user gets poked.</param>
    /// <param name="force">Light or hard slap.</param>
    /// <returns></returns>
    public static async Task PokeAsync(ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs, bool deleteResponseAsync, int pokeAmount, bool force)
    {
        var discordMember = componentInteractionCreateEventArgs.User.ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;
        var discordTargetMember = componentInteractionCreateEventArgs.Message.MentionedUsers[0].ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;

        await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var discordEmbedBuilder = new DiscordEmbedBuilder
        {
            Title = $"Poke {discordTargetMember.DisplayName}"
        };

        discordEmbedBuilder.WithFooter($"Requested by {componentInteractionCreateEventArgs.User.Username}", discordMember.AvatarUrl);

        await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

        var rightToMove = false;

        var discordRoleList = discordMember.Roles.ToList();

        foreach (var discordRoleItem in discordRoleList.Where(discordRoleItem => discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers)))
            rightToMove = true;

        var desktopHasValue = false;
        var webHasValue = false;
        var mobileHasValue = false;
        var presenceWasNull = false;

        if (discordTargetMember.Presence != null)
        {
            desktopHasValue = discordTargetMember.Presence.ClientStatus.Desktop.HasValue;
            webHasValue = discordTargetMember.Presence.ClientStatus.Web.HasValue;
            mobileHasValue = discordTargetMember.Presence.ClientStatus.Mobile.HasValue;
        }
        else
        {
            presenceWasNull = true;
        }

        if (discordTargetMember.VoiceState != null && rightToMove && (force || presenceWasNull || ((desktopHasValue || webHasValue) && !mobileHasValue)))
        {
            DiscordChannel currentChannel = default;
            DiscordChannel tempCategory = default;
            DiscordChannel tempChannel2 = default;
            DiscordChannel tempChannel1 = default;

            try
            {
                var discordEmojis = DiscordEmoji.FromName(Bot.Client, ":no_entry_sign:");

                tempCategory = componentInteractionCreateEventArgs.Interaction.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                tempChannel1 = componentInteractionCreateEventArgs.Interaction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
                tempChannel2 = componentInteractionCreateEventArgs.Interaction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while creating the channels!";
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                currentChannel = discordTargetMember.VoiceState.Channel;

                for (var i = 0; i < pokeAmount; i++)
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
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await discordTargetMember.ModifyAsync(x => x.VoiceChannel = currentChannel);
            }
            catch
            {
                discordEmbedBuilder.Description = "Error! User left?";
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempChannel2.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempChannel1.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempCategory.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
        }
        else if (discordTargetMember.VoiceState == null)
        {
            discordEmbedBuilder.Description = "User is not connected!";
            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
        else if (!rightToMove)
        {
            discordEmbedBuilder.Description = "Your not allowed to use that!";
            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
        else if (mobileHasValue)
        {
            var description = "Their phone will explode STOP!\n";

            var discordEmojisWhiteCheckMark = DiscordEmoji.FromName(Bot.Client, ":white_check_mark:");
            var discordEmojisCheckX = DiscordEmoji.FromName(Bot.Client, ":x:");

            if (discordTargetMember.Presence.ClientStatus.Desktop.HasValue)
                description += discordEmojisWhiteCheckMark + " Desktop" + "\n";
            else
                description += discordEmojisCheckX + " Desktop" + "\n";

            if (discordTargetMember.Presence.ClientStatus.Web.HasValue)
                description += discordEmojisWhiteCheckMark + " Web" + "\n";
            else
                description += discordEmojisCheckX + " Web" + "\n";

            if (discordTargetMember.Presence.ClientStatus.Mobile.HasValue)
                description += discordEmojisWhiteCheckMark + " Mobile";
            else
                description += discordEmojisCheckX + " Mobile";

            discordEmbedBuilder.Description = description;

            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        if (deleteResponseAsync)
        {
            for (var i = 3; i > 0; i--)
            {
                discordEmbedBuilder.AddField(new DiscordEmbedField("This message will be deleted in", $"{i} Seconds"));
                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                await Task.Delay(1000);
                discordEmbedBuilder.RemoveFieldAt(0);
            }

            await componentInteractionCreateEventArgs.Interaction.DeleteOriginalResponseAsync();
        }
    }

    /// <summary>
    ///     The function that creates or edits the database based on the new rating.
    /// </summary>
    /// <param name="componentInteractionCreateEventArgs">The componentInteractionCreateEventArgs.</param>
    /// <param name="rating">The rating value.</param>
    /// <returns></returns>
    public static async Task VoteRatingAsync(ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs, int rating)
    {
        var discordMember = componentInteractionCreateEventArgs.User.ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;
        var discordTargetMember = componentInteractionCreateEventArgs.Message.MentionedUsers[0].ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;

        var foundTargetMemberInDb = false;
        var memberIsFlagged91 = false;
        var discordEmbedBuilder = new DiscordEmbedBuilder();

        if (componentInteractionCreateEventArgs.Guild.Id == 928930967140331590)
        {
            var discordRole = componentInteractionCreateEventArgs.Guild.GetRole(980071522427363368);
            if (discordMember != null && discordMember.Roles.Contains(discordRole))
            {
                memberIsFlagged91 = true;
            }
        }

        if (memberIsFlagged91)
        {
            discordEmbedBuilder.Title = "Rating";
            discordEmbedBuilder.Description = "U are Flagged +91 u cant vote!";
        }
        else if (discordMember.Id == discordTargetMember.Id)
        {
            discordEmbedBuilder.Title = "Rating";
            discordEmbedBuilder.Description = "Nonono we dont do this around here! CHEATER!";
        }
        else
        {
            var sympathySystemObj = new SympathySystem
            {
                VotingUserID = discordTargetMember.Id,
                VotedUserID = discordTargetMember.Id,
                GuildID = componentInteractionCreateEventArgs.Guild.Id,
                VoteRating = rating
            };

            var sympathySystemsList = SympathySystem.ReadAll(componentInteractionCreateEventArgs.Guild.Id);

            foreach (var sympathySystemItem in sympathySystemsList.Where(sympathySystemItem => sympathySystemItem.VotingUserID == sympathySystemObj.VotingUserID && sympathySystemItem.VotedUserID == sympathySystemObj.VotedUserID))
                foundTargetMemberInDb = true;

            switch (foundTargetMemberInDb)
            {
                case false:
                    SympathySystem.Add(sympathySystemObj);
                    break;
                case true:
                    SympathySystem.Change(sympathySystemObj);
                    break;
            }

            discordEmbedBuilder.Title = "Rating";
            discordEmbedBuilder.Description = $"You gave {discordTargetMember.Mention} the Rating {rating}";
        }

        await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Setup assist for the Rating Roles.
    /// </summary>
    /// <param name="interactionContext">The interactionContext.</param>
    /// <param name="voteRating">The RatingValue the role stands for.</param>
    /// <param name="discordRole">The discordRole.</param>
    /// <returns></returns>
    [SlashCommand("RatingSetup", "Set up the roles for the Rating System!")]
    public static async Task RatingSetup(InteractionContext interactionContext, [ChoiceProvider(typeof(RatingSetupChoiceProvider))] [Option("Vote", "Setup")] string voteRating, [Option("Role", "@...")] DiscordRole discordRole)
    {
        var found = SympathySystem.CheckRoleInfoExists(interactionContext.Guild.Id, Convert.ToInt32(voteRating));

        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Setting Role!"));

        var sympathySystemObj = new SympathySystem
        {
            GuildID = interactionContext.Guild.Id,
            RoleInfo = new RoleInfoSympathySystem()
        };

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
        }

        switch (found)
        {
            case false:
                SympathySystem.AddRoleInfo(sympathySystemObj);
                break;
            case true:
                SympathySystem.ChangeRoleInfo(sympathySystemObj);
                break;
        }

        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{discordRole.Id} set for {voteRating}"));
    }

    /// <summary>
    ///     Command to view how many and what ratings a given user has.
    /// </summary>
    /// <param name="interactionContext">The interactionContext.</param>
    /// <param name="discordUser">The Discord User.</param>
    /// <returns></returns>
    [SlashCommand("ShowRating", "Shows the rating of an user!")]
    public static async Task ShowRating(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
    {
        var description = "```\n";
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        for (var i = 1; i < 6; i++)
            description += $"Rating with {i}: {SympathySystem.GetUserRatings(interactionContext.Guild.Id, discordUser.Id, i)}\n";
        description += "```";
        var discordEmbedBuilder = new DiscordEmbedBuilder
        {
            Title = $"Votes for {discordUser.Username}",
            Color = DiscordColor.Purple,
            Description = description
        };

        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Get the Avatar and Banner of an User.
    /// </summary>
    /// <param name="contextMenuContext">The contextMenuContext.</param>
    /// <returns></returns>
    [ContextMenu(ApplicationCommandType.User, "Get avatar & banner!")]
    public static async Task GetUserBannerAsync(ContextMenuContext contextMenuContext)
    {
        var user = await contextMenuContext.Client.GetUserAsync(contextMenuContext.TargetUser.Id);

        var discordEmbedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Avatar & Banner of {user.Username}",
                ImageUrl = user.BannerHash != null ? user.BannerUrl : null
            }.WithThumbnail(user.AvatarUrl).WithColor(user.BannerColor ?? DiscordColor.Purple).WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl).WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
        await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Creates an Invite link.
    /// </summary>
    /// <param name="interactionContext">The interactionContext.</param>
    /// <returns></returns>
    [SlashCommand("Invite", "Invite $chattenclown")]
    public static async Task InviteAsync(InteractionContext interactionContext)
    {
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var botInvite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(botInvite.AbsoluteUri));
    }
}