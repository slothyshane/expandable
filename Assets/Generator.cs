using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GameObject circlePrefab;
    [SerializeField]
    private Parameter parameters;
    private int rows;            // Number of rows
    private int columns;         // Number of columns
    private float radius;
    private Vector2 startPosition = new Vector2(-8, -4);

    // create a dictionary to store all the robots in scene
    private Dictionary<string, GameObject> robots = new Dictionary<string, GameObject>();
    private void Awake()
    {
        rows = parameters.rows;
        columns = parameters.columns;
    }
    void Start()
    {
        radius = circlePrefab.GetComponent<CircleCollider2D>().radius;
        GenerateCircleGrid();
        RemoveDefinedRobots();
    }

    void GenerateCircleGrid()
    {
        float xOffset = 2f * radius;
        float yOffset = radius * Mathf.Sqrt(3f);     // Vertical distance between rows
        int robotNumber = 1; // Initialize the robot counter

        for (int row = 0; row < rows; row++)
        {
            // Determine if the row should be offset
            bool isRowOffset = row % 2 == 1;

            for (int col = 0; col < columns; col++)
            {
                // Calculate the position of the circle
                float xPos = startPosition.x + col * xOffset + (isRowOffset ? radius : 0);
                float yPos = startPosition.y + row * yOffset;

                // Instantiate the circle prefab at the calculated position
                Vector2 position = new Vector2(xPos, yPos);

                GameObject newrobot = Instantiate(circlePrefab, position, Quaternion.identity, transform);
            
                newrobot.name = "Robot" + robotNumber;
                robots.Add(newrobot.name, newrobot);
                TextMeshPro textComponent = newrobot.GetComponentInChildren<TextMeshPro>();
                if (textComponent != null)
                    textComponent.text = robotNumber.ToString();
                robotNumber++; 
            }
        }
    }

    public RobotSingular GetRobot(string robotName)
    {
        if (robots.ContainsKey(robotName))
        {
            RobotSingular robot = robots[robotName].GetComponent<RobotSingular>();
            return robot;
        }
        return null;
    }

    public void AddRobot(string robotName, Vector2 pos)
    {
        int robotNum = 0;
        if (robots.ContainsKey(robotName))
        {
            Debug.LogWarning("Robot with name " + robotName + " already exists");
            return;
        }
        GameObject robot = Instantiate(circlePrefab, pos, Quaternion.identity, transform);
        robot.name = robotName;
        TextMeshPro textComponent = robot.GetComponentInChildren<TextMeshPro>();
        if (textComponent != null)
            //parse the int from the robotName
            robotNum = int.Parse(robotName.Substring(5));
            textComponent.text = robotNum.ToString();
        Debug.Log("Adding robot " + robotName + " at position " + pos); 
        robots.Add(robotName, robot);
    }

    public List<RobotSingular> GetAllRobots()
    {
        List<RobotSingular> robotList = new List<RobotSingular>();
        foreach (GameObject robot in robots.Values)
        {
            robotList.Add(robot.GetComponent<RobotSingular>());
        }
        return robotList;
    }

    public void ClearAllRobots()
    {
        foreach (GameObject robot in robots.Values)
        {
            Destroy(robot);
        }
        robots.Clear();
    }

    public void RemoveRobot(string robotName)
    {
        if (robots.ContainsKey(robotName))
        {
            Destroy(robots[robotName]);
            robots.Remove(robotName);
        }
        else
        {
            Debug.LogWarning("Robot with name " + robotName + " does not exist");
        }
        RenameRobots();
    }

    public void RemoveRobot(RobotSingular robot)
    {
        string robotName = robot.name;
        if (robots.ContainsKey(robotName))
        {
            Destroy(robots[robotName]);
            robots.Remove(robotName);
        }
        else
        {
            Debug.LogWarning("Robot with name " + robotName + " does not exist");
        }
        RenameRobots();
    }

    void RemoveDefinedRobots()
    {
        foreach (float robotNumber in parameters.robotsToBeRemoved)
        {
            string robotName = "Robot" + robotNumber;
            RemoveRobot(robotName);
        }
    }

    // if a robot has been removed, then rename all the robots to keep the order
    void RenameRobots() {
        int robotNumber = 1;
        // Create a new dictionary to store the renamed robots
        Dictionary<string, GameObject> renamedRobots = new Dictionary<string, GameObject>();
        // Iterate through the existing robots and rename them
        foreach (KeyValuePair<string, GameObject> entry in robots)
        {
            GameObject robot = entry.Value;
            string oldName = entry.Key;
            string newName = "Robot" + robotNumber;
            // Rename the robot
            robot.name = newName;
            RobotSingular robotSingular = robot.GetComponent<RobotSingular>();
            if (robotSingular != null)
            {
                robotSingular.robotNum = robotNumber;
            }
            // change the text in the robot
            TextMeshPro textComponent = robot.GetComponentInChildren<TextMeshPro>();

            if (textComponent != null)
                textComponent.text = robotNumber.ToString();
            renamedRobots.Add(newName, robot);
            robotNumber++;
        }
        // Clear the old dictionary and assign the renamed robots
        robots.Clear();
        foreach (KeyValuePair<string, GameObject> entry in renamedRobots)
        {
            robots.Add(entry.Key, entry.Value);
        }
    }

}
