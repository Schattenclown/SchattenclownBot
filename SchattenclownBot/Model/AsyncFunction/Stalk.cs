using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class Stalk
    {
        /*
        public static async Task StalkAsync(int executeSecond)
        {
            await Task.Run(async () =>
            {
                while (DateTime.Now.Second != executeSecond)
                {
                    await Task.Delay(1000);
                }

                List<DiscordGuild> guildList;
                do
                {
                    guildList = Bot.Client.Guilds.Values.ToList();
                    await Task.Delay(1000);
                } while (guildList.Count == 0);

                //Himari ID => 585177741003980859

                while (true)
                {
                    await Task.Delay(1000);
                    foreach(DiscordGuild guild in guildList)
                    {
                        foreach (var member in guild.Members.Where(x => x.Key == 585177741003980859))
                        {
                            if (member.Value.VoiceState != null)
                            {
                                //Bot.Client.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithTitle("Himari wurde gesichtet!").WithColor(new DiscordColor(0,255,40)).Build()));
                            }
                        }
                    }
                }

            });
        }
        */
        internal static Task StalkEvent(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User.Id == 585177741003980859 && e.Channel != null && e.Channel.Users.Where(x => x.Id == 304366130238193664).Count() == 0)
            {
                var invite = e.Channel.CreateInviteAsync().Result;
                client.GetChannelAsync(949720467567149116).Result.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithTitle("Himari wurde gesichtet!").WithColor(new DiscordColor(0, 255, 40)).WithDescription("Invite wurde erstellt").WithUrl(invite.Url).Build()));
            }
            return Task.CompletedTask;
        }
    }
}
