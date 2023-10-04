using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tabletop.Online
{
    [AddComponentMenu("")]
    public class NetworkRoomPlayerExt : NetworkRoomPlayer
    {
        public override void Start()
        {
            base.Start();

            //����Redis
            if (NetworkManager.singleton is NetworkRoomManagerExt room && room.roomSlots.Count == room.minPlayers)
            {
                room.SetRedisValue(OnlineRoomState.Full);
                print($"����ִ�и���Redis: RoomState.Full, port: {(room.transport as KcpTransport).port}");
            }
        }

        #region �����޲λص�
        public override void OnStartClient()
        {
            //Debug.Log($"OnStartClient {gameObject}");
        }

        public override void OnClientEnterRoom()
        {
            //Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");
        }

        //�����˳��Ŀͻ����ϵ�����Player����ص��˷���
        public override void OnClientExitRoom()
        {
            //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
        }
        #endregion

        public override void IndexChanged(int oldIndex, int newIndex)
        {
            //Debug.Log($"IndexChanged {newIndex}");
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            //Debug.Log($"ReadyStateChanged {newReadyState}");
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }
    }
}