﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Discord.AppCommands
{
   /// <summary>
   ///    Shows advanced usage of ChoiceProvider attribute with Reflection.
   ///    Notice how Ping inherits the ApplicationCommandsModule.
   /// </summary>
   public class TriggerHelp : ApplicationCommandsModule
   {
      /// <summary>
      ///    Slash command registers the name and command description.
      /// </summary>
      /// <param name="context">Interaction context</param>
      /// <param name="commandName">The name of the command to get help on</param>
      [SlashCommand("trigger_help" + Bot.isDevBot, "Sends the help menu for the bot.")]
      public static async Task Command(InteractionContext context,
         // ChoiceProvider calls the Provider() method, which gives a list of slash commands. This is called once, when commands are being registered to Discord.
         [ChoiceProvider(typeof(TriggerHelpChoiceProvider)), Option("command", "The name of the command to get help on.")] string commandName)
      {
         // Using the TriggerHelpChoiceProvider class, we know that whatever the user gives us *should* be in the Commands dictionary, provided that they use tab completion. If they don't, say we couldn't find the command. 
         if (!TriggerHelpChoiceProvider.Commands.TryGetValue(commandName, out MethodInfo command))
         {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
               Content = $"Error: Command {Formatter.InlineCode(Formatter.Sanitize(commandName))} not found!", IsEphemeral = true
            });
            return;
         }

         // HandlerReader the command's description from the SlashCommand attribute we used to register the command.
         SlashCommandAttribute slashCommandAttribute = command.GetCustomAttribute<SlashCommandAttribute>();
         DiscordEmbedBuilder discordEmbedBuilder = new()
         {
            Title = '/' + commandName, Description = slashCommandAttribute.Description, Color = new DiscordColor("#7b84d1")
         };

         // If the guild has a custom guild icon, set the embed's thumbnail to that icon.
         if (context.Guild != null && context.Guild.IconUrl != null)
         {
            // CHALLENGE: Replace the jpg to the highest resolution png file using the Discord RunInnerHandlerAsync.
            discordEmbedBuilder.WithThumbnail(context.Guild.IconUrl);
         }

         // Iterate through each of the command's parameters, selecting only the ones that have the Option attribute.
         foreach (ParameterInfo parameter in command.GetParameters())
         {
            OptionAttribute parameterChoice = parameter.GetCustomAttribute<OptionAttribute>(false);
            // If the option attribute doesn't exist on the method argument, skip it.
            if (parameterChoice == null)
            {
               continue;
            }

            // Add the argument's description to the embed, specifying if it's optional or required.
            discordEmbedBuilder.AddField(new DiscordEmbedField((parameter.IsOptional ? "(Optional) " : "(Required) ") + parameterChoice.Name, $"**Type:** {parameter.ParameterType.Name}\n**Description:** {parameterChoice.Description}"));
         }

         DiscordInteractionResponseBuilder discordInteractionResponseBuilder = new();
         discordInteractionResponseBuilder.AddEmbed(discordEmbedBuilder);
         await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
      }
   }

   /// <summary>
   ///    Choice provider for manage_permissions command
   /// </summary>
   public class TriggerHelpChoiceProvider : IChoiceProvider
   {
      internal static Dictionary<string, MethodInfo> Commands = new();

      /// <summary>
      ///    Using Reflection, we search our program for ApplicationCommandsModules and register them as commands.
      /// </summary>
      /// <returns>A list of application slash commands.</returns>
      public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
      {
         List<DiscordApplicationCommandOptionChoice> discordApplicationCommandOptionChoices = new();

         // All top level command classes
         IEnumerable<Type> commandClasses = Assembly.GetEntryAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(ApplicationCommandsModule)) && !type.IsNested);

         // Find all command or subgroup commands from the classes
         foreach (Type command in commandClasses)
         {
            SearchCommands(command);
         }

         // SearchCommands registers the commands into a Dictionary<string, MethodInfo>. Since we only need the command name, we can just select the keys.
         foreach (string commandName in Commands.Keys)
         {
            // Create the new choice option: new DiscordApplicationCommandOptionChoice("public name", "value"). Very similar to a dictionary.
            DiscordApplicationCommandOptionChoice discordApplicationCommandOptionChoice = new(commandName, commandName);
            // Add the new option to the other options.
            discordApplicationCommandOptionChoices.Add(discordApplicationCommandOptionChoice);
         }

         // Sort the options alphabetically, in case Discord doesn't do that for us already.
         discordApplicationCommandOptionChoices.Sort((DiscordApplicationCommandOptionChoice x, DiscordApplicationCommandOptionChoice y) => x.Name.CompareTo(y.Name));

         // Return our commands to the help function.
         return Task.FromResult(discordApplicationCommandOptionChoices.AsEnumerable());
      }

      /// <summary>
      ///    Adding all commands and subcommands to Commands field.
      /// </summary>
      /// <param name="type">Type of the command.</param>
      /// <param name="commandName">Name of the command.</param>
      public static void SearchCommands(Type type, string commandName = "")
      {
         // HandlerReader all nested group commands in the type variable/class
         IEnumerable<Type> nestedTypes = type.GetNestedTypes().Where(type => type?.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
         // If any nested group commands are available
         if (nestedTypes.Any())
         {
            // Iterate through each subgroup command
            foreach (Type nestedType in nestedTypes)
            {
               SlashCommandGroupAttribute slashCommandGroupAttribute = nestedType.GetCustomAttribute<SlashCommandGroupAttribute>();
               // Add the group command to the previous command name. This means it'd look like:
               // /groupCommand subcommand
               // instead of:
               // /subcommand
               commandName += ' ' + slashCommandGroupAttribute.Name;
               // There are still nested classes, throw it back into the recursive loop.
               SearchCommands(nestedType, commandName);
            }
         }
         else
         {
            // HandlerReader all slash commands in the class
            IEnumerable<MethodInfo> commands = type.GetMethods().Where(method => method.GetCustomAttribute<SlashCommandAttribute>() != null);
            if (commands.Any())
            {
               foreach (MethodInfo command in commands)
               {
                  SlashCommandAttribute slashCommandAttribute = command.GetCustomAttribute<SlashCommandAttribute>();
                  // We assign a temporary variable here, because if we added it to commandName, then our commands would look like:
                  // /groupCommand test
                  // /groupCommand test delete
                  // /groupCommand test delete create
                  // instead of:
                  // /groupCommand test
                  // /groupCommand delete
                  // /groupCommand create
                  string subCommand = commandName + ' ' + slashCommandAttribute.Name;
                  Commands.Add(subCommand.Trim(), command);
               }
            }
         }
      }
   }
}