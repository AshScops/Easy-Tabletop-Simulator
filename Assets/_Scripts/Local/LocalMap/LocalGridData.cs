using UnityEngine;

namespace Tabletop.Local
{
    public class LocalGridData
    {
        public LocalGridData(int x, int z, Transform parentTrans, Vector3 offset, float size = 1f)
        {
            X = x;
            Z = z;
            m_parentTrans = parentTrans;
            m_offset = offset;
            m_localPos = new Vector3(x * size, 0, z * size);
        }

        /// <summary>
        /// ��Ӧ�������������ĸ�
        /// </summary>
        public LocalIAttachable AttachArea;

        /// <summary>
        /// �Ƿ�ռ��/����
        /// </summary>
        public bool Occupied;

        /// <summary>
        /// ���������������
        /// </summary>
        public LocalDragObj DragObject;

        /// <summary>
        /// ��ǰ���Ӧ�ľֲ�����
        /// </summary>
        private Vector3 m_localPos;

        /// <summary>
        /// �����ĸ�����Ϊ�ο�ϵ
        /// </summary>
        private Transform m_parentTrans;

        /// <summary>
        /// ƫ��
        /// </summary>
        public Vector3 m_offset;

        /// <summary>
        /// ��ǰ���Ӧ����������
        /// </summary>
        public Vector3 WorldPos => m_localPos + m_parentTrans.position + m_offset;

        /// <summary>
        /// �������е�x����
        /// </summary>
        public int X;

        /// <summary>
        /// �������е�z����
        /// </summary>
        public int Z;
    }

}