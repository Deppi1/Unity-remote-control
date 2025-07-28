using BNG;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonChecker : MonoBehaviour
{
    private ServerConnections _serverScript;

    private Button _button;
    private ButtonChecker _buttonScript;

    public int id;
    public bool buttonInfo;

    public delegate void ButtonAction();
    public static event ButtonAction OnClicked;


    private void Start()
    {
        _serverScript = ServerConnections.instance;
        _serverScript._buttonInfo.Add(id, this);

        _button = GetComponent<Button>();
        //_button.onButtonDown.AddListener(ButtonDown);
        _button.onButtonUp.AddListener(ButtonUp);
    }

    public async void ButtonDown()
    {
        buttonInfo = true;
        await _serverScript._connection.SendAsync("SendBool", id, buttonInfo);
        Debug.Log("Down Unity");
    }

    private async void ButtonUp()
    {
        buttonInfo = !buttonInfo;
        await _serverScript._connection.SendAsync("SendBool", id, buttonInfo);
        Debug.Log("Up Unity");
    }
}
