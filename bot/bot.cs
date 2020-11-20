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
        public readonly static string[] InGameColors = { "red", "blue", "green", "pink", "orange", "yellow", "black", "white", "purple", "brown", "cyan", "lime" };
        public readonly static string[] EmojiList = {"🟥", "🇧", "🟩", "💗", "🟧", "🟨", "⬛", "⬜", "🟪", "🟫", "🇨", "✳"};
        private readonly static DiscordEmbed embedTemplate = new DiscordEmbedBuilder()
                                .WithDescription("Players, select your in-game color")
                                .WithColor(new DiscordColor(0xB21010)).Build();
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
            commands.SetHelpFormatter<HelpFormatter>();
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
                if (e.Before.Channel != e.After.Channel)
                {
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
                                    break;
                                }
                            }
                        }
                    }
                }
                return;
            };


            client.MessageReactionAdded  += MessageReactionAddedRemoved;
            client.MessageReactionRemoved += MessageReactionAddedRemoved;

            await client.ConnectAsync();
        }

        private static async Task MessageReactionAddedRemoved(DiscordClient s, DiscordEventArgs _e)
        {
            bool added=false;
            dynamic e;
            try {
                e = (MessageReactionAddEventArgs)_e;
                added=true;
            }
            catch(InvalidCastException)
            {
                e = (MessageReactionRemoveEventArgs)_e;
            }

            if(e.Message.Author!=client.CurrentUser)
                return;

            foreach (var game in games)
            {
                if (game.Value.startMessage != e.Message)
                    continue;

                if (((DiscordMember)e.User).VoiceState.Channel != game.Value.voiceChannel)
                    return;

                //check right emoji
                int eid;
                if(-1 == (eid = Array.IndexOf(EmojiList, e.Emoji.Name)))
                    return;

                var player = game.Value.players[eid];
                if(added)
                {
                    if(player.uid==null)
                        player.uid = (DiscordMember)e.User; // TODO only 1 color per DiscordMember
                }
                else
                {
                    if(player.uid == (DiscordMember)e.User)
                        player.uid = null;
                }

                await e.Message.ModifyAsync(null, buildMessage(game.Key, game.Value.voiceChannel.Name, game.Value.players));
                return;
            }
        }


        public static DiscordEmbed buildMessage(string code, string vcname, Player[] players)
        {
            var embedBuilder = new DiscordEmbedBuilder(embedTemplate).WithAuthor("🚀 "+code).WithTitle("🔊 "+vcname);
            int i=0;
            foreach (var player in players)
            {
                embedBuilder.AddField(EmojiList[i]+InGameColors[i], player.uid==null ? "-" : player.uid.Mention, true);
                i++;
            }
            return embedBuilder.Build();
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