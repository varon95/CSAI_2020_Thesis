using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;
using MLAgents.Sensors;
using System.IO.Ports;
using System.IO;
using NetMQ;
using NetMQ.Sockets;
using Random = UnityEngine.Random;

public class AgentScript : Agent
{
    // These Static Variables are accessed in "checkpoint" Script
    public Transform[] checkPointArray;
    public static Transform[] checkpointA;
    public static int currentCheckpoint = 0;
    public static int currentLap = 0;
    public Vector3 startPos;
    public Quaternion startRot;
    public int Lap;
    public int CheckP;
    //car parameters
    public float throttle;
    public float steer;

    //starting parameters
    int previousCheckpoint = 0;
    public Vector3 oldPosition;
    //rewards and punishments
    readonly float targetTimePunishmentStatic = 60.0f;
    float targetTimePunishment;

    readonly float targetTimeResetStatic = 30.0f;
    float targetTimeReset;

    readonly float wallConnectPunishment = -1f;
    //readonly float notMovingPunishment = -1.0f;
    readonly float checkPointReward = 1.0f;
    readonly float distanceRewardMultiplier = 0.1f;
    float distanceRewardToShow;

    //resetting to random waypoint
    GameObject[] spawnPoints;
    GameObject currentPoint;
    int spawnPointIndex;

    //points
    public GameObject front;
    public GameObject back;

    //points
    public GameObject front_real;
    public GameObject back_real;

    //distance reward
    float currentPosY;

    float currentPosX;
    float currentPosZ;
    float newDistanceX;
    float newDistanceZ;
    float oldDistanceX;
    float oldDistanceZ;
    public float directionReward;

    bool reset = true;

    Rigidbody rBody;

    //real car 
    public bool realCar;
    String[] portArray;
    SerialPort port;
    public float realCarThrottle = 0.25f;

    //if we use radio requester
    //source: https://github.com/off99555/Unity3D-Python-Communication/blob/master/README.md
    public bool radio;
    private RadioRequester _radioRequester;

    //serial messeges overwhelmed the port and slowed the whole program down.... so, lets just send unique messeges
    private float previousThr = -100f;
    private float previousStr = -100f;
    readonly float messageResetTimeStatic = 10f;
    private float messageResetTime = 5f;

    //randomization
    public bool randomization;

    public float strengthCoef = 25f;
    public float maxTurn = 30f;
    public float turnBias = 0f;

    float biasCarFrontX=0f;
    float biasCarFrontZ=0f;
    float biasCarBackX =0f;
    float biasCarBackZ =0f;

    //save results
    public string filename = "test";

    //real car body movement
    bool isConnected = false;

    //continous/discrete
    public bool isDiscrete;



    void Start()
    {
        

        if (radio)
        {
            _radioRequester = new RadioRequester();
            _radioRequester.Start();
        }
        if (realCar)
        {
            try
            {
                connectToAvailablePort();
                isConnected = true;
            }
            catch (Exception)
            {
                Debug.Log("no port available");
            }
        }

        
        rBody = GetComponent<Rigidbody>();

        startPos = this.transform.position;
        startRot = this.transform.rotation;
        currentCheckpoint = 0;
        currentLap = 0;
        CheckP = 0;

        oldPosition = this.transform.position;

        //target times
        targetTimePunishment = targetTimePunishmentStatic;
        targetTimeReset = targetTimeResetStatic;

        //distance to the next checkpoint
        oldDistanceX = Math.Abs(checkPointArray[currentCheckpoint].transform.position.x - this.transform.position.x);
        oldDistanceZ = Math.Abs(checkPointArray[currentCheckpoint].transform.position.z - this.transform.position.z);
        
    }

    void Update()
    {
        Lap = currentLap;
        checkpointA = checkPointArray;
        CheckP = currentCheckpoint;


        //Punish and reset if not moving
        /*
        targetTimePunishment -= Time.deltaTime;
        if (targetTimePunishment <= 0.0f)
        {
            Vector3 changeInDistance = oldPosition - this.transform.position;
            if (Math.Abs(changeInDistance.x) < 1.0f && Math.Abs(changeInDistance.y) < 1.0f && Math.Abs(changeInDistance.z) < 1.0f)
            {
                //Debug.Log("agent is not moving");
                SetReward(notMovingPunishment);
                reset = true;
                EndEpisode();
                //reset
            }
            targetTimePunishment = targetTimePunishmentStatic;
            oldPosition = this.transform.position;
        }
        */

        //reset after every given time
        targetTimeReset -= Time.deltaTime;
        if (targetTimeReset <= 0.0f)
        {
            //Debug.Log("auto reset");
            reset = true;
            Debug.Log(GetCumulativeReward().ToString());
            StreamWriter writer = new StreamWriter("D:/tilburg/thesis/unity/car_with_AI_test_vector/test_results/"+ filename + ".txt", true);
            writer.WriteLine(GetCumulativeReward().ToString());
            writer.Close();
            EndEpisode();
        }

        
        messageResetTime -= Time.deltaTime;
        if (messageResetTime<=0f)
        {
            messageResetTime = messageResetTimeStatic;
            previousThr = -100f;
            previousStr = -100f;
        }

        //move car body if connected to real car
        if (isConnected)
        {
            //moveTurnBody(this.gameObject, back_real, front_real);
        }

    }

    //move the car body if connected to real car
    void moveTurnBody(GameObject body, GameObject back, GameObject front)
    {
        var v3 = front.transform.position - back.transform.position;
        float angle = System.Convert.ToSingle(Mathf.Atan2(v3.z, v3.x) * 180 / 3.14);
        body.transform.eulerAngles = new Vector3(0.0f, -angle+90, 0.0f);
        body.transform.position = new Vector3(back.transform.position.x + (front.transform.position.x - back.transform.position.x) / 2, 0, back.transform.position.z + (front.transform.position.z - back.transform.position.z) / 2);

    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(GetCumulativeReward().ToString());
        StreamWriter writer = new StreamWriter("D:/tilburg/thesis/unity/car_with_AI_test_vector/test_results/" + filename + ".txt", true);
        writer.WriteLine(GetCumulativeReward().ToString());
        writer.Close();
        //Debug.Log("Collision Detected");
        SetReward(wallConnectPunishment);
        // /*
        reset = true;
        EndEpisode();
        // */
        //positionReset(); //if not end episode

    }

    public void positionReset()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;

       /*
        spawnPoints = GameObject.FindGameObjectsWithTag("waypoint");
        spawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
        currentPoint = spawnPoints[spawnPointIndex];

        this.transform.position = currentPoint.transform.position;
        this.transform.rotation = currentPoint.transform.rotation;

        previousCheckpoint = currentPoint.GetComponent<nextCheckpoint>().nextCheckpointInt;
        currentCheckpoint = currentPoint.GetComponent<nextCheckpoint>().nextCheckpointInt;
        */
       // /*
        this.transform.position = startPos;
        this.transform.rotation = startRot;


        previousCheckpoint = 0;
        currentCheckpoint = 0;

        //*/

        //target times
        targetTimePunishment = targetTimePunishmentStatic;
        targetTimeReset = targetTimeResetStatic;

        //distance to the next checkpoint
        oldDistanceX = Math.Abs(checkPointArray[currentCheckpoint].transform.position.x - this.transform.position.x);
        oldDistanceZ = Math.Abs(checkPointArray[currentCheckpoint].transform.position.z - this.transform.position.z);
    }


    public override void OnEpisodeBegin()
    {
        
        if (randomization)
        {
            randomize();
        }

        if (reset == true)
        {
            positionReset();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {



        if (radio)
        {
            sensor.AddObservation(RadioRequester.carFrontX);
            sensor.AddObservation(RadioRequester.carFrontZ);
            sensor.AddObservation(RadioRequester.carBackX);
            sensor.AddObservation(RadioRequester.carBackZ);

            front_real.transform.position = new Vector3(RadioRequester.carFrontX, 0.209f, RadioRequester.carFrontZ);
            back_real.transform.position =  new Vector3(RadioRequester.carBackX, 0.209f, RadioRequester.carBackZ);
        }

        else
        {
            //generate bias for front and back coordinates
            if (randomization)
            {
                randomizePoint();
            }

            // Agent front and back coordinates


            sensor.AddObservation(front.transform.position.x + biasCarFrontX); // /3.2-1
            sensor.AddObservation(front.transform.position.z + biasCarFrontZ); // / 2.4-1

            sensor.AddObservation(back.transform.position.x + biasCarBackX);
            sensor.AddObservation(back.transform.position.z + biasCarBackZ);

            front_real.transform.position = front.transform.position;
            back_real.transform.position = back.transform.position;
            //Debug.Log(Convert.ToSingle(front.transform.position.x /3.2-1).ToString());
            //Debug.Log(front.transform.position.x.ToString()+" , "+ front.transform.position.z.ToString() + " , " + back.transform.position.x.ToString() + " , " + back.transform.position.z.ToString());

            /*
            // Agent velocity
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.z);

            // Agent position
            sensor.AddObservation(this.transform.position.x);
            sensor.AddObservation(this.transform.position.z);
            */
        }


    }

    public override void OnActionReceived(float[] vectorAction)
    {

        //if (radio == 1) { Debug.Log(RadioRequester.carFrontX.ToString()); }

        if (RadioRequester.carFrontX == 0.0 && radio && realCar)
        {

            writeThrottle(0f);
            writeSteer(0f);
        }
        else
        {
            if (isDiscrete)
            {
                MoveAgent(vectorAction);
            }
            else
            {
                writeSteer(vectorAction[0]); //continous
            }

            if (realCar)
            {
                
                writeThrottle(realCarThrottle);
                //writeThrottle(0.5f);
            }
            else
            {
                writeThrottle(0.8f); //writeThrottle(vectorAction[1]); //writeThrottle(1.0f);
            }
            
        }


        if (currentCheckpoint != previousCheckpoint)
        {
            //distance to the next checkpoint
            oldDistanceX = Math.Abs(checkPointArray[currentCheckpoint].transform.position.x - this.transform.position.x);
            oldDistanceZ = Math.Abs(checkPointArray[currentCheckpoint].transform.position.z - this.transform.position.z);

            previousCheckpoint = currentCheckpoint;
            Debug.Log("checkpoint reached");
            AddReward(checkPointReward);
        }

        //distance reward
        currentPosX = this.transform.position.x;
        currentPosZ = this.transform.position.z;
        newDistanceX = Math.Abs(checkPointArray[currentCheckpoint].transform.position.x - currentPosX);
        newDistanceZ = Math.Abs(checkPointArray[currentCheckpoint].transform.position.z - currentPosZ);
        directionReward = Convert.ToSingle(Math.Round(((oldDistanceX - newDistanceX) + (oldDistanceZ - newDistanceZ)), 2)) * distanceRewardMultiplier;
        AddReward(directionReward);
        distanceRewardToShow += directionReward;
        //Debug.Log("dir rew: " + distanceRewardToShow.ToString());



        oldDistanceX = newDistanceX;
        oldDistanceZ = newDistanceZ;



        Debug.Log(GetCumulativeReward().ToString());

    }

    public override float[] Heuristic()
    {
        if (isDiscrete)
        {
            if (Input.GetKey(KeyCode.Q)) { return new float[] { 1 }; }
            if (Input.GetKey(KeyCode.W)) { return new float[] { 2 }; }
            /*
            if (Input.GetKey(KeyCode.E)) {  return new float[] { 3 }; }
            if (Input.GetKey(KeyCode.R)) {  return new float[] { 4 }; }
            */
            return new float[] { 0 };
        }
        else
        {
            var action = new float[2];
            action[0] = Input.GetAxis("Horizontal");
            action[1] = Input.GetAxis("Vertical");
            return action;
        }

    }

    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        var action = Mathf.FloorToInt(act[0]);

        // 1x3 action space
        switch (action)
        {
            case 0:
                writeSteer(0.0f);
                break;
            case 1:
                writeSteer(-1.0f);
                break;
            case 2:
                writeSteer(1.0f);
                break;
            //case 3:
            //    writeSteer(0.5f);
            //    break;
            //case 4:
            //    writeSteer(-0.5f);
            //    break;
            //

        }
    }

    void writeThrottle(float thr)
    {
        if (realCar)
        {
            try
            {
                if (previousThr!=thr)
                {
                    previousThr = thr;
                    port.Write("#SPED" + Convert.ToInt32((-1 * thr + 1) * 64).ToString() + "\n");
                }              
            }
            catch (Exception) { throttle = thr; }
        }
        else
        {
            throttle = thr;
        }

    }

    void writeSteer(float str)
    {
        if (realCar)
        {
            str = Convert.ToSingle(Math.Round(str, 1));

            try
            {
                if (previousStr!=str)
                {
                    previousStr = str;
                    port.Write("#TURN" + Convert.ToInt32((str * -0.8 + 1) * 64).ToString() + "\n");
                }              
            }
            catch (Exception) { steer = str; }
        }
        else
        {
            steer = str+turnBias;
        }
    }

    void connectToAvailablePort()
    {
        portArray = SerialPort.GetPortNames();
        port = new SerialPort(portArray[0], 9600, Parity.None, 8, StopBits.One);
        port.Open();
    }

    void randomize()
    {
        turnBias = Random.Range(-0.1f, 0.1f);
        strengthCoef = Random.Range(25, 35);
        maxTurn = Random.Range(20, 40);
        
    }

    void randomizePoint()
    {
        biasCarFrontX =Random.Range(-0.02f, 0.02f);
        biasCarFrontZ =Random.Range(-0.02f, 0.02f);
        biasCarBackX  =Random.Range(-0.02f, 0.02f);
        biasCarBackZ = Random.Range(-0.02f, 0.02f);

    }

    private void OnDestroy()
    {
        if (radio)
        {
            _radioRequester.Stop();
        }
        if (realCar)
        {
            writeThrottle(0f);
            writeSteer(0f);
            try
            {
                port.Close();
                Debug.Log("port closed");
            }
            catch (Exception) { }
            
        }
    }

} 