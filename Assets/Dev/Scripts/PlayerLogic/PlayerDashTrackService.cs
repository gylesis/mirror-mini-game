using Dev.Infrastructure.StaticData;
using Mirror;

namespace Dev.PlayerLogic
{
    public class PlayerDashTrackService : NetworkBehaviour
    {
        private GameSettings _gameSettings;
        public bool AllowToPunch => _hadPunched == false;

        private double _punchTime;
        
        [SyncVar] private bool _hadPunched;

        public void Init(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
        }

        public void Punch()
        {
            _hadPunched = true;
            _punchTime = NetworkTime.time;
        }

        private void Update()
        {
            if (isServer == false) return;

            if (_hadPunched)
            {
                var now = NetworkTime.time;

                if (now - _punchTime > _gameSettings.InvulnerabilityAfterPunchDuration)
                {
                    _hadPunched = false;
                }
            }
        }
    }
}