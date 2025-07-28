using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

namespace DIVE_Common
{
    public class UITriggerAdjusted : UITrigger
    {
        public Grabber ParentGrabber;

        public bool IsTimeout
        {
            get
            {
                return _inTimeout;
            }
        }

        private bool _inTimeout = false;
        private float _timeout = 0.0f;

        private void Update()
        {
            if (!_inTimeout)
                return;

            _timeout -= Time.deltaTime;
            if (_timeout < 0.0f)
                _inTimeout = false;
        }

        public void Timeout(float time)
        {
            Debug.Log("timeout!");
            _inTimeout = true;
            _timeout = time;
        }
    }
}
