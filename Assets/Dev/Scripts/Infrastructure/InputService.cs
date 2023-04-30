using Dev.CameraLogic;
using Mirror;
using UnityEngine;

namespace Dev.Infrastructure
{
    public class InputService : NetworkBehaviour
    {
        [SyncVar] private CameraController _cameraController;
        
        [HideInInspector] [SyncVar] public bool AllowToMove;

        [Server]
        public void Init(CameraController cameraController)
        {
            _cameraController = cameraController;
        }

        public bool TryGetInput(out MyInput input)
        {
            input = new MyInput();

            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            input.MouseDelta = mouseDelta;
            
            input.MoveDirection.x = Input.GetAxisRaw("Horizontal");
            input.MoveDirection.y = Input.GetAxisRaw("Vertical");

            input.MoveDirection.Normalize();

            input.LookDirection = _cameraController.GetLookDirection();

            input.Dash = Input.GetAxisRaw("Fire1") == 1 || Input.GetAxisRaw("Jump") == 1;

            return true;
        }
    }
}