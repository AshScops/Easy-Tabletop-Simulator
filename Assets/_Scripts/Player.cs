using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private GoChessColor m_currentColor = GoChessColor.Unknown;

    /// <summary>
    /// ��ǰ��Ҷ�Ӧ��һ���ӻ��ǰ���
    /// </summary>
    public GoChessColor CurrentColor
    {
        set { m_currentColor = value; }
        get { return m_currentColor; }
    }

    public override void OnStartClient()
    {

    }

    //[Command(requiresAuthority = false)]
    //public void CmdPlayerAdd()
    //{
    //    PlayManager.Instance.Add(this);
    //    this.m_currentColor = GoChessColor.Black;
    //}

    public override void OnStopClient()
    {

    }

    //[Command(requiresAuthority = false)]
    //public void CmdPlayerRemove()
    //{
    //    PlayManager.Instance.Remove(this);
    //}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isLocalPlayer)
        {
            print("LocalPlayerNid:" + netId);
        }

        //�����˳�
        if (Input.GetKeyDown(KeyCode.B) && isLocalPlayer)
        {
            PlayManager.Instance.Remove(this);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayManager.Instance.RestartGame();
        }
    }

    [TargetRpc]
    public void TargetSendMsg(NetworkConnectionToClient targetConn, string msg)
    {
        Debug.Log(msg);
    }
}
