using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure.StaticData;
using Dev.ScoreLogic;
using Mirror;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerDashCollisionsTrackService : NetworkBehaviour
    {
        [SerializeField] private LayerMask _playerLayerMask;

        private List<DashCollisionPollingContext> _owners = new List<DashCollisionPollingContext>(4);

        private Dictionary<uint, double> _dashesTimers = new Dictionary<uint, double>();

        private GameSettings _gameSettings;
        private PlayerSpawnService _playerSpawnService;
        private ScoreController _scoreController;

        public void Init(GameSettings gameSettings, PlayerSpawnService playerSpawnService,
            ScoreController scoreController)
        {
            _scoreController = scoreController;
            _playerSpawnService = playerSpawnService;
            _gameSettings = gameSettings;
        }

        [Server]
        public void OnPlayerDashed(NetworkIdentity networkIdentity)
        {
            SetIgnoreCollision(networkIdentity, true);

            StartPollingForCollisions(networkIdentity, _gameSettings.DashCooldown);
        }

        [Server]
        private void SetIgnoreCollision(NetworkIdentity me, bool ignore)
        {
            RpcSetIgnoreCollision(me, ignore);

            var otherPlayers = _playerSpawnService.GetPlayersBesidesMe(me);

            var mePlayer = me.gameObject.GetComponent<Player>();

            foreach (Player otherPlayer in otherPlayers)
            {
                Physics.IgnoreCollision(mePlayer.Collider, otherPlayer.Collider, ignore);
            }
        }

        [ClientRpc]
        private void RpcSetIgnoreCollision(NetworkIdentity me, bool ignore)
        {
            var players = _playerSpawnService.Players;

            var otherPlayers = players.Where(x => x.netId != me.netId).ToList();

            var mePlayer = me.gameObject.GetComponent<Player>();

            foreach (Player otherPlayer in otherPlayers)
            {
                Physics.IgnoreCollision(mePlayer.Collider, otherPlayer.Collider, ignore);
            }
        }

        [Server]
        private void StartPollingForCollisions(NetworkIdentity networkIdentity, float duration)
        {
            if(_dashesTimers.ContainsKey(networkIdentity.netId)) return;
            
            DashCollisionPollingContext context = new DashCollisionPollingContext();

            context.NetworkIdentity = networkIdentity;

            _dashesTimers.Add(context.NetworkIdentity.netId, duration);

            _owners.Add(context);
        }

        private void Update()
        {
            if (isServer == false) return;

            if (_owners.Count == 0) return;

            CountdownDashTimers();
            ProcessCollisions();
        }

        private void ProcessCollisions()
        {
            for (var index = _owners.Count - 1; index >= 0; index--)
            {
                DashCollisionPollingContext context = _owners[index];

                NetworkIdentity owner = context.NetworkIdentity;
                Transform ownerTransform = owner.transform;

                uint ownerNetId = owner.netId;

                var overlapSphere =
                    Physics.OverlapSphere(ownerTransform.position, _gameSettings.DashHitBoxRadius, _playerLayerMask);

                if (overlapSphere.Length == 0) continue;

                foreach (Collider overlappedCollider in overlapSphere)
                {
                    Transform parent = overlappedCollider.transform.parent;

                    if (parent == null) continue;

                    var tryGetComponent = parent.TryGetComponent<NetworkIdentity>(out var target);

                    if (tryGetComponent == false) continue;

                    var hitSelf = target.netId == ownerNetId;

                    if (hitSelf) continue;

                    OnCollision(owner, target);

                    // Remove(context);
                }
            }
        }

        private void OnCollision(NetworkIdentity owner, NetworkIdentity target)
        {
            var player = target.gameObject.GetComponent<Player>();

            if (player.PlayerDashTrackService.AllowToPunch)
            {
                _scoreController.EvaluateScore(owner);
                player.PlayerDashTrackService.Punch();
                player.PlayerView.ColorPunchedBody(_gameSettings.PunchColor,
                    _gameSettings.InvulnerabilityAfterPunchDuration);

                Debug.Log($"Punch! Owner is Player{owner.netId}, Target is Player{target.netId}");
                // apply as HIT
            }
        }

        private void CountdownDashTimers()
        {
            var nowTime = NetworkTime.time;

            // var delta = (nowTime - _lastTickTime);
            var delta = Time.deltaTime;

            var keys = _dashesTimers.Keys.ToList();

            for (var i = keys.Count - 1; i >= 0; i--)
            {
                var id = keys[i];

                var containsKey = _dashesTimers.ContainsKey(id);

                if (containsKey)
                {
                    var dashesTimer = _dashesTimers[id];

                    dashesTimer -= delta;

                    _dashesTimers[id] = dashesTimer;

                    if (dashesTimer < 0)
                    {
                        Remove(id);
                        continue;
                    }
                }
            }
        }

        private void Remove(DashCollisionPollingContext context)
        {
            if (_owners.Contains(context))
            {
                SetIgnoreCollision(context.NetworkIdentity, false);
                _owners.Remove(context);
            }
            
            _dashesTimers.Remove(context.NetworkIdentity.netId);
        }

        public void Remove(uint id)
        {
            DashCollisionPollingContext dashCollisionPollingContext =
                _owners.FirstOrDefault(x => x.NetworkIdentity.netId == id);

            Remove(dashCollisionPollingContext);
        }
    }
}