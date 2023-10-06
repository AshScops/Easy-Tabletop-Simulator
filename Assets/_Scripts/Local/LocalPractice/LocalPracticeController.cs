using QFramework;
using UnityEngine;

namespace Tabletop.Local
{
    public class LocalPracticeController : MonoBehaviour
    {
        public GameObject BlackBasket;
        public GameObject WhiteBasket;
        public GameObject Map;

        public static LocalPracticeController Instance;
        public GoChessColor PlayerColor;
        private IRobot m_robot;

        public EasyEvent<GoChessColor> WinEvent;

        public void Awake()
        {
            Instance = this;
            Init();
        }

        private LocalGoChessBasket m_blackBasket;
        private LocalGoChessBasket m_whiteBasket;
        private LocalMapObj m_map;
        protected void Init()
        {
            m_blackBasket = Instantiate(BlackBasket).GetComponent<LocalGoChessBasket>();
            m_whiteBasket = Instantiate(WhiteBasket).GetComponent<LocalGoChessBasket>();
            m_map = Instantiate(Map).GetComponent<LocalMapObj>();


            WinEvent = new EasyEvent<GoChessColor>();
            WinEvent.Register((winColor) =>
            {
                win = true;
                if (winColor == GoChessColor.Black)
                    winMsg = "�ڷ�ʤ����������¿�ʼ";
                else
                    winMsg = "�׷�ʤ����������¿�ʼ";
            });
        }

        private void Start()
        {
            if (PlayerColor == GoChessColor.White)
            {
                m_robot = new LocalGobangRobot(GoChessColor.Black, m_blackBasket, m_map);
            }
            else if (PlayerColor == GoChessColor.Black)
            {
                m_robot = new LocalGobangRobot(GoChessColor.White, m_whiteBasket, m_map);
            }
            else
            {
                print("���δѡ����ɫ");
            }
        }

        private bool win = false;
        private string winMsg;
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 200, Screen.width, Screen.height));

            if (m_map.CurrentColor.Value == PlayerColor && GUI.Button(new Rect(0, 40, 300, 20), "����"))
            {
                m_map.RetractLastStep();
            }
            if (win && GUI.Button(new Rect(40, 40, 300, 20), winMsg))
            {
                win = false;
                //�������ӷ�����¨

            }

            GUILayout.EndArea();
        }

    }

}