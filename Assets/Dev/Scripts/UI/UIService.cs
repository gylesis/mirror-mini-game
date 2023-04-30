using UnityEngine;

namespace Dev.UI
{
    public class UIService : MonoBehaviour
    {
        [SerializeField] private Curtain _curtain;

        public Curtain Curtain => _curtain;
    }
}