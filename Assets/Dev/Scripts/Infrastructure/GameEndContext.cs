using Dev.ScoreLogic;
using Mirror;

namespace Dev.Infrastructure
{
    public struct GameEndContext
    {
        public NetworkIdentity Winner;
        public ScoreData WinnerScoreData;
    }
}