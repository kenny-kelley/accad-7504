
using UnityEngine;

namespace Passer {
    using Humanoid.Tracking;

    public class MicrosoftKinect2Spawner : HumanoidSpawner {
#if hKINECT2

        public void Awake() {
            spawnedHumanoids = new HumanoidControl[6];
            nHumanoids = 0;

            //KinectDevice.Start();
            //KinectDevice.onPlayerAppears += OnPlayerAppears;
            //KinectDevice.onPlayerDisappears += OnPlayerDisappears;
        }

        public void Update() {
            //KinectDevice.Update();
        }

        public void OnPlayerAppears(int bodyID) {
            Debug.Log(bodyID + " appears");

            if (bodyID > 5)
                return; // we do not support more than 6 players

            HumanoidControl humanoid = SpawnHumanoid();
            if (humanoid == null)
                return;
            
            humanoid.kinectTracker.Enable();
            humanoid.kinectTracker.bodyID = bodyID;
            humanoid.kinectTracker.trackerTransform.transform.position = this.transform.position;

            humanoid.useGravity = false;
            humanoid.physics = false;
            spawnedHumanoids[bodyID] = humanoid;
            nHumanoids++;
        }

        public void OnPlayerDisappears(int bodyID) {
            Debug.Log(bodyID + " disappears");
            if (bodyID > 5)
                return; // we do not support more than 6 players

            DestroyHumanoid(spawnedHumanoids[bodyID]);
            nHumanoids--;
        }

#endif
    }
}
