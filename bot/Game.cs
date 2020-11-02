using DSharpPlus.Entities;

namespace Impostor.Plugins.ImpostorCord.Discord
{
    public class Game
    {
        public DiscordChannel gameStartingChannel; // storing the channel that the game started in
        public DiscordChannel voiceChannel;
        public Player[] players = {new Player(), // red
            new Player(), // blue
            new Player(), // green
            new Player(), // pink
            new Player(), // orange
            new Player(), // yellow
            new Player(), // black
            new Player(), // white
            new Player(), // purple
            new Player(), // brown
            new Player(), // cyan
            new Player()}; //lime
        public bool noVC()
        {
            if (voiceChannel == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class Player
    {
        public bool isDead = false;
        public DiscordMember uid;
    }
}