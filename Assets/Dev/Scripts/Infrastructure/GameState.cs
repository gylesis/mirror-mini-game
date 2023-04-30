using System;
using System.Collections.Generic;
using Dev.PlayerLogic;
using Dev.ScoreLogic;
using Dev.UI;
using Dev.Utils;
using Mirror;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class GameState : NetworkBehaviour
    {
        private ScoreController _scoreController;
        private GameStateDispatcher _gameStateDispatcher;
        private PlayerSpawnService _playerSpawnService;
        private UIService _uiService;

        private List<Player> _players = new List<Player>();

        private GameEndContext _gameEndContext;

        public void Init(ScoreController scoreController, GameStateDispatcher gameStateDispatcher,
            PlayerSpawnService playerSpawnService, UIService uiService)
        {
            _uiService = uiService;
            _playerSpawnService = playerSpawnService;
            _gameStateDispatcher = gameStateDispatcher;
            _scoreController = scoreController;
        }

        public override void OnStartServer()
        {
            _playerSpawnService.PlayerAdded += OnPlayerAdded;
            _playerSpawnService.PlayerRemoved += OnPlayerRemoved;
            _scoreController.MaxScoreReached += OnMaxScoreReached;
        }

        public override void OnStopServer()
        {
            _playerSpawnService.PlayerRemoved -= OnPlayerRemoved;
            _playerSpawnService.PlayerAdded -= OnPlayerAdded;
            _scoreController.MaxScoreReached -= OnMaxScoreReached;
        }

        private void OnPlayerRemoved(Player player)
        {
            _players.Remove(player);
        }

        private void OnPlayerAdded(Player player)
        {
            player.InputService.AllowToMove = false;

            _players.Add(player);

            var connectionsCount = NetworkServer.connections.Count;

            Debug.Log($"Connections count {connectionsCount}, prepared players {_players.Count}");

            if (_players.Count == connectionsCount)
            {
                Debug.Log($"All players prepared, Starting game");
                StartGame();
            }
        }

        [Server]
        private void OnMaxScoreReached(ScoreData winner)
        {
            var endContext = new GameEndContext();
            endContext.Winner = winner.NetworkIdentity;

            var scoreData = new ScoreData();
            scoreData.Score = winner.Score;
            scoreData.NetworkIdentity = winner.NetworkIdentity;

            endContext.WinnerScoreData = scoreData;

            _gameEndContext = endContext;

            SetPlayersInputState(false);

            NotifyEndGame(endContext);

            RestartGame();
        }

        [ClientRpc]
        private void NotifyEndGame(GameEndContext gameEndContext)
        {
            _gameStateDispatcher.EndGame(gameEndContext);
        }

        [Server]
        public void StartGame()
        {
            int time = 2;

            string body = "Starting in";

            _uiService.Curtain.ShowCountdown(body, time);

            this.DelayedCallback(time, (() => { OnGameStarted(); }));
        }

        private void OnGameStarted()
        {
            SetPlayersInputState(true);

            _playerSpawnService.FreeSpawnPoints();
        }

        [Server]
        public void RestartGame()
        {
            var str =
                $"Game is over, the winner is Player{_gameEndContext.Winner.netId}, with score {_gameEndContext.WinnerScoreData.Score}";
            _uiService.Curtain.ShowGenericText(true, str);

            var timeToFullRestart = 5;

            string body = "Restarting game in";

            _uiService.Curtain.ShowCountdown(body, timeToFullRestart);

            _players.ForEach(x => x.Rigidbody.velocity = Vector3.zero);

            this.DelayedCallback(timeToFullRestart, (Start));

            void Start()
            {
                _uiService.Curtain.ShowGenericText(false, String.Empty);

                foreach (Player player in _players)
                {
                    _playerSpawnService.PlacePlayerOnRandomPoint(player);

                    _scoreController.ResetScore(player.netIdentity);
                }


                StartGame();
            }

        }

        [Server]
        private void SetPlayersInputState(bool allowToMove)
        {
            foreach (Player player in _players)
            {
                player.InputService.AllowToMove = allowToMove;
            }
        }

    }
}