//source: https://www.youtube.com/watch?v=8xdXJtu6nig
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentScript))]
public class CarCont : MonoBehaviour
{
    public AgentScript Im;
    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steeringWheels;
    //public float strengthCoef = 20000f;
    //public float maxTurn = 20f;

    // Start is called before the first frame update
    void Start()
    {
        Im = GetComponent<AgentScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (WheelCollider wheel in throttleWheels)
        {
            wheel.motorTorque = Im.strengthCoef * Time.deltaTime * Im.throttle;
        }

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = Im.maxTurn * Im.steer;
        }
    }
}