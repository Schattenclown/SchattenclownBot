using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.ApplicationCommands.Attributes;

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
        //Create an TimerObject and add it to the Database.
        if (!TimeFormat(hour, minute))
        {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Wrong format for hour or minute!"));
            return;
        }

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

                var daysstring = "Day´s";
                if (Convert.ToInt32($"{timeSpan:ddd}") == 1)
                    daysstring = "Day  ";

                leaderboardString += "{" + $"{Convert.ToInt32($"{timeSpan:ddd}"),3} {daysstring} {timeSpan:hh}:{timeSpan:mm}" + "}" + $" Level {calculatedLevel,2} [{discordMemberObj.DisplayName}]\n";
                top30++;
                if (top30 == 30)
                    break;

                discordMemberObj = null;
            }
        }

        leaderboardString += "\n```";

        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(leaderboardString));
    }

    /// <summary>
    ///     Command to view ur connection time Level.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <returns></returns>
    [SlashCommand("MyLevel", "Look up your level!")]
    public static async Task MyLevel(InteractionContext interactionContext)
    {
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
        var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
        userLevelSystemListSorted.Reverse();
        int xp = 0, calculatedXpOverCurrentLevel = 0, calculatedXpSpanToReachNextLevel = 0, level = 0;

        var rank = "N/A";

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

        #region editLink

        //https://quickchart.io/sandbox/#%7B%22chart%22%3A%22%7B%5Cn%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%5C%22data%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22datasets%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%202500%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XP%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(80%2C%200%2C%20121%2C%200.4)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%205641%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XPNeeded%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(58%2C%200%2C%20179%2C%200.2)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%5D%2C%5Cn%20%20%20%20%5C%22labels%5C%22%3A%20%5B%5D%5Cn%20%20%7D%2C%5Cn%20%20%5C%22options%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22title%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22top%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2025%2C%5Cn%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%5C%22lineHeight%5C%22%3A%201.2%2C%5Cn%20%20%20%20%20%20%5C%22text%5C%22%3A%20%5C%22Title%20be%20here%5C%22%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22layout%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22left%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22right%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22top%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22bottom%5C%22%3A%2010%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22legend%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22scales%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22xAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22id%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22bottom%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22distribution%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22gridLines%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22lineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawBorder%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawOnChartArea%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawTicks%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22tickMarkLength%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineColor%5C%22%3A%20%5C%22%23e100ff%5C%22%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22ticks%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMin%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMax%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22stepSize%5C%22%3A%201000000%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22minRotation%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22maxRotation%5C%22%3A%2050%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22mirror%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22reverse%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22max%5C%22%3A%205641%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%5C%22yAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22displayFormats%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22plugins%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22datalabels%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22anchor%5C%22%3A%20%5C%22end%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderRadius%5C%22%3A%206%2C%5Cn%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23282828%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22font%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22family%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22size%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22style%5C%22%3A%20%5C%22normal%5C%22%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%7D%2C%5Cn%20%20%7D%5Cn%7D%22%2C%22width%22%3A1000%2C%22height%22%3A180%2C%22version%22%3A%222%22%2C%22backgroundColor%22%3A%22%232f3136%22%7D

        #endregion

        #region apikey
        string urlString = "https://quickchart.io/chart?w=1000&h=180&bkg=%232f3136&c={\"type\":\"horizontalBar\",\"data\":{\"datasets\":[{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
                      $"{calculatedXpOverCurrentLevel}" +
                      "],\"type\":\"horizontalBar\",\"label\":\"XP\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(80,0,121,0.4)\",\"borderWidth\":3,\"xAxisID\":\"X1\",},{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
                      $"{calculatedXpSpanToReachNextLevel}" +
                      "],\"type\":\"horizontalBar\",\"label\":\"XPNeeded\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(58,0,179,0.2)\",\"borderWidth\":3,\"xAxisID\":\"X1\"}],\"labels\":[]},\"options\":{\"title\":{\"display\":false,\"position\":\"top\",\"fontSize\":25,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":10,\"lineHeight\":1.2,\"text\":\"Titlebehere\"},\"layout\":{\"padding\":{\"left\":30,\"right\":30,\"top\":30,\"bottom\":10}},\"legend\":{\"display\":false},\"scales\":{\"xAxes\":[{\"id\":\"X1\",\"display\":true,\"position\":\"bottom\",\"type\":\"linear\",\"stacked\":false,\"time\":{},\"distribution\":\"linear\",\"gridLines\":{\"color\":\"%23e100ff\",\"lineWidth\":4,\"drawBorder\":true,\"drawOnChartArea\":true,\"drawTicks\":true,\"tickMarkLength\":10,\"zeroLineWidth\":4,\"zeroLineColor\":\"%23e100ff\"},\"ticks\":{\"display\":true,\"fontSize\":30,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":0,\"suggestedMin\":0,\"suggestedMax\":0,\"stepSize\":1000000,\"minRotation\":0,\"maxRotation\":50,\"mirror\":false,\"reverse\":false,\"max\":" +
                      $"{calculatedXpSpanToReachNextLevel}" +
                      "}}],\"yAxes\":[{\"stacked\":true,\"time\":{\"displayFormats\":{}}}]},\"plugins\":{\"datalabels\":{\"anchor\":\"end\",\"backgroundColor\":\"%23e241ff\",\"borderColor\":\"%23e241ff\",\"borderRadius\":6,\"padding\":4,\"color\":\"%23282828\",\"font\":{\"family\":\"sans-serif\",\"size\":30,\"style\":\"normal\"}},},}}";
        #endregion

        DiscordEmbedBuilder discordEmbedBuilder = new();
        discordEmbedBuilder.WithTitle(temp);
        discordEmbedBuilder.WithDescription("<@" + interactionContext.Member.Id + ">");
        discordEmbedBuilder.WithFooter("Rank #" + rank);
        discordEmbedBuilder.WithImageUrl(urlString);
        discordEmbedBuilder.Color = DiscordColor.Purple;

        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
    }

    /// <summary>
    ///     Command to view ur connection time Level.
    /// </summary>
    /// <param name="interactionContext">The interactionContext</param>
    /// <returns></returns>
    [SlashCommand("Level", "Look up someones level!")]
    public static async Task YourLevel(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
    {
        await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
        var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
        userLevelSystemListSorted.Reverse();
        int xp = 0, calculatedXpOverCurrentLevel = 0, calculatedXpSpanToReachNextLevel = 0, level = 0;

        var rank = "N/A";

        foreach (var userLevelSystemItem in userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == discordUser.Id))
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

        #region editLink

        //https://quickchart.io/sandbox/#%7B%22chart%22%3A%22%7B%5Cn%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%5C%22data%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22datasets%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%202500%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XP%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(80%2C%200%2C%20121%2C%200.4)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%205641%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XPNeeded%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(58%2C%200%2C%20179%2C%200.2)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%5D%2C%5Cn%20%20%20%20%5C%22labels%5C%22%3A%20%5B%5D%5Cn%20%20%7D%2C%5Cn%20%20%5C%22options%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22title%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22top%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2025%2C%5Cn%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%5C%22lineHeight%5C%22%3A%201.2%2C%5Cn%20%20%20%20%20%20%5C%22text%5C%22%3A%20%5C%22Title%20be%20here%5C%22%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22layout%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22left%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22right%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22top%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22bottom%5C%22%3A%2010%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22legend%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22scales%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22xAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22id%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22bottom%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22distribution%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22gridLines%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22lineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawBorder%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawOnChartArea%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawTicks%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22tickMarkLength%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineColor%5C%22%3A%20%5C%22%23e100ff%5C%22%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22ticks%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMin%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMax%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22stepSize%5C%22%3A%201000000%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22minRotation%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22maxRotation%5C%22%3A%2050%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22mirror%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22reverse%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22max%5C%22%3A%205641%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%5C%22yAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22displayFormats%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22plugins%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22datalabels%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22anchor%5C%22%3A%20%5C%22end%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderRadius%5C%22%3A%206%2C%5Cn%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23282828%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22font%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22family%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22size%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22style%5C%22%3A%20%5C%22normal%5C%22%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%7D%2C%5Cn%20%20%7D%5Cn%7D%22%2C%22width%22%3A1000%2C%22height%22%3A180%2C%22version%22%3A%222%22%2C%22backgroundColor%22%3A%22%232f3136%22%7D

        #endregion

        #region apikey
        string urlString = "https://quickchart.io/chart?w=1000&h=180&bkg=%232f3136&c={\"type\":\"horizontalBar\",\"data\":{\"datasets\":[{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
                      $"{calculatedXpOverCurrentLevel}" +
                      "],\"type\":\"horizontalBar\",\"label\":\"XP\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(80,0,121,0.4)\",\"borderWidth\":3,\"xAxisID\":\"X1\",},{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
                      $"{calculatedXpSpanToReachNextLevel}" +
                      "],\"type\":\"horizontalBar\",\"label\":\"XPNeeded\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(58,0,179,0.2)\",\"borderWidth\":3,\"xAxisID\":\"X1\"}],\"labels\":[]},\"options\":{\"title\":{\"display\":false,\"position\":\"top\",\"fontSize\":25,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":10,\"lineHeight\":1.2,\"text\":\"Titlebehere\"},\"layout\":{\"padding\":{\"left\":30,\"right\":30,\"top\":30,\"bottom\":10}},\"legend\":{\"display\":false},\"scales\":{\"xAxes\":[{\"id\":\"X1\",\"display\":true,\"position\":\"bottom\",\"type\":\"linear\",\"stacked\":false,\"time\":{},\"distribution\":\"linear\",\"gridLines\":{\"color\":\"%23e100ff\",\"lineWidth\":4,\"drawBorder\":true,\"drawOnChartArea\":true,\"drawTicks\":true,\"tickMarkLength\":10,\"zeroLineWidth\":4,\"zeroLineColor\":\"%23e100ff\"},\"ticks\":{\"display\":true,\"fontSize\":30,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":0,\"suggestedMin\":0,\"suggestedMax\":0,\"stepSize\":1000000,\"minRotation\":0,\"maxRotation\":50,\"mirror\":false,\"reverse\":false,\"max\":" +
                      $"{calculatedXpSpanToReachNextLevel}" +
                      "}}],\"yAxes\":[{\"stacked\":true,\"time\":{\"displayFormats\":{}}}]},\"plugins\":{\"datalabels\":{\"anchor\":\"end\",\"backgroundColor\":\"%23e241ff\",\"borderColor\":\"%23e241ff\",\"borderRadius\":6,\"padding\":4,\"color\":\"%23282828\",\"font\":{\"family\":\"sans-serif\",\"size\":30,\"style\":\"normal\"}},},}}";
        #endregion

        DiscordEmbedBuilder discordEmbedBuilder = new();
        discordEmbedBuilder.WithTitle(temp);
        discordEmbedBuilder.WithDescription("<@" + discordUser.Id + ">");
        discordEmbedBuilder.WithFooter("Rank #" + rank);
        discordEmbedBuilder.WithImageUrl(urlString);
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

    [SlashCommand("daddys_poke", "Harder daddy!")]
    public static async Task DaddysPoke(InteractionContext ctx, [Option("user", "@...")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
        var member = await ctx.Guild.GetMemberAsync(user.Id);
        if (member.VoiceState == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error: Not connected"));
            return;
        }
        try
        {
            var curVoice = member.VoiceState.Channel;
            var channels = await ctx.Guild.GetChannelsAsync();
            var voiceChannels = channels.Where(x => x.Type == ChannelType.Voice).Where(x => x.Id != curVoice.Id && !x.Users.Any());
            foreach (var channel in voiceChannels)
            {
                try
                {
                    await member.PlaceInAsync(channel);
                    await Task.Delay(1000);
                }
                catch (Exception)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Something went wrong!"));
                }
            }
            await member.PlaceInAsync(curVoice);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }
        catch (Exception)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Something went wrong!"));
        }
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
        var interactivity = interactionContext.Client.GetInteractivity();
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

        DiscordSelectComponent discordSelectComponent = new("force", "Select a method!", discordSelectComponentOptionList);

        await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke user <@{discordUser.Id}>!"));
        var msg = await interactionContext.GetOriginalResponseAsync();
        var intReq = await interactivity.WaitForSelectAsync(msg, "force", TimeSpan.FromMinutes(1));
        if (!intReq.TimedOut)
        {
            var res = intReq.Result.Values.First();
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordSelectComponent.Disable()));
            var targetMember = await interactionContext.Guild.GetMemberAsync(discordUser.Id);
            await PokeAsync(intReq.Result.Interaction, interactionContext.Member, targetMember, false, res == "light" ? 2 : 4, res == "hard");
        }
        else
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));

    }

    /// <summary>
    ///     Poke an User per contextmenu.
    /// </summary>
    /// <param name="contextMenuContext">The contextMenuContext</param>
    /// <returns></returns>
    [ContextMenu(ApplicationCommandType.User, "Poke user!")]
    public static async Task ContextMenuPoke(ContextMenuContext contextMenuContext)
    {
        var interactivity = contextMenuContext.Client.GetInteractivity();
        var discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
        discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
        discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

        var discordSelectComponent = new DiscordSelectComponent("force", "Select a method!", discordSelectComponentOptionList);

        await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke user <@{contextMenuContext.TargetMember.Id}>!"));
        var msg = await contextMenuContext.GetOriginalResponseAsync();
        var intReq = await interactivity.WaitForSelectAsync(msg, "force", TimeSpan.FromMinutes(1));
        if (!intReq.TimedOut)
        {
            var res = intReq.Result.Values.First();
            await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordSelectComponent.Disable()));
            await PokeAsync(intReq.Result.Interaction, contextMenuContext.Member, contextMenuContext.TargetMember, false, res == "light" ? 2 : 4, res == "hard");
        }
        else
            await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));
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
    public static async Task PokeAsync(DiscordInteraction interaction, DiscordMember member, DiscordMember target, bool deleteResponseAsync, int pokeAmount, bool force)
    {
        await interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var discordEmbedBuilder = new DiscordEmbedBuilder
        {
            Title = $"Poke {target.DisplayName}"
        };

        discordEmbedBuilder.WithFooter($"Requested by {member.DisplayName}", member.AvatarUrl);

        await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

        var rightToMove = false;

        var discordRoleList = member.Roles.ToList();

        foreach (var discordRoleItem in discordRoleList.Where(discordRoleItem => discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers)))
            rightToMove = true;

        var desktopHasValue = false;
        var webHasValue = false;
        var mobileHasValue = false;
        var presenceWasNull = false;

        if (target.Presence != null)
        {
            desktopHasValue = target.Presence.ClientStatus.Desktop.HasValue;
            webHasValue = target.Presence.ClientStatus.Web.HasValue;
            mobileHasValue = target.Presence.ClientStatus.Mobile.HasValue;
        }
        else
        {
            presenceWasNull = true;
        }

        if (target.VoiceState != null && rightToMove && (force || presenceWasNull || ((desktopHasValue || webHasValue) && !mobileHasValue)))
        {
            DiscordChannel currentChannel = default;
            DiscordChannel tempCategory = default;
            DiscordChannel tempChannel2 = default;
            DiscordChannel tempChannel1 = default;

            try
            {
                var discordEmojis = DiscordEmoji.FromName(Bot.Client, ":no_entry_sign:");

                tempCategory = interaction.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                tempChannel1 = interaction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
                tempChannel2 = interaction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while creating the channels!";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                currentChannel = target.VoiceState.Channel;

                for (var i = 0; i < pokeAmount; i++)
                {
                    await target.PlaceInAsync(tempChannel1);
                    await Task.Delay(250);
                    await target.PlaceInAsync(tempChannel2);
                    await Task.Delay(250);
                }

                await target.PlaceInAsync(tempChannel1);
                await Task.Delay(250);
            }
            catch
            {
                discordEmbedBuilder.Description = "Error! User left?";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await target.PlaceInAsync(currentChannel);
            }
            catch
            {
                discordEmbedBuilder.Description = "Error! User left?";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempChannel2.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempChannel1.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
                await tempCategory.DeleteAsync();
            }
            catch
            {
                discordEmbedBuilder.Description = "Error while deleting the channels!";
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
        }
        else if (target.VoiceState == null)
        {
            discordEmbedBuilder.Description = "User is not connected!";
            await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
        else if (!rightToMove)
        {
            discordEmbedBuilder.Description = "Your not allowed to use that!";
            await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
        else if (mobileHasValue)
        {
            var description = "Their phone will explode STOP!\n";

            var discordEmojisWhiteCheckMark = DiscordEmoji.FromName(Bot.Client, ":white_check_mark:");
            var discordEmojisCheckX = DiscordEmoji.FromName(Bot.Client, ":x:");

            if (target.Presence.ClientStatus.Desktop.HasValue)
                description += discordEmojisWhiteCheckMark + " Desktop" + "\n";
            else
                description += discordEmojisCheckX + " Desktop" + "\n";

            if (target.Presence.ClientStatus.Web.HasValue)
                description += discordEmojisWhiteCheckMark + " Web" + "\n";
            else
                description += discordEmojisCheckX + " Web" + "\n";

            if (target.Presence.ClientStatus.Mobile.HasValue)
                description += discordEmojisWhiteCheckMark + " Mobile";
            else
                description += discordEmojisCheckX + " Mobile";

            discordEmbedBuilder.Description = description;

            await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        if (deleteResponseAsync)
        {
            for (var i = 3; i > 0; i--)
            {
                discordEmbedBuilder.AddField(new DiscordEmbedField("This message will be deleted in", $"{i} Seconds"));
                await interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                await Task.Delay(1000);
                discordEmbedBuilder.RemoveFieldAt(0);
            }

            await interaction.DeleteOriginalResponseAsync();
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
            discordEmbedBuilder.Description = "U are Flagged +91 u cant vote!";
        }
        else if (discordMember.Id == discordTargetMember.Id)
        {
            discordEmbedBuilder.Description = "NoNoNo we don´t do this around here! CHEATER!";
        }
        else
        {
            var sympathySystemObj = new SympathySystem
            {
                VotingUserID = discordMember.Id,
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

            discordEmbedBuilder.Description = $"You gave {discordTargetMember.Mention} the Rating {rating}";
        }

        await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
    }

    /*  /// <summary>
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
        }*/

    [SlashCommand("Move", "Move the whole channel ur in to a different one!")]
    public static async Task Move(InteractionContext interactionContext, [Option("Channel", "#..."), ChannelTypes(ChannelType.Voice)] DiscordChannel discordTargetChannel)
    {
        if (interactionContext.Member.VoiceState.Channel != null)
        {
            var source = interactionContext.Member.VoiceState.Channel;

            var members = source.Users;
            foreach (var member in members)
                await member.PlaceInAsync(discordTargetChannel);

            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }
        else
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("U are not connected!"));
        }
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
        await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
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