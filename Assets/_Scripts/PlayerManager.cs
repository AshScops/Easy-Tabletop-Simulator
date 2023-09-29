using Mirror;
using System.Collections;
using System.Collections.Generic;
using Tabletop;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ������������
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance => m_instance;
    public static PlayerManager m_instance = null;

    private List<Player> m_players;

    private int maxPlayerCnt = 2;

    public override void OnStartServer()
    {
        if(m_instance == null)
        {
            m_instance = this;
            m_players = new List<Player>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Add(Player player)
    {
        m_players.Add(player);
        Debug.Log($"���Nid:{player.netId}������Ϸ");

        if (PlayerManager.Instance.Count() == maxPlayerCnt)
        {
            PlayerManager.Instance.ForEachWithIndex((i, player) =>
            {
                //TODO:���ڱ�¶�����ѡ��
                if (i == 0)
                    player.CurrentColor = GoChessColor.Black;
                else if (i == 1)
                    player.CurrentColor = GoChessColor.White;
            });
        }
    }

    public void Remove(Player player)
    {
        m_players.Remove(player);
        //SendAllMsg($"���Nid:{player.netId}�˳�");

        NetworkServer.SendToAll(new OppositeExitMessage()
        {
            MsgContent = $"���Nid:{player.netId}�˳�"
        },
        Channels.Reliable, true); ;

        //if (NetworkManager.singleton is NetworkRoomManager room &&
        //SceneManager.GetActiveScene().path == room.GameplayScene)
        //{
        //    room.StopClient();
        //}
    }

    public int Count()
    {
        return m_players.Count;
    }

    public void ForEachWithIndex(UnityAction<int, Player> callback)
    {
        for(int i = 0; i < m_players.Count; i++)
        {
            callback(i, m_players[i]);
        }
    }

    public void ForEach(UnityAction<Player> callback)
    {
        foreach(Player player in m_players)
        {
            callback(player);
        }
    }

    public void SendMsg(uint playerNid, string msg)
    {
        ForEach((player) =>
        {
            if (player.netId == playerNid)
            {
                player.TargetSendMsg(player.connectionToClient, msg);
            }
        });
    }

    public void SendAllMsg(string msg)
    {
        ForEach((player) =>
        {
            player.TargetSendMsg(player.connectionToClient, msg);
        });
    }

    public NetworkConnectionToClient GetConn(uint playerNid)
    {
        NetworkConnectionToClient res = null;
        ForEach((player) =>
        {
            if (player.netId == playerNid)
                res = player.connectionToClient;
        });
        return res;
    }

    public void RestartGame()
    {
        MapObject[] maps = GameObject.FindObjectsOfType<MapObject>();

        foreach(var map in maps)
        {
            map.RestartGame(NetworkClient.localPlayer.netId);
        }
    }


}
