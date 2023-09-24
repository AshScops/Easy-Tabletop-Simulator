using Mirror;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachArea : OutLineObj, IAttachable
{
    /// <summary>
    /// �����ĸ�Grids
    /// </summary>
    public EasyGrid<GridData> Grids;

    /// <summary>
    /// �ø��Ӧ�ĸ�GridData
    /// </summary>
    public GridData Grid;

    public MapObject Map;

    public override void OnStartServer()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();
    }

    [Server]
    public void Attach(DragObject dragObject)
    {
        if (Grid.Occupied) return;

        Grid.Occupied = true;
        Grid.DragObject = dragObject;

        //TODO:���������ʱ���³�������
        var piece = dragObject as GoChessPiece;
        if (piece is null)
        {
            print($"����ק���岢��Χ������");
            return;
        }
        else if (piece.VirtualColor.Value != Map.CurrentColor)
        {
            if (Map.CurrentColor == GoChessColor.Black)
                print($"��ǰ�Ǻ��ӻغϣ�����������Ч");
            else
                print($"��ǰ�ǰ��ӻغϣ�����������Ч");

            //TODO:������Чʱ�Զ��������Ƶ�һ��



            return;
        }

        StartCoroutine(piece.ApplyAttachTransform(transform, () =>
        {
            var rb = piece.transform.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.freezeRotation = true;

            //TODO:���������ʱ���³�������
            if(CheckWin(piece.VirtualColor.Value))
            {
                print("��⵽��������һ��");
                return;
            }

            if(Map.CurrentColor == GoChessColor.Black)
            {
                Map.CurrentColor = GoChessColor.White;
            }
            else
            {
                Map.CurrentColor = GoChessColor.Black;
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
                if (Grids[x, y].DragObject is GoChessPiece)
                {
                    var piece = Grids[x, y].DragObject as GoChessPiece;
                    if(piece.VirtualColor.Value != color)
                    {
                        lineDone = false;
                        break;
                    }
                }
            }
            if (lineDone)
                return true;
            //print("*************************************************************");

            startPos += direction;
        }

        return false;
    }

    
}
