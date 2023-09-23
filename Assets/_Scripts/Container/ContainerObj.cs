using Mirror;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContainerObj : OutLineObj, IAttachable
{
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

        AddContainTypes();

        foreach (var subClassType in ContainTypes)
        {
            DragObject dragObject = null;
            if (CountUnlimitedPrefab.TryGetComponent<DragObject>(out dragObject))
            {
                if (!dragObject.GetType().Equals(subClassType))
                {
                    Debug.LogError($"CountUnlimitedPrefab������{subClassType.Name}");
                }
            }
        }
    }

    /// <summary>
    /// Example: ContainTypes.Add(typeof(subClass of DragObject));
    /// </summary>
    protected abstract void AddContainTypes();

    protected abstract bool AddCondition(DragObject dragObj);

    public void Attach(DragObject dragObject)
    {
        Add(dragObject);
    }

    public void Add(DragObject dragObject)
    {
        if (!ContainTypes.Contains(dragObject.GetType()) || !AddCondition(dragObject))
        {
            print("��������װ�ش�����");
            return;
        }

        Contents.Add(dragObject);
        dragObject.RpcBeAdd();
    }

    public override void OnMouseDown()
    {
        CmdGet(Contents);
    }

    [Command(requiresAuthority = false)]
    public void CmdGet(List<DragObject> contents)
    {
        if (contents.Count == 0)
        {
            if (CountUnlimitedToggle)
            {
                var go = Instantiate(CountUnlimitedPrefab,
                    transform.position + Vector3.up * 10f, Quaternion.identity);
                NetworkServer.Spawn(go, connectionToClient);
                CurrentDragObj = go.GetComponent<DragObject>();
                RpcAfterGenerateHandler(CurrentDragObj);
            }
            else
            {
                print("�����ǿյ�");
                return;
            }
        }
        else
        {
            CurrentDragObj = contents[contents.Count - 1];
            contents.RemoveAt(contents.Count - 1);
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
        //�����Ҫʹ�õ��ǿͻ��˵����λ�ã����Ƿ���˵����λ�ã�OnMouseUpͬ
        CmdMouseDrag(Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDrag(Vector3 mousePos)
    {
        CurrentDragObj?.MouseDrag(mousePos);
    }

    public void OnMouseUp()
    {
        CmdMouseUp(Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseUp(Vector3 mousePos)
    {
        CurrentDragObj?.MouseUp(mousePos);
    }
}
