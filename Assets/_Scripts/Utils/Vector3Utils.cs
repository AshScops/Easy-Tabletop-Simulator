using UnityEngine;

public static class Vector3Utils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="screenPosition"></param>
    /// <param name="plane"></param>
    /// <param name="camera"></param>
    /// <returns>Ŀ���//ָ��Ŀ��������</returns>
    public static Vector3 GetPlaneInteractivePoint(Vector3 screenPosition, float plane = 0, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        var ray = camera.ScreenPointToRay(screenPosition);

        Vector3 dir = ray.direction;
        if (dir.y.Equals(0))
            return Vector3.zero;

        float mutiple = (plane - ray.origin.y) / dir.y;
        return ray.origin + ray.direction * mutiple;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="screenPosition"></param>
    /// <param name="plane"></param>
    /// <param name="camera"></param>
    /// <returns>����������������Ͷ�������ߣ��������ײ��</returns>
    public static bool GetClosetPoint(Vector3 screenPosition, Vector3 worldPos, out Vector3 hitPos, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        Ray ray = camera.ScreenPointToRay(screenPosition);
        var distance = (worldPos - camera.transform.position).magnitude + 100f;
        RaycastHit hit;
        if(Physics.Raycast(ray.origin, ray.direction, out hit, distance, LayerMask.GetMask("Raycast")))
        {
            hitPos = hit.point;
            return true;
        }
        hitPos = Vector3.zero;
        return false;
    }

}
