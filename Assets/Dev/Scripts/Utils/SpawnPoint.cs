using Mirror;
using UnityEngine;

namespace Dev.Utils
{
    public class SpawnPoint : NetworkBehaviour
    {
        [HideInInspector] [SyncVar] public bool IsBusy;
    }
}