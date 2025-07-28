using BNG;

namespace DIVE_Player
{
    public class HandControllerWithCustomPose : HandController
    {
        public HandPose CustomPose;

        private bool _useCustomPose = false;

        private readonly float GripAmountSensitivity = 0.05f; // To avoid false clicks

        private void Awake()
        {
            if(handPoser == null || !handPoser.isActiveAndEnabled) {
                handPoser = GetComponentInChildren<HandPoser>();
            }
        }

        protected override void Update()
        {
            UpdateUseCustomPoseValue();

            if (_useCustomPose)
            {
                UpdateFromInputs();
                ShowCustomPose();
            }
            else
            {
                base.Update();
            }
        }

        private void ShowCustomPose()
        {
            if (handPoser == null || handPoser.CurrentPose == CustomPose)
                return;

            handPoser.CurrentPose = CustomPose;
            handPoser.OnPoseChanged();
        }

        private bool CanShowCustomPose()
        {
            if (CustomPose != null && grabber != null && handPoser != null && grabber.HoldingItem == false && GripAmount < GripAmountSensitivity)
                return true;

            return false;
        }

        private void UpdateUseCustomPoseValue()
        {
            bool canShowCustomPose = CanShowCustomPose();

            if (_useCustomPose == canShowCustomPose)
                return;

            _useCustomPose = canShowCustomPose;

            if (_useCustomPose)
                EnableHandPoser();
            else
                EnableHandAnimator();
        }
    }
}
