using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;



/// <summary>
///    Source: https://netmq.readthedocs.io/en/latest/pub-sub/#subscriber
///    Source for python code: https://github.com/off99555/Unity3D-Python-Communication/blob/master/README.md
///    Source for python 2: http://zguide.zeromq.org/py:wuserver
/// </summary>
public class RadioRequester : RunAbleThread
{
    public static float carFrontX;
    public static float carFrontZ;
    public static float carBackX;
    public static float carBackZ;

    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet

        Debug.Log("Subscriber started");
        using (var subSocket = new SubscriberSocket())
        {
            //subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://127.0.0.1:5555");
            subSocket.Subscribe("22");
            Console.WriteLine("Subscriber socket connecting...");
            while (Running)
            {
                string messageTopicReceived = subSocket.ReceiveFrameString();
                string messageReceived = subSocket.ReceiveFrameString();
                //Debug.Log(messageReceived);
                carFrontX = Convert.ToSingle(messageReceived.Split(' ')[1]) / 100;
                carFrontZ = Convert.ToSingle(messageReceived.Split(' ')[2]) / 100;
                carBackX =  Convert.ToSingle(messageReceived.Split(' ')[3]) / 100;
                carBackZ =  Convert.ToSingle(messageReceived.Split(' ')[4]) / 100;

                //Debug.Log(carFrontX.ToString() + ":" + carFrontZ.ToString() + " " +carBackX.ToString() + ":" + carBackZ.ToString());

            }

        }
       NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
}