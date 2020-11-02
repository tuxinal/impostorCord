using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System;

namespace Impostor.Plugins.ImpostorCord.Discord
{

    public class Bot
    {
        public static Dictionary<string, Game> games = new Dictionary<string, Game>();
        public static DiscordClient client;
        static CommandsNextExtension commands;
        static private WebProxy _proxy;
        static private ICredentials credentials;
        public Bot(string token, string prefix, string proxy,string proxyUserName,string proxyPassword)
        {
            credentials = new NetworkCredential(proxyUserName,proxyPassword);
            _proxy = new WebProxy(proxy);
            _proxy.Credentials = credentials;
            MainAsync(token).ConfigureAwait(false).GetAwaiter().GetResult();
            commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { prefix }
            });
            commands.RegisterCommands<MyCommands>();
        }
        static async Task MainAsync(string token)
        {
            client = new DiscordClient(new DiscordConfiguration { Token = token, TokenType = TokenType.Bot, Proxy = _proxy });

            client.VoiceStateUpdated += async (DiscordClient client, VoiceStateUpdateEventArgs e) => //making sure to disconnect someone from their player when they leave the vc
            {
                if (e.Before.Channel != null && e.After.Channel == null)
                { // if they were in a channel BEFORE and they are not in a channel AFTER
                    foreach (KeyValuePair<string, Game> game in games)
                    {
                        if (game.Value.voiceChannel == e.Before.Channel)
                        { //checking through all games to find the one connected to the vc
                            DiscordMember member = e.Before.User as DiscordMember; // cast iscordUser to Member because member has a lot more control
                            DiscordChannel startChannel = game.Value.gameStartingChannel; // get the channel that the game started from
                            int count = 0;
                            foreach (Player player in game.Value.players)
                            { //getting through each player to find the one connected to the member
                                if (player.uid == member)
                                {
                                    games[game.Key].players[count].uid = null;
                                    await member.SetDeafAsync(false);
                                    await member.SetMuteAsync(false); // unmute and undeafen
                                    await startChannel.SendMessageAsync($"{member.Mention} has left");
                                }
                                count++;
                            }
                        }
                    }
                }
                return;
            };


            await client.ConnectAsync();
        }
        public static async Task Tasks(string code,int delay)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            foreach (Player player in games[code].players)
            {
                if (player.uid != null)
                {
                    if (!player.isDead)
                    {
                        await player.uid.SetDeafAsync(true);
                        await player.uid.SetMuteAsync(true);
                    }
                    else
                    {
                        await player.uid.SetDeafAsync(false);
                        await player.uid.SetMuteAsync(false);

                    }
                }
            }
        }
        public static async Task Lobby(string code)
        {
            int count = 0;
            foreach (Player player in games[code].players)
            {
                games[code].players[count].isDead = false;
                if (player.uid != null)
                {
                    await player.uid.SetMuteAsync(false);
                    await player.uid.SetDeafAsync(false);
                }
                count++;
            }
        }
        public static async Task Meeting(string code)
        {
            int count = 0;
            foreach (Player player in games[code].players)
            {
                if (player.uid != null)
                {
                    if (!player.isDead)
                    {
                        await player.uid.SetMuteAsync(false);
                        await player.uid.SetDeafAsync(false);
                    }
                    else
                    {
                        await player.uid.SetMuteAsync(true);
                        await player.uid.SetDeafAsync(false);

                    }
                }
                count++;
            }
        }
    }


}