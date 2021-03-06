using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Objects;
using System.Linq;
using System.Threading.Tasks;
using SchattenclownBot.Model.Discord.Main;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
    internal class VoteSystem : ApplicationCommandsModule
    {
        /// <summary>
        ///     Command to give an User a Rating.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="discordUser">The discordUser</param>
        /// <returns></returns>
        [SlashCommand("GiveRating" + Bot.isDevBot, "Give an User a rating!")]
        public static async Task GiveRatingAsync(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
            discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
            discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
            discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

            DiscordSelectComponent discordSelectComponent = new("Select a Rating!", discordSelectComponentOptionList, "give_rating");

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Give <@{discordUser.Id}> a Rating!"));
        }

        /// <summary>
        ///     Poke an User with the Context Menu.
        /// </summary>
        /// <param name="contextMenuContext">The contextMenuContext</param>
        /// <returns></returns>
        [ContextMenu(ApplicationCommandType.User, "Give Rating!")]
        public static async Task GiveRatingAsync(ContextMenuContext contextMenuContext)
        {
            DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[5];
            discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Rate 1", "rating_1", emoji: new DiscordComponentEmoji("😡"));
            discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Rate 2", "rating_2", emoji: new DiscordComponentEmoji("⚠️"));
            discordSelectComponentOptionList[2] = new DiscordSelectComponentOption("Rate 3", "rating_3", emoji: new DiscordComponentEmoji("🆗"));
            discordSelectComponentOptionList[3] = new DiscordSelectComponentOption("Rate 4", "rating_4", emoji: new DiscordComponentEmoji("💎"));
            discordSelectComponentOptionList[4] = new DiscordSelectComponentOption("Rate 5", "rating_5", emoji: new DiscordComponentEmoji("👑"));

            DiscordSelectComponent discordSelectComponent = new("Select a Rating!", discordSelectComponentOptionList, "give_rating");

            await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Give <@{contextMenuContext.TargetMember.Id}> a Rating!"));
        }

        public static async Task GaveRating(DiscordClient sender, ComponentInteractionCreateEventArgs eventArgs)
        {
            if (eventArgs.Values.Length > 0)
            {
                switch (eventArgs.Values[0])
                {
                    case "rating_1":
                        await VoteRatingAsync(eventArgs, 1);
                        break;
                    case "rating_2":
                        await VoteRatingAsync(eventArgs, 2);
                        break;
                    case "rating_3":
                        await VoteRatingAsync(eventArgs, 3);
                        break;
                    case "rating_4":
                        await VoteRatingAsync(eventArgs, 4);
                        break;
                    case "rating_5":
                        await VoteRatingAsync(eventArgs, 5);
                        break;
                }
            }
        }

        /// <summary>
        ///     The function that creates or edits the database based on the new rating.
        /// </summary>
        /// <param name="componentInteractionCreateEventArgs">The eventArgs.</param>
        /// <param name="rating">The rating value.</param>
        /// <returns></returns>
        public static async Task VoteRatingAsync(ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs, int rating)
        {
            DiscordMember discordMember = componentInteractionCreateEventArgs.User.ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;
            DiscordMember discordTargetMember = componentInteractionCreateEventArgs.Message.MentionedUsers[0].ConvertToMember(componentInteractionCreateEventArgs.Guild).Result;

            bool foundTargetMemberInDb = false;
            bool memberIsFlagged91 = false;
            DiscordEmbedBuilder discordEmbedBuilder = new();

            if (componentInteractionCreateEventArgs.Guild.Id == 928930967140331590)
            {
                DiscordRole discordRole = componentInteractionCreateEventArgs.Guild.GetRole(980071522427363368);
                if (discordMember != null && discordMember.Roles.Contains(discordRole))
                {
                    memberIsFlagged91 = true;
                }
            }

            if (memberIsFlagged91)
            {
                discordEmbedBuilder.Description = "U are Flagged +91 u cant vote!";
            }
            else if (discordMember != null && discordMember.Id == discordTargetMember.Id)
            {
                discordEmbedBuilder.Description = "NoNoNo we don´t do this around here! CHEATER!";
            }
            else
            {
                if (discordMember != null)
                {
                    SympathySystem sympathySystemObj = new()
                    {
                        VotingUserId = discordMember.Id,
                        VotedUserId = discordTargetMember.Id,
                        GuildId = componentInteractionCreateEventArgs.Guild.Id,
                        VoteRating = rating
                    };

                    System.Collections.Generic.List<SympathySystem> sympathySystemsList = SympathySystem.ReadAll(componentInteractionCreateEventArgs.Guild.Id);

                    foreach (SympathySystem dummy in sympathySystemsList.Where(sympathySystemItem => sympathySystemItem.VotingUserId == sympathySystemObj.VotingUserId && sympathySystemItem.VotedUserId == sympathySystemObj.VotedUserId))
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
                }

                discordEmbedBuilder.Description = $"You gave {discordTargetMember.Mention} the Rating {rating}";
            }

            await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        ///     Command to view how many and what ratings a given user has.
        /// </summary>
        /// <param name="interactionContext">The interactionContext.</param>
        /// <param name="discordUser">The Discord User.</param>
        /// <returns></returns>
        [SlashCommand("ShowRating" + Bot.isDevBot, "Shows the rating of an user!")]
        public static async Task ShowRatingAsync(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            string description = "```\n";
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            for (int i = 1; i < 6; i++)
                description += $"Rating with {i}: {SympathySystem.GetUserRatings(interactionContext.Guild.Id, discordUser.Id, i)}\n";
            description += "```";
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = $"Votes for {discordUser.Username}",
                Color = DiscordColor.Purple,
                Description = description
            };

            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /*/// <summary>
        ///     Setup assist for the Rating Roles.
        /// </summary>
        /// <param name="interactionContext">The interactionContext.</param>
        /// <param name="voteRating">The RatingValue the role stands for.</param>
        /// <param name="discordRole">The discordRole.</param>
        /// <returns></returns>
        [SlashCommand("RatingSetup" + Bot.isDevBot, "Set up the roles for the Rating System!")]
        public static async Task RatingSetup(InteractionContext interactionContext, [ChoiceProvider(typeof(RatingSetupChoiceProvider))][Option("Vote", "Setup")] string voteRating, [Option("Role", "@...")] DiscordRole discordRole)
        {
            bool found = SympathySystem.CheckRoleInfoExists(interactionContext.Guild.Id, Convert.ToInt32(voteRating));

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Setting Role!"));

            SympathySystem sympathySystemObj = new()
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
    }
}
