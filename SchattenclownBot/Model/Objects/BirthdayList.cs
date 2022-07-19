using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Objects
{
    public class BirthdayList
    {
        public static async Task GenerateBirthdayList()
        {
            var guildsList = Bot.Client.Guilds.ToList();
            var Birthdays = new List<KeyValuePair<ulong, DateTime>>();

            var levelSystemVirgin = true;
            do
            {
                if (Bot.Client.Guilds.ToList().Count != 0)
                {
                    guildsList = Bot.Client.Guilds.ToList();

                    levelSystemVirgin = false;
                }
                await Task.Delay(1000);
            } while (levelSystemVirgin);

            foreach (var guildItem in guildsList)
            {
                if (guildItem.Value.Id == 928930967140331590)
                {
                    var discordGuildObj = Bot.Client.GetGuildAsync(guildItem.Value.Id).Result;
                    var discordMembers = discordGuildObj.Members;
                    foreach (var discordMemberItem in discordMembers)
                    {
                        var birthday = GenerateDateTime(discordMemberItem.Value);
                        var dtcompare = new DateTime(9999, 9, 9);
                        if (birthday != dtcompare)
                            Birthdays.Add(new KeyValuePair<ulong, DateTime>(discordMemberItem.Value.Id, birthday));
                    }
                    break;
                }
            }

            if (Birthdays.Count != 0)
                Birthdays.Sort((ps1, ps2) => DateTime.Compare(ps1.Value.Date, ps2.Value.Date));

            var liststring = "";

            foreach (var item in Birthdays)
            {
                liststring += $"{item.Value.Day.ToString("00")}.{item.Value.Month.ToString("00")} --- <@{item.Key}>\n";
            }

            foreach (var guildItem in guildsList)
            {
                if (guildItem.Value.Id == 928930967140331590)
                {
                    var chn = await Bot.Client.GetChannelAsync(928938948221366334);
                    var eb = new DiscordEmbedBuilder();
                    eb.Color = DiscordColor.Red;
                    eb.WithDescription(liststring);
                    await chn.SendMessageAsync(eb.Build());
                    break;
                }
            }
        }
        public static DateTime GenerateDateTime(DiscordMember discordMember)
        {
            var birthdayday = "";
            var birthdaydayint = 0;
            var birthdaymonth = 0;

            var discordMemberRoleList = discordMember.Roles.ToList();

            var discordGuildObj = Bot.Client.GetGuildAsync(discordMember.Guild.Id).Result;

            var zehner0 = discordGuildObj.GetRole(945301296330723348);
            var zehner1 = discordGuildObj.GetRole(945301296993427517);
            var zehner2 = discordGuildObj.GetRole(945301298012647444);
            var zehner3 = discordGuildObj.GetRole(945301298704683048);

            if (discordMemberRoleList.Contains(zehner0))
                birthdayday += "0";
            if (discordMemberRoleList.Contains(zehner1))
                birthdayday += "1";
            if (discordMemberRoleList.Contains(zehner2))
                birthdayday += "2";
            if (discordMemberRoleList.Contains(zehner3))
                birthdayday += "3";

            var einer0 = discordGuildObj.GetRole(945301303649787904);
            var einer1 = discordGuildObj.GetRole(945301749437177856);
            var einer2 = discordGuildObj.GetRole(945301753035903036);
            var einer3 = discordGuildObj.GetRole(945302992507252736);
            var einer4 = discordGuildObj.GetRole(945302996630249532);
            var einer5 = discordGuildObj.GetRole(945302999604002836);
            var einer6 = discordGuildObj.GetRole(945303002917535845);
            var einer7 = discordGuildObj.GetRole(945309817252237332);
            var einer8 = discordGuildObj.GetRole(945309808096063498);
            var einer9 = discordGuildObj.GetRole(945309830934044683);

            if (discordMemberRoleList.Contains(einer0))
                birthdayday += "0";
            if (discordMemberRoleList.Contains(einer1))
                birthdayday += "1";
            if (discordMemberRoleList.Contains(einer2))
                birthdayday += "2";
            if (discordMemberRoleList.Contains(einer3))
                birthdayday += "3";
            if (discordMemberRoleList.Contains(einer4))
                birthdayday += "4";
            if (discordMemberRoleList.Contains(einer5))
                birthdayday += "5";
            if (discordMemberRoleList.Contains(einer6))
                birthdayday += "6";
            if (discordMemberRoleList.Contains(einer7))
                birthdayday += "7";
            if (discordMemberRoleList.Contains(einer8))
                birthdayday += "8";
            if (discordMemberRoleList.Contains(einer9))
                birthdayday += "9";

            var month1 = discordGuildObj.GetRole(945301816285990983);
            var month2 = discordGuildObj.GetRole(945301818559324211);
            var month3 = discordGuildObj.GetRole(945301830152372244);
            var month4 = discordGuildObj.GetRole(945309841403043870);
            var month5 = discordGuildObj.GetRole(945309844200624139);
            var month6 = discordGuildObj.GetRole(945309847048577114);
            var month7 = discordGuildObj.GetRole(945309847467999243);
            var month8 = discordGuildObj.GetRole(945309848273297458);
            var month9 = discordGuildObj.GetRole(945309848864718891);
            var month10 = discordGuildObj.GetRole(945329632205475900);
            var month11 = discordGuildObj.GetRole(945309850273988648);
            var month12 = discordGuildObj.GetRole(945310176163004476);

            if (discordMemberRoleList.Contains(month1))
                birthdaymonth = 1;
            if (discordMemberRoleList.Contains(month2))
                birthdaymonth = 2;
            if (discordMemberRoleList.Contains(month3))
                birthdaymonth = 3;
            if (discordMemberRoleList.Contains(month4))
                birthdaymonth = 4;
            if (discordMemberRoleList.Contains(month5))
                birthdaymonth = 5;
            if (discordMemberRoleList.Contains(month6))
                birthdaymonth = 6;
            if (discordMemberRoleList.Contains(month7))
                birthdaymonth = 7;
            if (discordMemberRoleList.Contains(month8))
                birthdaymonth = 8;
            if (discordMemberRoleList.Contains(month9))
                birthdaymonth = 9;
            if (discordMemberRoleList.Contains(month10))
                birthdaymonth = 10;
            if (discordMemberRoleList.Contains(month11))
                birthdaymonth = 11;
            if (discordMemberRoleList.Contains(month12))
                birthdaymonth = 12;

            birthdaydayint = Convert.ToInt32(birthdayday);

            DateTime dateTime;

            if (birthdaymonth is >= 1 and <= 12 && birthdaydayint is >= 1 and <= 31)
            {
                try
                {
                    dateTime = new DateTime(1, birthdaymonth, birthdaydayint);
                    return dateTime;
                }
                catch
                {
                    dateTime = new DateTime(9999, 9, 9);
                    return dateTime;
                }
            }

            dateTime = new DateTime(9999, 9, 9);
            return dateTime;
        }
    }
}
