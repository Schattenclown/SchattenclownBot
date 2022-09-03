using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.ChoiceProvider
{
    /// <summary>
    ///     sThe rating value setup choice provider.
    /// </summary>
    public class RatingSetupChoiceProvider : IChoiceProvider
    {
        /// <summary>
        ///     Providers the choices.
        /// </summary>
        /// <returns>choices</returns>
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            DiscordApplicationCommandOptionChoice[] choices = new DiscordApplicationCommandOptionChoice[5];

            for (int i = 0; i < 5; i++)
            {
                choices[i] = new DiscordApplicationCommandOptionChoice($"{i + 1}", $"{i + 1}");
            }
            await Task.Delay(1000);
            return choices;
        }
    }
}