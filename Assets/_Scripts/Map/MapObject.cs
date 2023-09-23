using Mirror;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject : NetworkBehaviour
{
    public GameObject AttachPrefab;

    [SerializeField]
    [Range(1, 20)]
    protected int width = 5;

    [Range(1, 20)]
    [SerializeField]
    protected int height = 5;

    [Range(0.1f, 3f)]
    [SerializeField]
    protected float gridSize = 1f;

    /// <summary>
    /// ����ԭ���ƫ��
    /// </summary>
    [SerializeField]
    protected Vector3 offset;

    protected EasyGrid<GridData> m_grids;
    public EasyGrid<GridData> Grids => m_grids;

    protected bool ServerStarted = false;

    protected Transform AttachParent;

    public override void OnStartServer()
    {
        if (AttachParent == null)
        {
            var GO = new GameObject("AttachParent");
            AttachParent = GO.transform;
        }

        MapInit();
        ServerStarted = true;
    }

    public override void OnStartClient()
    {
        if (AttachParent == null)
        {
            var GO = new GameObject("AttachParent");
            AttachParent = GO.transform;
        }
    }


    [Server]
    public void MapInit()
    {
        m_grids = new EasyGrid<GridData>(width, height);

        m_grids.ForEach((x, z, _) =>
        {
            m_grids[x, z] = new GridData(x, z, transform, offset, gridSize);
            var attachArea = GameObject.Instantiate(AttachPrefab, m_grids[x, z].WorldPos, Quaternion.identity, AttachParent);

            NetworkServer.Spawn(attachArea, connectionToClient);

            attachArea.GetComponent<AttachArea>().Grid = m_grids[x, z];
        });
    }


    private void Update()
    {
        if (!ServerStarted) return;

        Draw();
    }

    protected void Draw()
    {
        if(m_grids == null) return;

        m_grids.ForEach((x, z, gird) =>
        {
            if (x >= m_grids.Width - 1 || z >= m_grids.Height - 1)//����������̸�
            {

            }
            else
            {
                var tileWorldPos = gird.WorldPos;
                var leftBottomPos = tileWorldPos;
                var leftTopPos = tileWorldPos + new Vector3(0, 0, gridSize);
                var rightBottomPos = tileWorldPos + new Vector3(gridSize, 0, 0);
                var rightTopPos = tileWorldPos + new Vector3(gridSize, 0, gridSize);

                Debug.DrawLine(leftBottomPos, leftTopPos, Color.red);
                Debug.DrawLine(leftBottomPos, rightBottomPos, Color.red);
                Debug.DrawLine(rightTopPos, leftTopPos, Color.red);
                Debug.DrawLine(rightTopPos, rightBottomPos, Color.red);
            }
        });
    }

}