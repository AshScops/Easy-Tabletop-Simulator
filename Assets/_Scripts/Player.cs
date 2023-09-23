using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private void Update()
    {
    }

    public void GetAuthority(GameObject targetGo)
    {
        if(targetGo == null)
        {
            Debug.LogWarning("Ŀ�����Ϊ��");
            return;
        }

        var id = targetGo.GetComponent<NetworkIdentity>();

        if(id == null)
        {
            Debug.LogWarning("Ŀ��ű�Ϊ��");
            return;
        }

        CmdAuthority(id, connectionToClient);
    }

    [Command]
    public void CmdAuthority(NetworkIdentity id, NetworkConnectionToClient connClient)
    {
        id.RemoveClientAuthority();
        id.AssignClientAuthority(connClient);
    }

}
