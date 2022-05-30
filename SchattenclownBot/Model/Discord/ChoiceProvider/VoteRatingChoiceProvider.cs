using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;

namespace SchattenclownBot.Model.Discord.ChoiceProvider
{
    /// <summary>
    /// The Abotype choice provider.
    /// </summary>
    public class VoteRatingChoiceProvider : IChoiceProvider
    {
        /// <summary>
        /// Providers the choices.
        /// </summary>
        /// <returns>choices</returns>
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            DiscordApplicationCommandOptionChoice[] choices = new DiscordApplicationCommandOptionChoice[5];

            for (int i = 0; i < 5; i++)
            {
                choices[i] = new DiscordApplicationCommandOptionChoice($"{i + 1}", $"{i + 1}");
            }
            await Task.Delay(1000).ConfigureAwait(false);
            return choices;
        }
    }
}