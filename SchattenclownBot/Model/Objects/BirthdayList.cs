using SchattenclownBot.Model.Discord;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchattenclownBot.Model.Objects;
using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Objects
{
    public class BirthdayList
    {
        public static async Task GenerateBirthdayList()
        {
            List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.Client.Guilds.ToList();
            List<KeyValuePair<ulong, DateTime>> Birthdays = new List<KeyValuePair<ulong, DateTime>>();

            bool levelSystemVirign = true;
            do
            {
                if (DiscordBot.Client.Guilds.ToList().Count != 0)
                {
                    if (levelSystemVirign)
                    {
                        guildsList = DiscordBot.Client.Guilds.ToList();
                        
                        levelSystemVirign = false;
                    }
                }
                await Task.Delay(1000);
            } while (levelSystemVirign);

            foreach (var guildItem in guildsList)
            {
                if(guildItem.Value.Id == 928930967140331590)
                {
                    DiscordGuild discordGuildObj = DiscordBot.Client.GetGuildAsync(guildItem.Value.Id).Result;
                    IReadOnlyDictionary<ulong, DiscordMember> discordMembers = discordGuildObj.Members;
                    foreach (var discordMemberItem in discordMembers)
                    {
                        DateTime birthday = GenerateDateTime(discordMemberItem.Value);
                        DateTime dtcompare = new DateTime(9999, 9, 9);
                        if(birthday != dtcompare)
                            Birthdays.Add(new KeyValuePair<ulong, DateTime>(discordMemberItem.Value.Id, birthday));
                    }

                    break;
                }
            }
            if(Birthdays.Count != 0)
                Birthdays.Sort((ps1, ps2) => DateTime.Compare(ps1.Value.Date, ps2.Value.Date));

            string liststring = "";

            foreach (var item in Birthdays)
            {
                liststring += $"{item.Value.Day.ToString("00")}.{item.Value.Month.ToString("00")} --- <@{item.Key}>\n";
            }

            var chn = await DiscordBot.Client.GetChannelAsync(928938948221366334);
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
            eb.Color = DiscordColor.Red;
            eb.WithDescription(liststring);
            await chn.SendMessageAsync(eb.Build());
               
        }
        public static DateTime GenerateDateTime(DiscordMember discordMember)
        {
            string birthdayday = "";
            int birthdaymonth = 0;

            IEnumerable<DiscordRole> discordMemberRoleList = discordMember.Roles;

            DiscordGuild discordGuildObj = DiscordBot.Client.GetGuildAsync(discordMember.Guild.Id).Result;

            DiscordRole zehner0 = discordGuildObj.GetRole(945301296330723348);
            DiscordRole zehner1 = discordGuildObj.GetRole(945301296993427517);
            DiscordRole zehner2 = discordGuildObj.GetRole(945301298012647444);
            DiscordRole zehner3 = discordGuildObj.GetRole(945301298704683048);

            if(discordMemberRoleList.Contains(zehner0))
                birthdayday += "0";
            if(discordMemberRoleList.Contains(zehner1))
                birthdayday += "1";
            if(discordMemberRoleList.Contains(zehner2))
                birthdayday += "2";
            if(discordMemberRoleList.Contains(zehner3))
                birthdayday += "3";

            DiscordRole einer0 = discordGuildObj.GetRole(945301303649787904);
            DiscordRole einer1 = discordGuildObj.GetRole(945301749437177856);
            DiscordRole einer2 = discordGuildObj.GetRole(945301753035903036);
            DiscordRole einer3 = discordGuildObj.GetRole(945302992507252736);
            DiscordRole einer4 = discordGuildObj.GetRole(945302996630249532);
            DiscordRole einer5 = discordGuildObj.GetRole(945302999604002836);
            DiscordRole einer6 = discordGuildObj.GetRole(945303002917535845);
            DiscordRole einer7 = discordGuildObj.GetRole(945309817252237332);
            DiscordRole einer8 = discordGuildObj.GetRole(945309808096063498);
            DiscordRole einer9 = discordGuildObj.GetRole(945309830934044683);

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

            DiscordRole month1 = discordGuildObj.GetRole(945301816285990983);
            DiscordRole month2 = discordGuildObj.GetRole(945301818559324211);
            DiscordRole month3 = discordGuildObj.GetRole(945301830152372244);
            DiscordRole month4 = discordGuildObj.GetRole(945309841403043870);
            DiscordRole month5 = discordGuildObj.GetRole(945309844200624139);
            DiscordRole month6 = discordGuildObj.GetRole(945309847048577114);
            DiscordRole month7 = discordGuildObj.GetRole(945309847467999243);
            DiscordRole month8 = discordGuildObj.GetRole(945309848273297458);
            DiscordRole month9 = discordGuildObj.GetRole(945309848864718891);
            DiscordRole month10 = discordGuildObj.GetRole(945329632205475900);
            DiscordRole month11 = discordGuildObj.GetRole(945309850273988648);
            DiscordRole month12 = discordGuildObj.GetRole(945310176163004476);

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


            try
            {
                DateTime dt = new DateTime(1, birthdaymonth, Convert.ToInt32(birthdayday));
                return dt;
            }
            catch
            {
                DateTime dt = new DateTime(9999, 9, 9);
                return dt;
            }
        }
    }
}
