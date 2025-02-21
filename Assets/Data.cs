using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable] 
public class ProgressData
{
    public List<Wrapper> robotToExpand;
    public List<Wrapper> robotToShrink;
  
}
[System.Serializable]
public class Wrapper
{
    public List<RobotSingularData> robots;
}

[System.Serializable]
public class RobotSingularData
{
    public float growthRate;
    public float shrinkRate;
    public float adhesionStrength;
    public float maxAdhesionForce;
    public float adhesionRangeMax;
    public float maxRadius;
    public float minRadius;
    // location of the robot
    public Vector2 position;
    public int robotNum;
}

[System.Serializable]
public class SceneData
{
    public List<RobotLocationData> robotLocations;
}

[System.Serializable]
public class RobotLocationData
{
    public int robotNum;
    public Vector2 position;
}