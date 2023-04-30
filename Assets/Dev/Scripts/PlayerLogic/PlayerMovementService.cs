using System;
using System.Collections.Generic;
using Dev.Infrastructure;
using Dev.Infrastructure.StaticData;
using Mirror;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerMovementService : NetworkBehaviour
    {
        [SyncVar] private bool _isDashing;
        [SyncVar] private bool _allowToDash = true;

        [SyncVar] private Player _player;
        [SyncVar] private GameSettings _gameSettings;

        private double _dashPressedTime;
        [SyncVar] private Vector3 _dashTargetPos;
        private float _dashTargetMagnitude;
        private Vector3 _dashForceDirection;
        private double _dashForceTime;
        private bool _toForce;
        private PlayerDashCollisionsTrackService _playerDashCollisionsTrackService;

        public event Action<NetworkIdentity> DashPressed;

        public void Init(Player player, GameSettings gameSettings,
            PlayerDashCollisionsTrackService playerDashCollisionsTrackService)
        {
            _playerDashCollisionsTrackService = playerDashCollisionsTrackService;
            _gameSettings = gameSettings;
            _player = player;
        }

        private void Update()
        {
            if (isServer)
            {
                ServerUpdate();
            }

            if (isOwned == false) return;

            if (_isDashing) return;

            var tryGetInput = _player.InputService.TryGetInput(out var input);

            if (tryGetInput == false) return;

            if (_player.InputService.AllowToMove == false) return;

            _player.transform.rotation = Quaternion.LookRotation(input.LookDirection);

            Move(input);
            TryToDash(input);
        }

        private void Move(MyInput input)
        {
            Vector3 lookForward = input.LookDirection;
            Vector3 lookRight = Vector3.Cross(Vector3.up, input.LookDirection);

            Vector3 move = lookForward * input.MoveDirection.y +
                           lookRight * input.MoveDirection.x;

            move *= _gameSettings.MoveSpeed;
            move.y = _player.Rigidbody.velocity.y;

            CmdMove(move);
        }

        private void TryToDash(MyInput input)
        {
            var dash = input.Dash;

            if (dash == false) return;

            if (_player.PlayerDashTrackService.AllowToPunch == false) return;

            if (_allowToDash)
            {
                CmdDash(input.LookDirection);
            }
        }

        [Server]
        private void ServerUpdate()
        {
            if (isServer == false) return;

            var now = NetworkTime.time;

            if (now - _dashPressedTime > _gameSettings.DashCooldown)
            {
                _allowToDash = true;
            }
        }

        private void FixedUpdate()
        {
            if (isServer == false) return;

            if (_toForce)
            {
                Vector3 direction = (_dashTargetPos - _player.Rigidbody.transform.position);
                var magnitude = direction.magnitude;
                var value = (magnitude / _dashTargetMagnitude);
                var modifier = _gameSettings.DashVelocityFunction.Evaluate(value);

                //  Debug.Log($"Modifier {modifier}, value {value}, magnitude {magnitude}");

                RpcSetVelocity(_dashForceDirection * (modifier * _gameSettings.DashPower));

                if (magnitude < 0.2f)
                {
                    RpcSetVelocity(Vector3.zero);
                    // _player.Rigidbody.velocity = Vector3.zero;
                    _toForce = false;

                    OnDashFinished();
                    return;
                }

                if (NetworkTime.time - _dashForceTime > _gameSettings.DashTime)
                {
                    RpcSetVelocity(Vector3.zero);
                    //_player.Rigidbody.velocity = Vector3.zero;
                    _toForce = false;

                    OnDashFinished();
                }
            }
            
        }

        [Command]
        private void CmdMove(Vector3 velocity)
        {
            RpcSetVelocity(velocity);
        }
        
        [ClientRpc]
        private void RpcSetVelocity(Vector3 velociy)
        {
            _player.Rigidbody.velocity = velociy;
        }

        [Command]
        private void CmdDash(Vector3 direction)
        {
            _allowToDash = false;
            _dashForceTime = NetworkTime.time;
            _isDashing = true;

            _player.PlayerView.TiltModel(25);

            DashPressed?.Invoke(_player.netIdentity);

            _dashPressedTime = NetworkTime.time;

            var forceDirection = new Vector3(direction.x, 0, direction.z);

            Vector3 currentPos = _player.Rigidbody.transform.position;
            Vector3 targetPos = currentPos + forceDirection * _gameSettings.DashDistance;

            float radius = 0.5f;

            var hasObstacle = Physics.SphereCast(currentPos, radius, forceDirection, out var hit,
                _gameSettings.ObstaclesLayerMask);

            if (hasObstacle)
            {
                targetPos = hit.point + -forceDirection + Vector3.forward * radius;
            }

            Vector3 dashDirection = (targetPos - currentPos);

            _dashTargetPos = targetPos;
            _dashTargetMagnitude = dashDirection.magnitude;
            _dashForceDirection = dashDirection.normalized;

            // RpcRigidbodyAddForce(forceDirection * _gameSettings.DashPower);
            _player.RpcSetDamageColliderState(false);

            _toForce = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(_dashTargetPos, 0.5f);
        }

        [Server]
        private void OnDashFinished()
        {
            _playerDashCollisionsTrackService.Remove(_player.netId);

            _isDashing = false;
            _player.RpcSetDamageColliderState(true);

            _player.PlayerView.TiltModel(0);
        }
    }
}