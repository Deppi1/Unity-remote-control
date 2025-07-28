using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using UnityEngine;

public class WebScoketClient : MonoBehaviour
{

    WebSocket ws;

    void Start()
    {
        ws = new WebSocket("ws://77.106.101.23:8080/?user=unity");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message received from" + ((WebSocket)sender).Url + ", Data: " + e.Data);
        };
        ws.Connect();
    }


    void Update()
    {
        if (ws == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ws.Send("{\"action\" : \"SENDBTN\",\"id\" : 5,\"value\" : 0}");
        }
    }
}
