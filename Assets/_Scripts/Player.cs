using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public GoChessColor m_currentColor;

    /// <summary>
    /// ��ǰ��Ҷ�Ӧ��һ���ӻ��ǰ���
    /// </summary>
    public GoChessColor CurrentColor => m_currentColor;

    public override void OnStartClient()
    {
        if (isServer)
        {
            PlayManager.Instance.Add(this);
            CmdGiveColor();
        }
    }

    public override void OnStopClient()
    {
        if (isServer)
        {
            PlayManager.Instance.Remove(this);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isLocalPlayer)
        {
            print("LocalPlayerNid:" + netId);
        }
    }


    [Command]
    protected void CmdGiveColor()
    {
        if(isServer)
            m_currentColor = GoChessColor.Black;
        else
            m_currentColor = GoChessColor.White;
    }

    [TargetRpc]
    public void TargetSendMsg(NetworkConnectionToClient targetConn, string msg)
    {
        Debug.Log(msg);
    }


    //public void GetAuthority(GameObject targetGo)
    //{
    //    if (!isLocalPlayer) return;

    //    if(targetGo == null)
    //    {
    //        Debug.LogWarning("Ŀ�����Ϊ��");
    //        return;
    //    }

    //    var id = targetGo.GetComponent<NetworkIdentity>();

    //    if(id == null)
    //    {
    //        Debug.LogWarning("Ŀ��ű�Ϊ��");
    //        return;
    //    }

    //    CmdAuthority(id, connectionToClient);
    //}

    //[Command]
    //protected void CmdAuthority(NetworkIdentity id, NetworkConnectionToClient connClient)
    //{
    //    id.RemoveClientAuthority();
    //    id.AssignClientAuthority(connClient);
    //}

}
