using TMPro;
using UnityEngine;

namespace Dev.ScoreLogic
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _score;

        public void UpdateScore(ScoreData scoreData)
        {
            _score.text = $"Player{scoreData.NetworkIdentity.netId} - {scoreData.Score}";
        }
    }
}