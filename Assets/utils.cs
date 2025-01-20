using UnityEngine;

public static class Utils
{
    public static bool IsInLayerMask(GameObject gameObject, LayerMask layerMask)
    {
        // Shift the layer index to get a bitmask
        int objLayerMask = 1 << gameObject.layer;
        // Perform bitwise AND to check if the layer is in the LayerMask
        return (layerMask.value & objLayerMask) != 0;
    }

    public static bool CheckInline(Vector2 pos1, Vector2 pos2)
    {
        float dr = pos1.x - pos2.x;
        float dc = pos1.y - pos2.y;
        float expectedSlope = 0.577f;

        Debug.Log("dr: " + dr + " dc: " + dc);


        //check if dr is close to 0
        if (Mathf.Abs(dr) < 0.01)
        {
            Debug.Log("not inline");
            return false;
        }

        if (Mathf.Abs(dc) < 0.01)
        {
            Debug.Log("inline");
            return true;
        }

        float slope = dr / dc;
        if (Mathf.Abs(slope - expectedSlope) < 0.3)
        {
            Debug.Log("inline");
            return true;
        }
        Debug.Log("slope: " + slope);
        return false;
    }

}
