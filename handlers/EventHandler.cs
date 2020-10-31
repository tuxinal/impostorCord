using Microsoft.Extensions.Logging;
using Impostor.Api.Events;
using Impostor.Api.Events.Meeting;
using Impostor.Plugins.ImpostorCord.Discord;


namespace Impostor.Plugins.ImpostorCord.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<ImpostorCord> _logger;
        private Bot _bot;

        public GameEventListener(ILogger<ImpostorCord> logger, Bot bot)
        {
            _logger = logger;
            _bot = bot;
        }
        [EventListener]
        public async void OnMeetingStarted(IMeetingStartedEvent e)
        {
            foreach (var player in e.Game.Players)
            {
                if(player.Character.PlayerInfo.IsDead){
                    Bot.games[e.Game.Code.Code].players[player.Character.PlayerInfo.ColorId].isDead = true;
                }
            }
            await Bot.Meeting(e.Game.Code.Code); 
        }
        [EventListener]
        public async void OnMeetingEnded(IMeetingEndedEvent e)
        {
            await Bot.Tasks(e.Game.Code.Code);
        }
        [EventListener]
        public async void OnGameStarted(IGameStartedEvent e)
        {
            await Bot.Tasks(e.Game.Code.Code);

        }

        [EventListener]
        public async void OnGameEnded(IGameEndedEvent e)
        {
            await Bot.Lobby(e.Game.Code.Code);
        }
        [EventListener]
        public void OnGameCreated(IGameCreatedEvent e)
        {
            Bot.games.Add(e.Game.Code.Code,new Game());
        }
        [EventListener]
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            Bot.games.Remove(e.Game.Code.Code);
        }

    }
}