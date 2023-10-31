using System.Collections.Generic;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Integrations.Discord.ChoiceProvider
{
    public class RatingSetupChoiceProvider : IChoiceProvider
    {
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