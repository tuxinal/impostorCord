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
        private static string[] colors = { "red", "blue", "green", "pink", "orange", "yellow", "black", "white", "purple", "brown", "cyan", "lime" };

        [Command("join")]
        [Aliases("j")]
        public async Task join(CommandContext ctx, string color)
        {
            int colorIndex = Array.IndexOf(colors, color);
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
                            await ctx.RespondAsync($"{ctx.Member.Mention} is joined as {colors[colorIndex]}");
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
        public async Task newgame(CommandContext ctx, string code)
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
        public async Task endgame(CommandContext ctx, string code)
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
        public async Task forcejoin(CommandContext ctx, string color, DiscordMember member)
        {
            int colorIndex = Array.IndexOf(colors, color);
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
                            await ctx.RespondAsync($"{member.Mention} is joined as {colors[colorIndex]}");
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
        public async Task kick(CommandContext ctx, string color)
        {
            int colorIndex = Array.IndexOf(colors, color);
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
                            await ctx.RespondAsync($"Cleared memberdata from {colors[colorIndex]}");
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
    }
}