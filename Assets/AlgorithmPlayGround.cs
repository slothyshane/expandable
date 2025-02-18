using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// PlayGround
/// Press E to enable expansion mode
/// Press C to enable contraction mode
/// Use number 1 - 6 to indicate the simultaneous expansion and contraction of the robots
/// Left click to select the robot to expand 
/// Right click to select the robot to contract
/// Once selected, press enter to start the algorithm
/// </summary>

public class AlgorithmPlayGround : MonoBehaviour
{
    [SerializeField]
    private Parameter parmeters;
    public RobotManager RobotManager;
    public TextMeshProUGUI textComponent;
    Camera camera;

    private bool pathFound = false;
    RobotSingular expandRobot;
    RobotSingular shrinkRobot;
    private List<List<RobotSingular>> robotToExpand;
    private List<List<RobotSingular>> robotToShrink;
    private string mode = "Idle";
    private int keyboardNum = 1;
    private bool begin = false;
    private bool motion = false;

    //display the text on the screen


    private void Awake()
    {
        camera = Camera.main;
    }
    void Start()
    {
        robotToExpand = new List<List<RobotSingular>>();
        robotToShrink = new List<List<RobotSingular>>();

        // Create 6 inner lists and add them to the outer list.
        for (int i = 0; i < 6; i++)
        {
            List<RobotSingular> robotList = new List<RobotSingular>();
            robotToExpand.Add(robotList);
        }

        for (int i = 0; i < 6; i++)
        {
            List<RobotSingular> robotList = new List<RobotSingular>();
            robotToShrink.Add(robotList);
        }
    }

    void FixedUpdate()  
    {

    }

    private void Update()
    {
        KeyStrokeDetection();
        expandRobot = RobotManager.LeftClickOnRobot();
        shrinkRobot = RobotManager.RightClickOnRobot();

        if (expandRobot != null && mode != "Idle")
        {
            expandRobot.ChangeColor("red", 0.4f + 0.1f * keyboardNum);
            robotToExpand[keyboardNum - 1].Add(expandRobot);
            expandRobot.textComponent.text = robotToExpand[keyboardNum - 1].Count.ToString();
        }
        else if (shrinkRobot != null && mode != "Idle")
        {
            shrinkRobot.ChangeColor("green", 0.4f + 0.1f * keyboardNum);
            robotToShrink[keyboardNum - 1].Add(shrinkRobot);
            shrinkRobot.textComponent.text = robotToShrink[keyboardNum - 1].Count.ToString();
        }

        if (begin)
        {
            // inch worm motion for the robots
            foreach (List<RobotSingular> robotList in robotToExpand)
            {
                if (robotList.Count > 0)
                {
                    RobotManager.InchingForward(robotList);
                }
            }

            foreach (List<RobotSingular> robotList in robotToShrink)
            {
                if (robotList.Count > 0)
                {
                    RobotManager.InchingForwardReverse(robotList);
                }
            }

            // clear the list
            for (int i = 0; i < 6; i++)
            {
                robotToExpand[i].Clear();
                robotToShrink[i].Clear();
            }
        }

        MotionCheck();
    }

    private void KeyStrokeDetection()
    {
        if (motion == false && begin == false)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                mode = "Expand";
            }

            else if (Input.GetKeyDown(KeyCode.C))
            {
                mode = "Contract";
            }

            // get the number
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                keyboardNum = 1;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                keyboardNum = 2;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                keyboardNum = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                keyboardNum = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                keyboardNum = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                keyboardNum = 6;
            }
        }

        // check for space key
        if (Input.GetKeyDown(KeyCode.Return)) {
            begin = true;
            mode = "Executing";
        }
        textComponent.text = mode + " Mode: queue: " + keyboardNum;

    }

    public void MotionCheck() {
        if (begin == true) {
            if (RobotManager.MotionCheckAll() != true) {
                motion = false;
                mode = "Idle";
                begin = false;
            }
        }

    }

}

