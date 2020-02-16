#if hNEURON

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class NeuronTracker : Tracker {
        public override string name {
            get { return NeuronDevice.name; }
        }

        public string address = "127.0.0.1";
        public int port = 7001;

        public NeuronDevice device;
        public TrackerTransform neuronTransform;

        #region Start
        public override void StartTracker(HumanoidControl _humanoid) {
            humanoid = _humanoid;

            if (!enabled)
                return;

            device = new NeuronDevice();
            device.Init(address, port);
            neuronTransform = device.GetTracker();

            AddTracker(humanoid, "PerceptionNeuron");
        }
        #endregion

        #region Stop
        public override void StopTracker() {
            if (device != null)
                device.Stop();
        }
        #endregion

        #region Update
        public override void UpdateTracker() {
            if (!enabled ||
                    device == null ||
                    trackerTransform == null)
                return;

            //device.position = trackerTransform.position;
            //device.rotation = trackerTransform.rotation;
            device.Update();

            status = neuronTransform.status;
            trackerTransform.gameObject.SetActive(status != Status.Unavailable);
        }
        #endregion
    }
}
#endif