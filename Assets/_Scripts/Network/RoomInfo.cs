namespace Tabletop
{
    public struct RoomInfo
    {
        public string Name;
        public RoomState State;
        public ushort Port;

        public RoomInfo(string name, ushort port, RoomState state)
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