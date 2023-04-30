using Dev.Infrastructure;
using Dev.Infrastructure.StaticData;
using Mirror;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _hitBoxCollider;

        [SerializeField] private PlayerDashTrackService _playerDashTrackService;
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private Collider _collider;

        public Rigidbody Rigidbody => _rigidbody;
        public PlayerView PlayerView => _playerView;
        public Collider Collider => _collider;
        public PlayerDashTrackService PlayerDashTrackService => _playerDashTrackService;
        public InputService InputService => _inputService;

        public PlayerMovementService PlayerMovementService => _playerMovementService;

        [SyncVar] private InputService _inputService;
        [SyncVar] private GameSettings _gameSettings; // i guess its bad practices
        [SyncVar] private PlayerMovementService _playerMovementService; // player movement service should not ber here, there must be some Facade to access player's properties

        [Server]
        public void RpcInit(InputService inputService, GameSettings gameSettings, PlayerMovementService playerMovementService) 
        {
            _playerMovementService = playerMovementService;
            _gameSettings = gameSettings;
            _inputService = inputService;

            _playerDashTrackService.Init(gameSettings);
        }

        [ClientRpc]
        public void RpcSetDamageColliderState(bool isOn)
        {
            _hitBoxCollider.enabled = isOn;
        }
    }
}