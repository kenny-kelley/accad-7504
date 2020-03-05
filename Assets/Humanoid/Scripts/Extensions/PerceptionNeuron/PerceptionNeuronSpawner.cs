using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class PerceptionNeuronSpawner : HumanoidSpawner {
        public string address = "127.0.0.1";
        private NeuronDevice device;
#if hNEURON
        public void Awake() {
            spawnedHumanoids = new HumanoidControl[2];

            //NeuronDevice.onActorAppears += OnActorAppears;
            //NeuronDevice.Start(address, 7001, 7001); //, Neuron.NeuronConnection.SocketType.TCP);
            device = new NeuronDevice();
            device.Init(address, 7001);
        }

        public void Update() {
            device.Update();
        }

        public void OnApplicationQuit() {
            device.Stop();
        }

        public void OnActorAppears(int actorID) {
            Debug.Log("Actor " + actorID + " appears");

            if (nHumanoids > 2)
                return; // we do not support more than 2 actors

            HumanoidControl humanoid = SpawnHumanoid();
            if (humanoid == null)
                return;

            humanoid.neuronTracker.Enable();
            //humanoid.neuronTracker.actorID = actorID;
            humanoid.animatorEnabled = false;

            humanoid.useGravity = false;
            humanoid.physics = false;

            spawnedHumanoids[actorID] = humanoid;
            nHumanoids++;        
        }
#endif
    }
}