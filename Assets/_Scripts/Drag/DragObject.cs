using Mirror;
using QFramework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum DragObjState
{
    Available = 0,
    Moving,
    Freeze
}

/// <summary>
/// ����ק������һ������ײ��
/// </summary>
public abstract class DragObject : OutLineObj
{
    protected Rigidbody m_rigidbody;
    protected Collider m_collider;
    public GameObject ClonePrefab;
    protected GameObject m_currentClone = null;
    protected int m_cloneX = -1;
    protected int m_cloneZ = -1;

    protected BindableProperty<DragObjState> m_dragState;

    public ContainerObj Container;

    /// <summary>
    /// ���ʱ���µ�����
    /// </summary>
    //protected Vector3 m_originVector;

    /// <summary>
    /// �϶�ʱ���µ�����
    /// </summary>
    protected Vector3 m_currentVector;

    /// <summary>
    /// ���ʱ���µ���ʼλ��
    /// </summary>
    //protected Vector3 m_originPos;

    /// <summary>
    /// �������Y����һ��ƫ�ƣ���̧��m_yOffsetToTable������
    /// </summary>
    [Range(0.1f, 3f)]
    public float m_yOffsetToTable = 0.1f;

    protected Vector3 m_offset;

    protected float m_yTarget;


    public override void OnStartServer()
    {
        print("DragObject__OnStartServer");
        Init();
    }

    [Server]
    protected override void Init()
    {
        base.Init();

        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = transform.Find("model").GetComponent<Collider>();
        m_dragState = new BindableProperty<DragObjState>(DragObjState.Available);
    }

    /// <summary>
    /// Ϊ��ʱ���Բ���
    /// </summary>
    /// <returns></returns>
    [Server]
    protected virtual bool CheckHandleAddition(uint playerNid)
    {
        return false;
    }

    public override void OnMouseDown()
    {
        CmdMouseDown(NetworkClient.localPlayer.netId);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDown(uint playerNid)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayManager.Instance.SendMsg(playerNid, "�㲻��ʹ�öԷ�������");
            return;
        }
        MouseDown(PlayManager.Instance.GetConn(playerNid));
    }

    [Server]
    public void MouseDown(NetworkConnectionToClient conn)
    {
        if (m_dragState.Value != DragObjState.Available)
        {
            return;
        }
        m_rigidbody.isKinematic = true;
        m_collider.isTrigger = true;
        m_dragState.Value = DragObjState.Moving;
        ChangeLayer("IgnoreRaycast");
        TargetChangeLayer(conn, "IgnoreRaycast");
    }

    public void OnMouseDrag()
    {
        Vector3 hitPos;
        if (Vector3Utils.GetClosetPoint(Input.mousePosition, transform.position, out hitPos))
        {
            CmdMouseDrag(NetworkClient.localPlayer.netId, hitPos);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDrag(uint playerNid, Vector3 hitPos)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayManager.Instance.SendMsg(playerNid, "�㲻��ʹ�öԷ�������");
            return;
        }
        MouseDrag(hitPos);
    }

    public void MouseDrag(Vector3 hitPos)
    {
        if (m_dragState.Value != DragObjState.Moving)
        {
            return;
        }

        var yOffset = new Vector3(0, m_collider.bounds.center.y - m_collider.bounds.min.y, 0);
        transform.position = hitPos + yOffset;
        //else
        //{
        //    //CmdSetPosition(Vector3Utils.GetPlaneInteractivePoint(screenPos, transform.position.y));
        //}
    }


    public void OnMouseUp()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        CmdMouseUp(NetworkClient.localPlayer.netId, ray);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseUp(uint playerNid, Ray ray)
    {
        if (!CheckHandleAddition(playerNid))
        {
            PlayManager.Instance.SendMsg(playerNid, "�㲻��ʹ�öԷ�������");
            return;
        }
        MouseUp(PlayManager.Instance.GetConn(playerNid), playerNid, ray);
    }

    [Server]
    public void MouseUp(NetworkConnectionToClient conn, uint playerNid, Ray ray)
    {
        if (m_dragState.Value != DragObjState.Moving)
        {
            return;
        }

        m_dragState.Value = DragObjState.Available;
        m_rigidbody.isKinematic = false;
        m_collider.isTrigger = false;
        ChangeLayer("Raycast");
        TargetChangeLayer(conn, "Raycast");

        IAttachable attachObj = RaycastContanier(ray);
        if (attachObj != null)
        {
            attachObj.Attach(playerNid, this);
        }
        else
        {   
            //δ������ʩ����
            //TODO:�ɹ���ʩ����ֵ�����ڲ�ͬ���͵����࣬ʩ�ӷ���Ҳ��ͬ
            //m_rigidbody.AddForceAtPosition(targetForceVector * 200, transform.position + Vector3.up);
        }
    }

    [Server]
    protected IAttachable RaycastContanier(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            //Debug.Log($"��⵽AttachObj{hit.collider.transform.name}");
             
            Transform hitObjTrans = hit.collider.transform;
            IAttachable attachObj = null;
            if (hitObjTrans.TryGetComponent(out attachObj))
            {
                return attachObj;
            }
            else if (hitObjTrans.parent != null && hitObjTrans.parent.TryGetComponent(out attachObj))
            {
                return attachObj;
            }
        }
        return null;
    }

    [Server]
    public virtual IEnumerator ApplyAttachTransform(Transform attachTrans, UnityAction callback = null)
    {
        #region �ɷ����������������
        //Vector3 originPos = transform.position;
        //Quaternion originRot = transform.rotation;
        //float t = 0f;
        //while(t < 1f)
        //{
        //    yield return null;
        //    t += Time.deltaTime;
        //    t = t < 1f ? t : 1f;
        //    transform.position = Vector3.Lerp(originPos, m_currentClone.transform.position, t);
        //    transform.rotation = Quaternion.Lerp(originRot, m_currentClone.transform.rotation, t);
        //}
        #endregion

        //Ӧ����ת
        m_dragState.Value = DragObjState.Freeze;
        transform.rotation = attachTrans.rotation;

        //��ֵӦ������
        Vector3 targetPos = attachTrans.position +
            new Vector3(0, m_collider.bounds.center.y - m_collider.bounds.min.y, 0);
        while (Vector3.Distance(transform.position, targetPos) >= 0.01f)
        {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.05f);
        }
        transform.position = targetPos;

        //����һЩ״̬����
        m_currentClone.DestroySelf();
        m_currentClone = null;
        m_rigidbody.isKinematic = false;
        m_collider.isTrigger = false;

        if(callback != null)
            callback();
    }

    [Server]
    public virtual IEnumerator RecycleDragObject(uint playerNid, UnityAction callback = null)
    {
        //����ʱȡ���߹���ʾ
        RpcCancelHighlight();
        m_dragState.Value = DragObjState.Freeze;
        m_rigidbody.isKinematic = true;
        m_collider.isTrigger = true;
        m_currentClone.DestroySelf();
        m_currentClone = null;

        //��ֵӦ������
        Vector3 targetPos = Container.transform.position + new Vector3(0, 1f, 0);
        while (Vector3.Distance(transform.position, targetPos) >= 0.01f)
        {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.05f);
        }
        transform.position = targetPos;

        //����һЩ״̬����
        m_dragState.Value = DragObjState.Available;
        m_rigidbody.isKinematic = false;
        m_collider.isTrigger = false;

        //�Զ��Ż���¨
        Container.Attach(playerNid, this);

        if (callback != null)
            callback();
    }

    private void ChangeLayer(string layerName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach(var child in transforms)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    [TargetRpc]
    private void TargetChangeLayer(NetworkConnectionToClient conn, string layerName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (var child in transforms)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    //TODO:���ڷ���˺Ϳͻ��ζ�Ҫִ�еģ������ж�Ӧ������
    [Server]
    public void ServerBeAdd()
    {
        gameObject.SetActive(false);
        transform.position = transform.position + Vector3.up * 1f;
        transform.rotation = Quaternion.identity;
    }

    [ClientRpc]
    public void RpcBeAdd()
    {
        gameObject.SetActive(false);
    }

    //TODO:���ڷ���˺Ϳͻ��ζ�Ҫִ�еģ������ж�Ӧ������
    [Server]
    public void ServerBeGet()
    {
        gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcBeGet()
    {
        gameObject.SetActive(true);
    }

    public void Restart(uint playerNid)
    {
        StartCoroutine(RecycleDragObject(playerNid));
    }

}