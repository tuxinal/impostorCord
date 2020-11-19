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
        public static Config config;
        public Bot(Config _config)
        {
            config=_config;

            if (config.BotProxyEnabled)
            {
                _proxy = new WebProxy(config.BotProxyAddress);
                _proxy.Credentials = new NetworkCredential(config.BotProxyUsername, config.BotProxyPassword);;
            }

            MainAsync(config.Token).ConfigureAwait(false).GetAwaiter().GetResult();
            commands = client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { config.Prefix }
            });
            commands.RegisterCommands<MyCommands>();
        }

        static async Task MuteDiscordMember(Player player, bool needMute)
        {
            if(player.uid == null) return;
            try
            {
                if(player.game.DeadСanTalkDuringTasks)
                {
                    needMute ^= player.isDead; // mute dead + unmute alive or vice versa
                    var needDeaf = !player.isDead & needMute; // deaf only alive
                    if(needDeaf != player.isDeaf)
                    {
                        player.isDeaf = needDeaf;
                        await player.uid.SetDeafAsync(needDeaf);
                    }
                }
                else
                {
                    needMute |= player.isDead; // mute all ; unmute only alive
                }

                if(needMute != player.isMute)
                {
                    player.isMute = needMute;
                    await player.uid.SetMuteAsync(needMute);
                }

            } catch
            { // TODO better log
                System.Console.WriteLine("! Exception in Discord API method");
            }
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

                            foreach (Player player in game.Value.players)
                            { //getting through each player to find the one connected to the member
                                if (player.uid == member)
                                {
                                    player.isDead = false;
                                    await MuteDiscordMember(player, false);
                                    player.uid = null;
                                    await startChannel.SendMessageAsync($"{member.Mention} has left");
                                }
                            }
                        }
                    }
                }
                return;
            };


            await client.ConnectAsync();
        }
        public static async Task Tasks(string code)
        {
            foreach (Player player in games[code].players)
            {
                if (player.uid == null)
                    continue;

                MuteDiscordMember(player, true);
            }
        }
        public static async Task Lobby(string code)
        {

            foreach (Player player in games[code].players)
            {
                player.isDead = false;

                if (player.uid == null)
                    continue;

                MuteDiscordMember(player, false);
            }
        }
        public static async Task Meeting(string code)
        {

            foreach (Player player in games[code].players)
            {
                if (player.uid == null)
                    continue;

                MuteDiscordMember(player, false);
            }
        }
    }


}