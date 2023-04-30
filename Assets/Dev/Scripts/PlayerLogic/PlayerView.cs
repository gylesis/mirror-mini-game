using Mirror;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class PlayerView : NetworkBehaviour
    {
        [SerializeField] private Renderer _bodyRenderer;

        private Color _originColor;

        [SyncVar] private double _punchTime;
        [SyncVar] private float _duration;
        [SyncVar] private bool _coloring;

        [Server]
        public void ColorPunchedBody(Color color, float duration)
        {
            _coloring = true;
            _duration = duration;
            _punchTime = NetworkTime.time;

            _originColor = _bodyRenderer.material.color;

            RpcSetColor(color);
        }

        [ClientRpc]
        private void RpcSetColor(Color color)
        {
            _bodyRenderer.material.color = color;
        }

        [ClientRpc]
        public void TiltModel(float value)
        {
            Quaternion rotation = transform.rotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            eulerAngles.x = value;
            transform.rotation = Quaternion.Euler(eulerAngles);
        }
        
        private void Update()
        {
            if (isServer == false) return;

            if (_coloring == false) return;

            var passedTime = NetworkTime.time - _punchTime;

            if (passedTime >= _duration)
            {
                _coloring = false;
                RpcSetColor(_originColor);
            }
        }
    }
}