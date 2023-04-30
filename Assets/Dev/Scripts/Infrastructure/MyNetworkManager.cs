using Dev.PlayerLogic;
using Dev.Utils;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev.Infrastructure
{
    public class MyNetworkManager : NetworkRoomManager
    {
        private DependenciesContainer _dependenciesContainer;
        private PlayerSpawnService _playerSpawnService => _dependenciesContainer.PlayerSpawnService;

        private double _lastTickTime;

        public override void OnServerSceneChanged(string sceneName)
        {
            var isMainScene = sceneName == SceneManager.GetSceneByName("Main").path;

            if (isMainScene)
            {
                Cursor.visible = false;

                _dependenciesContainer = FindObjectOfType<DependenciesContainer>();
            }
            else
            {
                Cursor.visible = true;
            }
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"New player connected {conn.address}");
        }

        public override void OnRoomServerPlayersReady()
        {
            Debug.Log($"All Players ready, starting game");

            ServerChangeScene(GameplayScene);
        }

        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
        {
            NetworkServer.RemovePlayerForConnection(conn, roomPlayer);
            NetworkServer.Destroy(roomPlayer);
            Destroy(roomPlayer.gameObject);

            //return base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
            return _playerSpawnService.SpawnPlayer(conn);
        }

        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
        {
            _playerSpawnService.RemovePlayer(conn);
        }
    }
}