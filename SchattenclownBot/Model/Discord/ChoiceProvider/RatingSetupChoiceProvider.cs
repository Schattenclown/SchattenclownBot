using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;

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
            var choices = new DiscordApplicationCommandOptionChoice[5];

            for (var i = 0; i < 5; i++)
            {
                choices[i] = new DiscordApplicationCommandOptionChoice($"{i + 1}", $"{i + 1}");
            }
            await Task.Delay(1000);
            return choices;
        }
    }
}