using UnityEngine;
using Vuforia;

namespace myScript {
    public class TileTracker : VuforiaMonoBehaviour, ITrackableEventHandler {

        public string Name => _trackableName;
        public int Id => _trackableId;


        public FrameController frameController;

        private bool _vuforiaCreated;

        private int _trackableId;
        private string _trackableName;

        private TrackableBehaviour _behaviour;
        private TrackableBehaviour.Status _previousStatus;
        private TrackableBehaviour.Status _newStatus;

        private Color foundTracking = Color.green;
        private Color limitedTracking = Color.yellow;
        private Color extendedTracking = Color.blue;
        private Color lostTracking = Color.red;

        
        
        private void Update()
            {
                if (!_vuforiaCreated && VuforiaManager.Instance.Initialized) {
                    SetupTrackableBehavior();
                }
            }

        #region TrackingEvents

        private void OnTrackingFound()
            {
                frameController.ToggleFrameMeshes(true);
                frameController.TogglePinEntities(true);
            }

        private void OnExtendedTracking()
            {

            }

        private void OnLimitedTracking()
            {

            }

        private void OnTrackingLost()
            {
                frameController.TogglePinEntities(false);
                frameController.ToggleFrameMeshes(false);

            }

        private void SetupTrackableBehavior()
            {
                _behaviour = GetComponent<TrackableBehaviour>();

                if (_behaviour) {
                    _behaviour.RegisterTrackableEventHandler(this);
                    _vuforiaCreated = true;
                }
            }

        public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus,
            TrackableBehaviour.Status newStatus)
            {
                _previousStatus = previousStatus;
                _newStatus = newStatus;

                if (_newStatus == TrackableBehaviour.Status.DETECTED)
                    OnTrackingFound();
                else if (_newStatus == TrackableBehaviour.Status.TRACKED)
                    OnTrackingFound();
                else if (_newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
                    OnExtendedTracking();
                else if (_newStatus == TrackableBehaviour.Status.LIMITED)
                    OnLimitedTracking();
                else if (_previousStatus == TrackableBehaviour.Status.TRACKED && 
                         _newStatus == TrackableBehaviour.Status.NO_POSE)
                    OnTrackingLost();
                else
                    OnTrackingLost();
            }

        #endregion

    }
}