using UnityEngine;

namespace Dev
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _force;
        [SerializeField] private float _time;

        [SerializeField] private float _distance = 2;
        
        [SerializeField] private ForceMode _forceMode;
        
        [SerializeField] private AnimationCurve _velocity;

        private float _forceTime;
        private bool _toForce;

        private Vector3 _forceDirection;
        private Vector3 _targetPos;
        private float _targetMagnitude;
        
        [ContextMenu(nameof(AddForce))]
        public void AddForce()
        {
            _forceTime = Time.time;
            _toForce = true;
            
            Vector3 currentPos = _rigidbody.transform.position;
            Vector3 targetPos = currentPos + transform.forward * _distance;

            _targetPos = targetPos;
            
            Vector3 direction = (targetPos - currentPos);

            _targetMagnitude = direction.magnitude;

            _forceDirection = direction.normalized;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (_toForce == false)
                {
                    AddForce();
                }
            }
        }

        private void FixedUpdate()
        {
            if(_toForce == false) return;

            Vector3 direction = (_targetPos - _rigidbody.transform.position);
            var magnitude = direction.magnitude;
            var value = (magnitude / _targetMagnitude);
            var time = value;
            var modifier = _velocity.Evaluate(time);

            Debug.Log($"Modifier {modifier}, time {time}, value {value}");
            
            _rigidbody.velocity = _forceDirection * modifier * _force;
            
            if (Time.time - _forceTime > _time)
            {
                _rigidbody.velocity = Vector3.zero;
                _toForce = false;
            }
        }
    }
}