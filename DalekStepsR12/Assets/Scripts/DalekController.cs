using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DalekController : Agent
{
    // =======================================================================
    // Dalek vs Steps
    // Observations :
    //    Dalek Forward RayCasts (inc deting Tagged Objects) +
    //    (Local) Dalek Position Vector3  (-Arena Origin)
    //    (local) Ramp Position (-Arena Origin)
    //    Ramp Orientation (around Y axis) 
    //    (Target Tardis-Dalek) Vector3 postional difference   
    //
    // Discrete [] Action Space
    //    [0]  - Director Rotation Action Branch  [0 : None, 1: Turn Left, 2: Turn Right]  around Y Axis
    //    [1]  - Move Branch             [0: None, 1: Move Forward : Local Z Direcion] 

    // ==========================================================================
    public GameObject RampObject;
    public GameObject TargetTardisObject;
    public GameObject TheWallRefObject;

    float DalekRotationRate = 150.0f;
    float DalekMoveRate = 2.0f; 

    Rigidbody DalekRB;
    Rigidbody RampRB;

    int EpisodeStepCount; 

    // =======================================================================
    // Start is called before the first frame update
    public override void Initialize()
    {
        
        // Get the Daleks Own RigidBody
        DalekRB = GetComponent<Rigidbody>();
        DalekRB.centerOfMass = new Vector3(0.0f, -20.0f, 0.0f);    // Set very low [y] centre of Mass  
        RampRB = RampObject.GetComponent<Rigidbody>();
        RampRB.centerOfMass = new Vector3(0.0f, -10.0f, 25f); // A little forward

        OnEpisodeBegin();

    } // Initialize
      // =======================================================================

    public override void OnEpisodeBegin()
    {
        // Reset the Ramp Position
        float randomX = 3.0f + Random.Range(0.0f, 2.5f);
        float randomZ = Random.Range(-0.5f, 3.0f);  // Slightly to the Right  as Ramp will be facing right
        float randomRot = Random.Range(100.0f, 225.0f); // Limit to narrow right Facing Sector

        RampObject.transform.position = new Vector3(TheWallRefObject.transform.position.x + randomX, 1.0f, TheWallRefObject.transform.position.z + randomZ);
        RampObject.transform.rotation = Quaternion.Euler(0.0f, randomRot, 0.0f);
        RampRB.velocity = Vector3.zero;
        RampRB.angularVelocity = Vector3.zero;

        // Now Reset the Dalek Position
        randomX = 3.5f + Random.Range(0.0f, 2.0f);
        randomZ = Random.Range(-4.25f, 4.25f);
        randomRot = Random.Range(-180.0f, 180.0f);

        transform.position = new Vector3(TheWallRefObject.transform.position.x + randomX, 2.0f, TheWallRefObject.transform.position.z + randomZ);
        transform.rotation = Quaternion.Euler(0.0f, randomRot, 0.0f);
        DalekRB.velocity = Vector3.zero;
        DalekRB.angularVelocity = Vector3.zero;

        EpisodeStepCount = 0; 

    } // OnEpisodeBegin
    // =======================================================================
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observations :
        //    Dalek Forward RayCasts (inc deting Tagged Objects) +
        //    (Local) Dalek Position Vector3  (-Arena Origin)
        //    (local) Ramp Position (-Arena Origin)
        //    Ramp Orientation (around Y axis) 
        //    (Target Tardis-Dalek) Vector3 postional difference
        //    So a Total Vector Size of 10 floats (excl Raycast sensors) 

        Vector3 LocalDalekPosition = transform.position - TheWallRefObject.transform.position;
        sensor.AddObservation(LocalDalekPosition / 5.0f);

        Vector3 LocalRampPosition = RampObject.transform.position - TheWallRefObject.transform.position;
        sensor.AddObservation(LocalRampPosition / 5.0f);

        float RampHeading = RampObject.transform.rotation.eulerAngles.y;
        sensor.AddObservation(RampHeading / 180.0f);

        Vector3 TargetDirectionVector = TargetTardisObject.transform.position- transform.position;
        sensor.AddObservation(TargetDirectionVector / 5.0f);


    } //CollectObservations
    // =======================================================================
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
       
        // Discrete [] Action Space
        //    [0]  - Director Rotation Action Branch  [0 : None, 1: Turn Left, 2: Turn Right]  around Y Axis
        //    [1]  - Move Branch             [0: None, 1: Move Forward : Local Z Direcion] 
        // Very Samll Penalise All Move Acttions  (Smaller penalty if already on Higher Ground
        if (transform.position.y>0.75f) AddReward(-0.0001f);
        else AddReward(-0.00025f);

        // DiscreteActions[0] Roate Dalek:   0: None, 1: RotateLeft, 2:Rotate Right
        if (actionBuffers.DiscreteActions[0] == 1)  transform.Rotate(new Vector3(0.0f, DalekRotationRate * Time.deltaTime, 0.0f), Space.Self);  // Rotate Negative Action
        if (actionBuffers.DiscreteActions[0] == 2) transform.Rotate(new Vector3(0.0f, -DalekRotationRate * Time.deltaTime, 0.0f), Space.Self);     // Rotate Positive Action 

        // DiscreteActions[1] 0:,None, 1: Move Foward
        if (actionBuffers.DiscreteActions[1] == 1)
        {
            Vector3 DeltaMovementFwdMovement = Vector3.zero;
            DeltaMovementFwdMovement.z = 1.0f * DalekMoveRate * Time.deltaTime;
            transform.position = transform.position + transform.TransformDirection(DeltaMovementFwdMovement);
        }  // DiscreteActions[1]  Move Forward

        // Perform Win / Lose Checks
        if (transform.position.y < -2.0f) GameLost();
        if (RampObject.transform.position.y < -2.0f) GameLost();

        float DistanceToTardis = Vector3.Distance(transform.position, TargetTardisObject.transform.position);
        if ((transform.position.y > 1.0f) && (DistanceToTardis < 1.5f)) GameWon();

        EpisodeStepCount++;
        if (EpisodeStepCount > 6000) GameOverun(); 

    } // OnActionReceived
    // =======================================================================
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;

        if (Input.GetKey(KeyCode.RightArrow)) discreteActionsOut[0] = 1;    // Rotate Negative Action
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActionsOut[0] = 2;     // Rotate Positive Action 
        if (Input.GetKey(KeyCode.Space)) discreteActionsOut[1] = 1;      // Move Forward

    } // Heuristic
    // ==============================================================================================
    // Update is called once per frame
    void Update()
    {
       // None  - All Move to Huueristics Method

    } // Update
    // =======================================================================
    void FixedUpdate()
    {

      //  None - All Motions Move to OnActionsReceived Method 

    }  // FixedUpdate()
    // =======================================================================
    void GameOverun()
    {
        // Debug.Log("*** Ovverun ");

        SetReward(-2.0f);
        EndEpisode();
    }
    // =======================================================================
    void GameLost()
    {
       // Debug.Log("*** Game Lost ");

        SetReward(-5.0f);
        EndEpisode();
    }
    // =======================================================================
    void GameWon()
    {
        Debug.Log("A Dalek Has Found The Tardis !");

        SetReward(5.0f);
        EndEpisode();

    }
    // =======================================================================
}
