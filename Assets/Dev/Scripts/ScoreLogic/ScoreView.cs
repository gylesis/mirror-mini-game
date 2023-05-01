using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Dev.ScoreLogic
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private ScoreUI _scoreUIPrefab;

        private ScoreController _scoreController;

        private List<ScoreData> _scoreDatas = new List<ScoreData>();
        private Dictionary<NetworkIdentity, ScoreUI> _scoreUis = new Dictionary<NetworkIdentity, ScoreUI>();

        public void Init(ScoreController scoreController)
        {
            _scoreController = scoreController;
        }

        private void Start()
        {
            _scoreController.ScoreUpdated += OnScoreUpdated;
            _scoreController.ScoreRecordAdded += OnScoreRecordAdded;
            _scoreController.ScoreRecordRemoved += OnScoreRecordRemoved;
        }

        private void OnScoreRecordAdded(ScoreData scoreData)
        {
            if(_scoreDatas.Any(x => x.NetworkIdentity == scoreData.NetworkIdentity)) return;
            
            _scoreDatas.Add(scoreData);

            ScoreUI scoreUI = Instantiate(_scoreUIPrefab, _parent);
            scoreUI.UpdateScore(scoreData);

            _scoreUis.Add(scoreData.NetworkIdentity, scoreUI);
        }

        private void OnScoreRecordRemoved(NetworkIdentity networkIdentity)
        {
            var hasScore = _scoreDatas.Any(x => x.NetworkIdentity == networkIdentity);

            if (hasScore)
            {
                _scoreDatas.RemoveAt(_scoreDatas.FindIndex(x => x.NetworkIdentity.netId == networkIdentity.netId));
                ScoreUI scoreUi = _scoreUis[networkIdentity];
                
                Destroy(scoreUi.gameObject);
                
                _scoreUis.Remove(networkIdentity);
            }
        }

        private void OnScoreUpdated(ScoreData scoreData)
        {
            ScoreUI scoreUi = _scoreUis[scoreData.NetworkIdentity];
            scoreUi.UpdateScore(scoreData);
        }

        private void OnDestroy()
        {
            _scoreController.ScoreUpdated -= OnScoreUpdated;
            _scoreController.ScoreRecordAdded -= OnScoreRecordAdded;
            _scoreController.ScoreRecordRemoved -= OnScoreRecordRemoved;
        }
    }
}