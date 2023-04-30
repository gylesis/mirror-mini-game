using Dev.Infrastructure.StaticData;
using Dev.PlayerLogic;
using Dev.ScoreLogic;
using Dev.UI;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class DependenciesContainer : MonoBehaviour
    {
        [SerializeField] private PlayerSpawnService _playerSpawnService;
        [SerializeField] private PlayerDashCollisionsTrackService _playerDashCollisionsTrackService;
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private ScoreController _scoreController;
        [SerializeField] private GameState _gameState;
        [SerializeField] private UIService _uiService;


        public GameState GameState => _gameState;
        public UIService UIService => _uiService;
        public PlayerSpawnService PlayerSpawnService => _playerSpawnService;
        public PlayerDashCollisionsTrackService PlayerDashCollisionsTrackService => _playerDashCollisionsTrackService;
        public GameSettings GameSettings => _gameSettings;
        public ScoreController ScoreController => _scoreController;

        public void Init()
        {
            GameStateDispatcher gameStateDispatcher = new GameStateDispatcher();
            gameStateDispatcher.Add(_playerSpawnService);

            _gameState.Init(_scoreController, gameStateDispatcher, _playerSpawnService, _uiService);

            _scoreController.Init(_playerSpawnService, _gameSettings);
            _playerDashCollisionsTrackService.Init(_gameSettings, _playerSpawnService, _scoreController);
            _playerSpawnService.Init(_playerDashCollisionsTrackService, _gameSettings);
        }
    }
}