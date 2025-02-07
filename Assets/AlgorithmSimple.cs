using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public enum Motion { idle, simpleMotion };

public class AlgorithmSimple : MonoBehaviour
{
    [SerializeField]
    private Parameter parmeters;
    public RobotManager RobotManager;
    Camera camera;

    private bool pathFound = false;
    private Queue<Execution> exeQueue = new Queue<Execution>();
    private Motion currMotion = Motion.idle;
    RobotSingular start;
    RobotSingular end;

private void Awake()
    {
        camera = Camera.main;
    }
    void Start()
    {
       
    }

    void FixedUpdate()  
    {
        if (exeQueue.Count > 0) { 
            if (RobotManager.MotionCheckAll() == false) {
                Debug.Log("motion stopped");
                Execution currExe = exeQueue.Dequeue();
                if (currExe.nextMotion == Motion.simpleMotion)
                {
                    simpleMotion(currExe.start, currExe.end);
                }
            }
        }
    }

    private void Update()
    {
        if (start == null)
        {
            start = RobotManager.LeftClickOnRobot();
        }
        else 
        {
            start.ChangeColor("red");
            //Debug.Log("start: " + start.name);
            //Debug.Log("start pos: " + start.transform.position);
        }

        if (end == null)
        {
            end = RobotManager.RightClickOnRobot();
        }
        else
        {
            end.ChangeColor("green");
            //Debug.Log("end: " + end.name);
            //Debug.Log("end pos: " + end.transform.position);
        }

        // both robots have been clicked. this is where the algorithm starts
        if (start != null && end != null && pathFound == false) 
        {
            Transform startTransform = start.transform;
            Transform endTransform = end.transform;

            // get the pose of start
            Vector2 startPos = startTransform.position;
            Vector2 endPos = endTransform.position;

            if (Utils.CheckInline(startPos, endPos)) {
  
                exeQueue.Enqueue(new Execution(Motion.simpleMotion, start, end));
                //motionQueue.Enqueue(Motion.simpleMotion);
            }
            else
            {
                Vector2 middle = RobotManager.GetInlineDecompose(startPos, endPos);
                Debug.Log("middle: " + middle);
                RobotSingular middleRobot = RobotManager.FindRobotByPos(middle);
                Debug.Log("middle robot: " + middleRobot.name);

                List<RobotSingular> inBetweenStart = RobotManager.FindRobotInlineExclude(start, middleRobot);
                int inBetweenStartCount = inBetweenStart.Count;
                List<RobotSingular> inBetweenEnd = RobotManager.FindRobotInlineExclude(middleRobot, end);
                int inBetweenEndCount = inBetweenEnd.Count;
                Debug.Log("inBetweenStartCount: " + inBetweenStartCount + " inBetweenEndCount: " + inBetweenEndCount);
                RobotSingular startRobot = RobotManager.FindRobotByNumber(145);
                RobotSingular endRobot = RobotManager.FindRobotByNumber(160);
                //exeQueue.Enqueue(new Execution(Motion.simpleMotion, startRobot, endRobot));
                //exeQueue.Enqueue(new Execution(Motion.simpleMotion, startRobot, endRobot));
                startRobot = RobotManager.FindRobotByNumber(55);
                endRobot = RobotManager.FindRobotByNumber(154);
                exeQueue.Enqueue(new Execution(Motion.simpleMotion, startRobot, endRobot));
                exeQueue.Enqueue(new Execution(Motion.simpleMotion, startRobot, endRobot));
                exeQueue.Enqueue(new Execution(Motion.simpleMotion, startRobot, endRobot));


            }
            pathFound = true;
        }
    }

    public void simpleMotion(RobotSingular start, RobotSingular end) {
        RobotSingular robot1;
        RobotSingular robot2;
        RobotSingular robot3;
        RobotSingular robot4;
        List<RobotSingular> robotExpand = RobotManager.FindRobotInlineInclude(start, end);
        RobotManager.InchingForward(robotExpand);

        List<RobotSingular> robotShrink = RobotManager.FindParallelRobots(start, end, 1);
        List<RobotSingular> robotShrink2 = RobotManager.FindParallelRobots(start, end, 2);
        // every two robots 
        if (robotShrink.Count == 4)
        {
            robot1 = robotShrink[0];
            robot2 = robotShrink[1];
            robot3 = robotShrink[2];
            robot4 = robotShrink[3];

            List<RobotSingular> robotShrinkLayer1a = RobotManager.FindRobotInlineInclude(robot1, robot2);
            List<RobotSingular> robotShrinkLayer1b = RobotManager.FindRobotInlineInclude(robot3, robot4);
            RobotManager.InchingForwardReverse(robotShrinkLayer1a);
            RobotManager.InchingForwardReverse(robotShrinkLayer1b);
        }


        if (robotShrink2.Count == 4)
        {
            robot1 = robotShrink2[0];
            robot2 = robotShrink2[1];
            robot3 = robotShrink2[2];
            robot4 = robotShrink2[3];
            List<RobotSingular> robotShrinkLayer2a = RobotManager.FindRobotInlineInclude(robot1, robot2);
            List<RobotSingular> robotShrinkLayer2b = RobotManager.FindRobotInlineInclude(robot3, robot4);
            RobotManager.InchingForwardReverse(robotShrinkLayer2a);
            RobotManager.InchingForwardReverse(robotShrinkLayer2b);
        }
    }
}


public struct Execution
{
    public Motion nextMotion;
    public RobotSingular start;
    public RobotSingular end;

    public Execution(Motion motion, RobotSingular start = null, RobotSingular end = null)
    {
        nextMotion = motion;
        this.start = start;
        this.end = end;
    }
}