using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Impostor.Plugins.ImpostorCord.Discord
{
    public class MyCommands : BaseCommandModule
    {
        [Command("join")]
        [Aliases("j")]
        [Description("join the game that is connected to the current vc")]
        public async Task join(CommandContext ctx,[Description("a color from red, blue, green, pink, orange, yellow, black, white, purple, brown, cyan and lime")] string color)
        {
            int colorIndex = Array.IndexOf(Bot.InGameColors, color);
            if (colorIndex > -1) // check if color is part of colors
            {
                if (ctx.Member?.VoiceState?.Channel != null)
                {
                    bool gameFound = false;
                    foreach (KeyValuePair<string, Game> game in Bot.games)
                    {
                        if (game.Value.voiceChannel == ctx.Member.VoiceState.Channel)
                        {
                            gameFound = true;
                            Bot.games[game.Key].players[colorIndex].uid = ctx.Member;
                            await ctx.RespondAsync($"{ctx.Member.Mention} is joined as *{Bot.InGameColors[colorIndex]}*");
                            break;
                        }
                    }
                    if (!gameFound)
                    {
                        await ctx.RespondAsync("could not find a game in that voice channel");
                    }
                }
                else
                {
                    await ctx.RespondAsync("you aren't in a voice channel!");
                }
            }
            else
            {
                await ctx.RespondAsync("Invalid color!");
            }
        }
        [Command("newgame")]
        [Aliases("ng")]
        [Description("connect a game to current vc")]
        public async Task newgame(CommandContext ctx,[Description("your game code i.e. `GHBNEQ`")] string code)
        {
            if (ctx.Member?.VoiceState?.Channel != null)
            {
                if (Bot.games.Count != 0)
                {
                    try
                    {
                        if (Bot.games[code].noVC())
                        {
                            Bot.games[code].voiceChannel = ctx.Member.VoiceState.Channel;
                            Bot.games[code].gameStartingChannel = ctx.Channel;
                            await ctx.RespondAsync($"connected game {code} to vc {ctx.Member.VoiceState.Channel.Name}");
                        }
                        else
                        {
                            await ctx.RespondAsync("Game already connected to a voice channel");
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        await ctx.RespondAsync("The game you are looking for does not exist");
                    }
                }
                else
                {
                    await ctx.RespondAsync("There are no games running");
                }
            }
            else
            {
                await ctx.RespondAsync("you must join a voice channel first!");
            }
        }
        [Command("endgame")]
        [Aliases("eg")]
        [Description("disconnect game from current vc")]
        public async Task endgame(CommandContext ctx,[Description("your game code i.e. GHBNEQ")] string code)
        {
            if (ctx.Member?.VoiceState?.Channel != null)
            {
                if (Bot.games.Count != 0)
                {
                    try
                    {
                        if (Bot.games[code].voiceChannel == ctx.Member.VoiceState.Channel)
                        {
                            Bot.games[code].voiceChannel = null;
                            await ctx.RespondAsync($"ended game {code}");
                        }
                        else
                        {
                            await ctx.RespondAsync("this game is not connected to your voice channel");
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        await ctx.RespondAsync("this game does not exist");
                    }
                }
                else
                {
                    await ctx.RespondAsync("There are no games running");
                }
            }
            else
            {
                await ctx.RespondAsync("you must join a voice channel first!");
            }
        }
        [Command("forcejoin")]
        [Aliases("fj")]
        [Description("join a member as color")]
        public async Task forcejoin(CommandContext ctx,[Description("A color")] string color,[Description("mention of the member you want to join")] DiscordMember member)
        {
            int colorIndex = Array.IndexOf(Bot.InGameColors, color);
            if (colorIndex > -1)
            {
                if (member?.VoiceState?.Channel != null)
                {
                    bool gameFound = false;
                    foreach (KeyValuePair<string, Game> game in Bot.games)
                    {
                        if (game.Value.voiceChannel == member.VoiceState.Channel)
                        {
                            gameFound = true;
                            Bot.games[game.Key].players[colorIndex].uid = member;
                            await ctx.RespondAsync($"{member.Mention} is joined as {Bot.InGameColors[colorIndex]}");
                            break;
                        }
                    }
                    if (!gameFound)
                    {
                        await ctx.RespondAsync("could not find a game in that voice channel");
                    }
                }
                else
                {
                    await ctx.RespondAsync("member is not in a voice channel");
                }
            }
            else
            {
                await ctx.RespondAsync("Invalid color!");
            }
        }
        [Command("kick")]
        [Aliases("k")]
        [Description("clear specified color's member information")]
        public async Task kick(CommandContext ctx, string color)
        {
            int colorIndex = Array.IndexOf(Bot.InGameColors, color);
            if (colorIndex > -1)
            {
                if (ctx.Member?.VoiceState?.Channel != null)
                {
                    bool gameFound = false;
                    foreach (KeyValuePair<string, Game> game in Bot.games)
                    {
                        if (game.Value.voiceChannel == ctx.Member.VoiceState.Channel)
                        {
                            gameFound = true;
                            Bot.games[game.Key].players[colorIndex].uid = null;
                            await ctx.RespondAsync($"Cleared memberdata from {Bot.InGameColors[colorIndex]}");
                            break;
                        }
                    }
                    if (!gameFound)
                    {
                        await ctx.RespondAsync("Could not find a game connected to that voice channel");
                    }
                }
                else
                {
                    await ctx.RespondAsync("You must join the voice channel that you want to kick someone from");
                }
            }
            else
            {
                await ctx.RespondAsync("Invalid color!");
            }
        }
        [Command("deadtalk")]
        [Aliases("dt")]
        [Description("Disable/Enable if the dead can talk when alive players are doing tasks")]
        public async Task deadtalk(CommandContext ctx, bool deadCanTalk)
        {

            if (ctx.Member?.VoiceState?.Channel != null)
            {
                bool gameFound = false;
                foreach (KeyValuePair<string, Game> game in Bot.games)
                {
                    if (game.Value.voiceChannel == ctx.Member.VoiceState.Channel)
                    {
                        gameFound = true;
                        game.Value.DeadСanTalkDuringTasks = deadCanTalk;

                        await ctx.RespondAsync($"Dead players can talk during tasks in game `{game.Key}` is " + (deadCanTalk ? "*Enabled*" : "*Disabled*"));
                        // TODO allow change in lobby only
                        break;
                    }
                }
                if (!gameFound)
                {
                    await ctx.RespondAsync("could not find a game in that voice channel");
                }
            }
            else
            {
                await ctx.RespondAsync("you aren't in a voice channel!");
            }

        }
        [Command("players")]
        [Aliases("p")]
        [Description("list all current players")]
        public async Task players(CommandContext ctx)
        {
            if (ctx.Member?.VoiceState?.Channel != null)
            {
                bool gameFound = false;
                foreach (KeyValuePair<string, Game> game in Bot.games)
                {
                    if (game.Value.voiceChannel == ctx.Member.VoiceState.Channel)
                    {
                        gameFound = true;
                        string playersString = "";
                        int count = 0;
                        foreach (Player player in game.Value.players)
                        {
                            if (player.uid != null)
                            {
                                playersString += Bot.InGameColors[count] + " = " + player.uid.DisplayName + "\n";
                            }
                            count++;
                        }
                        if (playersString != "")
                        {
                            await ctx.RespondAsync($"```{playersString}```");
                        }
                        else
                        {
                            await ctx.RespondAsync("There are no players in that game");
                        }
                        break;
                    }
                }
                if (!gameFound)
                {
                    await ctx.RespondAsync("there is no game in your voice channel");
                }
            }
            else
            {
                await ctx.RespondAsync("You need to be in a voice channel");
            }
        }
    }
}