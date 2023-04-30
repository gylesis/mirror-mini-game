using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

namespace Dev.UI
{
    public class Curtain : NetworkBehaviour
    {
        [SerializeField] private TMP_Text _genericText;
        [SerializeField] private TMP_Text _countdownText;

        [ClientRpc]
        public void ShowGenericText(bool isOn, string str)
        {
            if (isOn)
            {
                _genericText.text = str;
            }

            _genericText.enabled = isOn;
        }

        [Server]
        public void ShowCountdown(string body, int seconds)
        {
            RpcSetTextCountdown(body, seconds);
        }

        [ClientRpc]
        private void RpcSetTextCountdown(string body, int seconds)
        {
            StartCoroutine(Countdown(body, seconds));
        }

        private IEnumerator Countdown(string body, int seconds)
        {
            _countdownText.enabled = true;

            for (int i = 0; i < seconds; i++)
            {
                int currentSecond = seconds - i;

                SetText($"{body} {currentSecond}...");

                yield return new WaitForSeconds(1);
            }

            _countdownText.enabled = false;
        }

        private void SetText(string str)
        {
            _countdownText.text = str;
        }
    }
}