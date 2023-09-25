using Mirror;
using Mirror.Examples.Basic;
using QFramework;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContainerObj : OutLineObj, IAttachable
{
    protected Collider m_collider;
    public Collider Collider => m_collider;

    public List<DragObject> Contents = new List<DragObject>();
    protected List<Type> ContainTypes = new List<Type>();

    [ToggleLeft]
    public bool CountUnlimitedToggle = false;
    [EnableIf("CountUnlimitedToggle")]
    public GameObject CountUnlimitedPrefab;

    public DragObject CurrentDragObj = null;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Init();
    }


    protected override void Init()
    {
        base.Init();

        m_collider = transform.Find("model").GetComponent<Collider>();

        AddContainTypes();
        foreach (var subClassType in ContainTypes)
        {
            DragObject dragObject = null;
            if (CountUnlimitedPrefab.TryGetComponent<DragObject>(out dragObject))
            {
                if (!dragObject.GetType().Equals(subClassType))
                {
                    Debug.LogError($"CountUnlimitedPrefab不挂载{subClassType.Name}");
                }
            }
        }
    }

    /// <summary>
    /// Example: ContainTypes.Add(typeof(subClass of DragObject));
    /// </summary>
    protected abstract void AddContainTypes();

    protected abstract bool AddCondition(DragObject dragObj);

    public void Attach(uint playerNid, DragObject dragObject)
    {
        Add(playerNid, dragObject);
    }

    public void Add(uint playerNid, DragObject dragObject)
    {
        if (!ContainTypes.Contains(dragObject.GetType()) || !AddCondition(dragObject))
        {
            PlayerManager.Instance.SendMsg(playerNid, "该容器不装载此物体");
            return;
        }

        dragObject.RpcBeAdd();
        Contents.Add(dragObject);
    }

    /// <summary>
    /// 为真时可以操作
    /// </summary>
    /// <returns></returns>
    [Server]
    protected virtual bool CheckHandleAddition(uint playerNid)
    {
        return false;
    }

    public override void OnMouseDown()
    {
         CmdGet(NetworkClient.localPlayer.netId);
    }

    [Command(requiresAuthority = false)]
    public void CmdGet(uint playerNid)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayerManager.Instance.SendMsg(playerNid, "你不能使用对方的棋篓");
            return;
        }

        if (Contents.Count == 0)
        {
            if (CountUnlimitedToggle)
            {
                var go = Instantiate(CountUnlimitedPrefab,
                    transform.position + Vector3.up * 10f, Quaternion.identity);
                NetworkServer.Spawn(go, connectionToClient);
                CurrentDragObj = go.GetComponent<DragObject>();
                CurrentDragObj.Container = this;
                RpcAfterGenerateHandler(CurrentDragObj);
            }
            else
            {
                PlayerManager.Instance.SendMsg(playerNid, "容器是空的");
                return;
            }
        }
        else
        {
            CurrentDragObj = Contents[Contents.Count - 1];
            Contents.RemoveAt(Contents.Count - 1);
        }

        CurrentDragObj.MouseDown();
        CurrentDragObj.RpcBeGet();
    }

    [ClientRpc]
    protected virtual void RpcAfterGenerateHandler(DragObject dragObj)
    {

    }

    public void OnMouseDrag()
    {
        //服务端要使用的是客户端的鼠标位置，而非服务端的鼠标位置，OnMouseUp同
        CmdMouseDrag(NetworkClient.localPlayer.netId, Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDrag(uint playerNid, Vector3 mousePos)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayerManager.Instance.SendMsg(playerNid, "你不能使用对方的棋篓");
            return;
        }
        CurrentDragObj?.MouseDrag(mousePos);
    }

    public void OnMouseUp()
    {
        CmdMouseUp(NetworkClient.localPlayer.netId, Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseUp(uint playerNid, Vector3 mousePos)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayerManager.Instance.SendMsg(playerNid, "你不能使用对方的棋篓");
            return;
        }
        CurrentDragObj?.MouseUp(playerNid, mousePos) ;
    }
}
