using System;
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
        
        private WaitForSeconds _waitForSeconds = new WaitForSeconds(1);

        [Server]
        public void ShowCountdownText(string body, int seconds)
        {
            StartCoroutine(Countdown(body, seconds));
        }

        [ClientRpc]
        public void ShowGenericText(bool isOn, string str)
        {
            if (isOn)
            {
                _genericText.text = str;
            }

            _genericText.enabled = isOn;
        }

        private IEnumerator Countdown(string body, int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                int currentSecond = seconds - i;

                var str = $"{body} {currentSecond}";

                ShowCountDownText(str, true);

                yield return _waitForSeconds;
            }

            ShowCountDownText(String.Empty, false);
        }

        [ClientRpc]
        private void ShowCountDownText(string text, bool isOn)
        {
            if (isOn)
            {
                _countdownText.text = text;
            }
            
            _countdownText.enabled = isOn;
        }
    }
}