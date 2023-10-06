using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.Local
{
    public class LocalGobangMapAttachArea : LocalMapAttachArea
    {
        public override void Attach(LocalDragObj dragObject)
        {
            //TODO:���������ʱ���³�������
            var piece = dragObject as LocalGoChessPiece;
            if (piece is null)
            {
                print($"����ק���岢��Χ������");
                return;
            }
            else if (piece.VirtualColor != Map.CurrentColor.Value)
            {
                if (Map.CurrentColor.Value == GoChessColor.Black)
                    print($"��ǰ�Ǻ��ӻغϣ�����������Ч");
                else
                    print($"��ǰ�ǰ��ӻغϣ�����������Ч");

                //������Чʱ�Զ��������ƻ���¨
                StartCoroutine(piece.RecycleDragObject());
                return;
            }

            if (Grid.Occupied) return;
            Grid.Occupied = true;
            Grid.DragObject = dragObject;

            //���ӻ�û�ƶ���Ŀ���ʱ������������������
            var currentColor = Map.CurrentColor.Value;
            Map.CurrentColor.Value = GoChessColor.Unknown;
            StartCoroutine(piece.ApplyAttachTransform(transform, () =>
            {
                //�������һ������
                Map.LastOutlineObj.Value?.CancelHighlight();
                Map.LastOutlineObj.Value = piece;

                var rb = piece.transform.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.freezeRotation = true;

                //TODO:���������ʱ���³�������
                if (Map.GameReferee.CheckWin(piece.VirtualColor, Grid, Grids))
                {
                    print("��⵽��������һ��");

                    //֪ͨĳ��ʤ��
                    LocalPracticeController.Instance.WinEvent.Trigger(piece.VirtualColor);
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
    }

}
