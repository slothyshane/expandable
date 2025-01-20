using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class RobotManager : MonoBehaviour
{
    [SerializeField]
    public Parameter parameters;
    public Generator generator;
    Camera camera;
    void Start()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 45;
        camera = Camera.main;

        //// test the function 
        //RobotSingular robot_start = FindRobot("Robot57");
        //RobotSingular robot_end = FindRobot("Robot64");

        //List<RobotSingular> neighbors = FindRobotInlineInclude(robot_start, robot_end);
        //InchingForwardReverse(neighbors);
        ////InchingForwardReverse(neighbors);
        ////foreach (RobotSingular neighbor in neighbors)
        ////{
        ////    Debug.Log(neighbor.name);
        ////}

    }


    public List<RobotSingular> GetNeigthbors(RobotSingular robot)
    {
        List<RobotSingular> robotsList = new List<RobotSingular>();
        float radius = robot.GetComponent<CircleCollider2D>().radius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(robot.transform.position, parameters.radiusCommunication * radius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != robot.gameObject)
            {
                robotsList.Add(colliders[i].gameObject.GetComponent<RobotSingular>());
            }
        }
        return robotsList;
    }

    // list of pairs
    public void SendCommandAll(RobotSingular robot, List<(State state, float delay)> commands)
    {
        foreach (var pair in commands)
        {
            robot.AddState(pair.state, pair.delay);
        }
    }

    public void SendCommand(RobotSingular robot, State state, float delay = 0f)
    {
        robot.AddState(state, delay);
    }

    public RobotSingular FindRobot(string robot_name)
    {
        RobotSingular robot = generator.GetRobot(robot_name);
        if (robot == null)
        {
            Debug.Log(robot_name + " not found");
            return null;
        }
        return robot;
    }

    public List<RobotSingular> FindRobotInlineExclude(RobotSingular startRobot, RobotSingular endRobot) {
        List<RobotSingular> robots = new List<RobotSingular>();
        // use raycast to find all the robots in the path
        Vector2 start = startRobot.transform.position;
        Vector2 end = endRobot.transform.position;
        Vector2 direction = end - start;
        float distance = Vector2.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject.GetComponent<RobotSingular>().name != startRobot.name && hit.collider.gameObject.GetComponent<RobotSingular>().name != endRobot.name)
                {
                    robots.Add(hit.collider.gameObject.GetComponent<RobotSingular>());
                }
            }
        }
        return robots;
    }

    public List<RobotSingular> FindRobotInlineInclude(RobotSingular startRobot, RobotSingular endRobot)
    {
        List<RobotSingular> robots = new List<RobotSingular>();
        // use raycast to find all the robots in the path
        Vector2 start = startRobot.transform.position;
        Vector2 end = endRobot.transform.position;
        Vector2 direction = end - start;
        float distance = Vector2.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null )
                {
                    robots.Add(hit.collider.gameObject.GetComponent<RobotSingular>());
                }
            }
        }
        return robots;
    }

    public void RemoveRobot(string robot_name)
    {
        generator.RemoveRobot(robot_name);
    }

    public void AddRobot(string robot_name, Vector2 pos)
    {
        generator.AddRobot(robot_name, pos);
    }

    // pre-scripted behavior
    // inching forward
    public void InchingForward(List<RobotSingular> robots) 
    {
        int robotCount = robots.Count;
        int robotCounter = 0;
        foreach (RobotSingular robot in robots)
        {
            SendCommand(robot, State.expand, robotCounter * parameters.delayIntervalParameterExpand);
            SendCommand(robot, State.idle, parameters.delayIntervalParameterExpand);
            SendCommand(robot, State.shrinkToOriginal, 0f);
            robotCounter++;
        }
    }

    public void InchingForwardReverse(List<RobotSingular> robots)
    {
        int robotCount = robots.Count;
        int robotCounter = 0;
        foreach (RobotSingular robot in robots)
        {
            SendCommand(robot, State.shrink, robotCounter * parameters.delayIntervalParameterShrink);
            SendCommand(robot, State.idle, parameters.delayIntervalParameterShrink);
            SendCommand(robot, State.expandToOriginal, 0f);
            robotCounter++;
        }
    }

    public void debugRobot(RobotSingular robot)
    {
        Debug.Log("--------------");
        Debug.Log(robot.name + " pos:" + robot.transform.position + " current:" + robot.GetCurrentState() + " next:" + robot.GetNextState());
        Debug.Log(robot.name + " queue_size:" + robot.GetQueueSize() + " force:" + robot.GetForce() + " radius:" + robot.GetRadius() + " velocity:" + robot.GetVelocity());
    }

    public RobotSingular LeftClickOnRobot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null)
            {
                RobotSingular robot = hit.collider.gameObject.GetComponent<RobotSingular>();
                return robot;
            }
        }
        return null;
    }

    public RobotSingular RightClickOnRobot()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null)
            {
                RobotSingular robot = hit.collider.gameObject.GetComponent<RobotSingular>();
                return robot;
            }
        }
        return null;
    }
}
