namespace Tabletop.Local
{
    public interface LocalIRetract
    {
        public void RecordStep(LocalMapAttachArea attachArea);

        /// <summary>
        /// �����Զ�ս������ʱ������һ��=��ȥ��ö���ӣ�
        /// �����Զ�սΧ��ʱ������һ��=��ȥ��ö����+��ԭ����Ե�����
        /// </summary>
        public void RetractLastStep();

        public void RetractAll();
    }
}
