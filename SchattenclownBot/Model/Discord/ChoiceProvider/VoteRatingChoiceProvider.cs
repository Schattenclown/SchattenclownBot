using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            DiscordApplicationCommandOptionChoice[] choices = new DiscordApplicationCommandOptionChoice[10];

            for (int i = 0; i < 10; i++)
            {
                choices[i] = new DiscordApplicationCommandOptionChoice($"{i + 1}", $"{i + 1}");
            }

            return choices;
        }
    }
}