using System;
using System.Collections;
using UnityEngine;

namespace Dev.Utils
{
    public static class Extensions
    {
        public static void DelayedCallback(this MonoBehaviour monoBehaviour, float delay, Action callback)
        {
            monoBehaviour.StartCoroutine(DelayedCallbackCoroutine(delay, callback));
        }
        
        private static IEnumerator DelayedCallbackCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            action.Invoke();
        }
        
    }
}