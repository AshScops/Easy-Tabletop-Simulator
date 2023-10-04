namespace Tabletop.Online
{
    public struct OnlineRoomInfo
    {
        public string Name;
        public OnlineRoomState State;
        public ushort Port;

        public OnlineRoomInfo(string name, ushort port, OnlineRoomState state)
        {
            Name = name;
            Port = port;
            State = state;
        }

        ////ͨ�� json ��ȡ���� string ֵ����ʵ�����ö�ٸ�ֵ
        //public string State
        //{
        //    set
        //    {
        //        m_state = (RoomState)System.Enum.Parse(typeof(RoomState), value);
        //    }
        //}
    }
}