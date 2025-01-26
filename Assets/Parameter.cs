
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

/*
 *  * This class is used to store the parameters for the game.
 *  
 *  */
[CreateAssetMenu(fileName = "Parameter", menuName = "Parameters/Parameters")]
public class Parameter : ScriptableObject
{
    // Individual Robot Parameters
    public float growthRate = 1f; // how fast the robot expands, unit is wrt the time step
    public float shirnkRate = 1f; // how fast the robot shrinks, unit is wrt the time step
    public float maxRadius = 1.275f; // the maximum radius of the robot, 0.5 is the original, this is the multiplier
    public float minRadius = 0.85f; // the minimum radius of the robot, 0.5 is the original, this is the multiplier
    public float adhesionStrength = 0.2f;  // the adhesion strength between the robots
    public float maxAdhesionForce = 15f;  // the maximum adhesion force between the robots
    public float adhesionRangeMax = 1.5f;  // the maximum adhesion range between the robots, 1.5 * radius

    // Grid Parameters
    public int rows = 4; // Number of rows 10
    public int columns = 10; // Number of columns 16
    public int rowsToShrink = 2; // Number of rows to shrink

    // Centralized Control Parameters
    public float delayIntervalParameterExpand = 2f;
    public float delayIntervalParameterShrink = 2f;

    // Robot Manager Parameters
    public float radiusCommunication = 2f; // the radius of communication between the robots


    // take in a list
    public List<float> robotsToBeRemoved = new List<float>(); 
}


//TODO: CONFIG2
//Takes too long to generate
//private int rows = 5;            // Number of rows
//private int columns = 12;         // Number of columns

//TODO: CONFIG3
//private int rows = 3;            // Number of rows
//private int columns = 14;         // Number of columns