using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// PlayGround
/// Press E to enable expansion mode
/// Press C to enable contraction mode
/// Use number 1 - 6 to indicate the simultaneous expansion and contraction of the robots
/// Left click to select the robot 
/// Once selected, press enter to start the algorithm
/// </summary>
//public enum UIState { Prep, expand, shrink};

public class AlgorithmPlayGround : MonoBehaviour
{
    [SerializeField]
    private Parameter parmeters;
    public RobotManager RobotManager;
    public TextMeshProUGUI textComponentLeft;
    public TextMeshProUGUI textComponentRight;
    public ProgressManager ProgressManager;
    public Button saveSceneButton;
    public Button loadSceneButton;
    public Button saveConfigButton;
    public Button loadConfigButton;
    Camera camera;

    private bool pathFound = false;
    RobotSingular selectRobot;
    Vector3 mousePos = Vector3.zero;
    private List<List<RobotSingular>> robotToExpand;
    private List<List<RobotSingular>> robotToShrink;
    private string mode = "Prep";
    private int keyboardNum = 1;
    private bool begin = false;
    private bool motion = false;

    private bool saveConfig = false;
    private bool loadConfig = false;



    private void Awake()
    {
        camera = Camera.main;

    }
    void Start()
    {
        Reset();
        saveConfigButton.onClick.AddListener(OnSaveConfigButtonClick);
        loadConfigButton.onClick.AddListener(OnLoadConfigButtonClick);
        saveSceneButton.onClick.AddListener(OnSaveSceneButtonClick);
        loadSceneButton.onClick.AddListener(OnLoadSceneButtonClick);
    }

    void OnSaveConfigButtonClick()
    {
        SaveProgress();
        saveConfig = true;
        textComponentRight.text = "Config Saved";
    }

    void OnLoadConfigButtonClick()
    {
        LoadProgress();
        loadConfig = true;
        textComponentRight.text = "Config Loaded";
    }

    void OnSaveSceneButtonClick() { 
        if (mode == "Prep")
        {
            SaveScene();
            textComponentRight.text = "Scene Saved";
        }
        else
        {
            textComponentRight.text = "Cannot save scene in current mode";
        }

    }

    void OnLoadSceneButtonClick()
    {
        if (mode == "Prep")
        {
            LoadScene();
            textComponentRight.text = "Scene Loaded";
        }
        else
        {
            textComponentRight.text = "Cannot load scene in current mode";
        }

    }

    void FixedUpdate()  
    {

    }

    private void Update()
    {
        KeyStrokeDetection();
        (selectRobot, mousePos) = RobotManager.LeftClickOnRobotSpecial();

        if (selectRobot != null && mode == "Expand")
        {
            selectRobot.ChangeColor("red", 0.3f + 0.1f * keyboardNum);
            robotToExpand[keyboardNum - 1].Add(selectRobot);
            selectRobot.textComponent.text = robotToExpand[keyboardNum - 1].Count.ToString();
        }
        else if (selectRobot != null && mode == "Contract")
        {
            selectRobot.ChangeColor("green", 0.3f + 0.1f * keyboardNum);
            robotToShrink[keyboardNum - 1].Add(selectRobot);
            selectRobot.textComponent.text = robotToShrink[keyboardNum - 1].Count.ToString();
        }
        else if (selectRobot != null && mode == "Prep")
        {
            // remove that robot from the scene
            RobotManager.RemoveRobot(selectRobot);
        }
        else if (selectRobot == null && mode == "Prep" && mousePos != Vector3.zero)
        {
            // add robot to the scene

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
                keyboardNum = 1;
            }

            else if (Input.GetKeyDown(KeyCode.C))
            {
                keyboardNum = 1;
                mode = "Contract";
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                saveConfig = true;
                
            }
            //else if (Input.GetKeyDown(KeyCode.L))
            //{
            //    loadConfig = true;
            //    LoadProgress();
            //}

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
        textComponentLeft.text = mode + " Mode: queue: " + keyboardNum;

    }

    public void MotionCheck() {
        if (begin == true) {
            if (RobotManager.MotionCheckAll() != true) {
                motion = false;
                mode = "Prep";
                begin = false;
                saveConfig = false;
                loadConfig = false;
                textComponentRight.text = "Press E to expand, C to contract, S to save, L to load";
            }
        }

    }
    public void Reset()
    {
        motion = false;
        mode = "Prep";
        begin = false;
        saveConfig = false;
        loadConfig = false;
        // clear the list

        textComponentRight.text = "Press E to expand, C to contract, S to save, L to load";
        textComponentLeft.text = mode + " Mode: queue: " + keyboardNum;

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

    public void SaveProgress()
    {
        ProgressData dataToSave = new ProgressData();
        dataToSave.robotToExpand = new List<Wrapper>();
        dataToSave.robotToShrink = new List<Wrapper>();

        // loop through the list and save the data
        for (int i = 0; i < robotToExpand.Count; i++)
        {
            Wrapper wrapper = new Wrapper();
            wrapper.robots = new List<RobotSingularData>();
            foreach (RobotSingular robot in robotToExpand[i])
            {
                RobotSingularData robotData = robot.GetData();
                wrapper.robots.Add(robotData);   

            }
            dataToSave.robotToExpand.Add(wrapper);
        }

        for (int i = 0; i < robotToShrink.Count; i++)
        {
            Wrapper wrapper = new Wrapper();
            wrapper.robots = new List<RobotSingularData>();
            foreach (RobotSingular robot in robotToShrink[i])
            {
                RobotSingularData robotData = robot.GetData();
                wrapper.robots.Add(robotData);
            }
            dataToSave.robotToShrink.Add(wrapper);
        }

        ProgressManager.SaveProgress(dataToSave);
        textComponentRight.text = "Progress Saved";

    }

    public void LoadProgress()
    {
        // reset everything
        Reset();
        ProgressData loadedData = ProgressManager.LoadProgress();
        if (loadedData == null)
        {
            textComponentRight.text = "Error Loading Progress";
            return;
        }
        List<Wrapper> robotToExpandData = loadedData.robotToExpand;
        List<Wrapper> robotToShrinkData = loadedData.robotToShrink;

        textComponentRight.text = "Progress Loaded";

        //loop through the list and update the text and color component
        for (int i = 0; i < robotToExpandData.Count; i++)
        {
            foreach (RobotSingularData robotData in robotToExpandData[i].robots)
            {
                RobotSingular robot = RobotManager.FindRobotByNumber(robotData.robotNum);
                if (robot != null)
                {
                    robot.SetData(robotData);
                    robot.ChangeColor("red", 0.3f + 0.1f * (i + 1));
                    robotToExpand[i].Add(robot);
                    robot.textComponent.text = robotToExpand[i].Count.ToString();
                }
            }
        }

        for (int i = 0; i < robotToShrinkData.Count; i++)
        {
            foreach (RobotSingularData robotData in robotToShrinkData[i].robots)
            {
                RobotSingular robot = RobotManager.FindRobotByNumber(robotData.robotNum);
                if (robot != null)
                {
                    robot.SetData(robotData);
                    robot.ChangeColor("green", 0.3f + 0.1f * (i + 1));
                    robotToShrink[i].Add(robot);
                    robot.textComponent.text = robotToShrink[i].Count.ToString();   
                }
            }
        }
    }

    public void SaveScene()
    {
        SceneData dataToSave = new SceneData();
        dataToSave.robotLocations = new List<RobotLocationData>();
        // loop through the list and save the data
        List<RobotSingular> allRobots = RobotManager.GetAllRobots();
        foreach (RobotSingular robot in allRobots)
        {
            RobotLocationData robotData = new RobotLocationData();
            robotData.robotNum = robot.robotNum;
            robotData.position = robot.transform.position;
            dataToSave.robotLocations.Add(robotData);
        }
        ProgressManager.SaveScene(dataToSave);
    }

    public void LoadScene()
    {
        SceneData dataToLoad = new SceneData();
        // clear all the robots first
        RobotManager.ClearAllRobots();
        // load the data
        dataToLoad = ProgressManager.LoadScene();
        if (dataToLoad == null)
        {
            textComponentRight.text = "Error Loading Scene";
            return;
        }
        // loop through the list and update the text and color component
        foreach (RobotLocationData robotData in dataToLoad.robotLocations)
        {
            // create a new robot
            RobotManager.AddRobot("Robot" + robotData.robotNum, robotData.position);
        }
    }

}

