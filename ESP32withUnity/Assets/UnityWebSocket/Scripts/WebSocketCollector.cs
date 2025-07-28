using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using WebSocketSharp;
using UnityEngine;
public enum ActionType
{
    None,
    SENDBTN,
    GETBTN,
    SENDVALVE,
    GETVALVE,
    FINISHSTAGE
};

public class WebSocketCollector : MonoBehaviour
{
    WebSocket ws;
    public List<GetValveInfo> ValveInfo = new List<GetValveInfo>();

    void Start()
    {
        ws = new WebSocket("ws://192.168.0.51:8080/?user=unity");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data);
            GetMessage(e.Data);

        };
        ws.Connect();

        for (int i = 0; i < ValveInfo.Count; i++)
        {
            ValveInfo[i].ValveRotation.AddListener(OnValveChange);
        }
    }

    private void GetMessage(string message)
    {
        SendData desJsonValveInfo = JsonSerializer.Deserialize<SendData>(message);


        for (int i = 0; i < ValveInfo.Count; i++)
        {
            if (ValveInfo[i].Id == desJsonValveInfo.id)
            {
                SendData data = new SendData(ActionType.FINISHSTAGE.ToString(), 0, 0f);
                string jsonValveInfo = JsonSerializer.Serialize(data);
                ws.Send(jsonValveInfo);

                ValveInfo[i].SetAngleFromServer(desJsonValveInfo.value * 18);
                break;
            }
        }    
    }

    private void OnValveChange(GetValveInfo arg0)
    {
        SendData data = new SendData(arg0.Action.ToString(), arg0.Id, arg0.Value);
        string jsonValveInfo = JsonSerializer.Serialize(data);
        ws.Send(jsonValveInfo);
    }

    public class SendData
    {
        public string action { get; }
        public int id { get; }
        public float value { get; }

        public SendData(string action, int id, float value)
        {
            this.action = action;
            this.id = id;
            this.value = value;
        }
    }
}
