using StackExchange.Redis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop
{
    public class RoomChecker : MonoBehaviour
    {
        void Start()
        {
            //using (ConnectionMultiplexer conn = RedisHelper.RedisConn)
            //{
            //    var db = conn.GetDatabase(); //��Redis�л�������ݿ�Ľ���ʽ����
            //    //db.StringSet("����", "������ķ���/�˿�״̬");

            //    for(int i = 1004; i <= 1007; i++)
            //    {
            //        var key = "Room" + i.ToString();
            //        if (db.KeyExists(key))
            //        {
            //            string state = db.StringGet(key);
            //            //if(state.Equals("Available"))
            //            //{

            //            //}
            //            print($"{i}: {state}");
            //        }
            //    }
            //}
        }
    }
}