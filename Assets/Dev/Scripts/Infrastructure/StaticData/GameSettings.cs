using UnityEngine;

namespace Dev.Infrastructure.StaticData
{
    [CreateAssetMenu(menuName = "Static Data/GameSettings", fileName = "GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [Header("Movement")] [SerializeField] private float _moveSpeed = 5f;

        [Header("Dash")] [SerializeField] private float _dashPower = 15f;
        [SerializeField] private float _dashCooldown = 2f;
        [SerializeField] private double _dashTime = 0.2f;
        [SerializeField] private float _dashHitBoxRadius = 2.5f;
        [SerializeField] private float _invulnerabilityAfterPunchDuration = 1;
        [SerializeField] private Color _punchColor = Color.magenta;
        [SerializeField] private AnimationCurve _dashVelocityFunction;
        [SerializeField] private float _dashDistance;
        [SerializeField] private LayerMask _obstaclesLayerMask;
        
        [Header("Camera")] [SerializeField] private float _cameraFollowSpeed = 6f;
        [SerializeField] private float _cameraSensitivity = 15f;
        [SerializeField] private Vector3 _cameraLocalOffset = new Vector3(0,10,-10);
    
        [Header("Other")] [SerializeField] private int _scoreWinAmount = 3;

        public LayerMask ObstaclesLayerMask => _obstaclesLayerMask;

        public float DashDistance => _dashDistance;

        public AnimationCurve DashVelocityFunction => _dashVelocityFunction;
        public Vector3 CameraLocalOffset => _cameraLocalOffset;
        public int ScoreWinAmount => _scoreWinAmount;
        public Color PunchColor => _punchColor;
        public float InvulnerabilityAfterPunchDuration => _invulnerabilityAfterPunchDuration;
        public float DashHitBoxRadius => _dashHitBoxRadius;
        public float MoveSpeed => _moveSpeed;
        public double DashTime => _dashTime;
        public float DashPower => _dashPower;
        public float CameraFollowSpeed => _cameraFollowSpeed;
        public float CameraSensitivity => _cameraSensitivity;
        public float DashCooldown => _dashCooldown;
    }
}