using LitJson;
using kcp2k;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Tabletop
{
    [AddComponentMenu("")]
    public class NetworkRoomManagerExt : NetworkRoomManager
    {
        public static new NetworkRoomManagerExt singleton { get; private set; }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            singleton = this;

            //���ΪDSʱ�ǵù�ѡ����
            if (autoStartServerBuild)
            {
                InitForServerBuild();
            }
        }

        public override void Update()
        {
            base.Update();

            //��Cɨ�跿��
            if(Input.GetKeyDown(KeyCode.C))
            {
                StartCoroutine(CheckRoomAvaliable());
            }
        }

        /// <summary>
        /// This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName)
        {
            // spawn the initial batch of Rewards
            if (sceneName == GameplayScene)
            {
                //��ʼ��Ϸʱ����Redis
                SetRedisValue(RoomState.Started);
            }
        }

        /// <summary>
        /// Called just after GamePlayer object is instantiated and just before it replaces RoomPlayer object.
        /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
        /// into the GamePlayer object as it is about to enter the Online scene.
        /// </summary>
        /// <param name="roomPlayer"></param>
        /// <param name="gamePlayer"></param>
        /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            //PlayerScore playerScore = gamePlayer.GetComponent<PlayerScore>();
            //playerScore.index = roomPlayer.GetComponent<NetworkRoomPlayer>().index;
            return true;
        }

        public override void OnRoomStopClient()
        {
            base.OnRoomStopClient();
        }

        public override void OnRoomStopServer()
        {
            base.OnRoomStopServer();
        }

        /*
            This code below is to demonstrate how to do a Start button that only appears for the Host player
            showStartButton is a local bool that's needed because OnRoomServerPlayersReady is only fired when
            all players are ready, but if a player cancels their ready state there's no callback to set it back to false
            Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
            Setting showStartButton false when the button is pressed hides it in the game scene since NetworkRoomManager
            is set as DontDestroyOnLoad = true.
        */

        bool showStartButton;

        //        public override void OnRoomServerPlayersReady()
        //        {
        //            // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
        //#if UNITY_SERVER
        //            base.OnRoomServerPlayersReady();
        //#else
        //            showStartButton = true;
        //#endif
        //        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
            {
                print("��Ϸ��ʼ��");
                showStartButton = false;

                //�л�������ʱ����Զ�����Player��gameObject
                ServerChangeScene(GameplayScene);
            }

            GUILayout.BeginArea(new Rect(0, 300, Screen.width, Screen.height));
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (GUI.Button(new Rect(20 + i * 90, 40, 80, 20), Rooms[i].RoomName + Rooms[i].RoomState.ToString()))
                {
                    (transport as KcpTransport).port = Rooms[i].Port;
                }
            }
            GUILayout.EndArea();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }


        bool disconnect = false;
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            //ħ��
            //print("OnServerDisconnect:PlayManager�����Ƴ���Player������");

            //����GamePlay�����󣨼�conn�������ΪPlayer�󣩲ŷ����˳���Ϣ
            if (conn.identity != null && conn.identity.TryGetComponent(out Player player))
            {
                PlayerManager.Instance.Remove(player);

                if(!disconnect)
                {
                    disconnect = true;
                    NetworkServer.SendToReady(new OppositeExitMessage()
                    {
                        MsgContent = $"���Nid:{player.netId}�˳�"
                    },
                    Channels.Reliable);
                }
            }

            base.OnServerDisconnect(conn);

            //����˳�ʱ�����ۺ����˳�������Redis
            SetRedisValue(RoomState.Available);

            if (roomSlots.Count == 0)
            {
                ServerChangeScene(RoomScene);
            }
        }


        public void SetPort(string port)
        {
            (transport as KcpTransport).port = ushort.Parse(port);
        }

        public void InitForServerBuild()
        {
            Debug.Log("InitForServerBuild");
            string CommandLine = Environment.CommandLine;
            string[] CommandLineArgs = Environment.GetCommandLineArgs();
            for (int i = 0; i < CommandLineArgs.Length; i++)
            {
                if (CommandLineArgs[i] == "--port" && i + 1 < CommandLineArgs.Length)
                {
                    string port = CommandLineArgs[i + 1];
                    SetPort(port);
                }
            }
            //NetworkRoomManagerExt.singleton.StartServer();

            //�ղ���ʱ����Redis
            StartCoroutine(SetRedisValue(RoomState.Available));
        }


        public readonly string url = "https://localhost:10003/";
        public IEnumerator SetRedisValue(RoomState roomState)
        {
            WWWForm form = new WWWForm();
            form.AddField("port", (transport as KcpTransport).port);
            form.AddField("state", Enum.GetName(typeof(RoomState), roomState));

            using (UnityWebRequest request = UnityWebRequest.Post($"{url}SetRoomState", form))
            {
                request.certificateHandler = new WebRequestCertificate();
                //��@RequestParam����˵Ĳ���ֻ֧�ֱ�����֧��Json��ʽ��@RequestBody��ǵĲ�����֧��Json
                //request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"SetRedisValue Success, info:{request.downloadHandler.text}");
                }
                else
                {
                    Debug.Log(request.result);
                }
            }
        }


        public List<RoomInfo> Rooms = new List<RoomInfo>();
        public IEnumerator CheckRoomAvaliable()
        {
            Rooms.Clear();

            using (UnityWebRequest request = UnityWebRequest.Get($"{url}GetRooms"))
            {
                request.certificateHandler = new WebRequestCertificate();
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"SetRedisValue Success, info:{request.downloadHandler.text}");
                    string receiveContent = request.downloadHandler.text;
                    List<RoomInfo> rooms = JsonMapper.ToObject<List<RoomInfo>>(receiveContent);
                }
                else
                {
                    Debug.Log(request.result);
                }
            }
        }
    }
}

