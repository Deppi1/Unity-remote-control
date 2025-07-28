using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using System;

namespace DIVE_Common
{
    public class SteeringWheelAdjusted : SteeringWheel
    {
        public float StartingAngle;
        public float AngleOfRelease = 0.0f;
        public bool StopSmoothingOnRelease = false;

        private bool InverseRotation = false;
        private bool ReleasingThroughRotation = false;
        private float AngleOfGrabbing = 0.0f;

        public event Action OnGrabbableRelease;
        // Start is called before the first frame update
        void Start()
        {
            if (MinAngle < 1 && MinAngle > -1)
            {
                MinAngle++;
                MaxAngle++;
                StartingAngle++;
            }
            AdjustWheelAngle(StartingAngle);
            if (AngleOfRelease != 0.0f)
            {
                ReleasingThroughRotation = true;
            }
        }

        public void AdjustWheelAngle(float newAngle)
        {
            // something is wrong here
            targetAngle = newAngle;
            previousTargetAngle = newAngle - 1.0f;
            smoothedAngle = newAngle;
            ApplyAngleToSteeringWheel(newAngle);
            CallEvents();
            UpdatePreviewText();
            UpdatePreviousAngle(newAngle);
        }

        public void SetOpened()
        {
            AdjustWheelAngle(MaxAngle);
        }

        public void SetClosed()
        {
            AdjustWheelAngle(MinAngle);
        }

        public void EndSmoothing()
        {
            targetAngle = smoothedAngle;
        }

        public void SetInverseRotation(bool value)
        {
            InverseRotation = value;
        }

        public override void UpdateAngleCalculations()
        {
            float angleAdjustment = 0f;

            // Add first Grabber
            if (PrimaryGrabber)
            {
                rotatePosition = transform.InverseTransformPoint(PrimaryGrabber.transform.position);
                rotatePosition = new Vector3(rotatePosition.x, rotatePosition.y, 0);

                // Add in the angles to turn
                angleAdjustment += GetRelativeAngle(rotatePosition, previousPrimaryPosition);

                previousPrimaryPosition = rotatePosition;
            }

            // Add second Grabber
            if (AllowTwoHanded && SecondaryGrabber != null)
            {
                rotatePosition = transform.InverseTransformPoint(SecondaryGrabber.transform.position);
                rotatePosition = new Vector3(rotatePosition.x, rotatePosition.y, 0);

                // Add in the angles to turn
                angleAdjustment += GetRelativeAngle(rotatePosition, previousSecondaryPosition);

                previousSecondaryPosition = rotatePosition;
            }

            // Divide by two if being held by two hands
            if (PrimaryGrabber != null && SecondaryGrabber != null)
            {
                angleAdjustment *= 0.5f;
            }

            // Apply the angle adjustment
            if (!InverseRotation)
                targetAngle -= angleAdjustment;
            else
                targetAngle += angleAdjustment;

            // Update Smooth Angle
            // Instant Rotation
            if (RotationSpeed == 0)
            {
                smoothedAngle = targetAngle;
            }
            // Apply smoothing based on RotationSpeed
            else
            {
                smoothedAngle = Mathf.Lerp(smoothedAngle, targetAngle, Time.deltaTime * RotationSpeed);
            }

            // Scrub the final results
            if (MinAngle != 0 && MaxAngle != 0)
            {
                targetAngle = Mathf.Clamp(targetAngle, MinAngle, MaxAngle);
                smoothedAngle = Mathf.Clamp(smoothedAngle, MinAngle, MaxAngle);
            }
        }

        public override void OnGrab(Grabber grabber)
        {
            base.OnGrab(grabber);
            AngleOfGrabbing = Angle;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            AngleOfGrabbing = 0.0f;
            if (StopSmoothingOnRelease)
            {
                EndSmoothing();
            }
            OnGrabbableRelease?.Invoke();
        }

        public override void CallEvents()
        {
            base.CallEvents();

            // check when to release
            if (targetAngle != previousTargetAngle)
            {
                if (!ReleasingThroughRotation)
                    return;
                float diff = Mathf.Abs(targetAngle - AngleOfGrabbing);
                if (diff > AngleOfRelease)
                {
                    Debug.Log("Angle of release exceeded, releasing!");
                    if (PrimaryGrabber != null)
                    {
                        PrimaryGrabber.TryRelease();
                    }
                    if (SecondaryGrabber != null)
                    {
                        SecondaryGrabber.TryRelease();
                    }
                }
            }
        }
    }

}
