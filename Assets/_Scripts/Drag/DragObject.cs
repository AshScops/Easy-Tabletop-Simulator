using Mirror;
using QFramework;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Tabletop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public void SetDragState(DragObjState targetState)
    {
        m_dragState.Value = targetState;
    }

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

    protected virtual void Awake()
    {
        Init();
    }

    protected override void Init()
    {
        base.Init();

        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = transform.Find("model").GetComponent<Collider>();
        m_dragState = new BindableProperty<DragObjState>(DragObjState.Available);
    }

    public override void OnMouseDown()
    {
        CmdMouseDown();
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDown()
    {
        MouseDown();
    }

    public void MouseDown()
    {
        if (m_dragState.Value != DragObjState.Available)
        {
            return;
        }

        m_dragState.Value = DragObjState.Moving;
        m_rigidbody.isKinematic = true;
        m_collider.isTrigger = true;
        ChangeLayer("IgnoreRaycast");
    }


    public void OnMouseDrag()
    {
        CmdMouseDrag(Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseDrag(Vector3 screenPos)
    {
        MouseDrag(screenPos);
    }

    public void MouseDrag(Vector3 screenPos)
    {
        if (m_dragState.Value != DragObjState.Moving)
        {
            return;
        }

        Vector3 hitPos;
        if (Vector3Utils.GetClosetPoint(screenPos, transform.position, out hitPos))
        {
            var yOffset = new Vector3(0, m_collider.bounds.center.y - m_collider.bounds.min.y, 0);
            transform.position = hitPos + yOffset;
        }
        else
        {
            transform.position = Vector3Utils.GetPlaneInteractivePoint(screenPos, transform.position.y);
        }
    }

    public void OnMouseUp()
    {
        CmdMouseUp(Input.mousePosition);
    }

    [Command(requiresAuthority = false)]
    public void CmdMouseUp(Vector3 mousePos)
    {
        MouseUp(mousePos);
    }
    
    public void MouseUp(Vector3 screenPos)
    {
        if (m_dragState.Value != DragObjState.Moving)
        {
            return;
        }

        //δ������ʩ����
        m_dragState.Value = DragObjState.Available;
        m_rigidbody.isKinematic = false;
        m_collider.isTrigger = false;
        ChangeLayer("Raycast");

        Vector3 lastPoint = Vector3Utils.GetPlaneInteractivePoint(screenPos, transform.position.y);
        Vector3 targetForceVector = lastPoint - transform.position;

        if (RaycastContanier(screenPos))
        {

        }
        else
        {
            //TODO:�ɹ���ʩ����ֵ�����ڲ�ͬ���͵����࣬ʩ�ӷ���Ҳ��ͬ
            m_rigidbody.AddForceAtPosition(targetForceVector * 200, transform.position + Vector3.up);
        }
    }

    protected virtual bool RaycastContanier(Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            //Debug.Log($"��⵽AttachObj{hit.collider.transform.name}");
             
            Transform hitObjTrans = hit.collider.transform;
            IAttachable attachObj = null;
            if (hitObjTrans.TryGetComponent<IAttachable>(out attachObj))
            {
                attachObj.Attach(this);
                return true;
            }
            else if (hitObjTrans.parent != null && hitObjTrans.parent.TryGetComponent<IAttachable>(out attachObj))
            {
                attachObj.Attach(this);
                return true;
            }
        }
        return false;
    }

    public virtual IEnumerator ApplyAttachTransform(Transform attachTrans, UnityAction callback)
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

        m_dragState.Value = DragObjState.Freeze;

        //Ӧ����ת
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

        m_rigidbody.isKinematic = true;
        m_collider.isTrigger = false;

        if(callback != null)
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


}