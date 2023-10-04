using Mirror;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.Online
{
    public class OnlineAttachArea : OnlineOutLineObj, OnlineIAttachable
    {
        /// <summary>
        /// �����ĸ�Grids
        /// </summary>
        [HideInInspector] public EasyGrid<OnlineGridData> Grids;

        /// <summary>
        /// �ø��Ӧ�ĸ�GridData
        /// </summary>
        [HideInInspector] public OnlineGridData Grid;

        [HideInInspector] public OnlineMapObj Map;

        public override void OnStartServer()
        {
            Init();
        }

        protected override void Init()
        {
            base.Init();
        }


        public void Attach(uint playerNid, OnlineDragObj dragObject)
        {
            //TODO:���������ʱ���³�������
            var piece = dragObject as OnlineGoChessPiece;
            if (piece is null)
            {
                OnlinePlayerManager.Instance.SendMsg(playerNid, $"����ק���岢��Χ������");
                return;
            }
            else if (piece.VirtualColor != Map.CurrentColor.Value)
            {
                if (Map.CurrentColor.Value == GoChessColor.Black)
                    OnlinePlayerManager.Instance.SendMsg(playerNid, $"��ǰ�Ǻ��ӻغϣ�����������Ч");
                else
                    OnlinePlayerManager.Instance.SendMsg(playerNid, $"��ǰ�ǰ��ӻغϣ�����������Ч");

                //������Чʱ�Զ��������ƻ���¨
                StartCoroutine(piece.RecycleDragObject(playerNid));
                return;
            }

            if (Grid.Occupied) return;
            Grid.Occupied = true;
            Grid.DragObject = dragObject;

            //���ӻ�û�ƶ���Ŀ���ʱ��׼������������
            var currentColor = Map.CurrentColor.Value;
            Map.CurrentColor.Value = GoChessColor.Unknown;
            StartCoroutine(piece.ApplyAttachTransform(transform, () =>
            {
                var rb = piece.transform.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.freezeRotation = true;

                //TODO:���������ʱ���³�������
                if (CheckWin(piece.VirtualColor))
                {
                    OnlinePlayerManager.Instance.SendAllMsg("��⵽��������һ��");

                    //TODO:������̣����¿�ʼ

                    Map.CurrentColor.Value = GoChessColor.Unknown;
                    return;
                }

                //�غ�ת��
                if (currentColor == GoChessColor.Black)
                {
                    Map.CurrentColor.Value = GoChessColor.White;
                }
                else if (currentColor == GoChessColor.White)
                {
                    Map.CurrentColor.Value = GoChessColor.Black;
                }
            }));
        }

        /// <summary>
        /// ÿ�ν���������״����⡰�Ա���Ϊ���ĵľֲ�9x9������
        /// </summary>
        private bool CheckWin(GoChessColor color)
        {
            var centerPos = new Vector2Int(Grid.X, Grid.Z);

            bool res =
            CheckSingleLine(centerPos, new Vector2Int(1, -1), color) ||
            CheckSingleLine(centerPos, new Vector2Int(0, -1), color) ||
            CheckSingleLine(centerPos, new Vector2Int(-1, -1), color) ||
            CheckSingleLine(centerPos, new Vector2Int(1, 0), color);

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPos"></param>
        /// <param name="direction"></param>
        /// <param name=""></param>
        private bool CheckSingleLine(Vector2Int centerPos, Vector2Int direction, GoChessColor color)
        {
            var startPos = centerPos - direction * 4;
            var endPos = centerPos + direction;

            //������겻��Խ��
            while (startPos.x < 0 || startPos.x >= Grids.Width)
            {
                startPos += direction;
            }
            while (startPos.y < 0 || startPos.y >= Grids.Height)
            {
                startPos += direction;
            }

            //print("startPos:" + startPos);
            //print("endPos:" + endPos);

            while (startPos.x != endPos.x || startPos.y != endPos.y)
            {
                var x = startPos.x;
                var y = startPos.y;
                var xOffset = direction.x;
                var yOffset = direction.y;

                bool lineDone = true;
                //print("*************************************************************");
                for (int i = 0; i < 5; i++, x += xOffset, y += yOffset)
                {
                    //print("x, y:" + x + " " + y);
                    //����Խ��
                    if (x < 0 || x >= Grids.Width || y < 0 || y >= Grids.Height ||
                       !Grids[x, y].Occupied)
                    {
                        lineDone = false;
                        break;
                    }

                    //�ж���ɫ
                    if (Grids[x, y].DragObject is OnlineGoChessPiece)
                    {
                        var piece = Grids[x, y].DragObject as OnlineGoChessPiece;
                        if (piece.VirtualColor != color)
                        {
                            lineDone = false;
                            break;
                        }
                    }
                }

                //�Զ�����������True��(TODO)���õ�������outһ��List���ⲿʹ�ã����������㷨��дҵ���߼�
                if (lineDone)
                {
                    x = startPos.x;
                    y = startPos.y;
                    for (int i = 0; i < 5; i++, x += xOffset, y += yOffset)
                    {
                        Grids[x, y].DragObject.RpcFreezeHighlight(Color.green);
                    }
                    return true;
                }
                //print("*************************************************************");

                startPos += direction;
            }

            return false;
        }



    }

}
