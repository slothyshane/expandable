using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
public enum State { idle, expand, shrink, expandToOriginal, shrinkToOriginal, wait };

[System.Serializable]
public class RobotSingular : MonoBehaviour
{
    // constants
    [SerializeField]
    private Parameter parameters;
    private float growthRate;
    private float shrinkRate;
    private float adhesionStrength;
    private float maxAdhesionForce;
    private float adhesionRangeMax;
    private float maxRadius;
    private float minRadius; 

    private float stateChangeTimer = 0f;
    private float stateWaitTime = 0f;
    private State stateToBeChanged;
    private float shapeSimilarityFactor = 1f;
    public float growthTimestepCount; 
    public float shrinkTimestepCount;

    //force estimation
    private Vector2 previousVelocity = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;

    // variables
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private float colliderRadiusInit;
    public LayerMask adhesionLayer;
    private Rigidbody2D Rigidbody2D;
    private State state = State.idle;
    private State prevState = State.idle;
    private float radiusDiff;

    private Vector3 prevPos;
    private Vector3 prevScale;
    private float stopThreshold = 0.01f;
    private Queue<RobotState> stateQueue = new Queue<RobotState>();
    public TextMeshPro textComponent;
    private Vector2 totalForce = Vector2.zero;
    public int robotNum;


    private bool activateDisplay = false;
    private bool loadFromFile = false;


    void Awake()
    {
        if (loadFromFile == false)
        {
            growthRate = parameters.growthRate;
            shrinkRate = parameters.shirnkRate;
            maxRadius = parameters.maxRadius;
            minRadius = parameters.minRadius;
            adhesionStrength = parameters.adhesionStrength;
            maxAdhesionForce = parameters.maxAdhesionForce;
            adhesionRangeMax = parameters.adhesionRangeMax;
            prevPos = transform.position;
            prevScale = transform.localScale;
        }
    }

    void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliderRadiusInit = circleCollider.radius;
        growthTimestepCount = 0.2f  / (growthRate * Time.fixedDeltaTime);
        shrinkTimestepCount = 0.2f / (shrinkRate * Time.fixedDeltaTime);
        radiusDiff = (maxRadius - minRadius) * colliderRadiusInit;
        Rigidbody2D = GetComponent<Rigidbody2D>();
        robotNum = Int32.Parse(gameObject.name.Substring(5));
    }

    // Update is called once per frame
    void Update()
    {
        if (state != prevState)
        {
            //Debug.Log("state: " + state);
            prevState = state;
        }
        int queue_count = stateQueue.Count;
        if (state == State.expand)
        {
            Expand();
        }
        else if (state == State.shrink)
        {
            Shrink();
        }
        else if (state == State.expandToOriginal)
        {
            ExpandToOriginal();
        }
        else if (state == State.shrinkToOriginal)
        {
            ShrinkToOriginal();
        }
        else if (state == State.idle)
        {
            RobotState next_state;
            if (stateQueue.Count > 0)
            {
                next_state = stateQueue.Dequeue();
                if (next_state.delay != 0f)
                {
                    state = State.wait;
                    stateToBeChanged = next_state.nextState;
                    stateWaitTime = next_state.delay;
                    
                    if (next_state.maxRadiusCommand != 0)
                    {
                        maxRadius = next_state.maxRadiusCommand;
                    }
                    else
                    {
                        maxRadius = parameters.maxRadius;
                    }
                    if (next_state.minRadiusCommand != 0)
                    {
                        minRadius = next_state.minRadiusCommand;
                    }
                    else
                    {
                        minRadius = parameters.minRadius;
                    }
                    if (next_state.growthRateCommand != 0)
                    {
                        growthRate = next_state.growthRateCommand;
                    }
                    else
                    {
                        growthRate = parameters.growthRate;
                    }

                    if (next_state.shrinkRateCommand != 0)
                    {
                        shrinkRate = next_state.shrinkRateCommand;
                    }
                    else
                    {
                        shrinkRate = parameters.shirnkRate;
                    }

                    stateChangeTimer = 0f;
                }
                else
                {
                    state = next_state.nextState;
                }
            }
        }
        else if (state == State.wait)
        {
            stateChangeTimer += Time.deltaTime;
            //Debug.Log("stateChangeTimer: "s + stateChangeTimer);
            if (stateChangeTimer >= stateWaitTime)
            {
                state = stateToBeChanged;
            }
        }

        if (spriteRenderer.color != Color.white && stateQueue.Count == 0 && queue_count != 0)
        {

           ResetColor();
           textComponent.text = robotNum.ToString();
        }

    }

    void FixedUpdate()
    {

        //update at a specific rate
        CalculateForce();
    }

    // PUBLIC FUNCTIONS
    public void AddState(State newState, float delay = 0f)
    {
        RobotState newRobotState = new RobotState(newState, delay);
        stateQueue.Enqueue(newRobotState);
    }

    public void ResetRobot() {
        // clear the queue 
        stateQueue.Clear();
        if (transform.localScale.x > 1)
        {
            state = State.shrinkToOriginal;
        }
        else if (transform.localScale.x < 1)
        {
            state = State.expandToOriginal;
        }
        else
        {
            state = State.idle;
        }
        ResetColor();
    }

    public State GetCurrentState()
    {
        return state;
    }

    public State GetNextState()
    {
        if (stateQueue.Count > 0)
        {
            return stateQueue.Peek().nextState;
        }
        return State.idle;
    }

    public State GetPreviousState()
    {
        return prevState;
    }

    public int GetQueueSize()
    {
        return stateQueue.Count;
    }

    public float GetForce()
    {
        return totalForce.magnitude;
    }

    public float GetRadius()
    {
        return transform.localScale.x;
    }

    public float GetVelocity()
    {
        return Rigidbody2D.velocity.magnitude;
    }

    public void ChangeColor(string color, float shade = 1f)
    {
        // find the color
        if (color != null)
        {
            Color baseColor = color.ToLower() switch
            {
                "red" => Color.red,
                "blue" => Color.blue,
                "green" => Color.green,
                "yellow" => Color.yellow,
                "black" => Color.black,
                "white" => Color.white,
                _ => Color.white
            };

            // Adjust the shade
            Color newColor = baseColor * shade;
            spriteRenderer.color = newColor;
        }
    }
    public void ResetColor()
    {
        // change the color to white
        spriteRenderer.color = Color.white;
    }

    // Private Functions
    private void Expand()
    {
        float growth = growthRate * Time.fixedDeltaTime;
        //Debug.Log("scale: " + transform.localScale.x);
        if (transform.localScale.x < maxRadius)
        {
            transform.localScale += new Vector3(growth, growth, 0);
        }
        else {
            state = State.idle;
        }
    }

    private void ExpandToOriginal()
    {
        float growth = growthRate * Time.fixedDeltaTime;
        //Debug.Log("scale: " + transform.localScale.x);
        if (transform.localScale.x < 1)
        {
            transform.localScale += new Vector3(growth, growth, 0);
            //update_collider_radius();
            if (transform.localScale.x > 1f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else {
            state = State.idle;
        }
    }

    private void Shrink()
    {
        float growth = shrinkRate * Time.fixedDeltaTime;
        if (transform.localScale.x > minRadius)
        {
            transform.localScale -= new Vector3(growth, growth, 0);
        }
        else
        {
            state = State.idle;
        }
    }

    private void ShrinkToOriginal()
    {
        float growth = shrinkRate * Time.fixedDeltaTime;
        if (transform.localScale.x > 1)
        {
            transform.localScale -= new Vector3(growth, growth, 0);
            //update_collider_radius();

            if (transform.localScale.x < 1f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            state = State.idle;
        }
    }

    private void ResetShape()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    private void ReturnToShape() { 
        if (transform.localScale.x > 1)
        {
           ExpandToOriginal();
        }
        else if (transform.localScale.x < 1)
        {
            ShrinkToOriginal();
        }
        else
        {
            ResetShape();
        }
    }

    public bool CheckInMotion()
    {
        // Current values
        Vector3 currentPosition = transform.position;
        Vector3 currentScale = transform.localScale;

        // Check position difference
        float posDiff = (currentPosition - prevPos).sqrMagnitude;

        // Check scale difference
        float scaleDiff = (currentScale - prevScale).sqrMagnitude;

        prevPos = currentPosition;
        prevScale = currentScale;

        if (posDiff < stopThreshold && scaleDiff < stopThreshold && stateQueue.Count == 0)
        {
            return false;
        }

        return true;
    }

    private void CalculateForce() {
       
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider.radius * transform.lossyScale.x * adhesionRangeMax, adhesionLayer); // get all the possible interactions
        totalForce = Vector2.zero;
        foreach (Collider2D collider in colliders)
        {

            if (collider.gameObject != gameObject)
            {

                // check for adhesion layer
                if (!Utils.IsInLayerMask(collider.gameObject, adhesionLayer))
                {
                    continue;
                }

                Rigidbody2D rb = collider.gameObject.GetComponent<Rigidbody2D>(); // get the rigidbody of the other object
                CircleCollider2D col = collider.gameObject.GetComponent<CircleCollider2D>();
                float ColColRadius = col.radius;

                float colliderWorldRadius = circleCollider.radius * transform.lossyScale.x;
                float colColliderWorldRadius = col.radius * col.transform.lossyScale.x;

                Vector2 direction = (rb.position - Rigidbody2D.position);
                float dist = direction.magnitude - colliderWorldRadius - colColliderWorldRadius;

                //clip the distance to min 0.01
 
                dist = Mathf.Clamp(dist, 0.01f, dist);
                if (dist > adhesionRangeMax * colliderWorldRadius) // skip if the distance is greater than the adhesion range
                {
                    if (gameObject.name == "Robot25")
                    {
                        Debug.Log("skipping");
                    }
                    continue;
                }

                float forceMag = adhesionStrength / (dist);

                float colRadiusNorm = colColliderWorldRadius; 
                float radiusNorm = colliderWorldRadius;
                // the similarity score is calculated based on the difference in the radius
                if (Mathf.Abs(colRadiusNorm - radiusNorm) > 0.9 * radiusDiff)
                {
                    shapeSimilarityFactor = 0.2f;
                }
                else
                    shapeSimilarityFactor = 1 - Mathf.Clamp((Mathf.Abs(colRadiusNorm - radiusNorm) * (1 / radiusDiff)), 0.0f, 0.8f);
 
                //forceMag = Mathf.Clamp(forceMag, 0, maxAdhesionForce);
                forceMag = forceMag * shapeSimilarityFactor;
                Vector2 force = direction.normalized * forceMag;
                //clear the forces 

                Rigidbody2D.AddForce(force, ForceMode2D.Force);

                totalForce += force;

                // UPDATE the force

                //int totalForceInt = (int)totalForce.magnitude;
                //textComponent.text = totalForceInt.ToString();

                // TESTING: get the name of the object
                //if (gameObject.name == "Robot25")
                //{
                //    Debug.Log("colliding with robot: " + collider.gameObject.name + " self-Radius:" + colliderWorldRadius + " force:" + force + " similarity:" + shapeSimilarityFactor + " dist:" + dist + " force_mag:" + forceMag + " colliderRadius:" + colColliderWorldRadius);
                //}

            }
        }

        // simulate friction
        if (Rigidbody2D.velocity.magnitude > 0.01)
        {
            //reduce the velocity
            Rigidbody2D.velocity = Rigidbody2D.velocity * 0.8f;
        }
        else if (Rigidbody2D.velocity.magnitude <= 0.01)
        {
            //set the velocity to zero
            Rigidbody2D.velocity = Vector2.zero;

        }
        previousVelocity = currentVelocity;
    }

    public RobotSingularData GetData() {
        RobotSingularData robotData = new RobotSingularData();
        robotData.growthRate = growthRate;
        robotData.shrinkRate = shrinkRate;
        robotData.adhesionStrength = adhesionStrength;
        robotData.maxAdhesionForce = maxAdhesionForce;
        robotData.adhesionRangeMax = adhesionRangeMax;
        robotData.maxRadius = maxRadius;
        robotData.minRadius = minRadius;
        robotData.position = transform.position;
        robotData.robotNum = robotNum;
        return robotData;
    }

    public void SetData(RobotSingularData robotData)
    {
        growthRate = robotData.growthRate;
        shrinkRate = robotData.shrinkRate;
        adhesionStrength = robotData.adhesionStrength;
        maxAdhesionForce = robotData.maxAdhesionForce;
        adhesionRangeMax = robotData.adhesionRangeMax;
        maxRadius = robotData.maxRadius;
        minRadius = robotData.minRadius;
    }
}

public struct RobotState {
    public State nextState;
    public float delay;
    public float maxRadiusCommand;
    public float minRadiusCommand;
    public float growthRateCommand;
    public float shrinkRateCommand;

    public RobotState(State state, float delay = 0f, float maxRadius = 0, float minRadius = 0, float growthRate = 0, float shrinkRate = 0)
    {
        nextState = state;
        this.delay = delay;
        maxRadiusCommand = maxRadius;
        minRadiusCommand = minRadius;
        growthRateCommand = growthRate;
        shrinkRateCommand = shrinkRate;
    }
}