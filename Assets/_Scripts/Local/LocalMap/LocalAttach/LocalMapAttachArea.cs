using QFramework;
using UnityEngine;

namespace Tabletop.Local
{
    public abstract class LocalMapAttachArea : LocalOutLineObj, LocalIAttachable
    {
        /// <summary>
        /// �����ĸ�Grids
        /// </summary>
        [HideInInspector] public EasyGrid<LocalGridData> Grids;

        /// <summary>
        /// �ø��Ӧ�ĸ�GridData
        /// </summary>
        [HideInInspector] public LocalGridData Grid;
        [HideInInspector] public LocalMapObj Map;


        public abstract void Attach(LocalDragObj dragObject);
    }

}
