using Mirror;
using System;
using UnityEngine;

namespace Tabletop.Online
{
    public class OnlinePlayer : NetworkBehaviour
    {
        private GoChessColor m_currentColor = GoChessColor.Unknown;

        /// <summary>
        /// ��ǰ��Ҷ�Ӧ��һ���ӻ��ǰ���
        /// </summary>
        public GoChessColor CurrentColor
        {
            set { m_currentColor = value; }
            get { return m_currentColor; }
        }

        public override void OnStartServer()
        {
            OnlinePlayerManager.Instance.Add(this);
        }

        [Client]
        public override void OnStartClient()
        {
            NetworkClient.ReplaceHandler<OppositeExitMessage>(OnOppositeExit);
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<OppositeExitMessage>();
        }

        /// <summary>
        /// TODO:��ʱ���ñ�־λ��ǣ��պ��ΪUI��ʾ
        /// </summary>
        public bool oppositeExit = false;
        [ClientCallback]
        private void OnOppositeExit(OppositeExitMessage msg)
        {
            print(NetworkClient.localPlayer.netId);
            oppositeExit = true;
            print(msg.MsgContent + "��B��StopClient");
            print("OnOppositeExit:oppositeExit" + oppositeExit);
        }


        private void Update()
        {
            if (!isLocalPlayer) return;

            //oppositeExit = true;
            if (Input.GetKeyDown(KeyCode.B))
            {
                print("B, oppositeExit:" + oppositeExit);
                if (oppositeExit)
                    NetworkManager.singleton.StopClient();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                print("LocalPlayerNid:" + netId);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnlinePlayerManager.Instance.RestartGame();
            }
        }

        [TargetRpc]
        public void TargetSendMsg(NetworkConnectionToClient targetConn, string msg)
        {
            Debug.Log(msg);
        }
    }
}