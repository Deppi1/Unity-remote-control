using BNG;
using System.Collections.Generic;
using UnityEngine;

namespace DIVE_Player
{
    [RequireComponent(typeof(Collider))]
    public class CustomHandPoseZone : MonoBehaviour
    {
        [SerializeField] private HandPose Pose;

        private Dictionary<Collider, HandControllerWithCustomPose> _handControllers = new Dictionary<Collider, HandControllerWithCustomPose>();

        private void OnTriggerEnter(Collider other)
        {
            if (_handControllers.ContainsKey(other) == true)
            {
                SetCustomPose(other, Pose);
                return;
            }

            var handController = other.GetComponentInParent<HandControllerWithCustomPose>();

            if (handController == null)
                return;
            
            handController.CustomPose = Pose;

            _handControllers[other] = handController;
        }

        private void OnTriggerExit(Collider other)
        {
            SetCustomPose(other, null);
        }

     
        private void SetCustomPose(Collider handControllersKey, HandPose pose)
        {
            if (_handControllers.ContainsKey(handControllersKey) == false)
                return;

            var handController = _handControllers[handControllersKey];
            handController.CustomPose = pose;
        }
    }
}