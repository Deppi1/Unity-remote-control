using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class ServerConnections : MonoBehaviour
{
    public static ServerConnections instance = null;

    [SerializeField]
    private string _url;
    
    public Dictionary<int, ButtonChecker> _buttonInfo = new Dictionary<int, ButtonChecker>();
    public HubConnection _connection;
    

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        InitializeManager();   
    }

    private async void InitializeManager()
    {
        try
        {
            Debug.Log("Connection...");
            _connection = new HubConnectionBuilder()
                        .WithUrl(_url)
                        .Build();

            // Прием сообщения с сервера под обработчиком события ReceiveBool
            _connection.On<int, bool>("ReceiveBool", (buttonId, buttonValue) =>
            {
                if (_buttonInfo.ContainsKey(buttonId))
                {
                    ButtonChecker buttonChecker = _buttonInfo[buttonId];
                    buttonChecker.buttonInfo = buttonValue;
                }
            });

            await _connection.StartAsync();
            Debug.Log("Connected!");
        }
        catch
        {
            Debug.Log("Connection Error");
        }
    }
}
