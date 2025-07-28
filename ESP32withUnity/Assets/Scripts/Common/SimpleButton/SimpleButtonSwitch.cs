using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIVE_Common
{
    public class SimpleButtonSwitch : SimpleButton
    {
        public Transform Graphics;
        public Vector3 GraphicsAngleOn;
        public Vector3 GraphicsAngleOff;
        public MeshRenderer Indicator;
        public Material IndicatorColorActive;
        public Material IndicatorColorInactive;

        private void Start()
        {
            UpdateSwitch();
        }

        protected override void OnButtonDown()
        {
            base.OnButtonDown();
            UpdateSwitch();
        }

        private void UpdateSwitch()
        {
            if (IsActive)
            {
                Graphics.localEulerAngles = GraphicsAngleOn;
                if (Indicator != null)
                    Indicator.material = IndicatorColorActive;
            }
            else
            {
                Graphics.localEulerAngles = GraphicsAngleOff;
                if (Indicator != null)
                    Indicator.material = IndicatorColorInactive;
            }
        }
    }

}
