using UnityEngine;

namespace Dev.Infrastructure
{
    public struct MyInput
    {
        public Vector2 MoveDirection;
        public Vector3 LookDirection;

        public Vector2 MouseDelta;
        
        public bool Dash;
    }
}