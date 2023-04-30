using Dev.Infrastructure.StaticData;
using Dev.PlayerLogic;
using Mirror;
using UnityEngine;

namespace Dev.CameraLogic
{
    public class CameraController : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _center;

        [SyncVar] private NetworkIdentity _target;
        [SyncVar] private GameSettings _gameSettings;
        [SyncVar] private Player _player;

        public override void OnStartClient()
        {
            gameObject.SetActive(isOwned);

            if (Camera.main)
            {
                Camera.main.gameObject.SetActive(false);
            }
        }

        [Server]
        public void Init(GameSettings gameSettings, Player player)
        {
            _player = player;
            _gameSettings = gameSettings;
        }

        [Server]
        public void AssignTarget(NetworkIdentity player)
        {
            _target = player;
        }

        public Vector3 GetLookDirection()
        {
            return Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        }

        private void LateUpdate()
        {
            if (isOwned == false) return;

            if (_target == null) return;

            Vector3 targetPos = _target.transform.position;

            SetCameraLocalPos(targetPos);

            SetRotation();

            _center.position =
                Vector3.Lerp(_center.position, targetPos, Time.deltaTime * _gameSettings.CameraFollowSpeed);
        }

        private void SetRotation()
        {
            var tryGetInput = _player.InputService.TryGetInput(out var input);
            
            if(tryGetInput == false) return;

            var deltaX = input.MouseDelta.x;
            var deltaY = -input.MouseDelta.y;

            Vector3 eulerAngles = _center.transform.localEulerAngles;

            eulerAngles.y += deltaX * _gameSettings.CameraSensitivity;
            eulerAngles.x += deltaY * _gameSettings.CameraSensitivity;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, 360 - 40, 360 - 10);

            _center.transform.localEulerAngles = eulerAngles;
        }

        private void SetCameraLocalPos(Vector3 targetPos)
        {
            Vector3 cameraOffset = targetPos;

            cameraOffset.x = _gameSettings.CameraLocalOffset.x;
            cameraOffset.y = _gameSettings.CameraLocalOffset.y;
            cameraOffset.z = _gameSettings.CameraLocalOffset.z;

            _camera.transform.localPosition = cameraOffset;
        }
    }
}