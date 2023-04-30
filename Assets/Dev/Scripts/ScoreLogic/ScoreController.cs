using System;
using System.Linq;
using Dev.Infrastructure.StaticData;
using Dev.PlayerLogic;
using Mirror;
using UnityEngine;

namespace Dev.ScoreLogic
{
    public class ScoreController : NetworkBehaviour
    {
        [SerializeField] private ScoreView _scoreView;
        
        private PlayerSpawnService _playerSpawnService;
        private GameSettings _gameSettings;

        private readonly SyncList<ScoreData> _scoreDatas = new SyncList<ScoreData>();

        public event Action<ScoreData> ScoreUpdated;
        public event Action<ScoreData> ScoreRecordAdded;
        public event Action<NetworkIdentity> ScoreRecordRemoved;
        public event Action<ScoreData> MaxScoreReached;

        public void Init(PlayerSpawnService playerSpawnService, GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
            _playerSpawnService = playerSpawnService;
            _scoreView.Init(this);
        }

        public override void OnStartServer()
        {
            _playerSpawnService.PlayerAdded += OnPlayerAdded;
            _playerSpawnService.PlayerRemoved += OnPlayerRemoved;
        }

        public override void OnStartClient()
        {
            if (isServer) return;

            CmdRequestScoreDatas();
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestScoreDatas()
        {
            foreach (ScoreData scoreData in _scoreDatas)
            {
                RpcInitScoreDatas(scoreData);
            }
        }

        [ClientRpc]
        private void RpcInitScoreDatas(ScoreData scoreData)
        {
            if (isServer) return;
            
            ScoreRecordAdded?.Invoke(scoreData);
        }

        private void OnPlayerRemoved(Player player)
        {
            RemoveScoreRecord(player.netIdentity);
        }

        private void OnPlayerAdded(Player player)
        {
            AddScoreRecord(player.netIdentity);
        }

        [Server]
        public void AddScoreRecord(NetworkIdentity networkIdentity, int score = 0)
        {
            var scoreData = new ScoreData();
            scoreData.Score = score;
            scoreData.NetworkIdentity = networkIdentity;

            _scoreDatas.Add(scoreData);

            ScoreRecordAdded?.Invoke(scoreData);
        }

        [Server]
        public void RemoveScoreRecord(NetworkIdentity networkIdentity)
        {
            ScoreData scoreData = _scoreDatas.FirstOrDefault(x => x.NetworkIdentity.netId == networkIdentity.netId);

            if (scoreData.NetworkIdentity != null)
            {
                ScoreRecordRemoved?.Invoke(networkIdentity);
                _scoreDatas.Remove(scoreData);
            }
        }

        [Server]
        public void EvaluateScore(NetworkIdentity networkIdentity, int score = 1)
        {
            var hasData = TryGetScoreData(networkIdentity, out var scoreData);

            if (hasData == false) return;

            bool isWinner;

            scoreData.Score += score;

            isWinner = scoreData.Score >= _gameSettings.ScoreWinAmount;

            SetScore(networkIdentity, scoreData.Score);

            if (isWinner)
            {
                MaxScoreReached?.Invoke(scoreData);
            }
        }

        [Server]
        public void ResetScore(NetworkIdentity networkIdentity)
        {
            SetScore(networkIdentity, 0);
        }

        private bool TryGetScoreData(NetworkIdentity networkIdentity, out ScoreData scoreData)
        {
            scoreData = _scoreDatas.FirstOrDefault((data => data.NetworkIdentity.netId == networkIdentity.netId));

            return _scoreDatas.Contains(scoreData);
        }

        [Server]
        private void SetScore(NetworkIdentity networkIdentity, int score)
        {
            if (TryGetScoreData(networkIdentity, out var scoreData))
            {
                scoreData.Score = score;

                _scoreDatas[_scoreDatas.FindIndex(x => x.NetworkIdentity.netId == scoreData.NetworkIdentity.netId)] =
                    scoreData;

                RpcOnScoreUpdate(scoreData);
            }
        }

        [ClientRpc]
        private void RpcOnScoreUpdate(ScoreData scoreData)
        {
            ScoreUpdated?.Invoke(scoreData);
        }

        public override void OnStopClient()
        {
            if (isServer) return;

            ScoreRecordRemoved?.Invoke(netIdentity);
        }
    }
}