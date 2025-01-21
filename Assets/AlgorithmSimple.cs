using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class AlgorithmSimple : MonoBehaviour
{
    [SerializeField]
    private Parameter parmeters;
    public RobotManager RobotManager;
    Camera camera;
    List<GameObject> robot_to_expand = new List<GameObject>();
    Stack<RobotSingular> RobotSingulars_expand = new Stack<RobotSingular>();

    List<GameObject> robot_to_shrink = new List<GameObject>();
    Stack<RobotSingular> RobotSingulars_shrink = new Stack<RobotSingular>();

    private int robot_expand_count = 0;
    private int robot_shrink_count = 0;
    private GameObject robot_obj_test;
    private RobotSingular robot_script_test;
    private float delay_interval_parameter_expand;
    private float delay_interval_parameter_shrink;
    private int? start;
    private int? end;
    private int rows_to_shrink;
    private bool pathFound = false;
    private bool shrinkPathFound = false;
    private float radius_robot;
    private Stack<int> robot_shrink_total_number = new Stack<int>();


    private void Awake()
    {
        rows_to_shrink = parmeters.rowsToShrink;
        camera = Camera.main;
    }
    void Start()
    {
        robot_obj_test = GameObject.Find("Robot1");
        radius_robot = robot_obj_test.GetComponent<CircleCollider2D>().radius;
    }

    void FixedUpdate()  
    {
        //robot_script_test.debug();
        if (pathFound == true && RobotSingulars_expand.Count == 0 && RobotSingulars_shrink.Count == 0)
        {
            Reset();
        }
        if (RobotSingulars_expand.Count != 0)
        {
            RobotSingular robot_expand = RobotSingulars_expand.Pop();
            
            robot_expand.AddState(State.expand, delay_interval_parameter_expand * robot_expand_count);
            robot_expand.AddState(State.idle, delay_interval_parameter_expand);
            robot_expand.AddState(State.shrinkToOriginal);
            
            robot_expand_count++;
        }
        else { robot_expand_count = 0; }

        if (RobotSingulars_shrink.Count != 0)
        {
 
            int number = robot_shrink_total_number.Peek();
            RobotSingular robot_shrink = RobotSingulars_shrink.Pop();
            robot_shrink.AddState(State.shrink, delay_interval_parameter_shrink * robot_shrink_count);
            robot_shrink.AddState(State.idle, 2.9f * delay_interval_parameter_shrink);
            robot_shrink.AddState(State.expandToOriginal);
            robot_shrink_count++;

            if (robot_shrink_count >= number)
            {
                robot_shrink_count = 0;
                robot_shrink_total_number.Pop();
            }
         
        }
        else { robot_shrink_count = 0; }



    }

    private void Update()
    {

        RobotSingular start = RobotManager.LeftClickOnRobot();
        if (start != null)
        {
            start.ChangeColor("red");
        }

        RobotSingular end = RobotManager.RightClickOnRobot();
        if (end != null)
        {
           end.ChangeColor("green");
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
                List<RobotSingular> robots = RobotManager.FindRobotInlineInclude(start, end);
                RobotManager.InchingForward(robots);

            }
            else
            {
                //List<Vector3> disjunct_motion = get_inline_transform(startTransform.position, endTransform.position);

                //for (int i = 0; i < disjunct_motion.Count; i += 2)
                //    {
                //        Vector3 startPos_new = disjunct_motion[i];
                //        Vector3 endPos_new = disjunct_motion[i + 1];
                //        inlineMotionGenerate(startPos_new, endPos_new);
                //    }
            }
        }
    }

    // return a list of transform
    private List<Vector3> get_inline_transform(Vector3 startPos, Vector3 endPos) // the order is start, end
    {
        List<Vector3> transforms = new List<Vector3>();
        Vector3 start_pos_first = startPos;
        Vector3 start_pos_second = startPos;
        Vector3 end_pos_first = endPos;
        Vector3 end_pos_second = endPos;
        //create a copy of startTransform
        
        if (start_pos_first.x < end_pos_second.x) 
        {
            // move x by a robot radius
            while (Utils.CheckInline(start_pos_second, end_pos_second) == false)
            {
                start_pos_second.x += radius_robot * 2;
            }
            end_pos_first = start_pos_second;


        }

        transforms.Add(start_pos_first);
        transforms.Add(end_pos_first);
        transforms.Add(start_pos_second);
        transforms.Add(end_pos_second);

        return transforms;
    }


    private void inlineMotionGenerate(Vector3 startPos, Vector3 endPos)

    {
        Vector3 direction_to_move = endPos - startPos;

        // the opposite direction
        Vector3 direction_to_move_opposite = startPos - endPos;

        //move by a robot radius
        direction_to_move_opposite = direction_to_move_opposite.normalized * radius_robot * 2;

        // find all the robots on the path, this is the expansion list
        RaycastHit2D[] hits = Physics2D.LinecastAll(startPos, endPos);
        RaycastHit2D[] hits_additional = Physics2D.LinecastAll(direction_to_move_opposite + startPos, endPos);

        int number_of_robots = hits.Length;
        Debug.Log("number of robots: " + number_of_robots);
        bool first_added = false;

        if (hits_additional.Length > 0)
        {
            pathFound = true;
            foreach (RaycastHit2D hit in hits_additional)
            {
                if (hit.collider != null)
                {
                    robot_to_expand.Add(hit.collider.gameObject);
                    SpriteRenderer spriteRenderer = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && first_added == true)
                    {
                        spriteRenderer.color = Color.gray;
                    }
                    else if (spriteRenderer != null && first_added == false)
                    {
                        spriteRenderer.color = Color.blue;
                        first_added = true;
                    }

                    RobotSingulars_expand.Push(hit.collider.gameObject.GetComponent<RobotSingular>());
                }
            }
        }

        // duplicate the RobotSingulars_expand and add to itself
        var RobotSingulars_expand_array = RobotSingulars_expand.ToArray();
        // reverse the array
        System.Array.Reverse(RobotSingulars_expand_array);
        for (int i = 0; i < number_of_robots - 1; i++)
        {
            foreach (RobotSingular robot in RobotSingulars_expand_array)
            {
                RobotSingulars_expand.Push(robot);
            }
        }

        // find all the robots on the path, this is the shrinking list
        Vector2 shrink_start_1, shrink_end_1, shrink_start_2, shrink_end_2;
        if (rows_to_shrink != 0 && start != null && end != null && shrinkPathFound == false)
        {
            shrinkPathFound = true;
            for (int i = 1; i < rows_to_shrink + 1; i++)
            {
                // check the transform to decide which way to move
                if (Mathf.Abs(startPos.x - endPos.x) >= Mathf.Abs(startPos.y - endPos.y))
                {
                    float offset;
                    if (startPos.x < endPos.x)
                    {
                        offset = radius_robot * i;
                    }
                    else
                    {
                        offset = -radius_robot * i;
                    }
                    shrink_start_1 = new Vector2(startPos.x - offset, startPos.y + radius_robot * 2 * i);
                    shrink_end_1 = new Vector2(endPos.x + offset, endPos.y + radius_robot * 2 * i);
                    shrink_start_2 = new Vector2(startPos.x - offset, startPos.y - radius_robot * 2 * i);
                    shrink_end_2 = new Vector2(endPos.x + offset, endPos.y - radius_robot * 2 * i);
                }
                else
                {
                    float offset;
                    if (startPos.y < endPos.y)
                    {
                        offset = radius_robot * i;
                    }
                    else
                    {
                        offset = -radius_robot * i;
                    }
                    shrink_start_1 = new Vector2(startPos.x + radius_robot * 2 * i, startPos.y - offset);
                    shrink_end_1 = new Vector2(endPos.x + radius_robot * 2 * i, endPos.y + offset);
                    shrink_start_2 = new Vector2(startPos.x - radius_robot * 2 * i, startPos.y - offset);
                    shrink_end_2 = new Vector2(endPos.x - radius_robot * 2 * i, endPos.y + offset);
                }


                RaycastHit2D[] hits1 = Physics2D.LinecastAll(shrink_start_1, shrink_end_1);
                if (hits1.Length > 0)
                {
                    foreach (RaycastHit2D hit in hits1)
                    {
                        if (hit.collider != null)
                        {
                            robot_to_shrink.Add(hit.collider.gameObject);
                            //change the color
                            SpriteRenderer spriteRenderer = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                spriteRenderer.color = Color.blue;
                            }
                            RobotSingulars_shrink.Push(hit.collider.gameObject.GetComponent<RobotSingular>());
                        }
                    }
                    robot_shrink_total_number.Push(hits1.Length);
                }


                RaycastHit2D[] hits2 = Physics2D.LinecastAll(shrink_start_2, shrink_end_2);
                if (hits2.Length > 0)
                {
                    foreach (RaycastHit2D hit in hits2)
                    {
                        if (hit.collider != null)
                        {
                            robot_to_shrink.Add(hit.collider.gameObject);
                            SpriteRenderer spriteRenderer = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                spriteRenderer.color = Color.blue;
                            }
                            RobotSingulars_shrink.Push(hit.collider.gameObject.GetComponent<RobotSingular>());
                        }
                    }
                    robot_shrink_total_number.Push(hits2.Length);
                }
            }
            var RobotSingulars_shrink_array = RobotSingulars_shrink.ToArray();
            var total_number = robot_shrink_total_number.ToArray();
            // reverse the array
            System.Array.Reverse(RobotSingulars_shrink_array);
            System.Array.Reverse(total_number);
            for (int j = 0; j < number_of_robots - 1; j++)
            {
                foreach (RobotSingular robot in RobotSingulars_shrink_array)
                {
                    RobotSingulars_shrink.Push(robot);
                }

                foreach (int number in total_number)
                {
                    robot_shrink_total_number.Push(number);
                }

            }

        }
    }

    private void Reset()
    {
        robot_to_expand.Clear();
        RobotSingulars_expand.Clear();
        robot_to_shrink.Clear();
        RobotSingulars_shrink.Clear();
        robot_expand_count = 0;
        robot_shrink_count = 0;
        start = null;
        end = null;
        pathFound = false;
        shrinkPathFound = false;
        robot_shrink_total_number.Clear();
    }
}
