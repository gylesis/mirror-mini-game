using UnityEngine;

namespace Dev.Infrastructure
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private DependenciesContainer _dependenciesContainer;

        private void Awake()
        {
            _dependenciesContainer.Init();
        }
    }
}