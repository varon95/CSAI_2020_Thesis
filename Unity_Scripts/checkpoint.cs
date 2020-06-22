//Source: https://answers.unity.com/questions/290652/add-checkpoints-and-laps-unity-car-tutorial.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    void Start()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        //Is it the Player who enters the collider?
        if (!other.CompareTag("Player"))
        {
            //Debug.Log("not player");
            return; //If it's not the player dont continue
        }


        if (transform == AgentScript.checkpointA[AgentScript.currentCheckpoint].transform)
        {
            //Check so we dont exceed our checkpoint quantity
            if (AgentScript.currentCheckpoint + 1 < AgentScript.checkpointA.Length)
            {
                //Add to currentLap if currentCheckpoint is 0
                if (AgentScript.currentCheckpoint == 0)
                    AgentScript.currentLap++;
                //increase checkpoint number
                AgentScript.currentCheckpoint++;
            }
            else
            {
                //If we dont have any Checkpoints left, go back to 0
                AgentScript.currentCheckpoint = 0;
            }
        }


    }
}
