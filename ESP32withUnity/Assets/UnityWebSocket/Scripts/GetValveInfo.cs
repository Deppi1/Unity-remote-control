using BNG;
using DIVE_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GetValveInfo : MonoBehaviour
{
    public ActionType Action = ActionType.None;
    public int Id;
    public float Value;
    private SteeringWheelAdjusted _valve;
    private GrabbableUnityEvents _grabbableUnityEvents;

    public UnityEvent<GetValveInfo> ValveRotation = new UnityEvent<GetValveInfo>();
    void Start()
    {
        _grabbableUnityEvents = gameObject.GetComponent<GrabbableUnityEvents>();
        _valve = gameObject.GetComponent<SteeringWheelAdjusted>();

        _valve.onAngleChange.AddListener(ValveValue);
        _grabbableUnityEvents.onRelease.AddListener(ValveRelease);
    }

    public void ValveValue(float value)
    {
        Value = Mathf.Round(value / 18);
    }

    private void ValveRelease()
    {
        ValveRotation.Invoke(this);
    }

    public void SetAngleFromServer(float value)
    {
        _valve.AdjustWheelAngle(value);
    }


}
