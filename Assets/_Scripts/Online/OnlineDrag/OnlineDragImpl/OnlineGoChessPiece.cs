using Mirror;
using QFramework;
using UnityEngine;


namespace Tabletop.Online
{
    public class OnlineGoChessPiece : OnlineDragObject
    {
        //�ڰ�����
        public GoChessColor m_virtualColor;
        public GoChessColor VirtualColor
        {
            set
            {
                m_virtualColor = value;
                RpcColorChange(m_virtualColor);
            }
            get { return m_virtualColor; }
        }

        public EasyEvent<GoChessColor> ColorChange;

        public Material WhiteMaterial;
        public Material BlackMaterial;

        public override void OnStartClient()
        {
            base.OnStartClient();
            CmdSyncState();
        }

        [Command(requiresAuthority = false)]
        public void CmdSyncState()
        {
            RpcColorChange(m_virtualColor);
        }

        [Server]
        protected override void Init()
        {
            base.Init();

            VirtualColor = GoChessColor.Unknown;
            ColorChange = new EasyEvent<GoChessColor>();
        }

        [ClientRpc]
        private void RpcColorChange(GoChessColor virtualColor)
        {
            //�޸�Ϊ��Ӧ����
            if (virtualColor == GoChessColor.White)
            {
                transform.Find("model").GetComponent<MeshRenderer>().material = WhiteMaterial;
            }
            else if (virtualColor == GoChessColor.Black)
            {
                transform.Find("model").GetComponent<MeshRenderer>().material = BlackMaterial;
            }
            else
            {
                print("������ɫδ֪");
            }
        }

        [Server]
        protected override bool CheckHandleAddition(uint playerNid)
        {
            bool res = false;
            OnlinePlayerManager.Instance.ForEach((player) =>
            {
                if (player.netId == playerNid && player.CurrentColor == VirtualColor)
                    res = true;
            });
            return res;
        }

    }

}