using System.Collections.Generic;
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
        if (Mathf.Abs(slope - expectedSlope) < 0.2)
        {
            Debug.Log("inline");
            return true;
        }
        Debug.Log("slope: " + slope);
        return false;
    }



    // Returns true if lines intersect, and sets 'intersection' to the point.
    // lineAStart, lineAEnd define the first line
    // lineBStart, lineBEnd define the second line
    // TODO:This function still has some issues that need to be fixed
    public static Vector2 GetLineIntersection(
        Vector2 lineAStart, Vector2 lineAEnd,
        Vector2 lineBStart, Vector2 lineBEnd)
    {
        Vector2 intersection = Vector2.zero;

        float x1 = lineAStart.x;
        float y1 = lineAStart.y;
        float x2 = lineAEnd.x;
        float y2 = lineAEnd.y;

        float x3 = lineBStart.x;
        float y3 = lineBStart.y;
        float x4 = lineBEnd.x;
        float y4 = lineBEnd.y;

        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        if (Mathf.Abs(denominator) < Mathf.Epsilon)
        {
            return intersection;
        }

        float numeratorX = (x1 * y2 - y1 * x2);
        float numeratorY = (x3 * y4 - y3 * x4);

        float px = (numeratorX * (x3 - x4) - (x1 - x2) * numeratorY) / denominator;
        float py = (numeratorX * (y3 - y4) - (y1 - y2) * numeratorY) / denominator;

        intersection = new Vector2(px, py);
        return intersection;
    }

    public static Vector2 GetRobotIntersection(List<RobotSingular> startRobotNeighbors, List<RobotSingular> endRobotNeighbors, Vector2 start, Vector2 end)
    { 
        for (int i = 0; i < startRobotNeighbors.Count; i++)
        {
            for (int j = 0; j < endRobotNeighbors.Count; j++)
            {
                Vector2 intersection = GetLineIntersection(start, startRobotNeighbors[i].transform.position, end, endRobotNeighbors[j].transform.position);
                //insection can not exceed the bound of the two lines
                if (intersection != Vector2.zero && Utils.CheckInline(start, intersection) && Utils.CheckInline(end, intersection))
                {
                    return intersection;
                }
            } 
        }
        return Vector2.zero;
    }
}

