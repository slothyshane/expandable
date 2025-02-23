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
    float radiusCommunication;
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


    public List<RobotSingular> GetNeigthbors(RobotSingular robot, float communicationRadius = 2 )
    {
        List<RobotSingular> robotsList = new List<RobotSingular>();
        float radius = robot.GetComponent<CircleCollider2D>().radius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(robot.transform.position, communicationRadius * radius);
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

    //public List<RobotSingular> FindAllRobotsInline(Vector2 start, Vector2 end)
    //{
    //    List<RobotSingular> robots = new List<RobotSingular>();

    
    //}

    public List<RobotSingular> FindRobotInlineExclude(Vector2 start, Vector2 end)
    {
        List<RobotSingular> robots = new List<RobotSingular>();
        Vector2 direction = end - start;
        float distance = Vector2.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
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

    public List<RobotSingular> FindRobotInlineInclude(Vector2 start, Vector2 end)
    {
        List<RobotSingular> robots = new List<RobotSingular>();
        Vector2 direction = end - start;
        float distance = Vector2.Distance(start, end);
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, distance);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    robots.Add(hit.collider.gameObject.GetComponent<RobotSingular>());
                }
            }
        }
        return robots;
    }

    public List<RobotSingular> FindParallelRobots(RobotSingular start, RobotSingular end, int layer)
    {
        List<RobotSingular> robots = new List<RobotSingular>();
        Vector3 startPos = start.transform.position;
        Vector3 endPos = end.transform.position;
        Vector3 directionToMove = endPos - startPos;
        Vector3 directionToMoveOpposite = startPos - endPos;
        float radiusRobot = start.GetComponent<CircleCollider2D>().radius;
        float offset;
        Vector2 shrinkStartSide1;
        Vector2 shrinkEndSide1;
        Vector2 shrinkStartSide2;
        Vector2 shrinkEndSide2;
        if (Mathf.Abs(startPos.x - endPos.x) >= Mathf.Abs(startPos.y - endPos.y))
        {
            
            if (startPos.x < endPos.x)
            {
                offset = radiusRobot * layer;
            }
            else
            {
                offset = -radiusRobot * layer;
            }
            shrinkStartSide1 = new Vector2(startPos.x - offset / 2 , startPos.y + radiusRobot * 2 * layer);
            shrinkEndSide1 = new Vector2(endPos.x + offset /2 , endPos.y + radiusRobot * 2 * layer);
            shrinkStartSide2 = new Vector2(startPos.x - offset/2, startPos.y - radiusRobot * 2 * layer);
            shrinkEndSide2 = new Vector2(endPos.x + offset/2, endPos.y - radiusRobot * 2 * layer);
        }
        else
        {
            if (startPos.y < endPos.y)
            {
                offset = radiusRobot * layer;
            }
            else
            {
                offset = -radiusRobot * layer;
            }
            shrinkStartSide1 = new Vector2(startPos.x + radiusRobot * 2 * layer, startPos.y - offset / 2);
            shrinkEndSide1 = new Vector2(endPos.x + radiusRobot * 2 * layer, endPos.y + offset / 2);
            shrinkStartSide2 = new Vector2(startPos.x - radiusRobot * 2 * layer, startPos.y - offset / 2 );
            shrinkEndSide2 = new Vector2(endPos.x - radiusRobot * 2 * layer, endPos.y + offset / 2 );
        }

        // find the robots

        List<RobotSingular> robotShrink = FindRobotInlineInclude(shrinkStartSide1, shrinkEndSide1);
        if (robotShrink.Count < 2)
        {
            return robots;
        }


        RobotSingular robot1 = robotShrink[0]; // first robot
        RobotSingular robot2 = robotShrink[robotShrink.Count - 1];    //last robot

        robotShrink = FindRobotInlineInclude(shrinkStartSide2, shrinkEndSide2);
        if (robotShrink.Count < 2)
        {
            return robots;
        }
        RobotSingular robot3 = robotShrink[0]; // first robot
        RobotSingular robot4 = robotShrink[robotShrink.Count - 1];    //last robot

        // add to the list
        robots.Add(robot1);
        robots.Add(robot2);
        robots.Add(robot3);
        robots.Add(robot4);
        return robots;
    }

    public List<Vector2> FindParallelRobots(Vector3 start, Vector3 end, int layer, float radius)
    {
        List<Vector2> robots = new List<Vector2>();
        Vector3 startPos = start;
        Vector3 endPos = end;
        Vector3 directionToMove = endPos - startPos;
        Vector3 directionToMoveOpposite = startPos - endPos;
        float radiusRobot = radius;
        float offset;
        Vector2 shrinkStartSide1;
        Vector2 shrinkEndSide1;
        Vector2 shrinkStartSide2;
        Vector2 shrinkEndSide2;
        if (Mathf.Abs(startPos.x - endPos.x) >= Mathf.Abs(startPos.y - endPos.y))
        {

            if (startPos.x < endPos.x)
            {
                offset = radiusRobot * layer;
            }
            else
            {
                offset = -radiusRobot * layer;
            }
            shrinkStartSide1 = new Vector2(startPos.x - offset, startPos.y + radiusRobot * 2 * layer);
            shrinkEndSide1 = new Vector2(endPos.x + offset, endPos.y + radiusRobot * 2 * layer);
            shrinkStartSide2 = new Vector2(startPos.x - offset, startPos.y - radiusRobot * 2 * layer);
            shrinkEndSide2 = new Vector2(endPos.x + offset, endPos.y - radiusRobot * 2 * layer);
        }
        else
        {
            if (startPos.y < endPos.y)
            {
                offset = radiusRobot * layer;
            }
            else
            {
                offset = -radiusRobot * layer;
            }
            shrinkStartSide1 = new Vector2(startPos.x + radiusRobot * 2 * layer, startPos.y - offset);
            shrinkEndSide1 = new Vector2(endPos.x + radiusRobot * 2 * layer, endPos.y + offset);
            shrinkStartSide2 = new Vector2(startPos.x - radiusRobot * 2 * layer, startPos.y - offset);
            shrinkEndSide2 = new Vector2(endPos.x - radiusRobot * 2 * layer, endPos.y + offset);
        }

        // add to the list
        robots.Add(shrinkStartSide1);
        robots.Add(shrinkEndSide1);
        robots.Add(shrinkStartSide2);
        robots.Add(shrinkEndSide2);
        return robots;
    }

    public void RemoveRobot(string robot_name)
    {
        generator.RemoveRobot(robot_name);
    }

    public void RemoveRobot(RobotSingular robot)
    {
        generator.RemoveRobot(robot);
    }

    public void AddRobot(string robot_name, Vector2 pos)
    {
        generator.AddRobot(robot_name, pos);
    }

    public void AddRobot(Vector2 pos)
    {
        string robot_name = "Robot" + (generator.GetAllRobots().Count + 1);
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
            SendCommand(robot, State.idle, parameters.delayIntervalParameterExpand * parameters.delayIntervalParameterExpandIdleMultiplier);
            SendCommand(robot, State.shrinkToOriginal, 0f);
            robotCounter++;
        }
    }

    public void InchingForwardLastOneStill(List<RobotSingular> robots)
    {
        int robotCount = robots.Count;
        int robotCounter = 0;
        foreach (RobotSingular robot in robots)
        {
            if ( robotCounter != robots.Count - 1)
            {
                SendCommand(robot, State.expand, robotCounter * parameters.delayIntervalParameterExpand);
                SendCommand(robot, State.idle, parameters.delayIntervalParameterExpand * parameters.delayIntervalParameterExpandIdleMultiplier);
                SendCommand(robot, State.shrinkToOriginal, 0f);
            }
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
            SendCommand(robot, State.idle, parameters.delayIntervalParameterShrink * parameters.delayIntervalParameterShrinkIdleMultiplier);
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

    public (RobotSingular, Vector3 pos) LeftClickOnRobotSpecial()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null)
            {
                RobotSingular robot = hit.collider.gameObject.GetComponent<RobotSingular>();
                return (robot, Input.mousePosition);
            }
            else
            {
                return (null, Input.mousePosition);
            }
        }
        return (null, Vector3.zero);
    }

    public RobotSingular FindRobotByPos(Vector2 pos) {
        RobotSingular robot;
        Collider2D collider = Physics2D.OverlapPoint(pos);
        if (collider != null)
        {
            robot = collider.gameObject.GetComponent<RobotSingular>();
            return robot;
        }
        return null;
    }

    public Vector2 GetInlineDecompose(Vector2 startPos, Vector2 endPos) // the order is start, end
    {
        Vector2 inline = Vector2.zero;
        // get the robots at both positions
        RobotSingular robotStart = FindRobotByPos(startPos);
        RobotSingular robotEnd = FindRobotByPos(endPos);

        // get all the neights of start
        List<RobotSingular> startRobotNeighbors = GetNeigthbors(robotStart);
        List<RobotSingular> endRobotNeighbors = GetNeigthbors(robotEnd);

        Vector2 robotIntersection = Utils.GetRobotIntersection(startRobotNeighbors, endRobotNeighbors, startPos, endPos);

        inline = robotIntersection;
        
        return inline;
    }
    public RobotSingular FindRobotByNumber(int number)
    {
        string robotName = "Robot" + number;
        RobotSingular robot = generator.GetRobot(robotName);
        if (robot == null)
        {
            Debug.Log(robotName + " not found");
            return null;
        }
        return robot;
    }

    public bool MotionCheck(List<RobotSingular> robots)
    {
        foreach(RobotSingular robot in robots)
        {
            if (robot.CheckInMotion())
            {
                return true;
            }
        }
        return false;
    }

    public bool MotionCheckAll() { 
        List<RobotSingular> robots = generator.GetAllRobots();
        return MotionCheck(robots);                 
    }

    public List<RobotSingular> GetAllRobots()
    {
        List<RobotSingular> robots = generator.GetAllRobots();
        return robots;
    }

    public void ClearAllRobots()
    {
        generator.ClearAllRobots();
    }
}
