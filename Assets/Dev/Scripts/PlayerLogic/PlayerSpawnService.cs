using System;
using System.Collections.Generic;
using System.Linq;
using Dev.CameraLogic;
using Dev.Infrastructure;
using Dev.Infrastructure.StaticData;
using Dev.Utils;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dev.PlayerLogic
{
    public class PlayerSpawnService : NetworkBehaviour, IGameEndedListener
    {
        [SerializeField] private SpawnPoint[] _spawnPoints;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private CameraController _cameraControllerPrefab;
        [SerializeField] private InputService _inputService;
        [SerializeField] private PlayerMovementService _playerMovementService;

        private readonly SyncList<Player> _players = new SyncList<Player>();

        private Dictionary<NetworkConnectionToClient, Player> _playersAlive =
            new Dictionary<NetworkConnectionToClient, Player>();

        private Dictionary<Player, List<GameObject>> _playerAuthorities = new Dictionary<Player, List<GameObject>>();


        public List<Player> Players => _players.ToList();

        public event Action<Player> PlayerAdded;
        public event Action<Player> PlayerRemoved;

        private PlayerDashCollisionsTrackService _playerDashCollisionsTrackService;
        private GameSettings _gameSettings;

        public void Init(PlayerDashCollisionsTrackService playerDashCollisionsTrackService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _playerDashCollisionsTrackService = playerDashCollisionsTrackService;
        }

        public List<Player> GetPlayersBesidesMe(NetworkIdentity networkIdentity)
        {
            var otherPlayers = _players.Where(x => x.netId != networkIdentity.netId).ToList();

            return otherPlayers;
        }

        public GameObject SpawnPlayer(NetworkConnectionToClient conn)
        {
            //Debug.Log($"Spawn player");

            Player player = SetPlayer(conn);

            PlayerMovementService playerMovementService = SetPlayerController(conn, player);
            CameraController cameraController = SetCamera(player, conn);
            InputService inputService = SetInputService(conn, cameraController);

            player.RpcInit(inputService, _gameSettings, playerMovementService);

            player.PlayerMovementService.DashPressed += OnPlayerOnDashPressed;
            
            var authorities = new List<GameObject>();
            authorities.Add(cameraController.gameObject);
            authorities.Add(inputService.gameObject);
            authorities.Add(playerMovementService.gameObject);

            _playerAuthorities.Add(player, authorities);

            PlayerAdded?.Invoke(player);

            return player.gameObject;
        }

        private PlayerMovementService SetPlayerController(NetworkConnectionToClient conn, Player player)
        {
            PlayerMovementService playerMovementService = Instantiate(_playerMovementService);
            playerMovementService.Init(player, _gameSettings, _playerDashCollisionsTrackService);
            NetworkServer.Spawn(playerMovementService.gameObject, conn);

            return playerMovementService;
        }

        public void OnGameEnded(GameEndContext context) // just test
        {
            Debug.Log($"Game ended, winner is Player{context.Winner.netId}");
        }

        private SpawnPoint GetFreePoint()
        {
            var freePoints = _spawnPoints.Where(x => x.IsBusy == false).ToList();

            SpawnPoint spawnPoint = freePoints[Random.Range(0, freePoints.Count)];

            spawnPoint.IsBusy = true;

            return spawnPoint;
        }

        private Player SetPlayer(NetworkConnectionToClient conn)
        {
            SpawnPoint spawnPoint = GetFreePoint();

            Player player = Instantiate(_playerPrefab, spawnPoint.transform.position, Quaternion.identity);

            Vector3 position = player.transform.position;
            position.y = 2f;

            player.transform.position = position;
            
            NetworkServer.Spawn(player.gameObject, conn);

            NetworkServer.AddPlayerForConnection(conn, player.gameObject);

            _playersAlive.Add(conn, player);
            _players.Add(player);

            return player;
        }

        private InputService SetInputService(NetworkConnectionToClient conn, CameraController cameraController)
        {
            InputService inputService = Instantiate(_inputService);
            inputService.Init(cameraController);

            NetworkServer.Spawn(inputService.gameObject, conn);
            return inputService;
        }

        private void OnPlayerOnDashPressed(NetworkIdentity networkIdentity)
        {
            _playerDashCollisionsTrackService.OnPlayerDashed(networkIdentity);
        }

        private CameraController SetCamera(Player player, NetworkConnectionToClient conn)
        {
            CameraController cameraController =
                Instantiate(_cameraControllerPrefab, player.transform.position, Quaternion.identity);

            cameraController.Init(_gameSettings, player);
            cameraController.AssignTarget(player.netIdentity);

            NetworkServer.Spawn(cameraController.gameObject, conn);

            return cameraController;
        }

        public void PlacePlayerOnRandomPoint(Player player)
        {
            SpawnPoint spawnPoint = GetFreePoint();

            Vector3 pos = spawnPoint.transform.position;
            pos.y = 2;

            player.SetPos(pos);
        }

        public void FreeSpawnPoints()
        {
            foreach (SpawnPoint spawnPoint in _spawnPoints)
            {
                spawnPoint.IsBusy = false;
            }
        }

        public void RemovePlayer(NetworkConnectionToClient conn)
        {
            //Debug.Log($"Remove player");

            if (_playersAlive.ContainsKey(conn))
            {
                Player player = _playersAlive[conn];

                player.PlayerMovementService.DashPressed -= OnPlayerOnDashPressed;

                var playerAuthorities = _playerAuthorities[player];

                foreach (GameObject gameObj in playerAuthorities)
                {
                    NetworkServer.Destroy(gameObj);
                    Destroy(gameObj);
                }

                _playerDashCollisionsTrackService.Remove(player.netId);
                
                _playersAlive.Remove(conn);
                _players.Remove(player);
                _playerAuthorities.Remove(player);

                PlayerRemoved?.Invoke(player);

                NetworkServer.Destroy(player.gameObject);
                NetworkServer.DestroyPlayerForConnection(conn);

                Destroy(player);
            }
        }
    }
}