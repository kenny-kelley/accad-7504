using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace Passer {

    [System.Serializable]
    public class HandInteraction {
        public const float kinematicMass = 1; // masses < 1 will move kinematic when not colliding
        public const float maxGrabbingMass = 10; // maxMass you can grab is 10

        #region Start
        public static void StartInteraction(HandTarget handTarget) {
            // Remote humanoids should not interact
            if (handTarget.humanoid.isRemote)
                return;

            // Gun Interaction pointer creates an Event System
            // First solve that before enabling this warning
            // because the warning will appear in standard Grocery Store demo scene

            //EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>();
            //foreach (EventSystem eventSystem in eventSystems) {
            //    if (eventSystem != null) {
            //        HumanoidControl eventSystemHumanoid = eventSystem.GetComponent<HumanoidControl>();
            //        if (eventSystemHumanoid == null)
            //            Debug.LogWarning("Another EventSystem found: [" + eventSystem.gameObject.name + "]. Humanoid Control interaction may not work properly");
            //    }
            //}

            handTarget.inputModule = handTarget.humanoid.GetComponent<Interaction>();
            if (handTarget.inputModule == null) {
                handTarget.inputModule = Object.FindObjectOfType<Interaction>();
                if (handTarget.inputModule == null) {
                    handTarget.inputModule = handTarget.humanoid.gameObject.AddComponent<Interaction>();
                }
            }

            handTarget.inputModule.EnableTouchInput(handTarget.humanoid, handTarget.isLeft, 0);
        }
        #endregion

        #region Update
        public static void UpdateInteraction() {
            // This interferes with the HandPhysics which also sets the touchingObject...

            //InputDeviceIDs inputDeviceID = isLeft ? InputDeviceIDs.LeftHand : InputDeviceIDs.RightHand;
            //touchingObject = inputModule.GetTouchObject(inputDeviceID);
        }
        #endregion

        #region Touching
        public static void OnTouchStart(HandTarget handTarget, GameObject obj) {
            GrabCheck(handTarget, obj);
            if (handTarget.inputModule != null)
                handTarget.inputModule.OnFingerTouchStart(handTarget.isLeft, obj);
        }

        public static void OnTouchEnd(HandTarget handTarget, GameObject obj) {
            if (handTarget.inputModule != null && obj == handTarget.touchedObject)
                handTarget.inputModule.OnFingerTouchEnd(handTarget.isLeft);
        }
        #endregion

        #region Grabbing
        private static bool grabChecking;

        public static void GrabCheck(HandTarget handTarget, GameObject obj) {
            if (grabChecking || handTarget.grabbedObject != null || handTarget.humanoid.isRemote)
                return;

            grabChecking = true;
            float handCurl = handTarget.HandCurl();
            if (handCurl > 2 && CanBeGrabbed(handTarget, obj)) {
                Grab(handTarget, obj);
            }
            grabChecking = false;
        }

        public static void Grab(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            //if (handTarget.humanoid.humanoidNetworking != null)
            //    handTarget.humanoid.humanoidNetworking.Grab(handTarget, obj, rangeCheck);

            LocalGrab(handTarget, obj, rangeCheck);
        }

        public static bool CanBeGrabbed(HandTarget handTarget, GameObject obj) {
            if (obj == null || obj == handTarget.humanoid.gameObject ||
                (handTarget.humanoid.characterRigidbody != null && obj == handTarget.humanoid.characterRigidbody.gameObject) ||
                // This check prevents two handed grabbing
                //(handTarget.otherHand.handRigidbody != null && obj == handTarget.otherHand.handRigidbody.gameObject) ||
                obj == handTarget.humanoid.headTarget.gameObject
                )
                return false;

            // We cannot grab 2D objects like UI
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
                return false;

            return true;
        }

        public static void LocalGrab(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
            Transform objTransform = obj.GetComponent<Transform>();

            if (handTarget.grabbedObject == null) {
                if (objRigidbody != null) {
                    NoGrab noGrab = objRigidbody.GetComponent<NoGrab>();
                    if (noGrab != null)
                        return;
                }

                if (objRigidbody != null && objRigidbody.mass > maxGrabbingMass)
                    return;

                bool grabbed = false;
                if (objRigidbody != null) {
                    grabbed = GrabRigidbody(handTarget, objRigidbody, rangeCheck);
                }
                else {
                    grabbed = GrabStaticObject(handTarget, objTransform, rangeCheck);
                }

                if (grabbed) {
                    if (handTarget.humanoid.physics) {
                        //HumanoidTarget.SetColliderToTrigger(handTarget.hand.bone.transform.gameObject, true);
                        AdvancedHandPhysics.SetNonKinematic(handTarget.handRigidbody, handTarget.colliders);
                        //handTarget.colliders = HumanoidTarget.SetColliderToTrigger(handTarget.hand.bone.transform.gameObject);
                        if (handTarget.handPhysics != null && handTarget.handPhysics.mode != AdvancedHandPhysics.PhysicsMode.ForceLess)
                            handTarget.handPhysics.DeterminePhysicsMode(kinematicMass);
                    }

                    handTarget.SendMessage("OnGrabbing", handTarget.grabbedObject, SendMessageOptions.DontRequireReceiver);
                    handTarget.grabbedObject.SendMessage("OnGrabbed", handTarget, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        public static bool GrabRigidbody(HandTarget handTarget, Rigidbody objRigidbody, bool rangeCheck = true) {
            //Debug.Log("GrabRigidbody");


            if (objRigidbody == handTarget.otherHand.handRigidbody)
                return false;

            if (handTarget.humanoid.humanoidNetworking != null)
                handTarget.humanoid.humanoidNetworking.Grab(handTarget, objRigidbody.gameObject, rangeCheck);

            GameObject obj = objRigidbody.gameObject;

            Handle[] handles = objRigidbody.GetComponentsInChildren<Handle>();
            for (int i = 0; i < handles.Length; i++) {
                Vector3 handlePosition = handles[i].transform.TransformPoint(handles[i].position);
                float grabDistance = Vector3.Distance(handTarget.palmPosition, handlePosition);

                if ((handTarget.isLeft && handles[i].hand == Handle.Hand.Right) ||
                    (!handTarget.isLeft && handles[i].hand == Handle.Hand.Left))
                    continue;

                if (grabDistance < handles[i].range || !rangeCheck) {
                    if (handles[i].grabType == Handle.GrabType.NoGrab)
                        return false;
                    else {
                        GrabRigidbodyHandle(handTarget, objRigidbody, handles[i]);
                        handles[i].handTarget = handTarget;
                        handTarget.grabbedObject = obj;
                        return true;
                    }
                }
            }

            if (rangeCheck == false) {
                //float grabDistance = Vector3.Distance(handTarget.handPalm.position, objRigidbody.position);
                float grabDistance = Vector3.Distance(handTarget.hand.bone.transform.position, objRigidbody.position);
                if (grabDistance > 0.2F) // Object is far away, move it into the hand
                    MoveObjectToHand(handTarget, objRigidbody.transform);
            }

            Joint joint = objRigidbody.GetComponent<Joint>();
            AdvancedHandPhysics otherHandPhysics = objRigidbody.GetComponent<AdvancedHandPhysics>();

            if (joint != null || objRigidbody.constraints != RigidbodyConstraints.None || otherHandPhysics != null) {
                GrabRigidbodyJoint(handTarget, objRigidbody);
            }
            else {
                GrabRigidbodyParenting(handTarget, objRigidbody);
            }
            handTarget.grabbedObject = obj;
            handTarget.grabbedRigidbody = true;
            return true;
        }

        private static void GrabRigidbodyHandle(HandTarget handTarget, Rigidbody objRigidbody, Handle handle) {
            Transform objTransform = objRigidbody.transform;

            if (AlreadyGrabbedWithOtherHand(handTarget, objRigidbody)) {
                GrabRigidbodyBarHandle2(handTarget, objRigidbody, handle);
                return;
            }

            Joint joint = objRigidbody.GetComponent<Joint>();
            if (joint != null || objRigidbody.constraints != RigidbodyConstraints.None) {
                MoveHandBoneToHandle(handTarget, handle);

                // To add: if handle.rotation = true
                Vector3 handleWorldPosition = handle.transform.TransformPoint(handle.position);
                Vector3 handleLocalPosition = handTarget.hand.bone.transform.InverseTransformPoint(handleWorldPosition);

                Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
                Vector3 handleRotationAxis = handleWorldRotation * Vector3.up;

                Vector3 handleLocalRotationAxis = handTarget.hand.bone.transform.InverseTransformDirection(handleRotationAxis);

                GrabRigidbodyJoint(handTarget, objRigidbody, handleLocalPosition, handleLocalRotationAxis);
            }
            else {
                MoveObjectToHand(handTarget, objTransform, handle);
                GrabRigidbodyParenting(handTarget, objRigidbody);
            }
            handTarget.grabbedHandle = handle;
            if (handle.pose != null) {
                handTarget.poseMixer.SetPoseValue(handle.pose, 1);
            }
            handTarget.grabbedRigidbody = true;
        }

        // Grab with second hand moving to object
        private static void GrabRigidbodyBarHandle2(HandTarget handTarget, Rigidbody objRigidbody, Handle handle) {
            //Debug.Log("Grab Second Hand");
            Transform objTransform = handle.transform;

            MoveHandBoneToHandle(handTarget, handle);
            GrabRigidbodyJoint(handTarget, objRigidbody);
            handTarget.grabbedHandle = handle;

            handTarget.handPhysics.mode = AdvancedHandPhysics.PhysicsMode.ForceLess;

            handTarget.handMovements.toOtherHandle = handTarget.grabbedHandle.GetWorldPosition() - handTarget.otherHand.grabbedHandle.GetWorldPosition();
            Quaternion hand2handle = Quaternion.LookRotation(handTarget.handMovements.toOtherHandle);
            handTarget.otherHand.handMovements.hand2handle = Quaternion.Inverse(hand2handle) * handTarget.otherHand.hand.target.transform.rotation;

            handTarget.grabbedRigidbody = true;
        }

        public static void MoveHandBoneToHandle(HandTarget handTarget, Handle handle) {
            //Vector3 handPosition;
            //Quaternion handRotation;
            //GetGrabPosition(handTarget, handle, out handPosition, out handRotation);

            if (handle.grabType == Handle.GrabType.DefaultGrab ||
                handle.grabType == Handle.GrabType.BarGrab) {

                // Should use GetGrabPosition
                //handTarget.hand.bone.transform.rotation = handRotation;

                Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
                Quaternion palm2handRot = Quaternion.Inverse(handTarget.handPalm.localRotation);
                handTarget.hand.bone.transform.rotation = handleWorldRotation * palm2handRot;
            }

            if (handle.grabType == Handle.GrabType.DefaultGrab || 
                handle.grabType == Handle.GrabType.BarGrab ||
                handle.grabType == Handle.GrabType.BallGrab) {

                //handTarget.hand.bone.transform.position = handPosition;
                Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
                Vector3 palm2handPos = handTarget.hand.bone.transform.position - handTarget.handPalm.position;
                handTarget.hand.bone.transform.position = handleWPos + palm2handPos;
            }
        }

        public static void MoveAndGrabHandle(HandTarget handTarget, Handle handle) {
            if (handTarget == null || handle == null)
                return;

            MoveHandTargetToHandle(handTarget, handle);
            GrabHandle(handTarget, handle);
        }

        public static void MoveHandTargetToHandle(HandTarget handTarget, Handle handle) {
            // Should use GetGrabPosition
            Quaternion handleWorldRotation = handle.transform.rotation * Quaternion.Euler(handle.rotation);
            Quaternion palm2handRot = Quaternion.Inverse(Quaternion.Inverse(handTarget.hand.bone.targetRotation) * handTarget.palmRotation);
            handTarget.hand.target.transform.rotation = handleWorldRotation * palm2handRot;

            Vector3 handleWorldPosition = handle.transform.TransformPoint(handle.position);
            handTarget.hand.target.transform.position = handleWorldPosition - handTarget.hand.target.transform.rotation * handTarget.localPalmPosition;
        }

        public static void GetGrabPosition(HandTarget handTarget, Handle handle, out Vector3 handPosition, out Quaternion handRotation) {
            Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
            Quaternion handleWRot = handle.transform.rotation * Quaternion.Euler(handle.rotation);

            GetGrabPosition(handTarget, handleWPos, handleWRot, out handPosition, out handRotation);
        }

        private static void GetGrabPosition(HandTarget handTarget, Vector3 targetPosition, Quaternion targetRotation, out Vector3 handPosition, out Quaternion handRotation) {
            Quaternion palm2handRot = Quaternion.Inverse(handTarget.handPalm.localRotation) * handTarget.hand.bone.toTargetRotation;
            handRotation = targetRotation * palm2handRot;

            Vector3 hand2palmPos = handTarget.handPalm.localPosition;
            Vector3 hand2palmWorld = handTarget.hand.bone.transform.TransformVector(hand2palmPos);
            Vector3 hand2palmTarget = handTarget.hand.target.transform.InverseTransformVector(hand2palmWorld); // + new Vector3(0, -0.03F, 0); // small offset to prevent fingers colliding with collider
            handPosition = targetPosition + handRotation * -hand2palmTarget;
            Debug.DrawLine(targetPosition, handPosition);
        }

        // This is not fully completed, no parenting of joints are created yet
        public static void GrabHandle(HandTarget handTarget, Handle handle) {
            handTarget.grabbedHandle = handle;
            handTarget.grabbedObject = handle.gameObject;
            handle.handTarget = handTarget;

            if (handle.pose != null)
                handTarget.SetPose(handle.pose);
        }

        public static void MoveObjectToHand(HandTarget handTarget, Transform objTransform, Handle handle) {
            //Quaternion localHandleRotation = handle.transform.rotation * Quaternion.Inverse(objTransform.rotation) 
            Quaternion handleRotation = handle.transform.rotation * Quaternion.Inverse(objTransform.rotation) * Quaternion.Euler(handle.rotation);
            objTransform.rotation = handTarget.palmRotation * Quaternion.Inverse(handleRotation); // Quaternion.Euler(handle.rotation));

            Vector3 handleWPos = handle.transform.TransformPoint(handle.position);
            objTransform.Translate(handTarget.palmPosition - handleWPos, Space.World);
        }
        public static void MoveObjectToHand(HandTarget handTarget, Transform objTransform) {
            objTransform.position = handTarget.palmPosition;
        }


        private static bool AlreadyGrabbedWithOtherHand(HandTarget handTarget, Rigidbody objRigidbody) {
            if (handTarget.otherHand == null || handTarget.otherHand.hand.bone.transform == null)
                return false;

            if (objRigidbody.transform == handTarget.otherHand.hand.bone.transform)
                return true;

            if (objRigidbody.isKinematic) {
                Transform parent = objRigidbody.transform.parent;
                if (parent == null)
                    return false;

                Rigidbody parentRigidbody = parent.GetComponentInParent<Rigidbody>();
                if (parentRigidbody == null)
                    return false;

                return AlreadyGrabbedWithOtherHand(handTarget, parentRigidbody);
            }

            return false;
        }
        public static void GrabRigidbodyJoint(HandTarget handTarget, Rigidbody objRigidbody) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            ConfigurableJoint joint = handTarget.hand.bone.transform.gameObject.AddComponent<ConfigurableJoint>();
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.01F;
            joint.projectionAngle = 1;

            Collider c = objRigidbody.transform.GetComponentInChildren<Collider>();
            joint.connectedBody = c.attachedRigidbody;
        }

        private static void GrabRigidbodyJoint(HandTarget handTarget, Rigidbody objRigidbody, Vector3 anchorPoint, Vector3 rotationAxis) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            ConfigurableJoint joint = handTarget.hand.bone.transform.gameObject.AddComponent<ConfigurableJoint>();
            Collider c = objRigidbody.transform.GetComponentInChildren<Collider>();
            joint.connectedBody = c.attachedRigidbody;

            joint.anchor = anchorPoint;
            joint.axis = rotationAxis;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Locked; // Free;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.01F;
            joint.projectionAngle = 1;

            handTarget.storedCOM = objRigidbody.centerOfMass;
            objRigidbody.centerOfMass = joint.connectedAnchor;
        }

        private static void GrabRigidbodyParenting(HandTarget handTarget, Rigidbody objRigidbody) {
            GrabMassRedistribution(handTarget.hand.bone.transform.GetComponent<Rigidbody>(), objRigidbody);

            handTarget.grabbedRBdata = new StoredRigidbody(objRigidbody);
            objRigidbody.transform.parent = handTarget.handPalm;

            if (handTarget.humanoid.humanoidNetworking != null)
                handTarget.humanoid.humanoidNetworking.DisableNetworkSync(objRigidbody.gameObject);
//#if hNW_PHOTON
//            PhotonTransformView transformView = objRigidbody.GetComponent<PhotonTransformView>();
//            if (transformView != null) {
//                transformView.m_PositionModel.SynchronizeEnabled = false;
//                transformView.m_RotationModel.SynchronizeEnabled = false;
//            }
//#endif

            if (Application.isPlaying)
                Object.Destroy(objRigidbody);
            else
                Object.DestroyImmediate(objRigidbody, true);
        }

        public static bool GrabStaticObject(HandTarget handTarget, Transform objTransform, bool rangeCheck = true) {
            //Debug.Log("GrabStaticObject");
            Handle[] handles = objTransform.GetComponentsInChildren<Handle>();
            for (int i = 0; i < handles.Length; i++) {
                if ((handTarget.isLeft && handles[i].hand == Handle.Hand.Right) ||
                    (!handTarget.isLeft && handles[i].hand == Handle.Hand.Left))
                    continue;

                Vector3 handlePosition = handles[i].transform.TransformPoint(handles[i].position);

                if (!rangeCheck || Vector3.Distance(handTarget.palmPosition, handlePosition) < handles[i].range) {
                    if (handles[i].grabType == Handle.GrabType.NoGrab)
                        return false;
                    else {
                        if (handTarget.humanoid.humanoidNetworking != null)
                            handTarget.humanoid.humanoidNetworking.Grab(handTarget, objTransform.gameObject, rangeCheck);

                        GrabStaticHandle(handTarget, handles[i]);
                        handles[i].handTarget = handTarget;
                        handTarget.grabbedObject = handles[i].gameObject;
                        handTarget.grabbedRigidbody = false;
                        return true;
                    }
                }
            }
            // Grabbing static objects is only possible with a handle
            return false;
        }

        // Grab with hand moving to object

        private static void GrabStaticHandle(HandTarget handTarget, Handle handle) {
            //Debug.Log("GrabStaticBarHandle");
            Transform objTransform = handle.transform;

            MoveHandBoneToHandle(handTarget/*, objTransform*/, handle);
            GrabStaticJoint(handTarget, objTransform);

            handTarget.grabbedHandle = handle;
            if (handle.pose != null) {
                handTarget.poseMixer.SetPoseValue(handle.pose, 1);
            }
        }

        private static void GrabStaticJoint(HandTarget handTarget, Transform objTransform) {
            //FixedJoint joint = target.handBone.parent.gameObject.AddComponent<FixedJoint>();
            handTarget.hand.bone.transform.gameObject.AddComponent<FixedJoint>();

            Collider c = objTransform.GetComponentInChildren<Collider>();
            /* does not make sense: a static collider does not have a (attached) rigidbody
            joint.connectedBody = c.attachedRigidbody;
            */

            if (c != null) {
                Collider[] handColliders = handTarget.hand.bone.transform.GetComponentsInChildren<Collider>();
                foreach (Collider handCollider in handColliders) {
                    Physics.IgnoreCollision(c, handCollider);
                    Debug.Log("Ignore Collision " + handCollider.name + " - " + c.name);
                }
            }
        }

        private static void GrabMassRedistribution(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            //handRigidbody.mass = KinematicPhysics.CalculateTotalMass(grabbedRigidbody);
            //grabbedRigidbody.mass *= 0.01F;
        }

        private static void GrabMassRestoration(Rigidbody handRigidbody, Rigidbody grabbedRigidbody) {
            //grabbedRigidbody.mass *= 100F;//handRigidbody.mass;
            //handRigidbody.mass = 1F;
        }

        public static void HandGrabPosition(HandTarget handTarget, Vector3 targetPosition, Quaternion targetRotation, Transform handPalm, out Vector3 handPosition, out Quaternion handRotation) {
            Quaternion palm2handRot = Quaternion.Inverse(handPalm.rotation) * handTarget.hand.bone.transform.rotation;
            handRotation = targetRotation * palm2handRot;

            Vector3 localPalmPosition = handTarget.hand.bone.transform.InverseTransformPoint(handPalm.position);
            handPosition = targetPosition - handRotation * localPalmPosition;
        }

        #endregion

        #region Letting go
        public static void CheckLetGo(HandTarget handTarget) {
            if (handTarget.grabbedObject == null || handTarget.grabType == HandTarget.GrabType.Pinch)
                return;

            float handCurl = handTarget.HandCurl();
            bool fingersGrabbing = (handCurl >= 1.5F);
            bool pulledLoose = PulledLoose(handTarget);
            if (!fingersGrabbing || pulledLoose) {
                LetGo(handTarget);
            }
        }

        private static bool PulledLoose(HandTarget handTarget) {
            float forearmStretch = Vector3.Distance(handTarget.hand.bone.transform.position, handTarget.forearm.bone.transform.position) - handTarget.forearm.bone.length;
            if (forearmStretch > 0.15F) {
                //Debug.Log("Pulled loose 1");
                return true;
            }

            if (handTarget.grabbedHandle != null) {
                Vector3 handlePosition = handTarget.grabbedHandle.worldPosition;
                float handle2palm = Vector3.Distance(handlePosition, handTarget.palmPosition);
                if (handle2palm > 0.15F) {
                    //Debug.Log("Pulled loose 2");
                    return true;
                }
            }
            return false;
        }

        public static void LetGo(HandTarget target) {
            if (target.humanoid.humanoidNetworking != null)
                target.humanoid.humanoidNetworking.LetGo(target);

            LocalLetGo(target);
        }

        public static void LocalLetGo(HandTarget handTarget) {
            //Debug.Log("LetGo");
            if (handTarget.hand.bone.transform == null || handTarget.grabbedObject == null)
                return;


            if (handTarget.humanoid.physics)
                AdvancedHandPhysics.SetNonKinematic(handTarget.handRigidbody, handTarget.colliders);
                //AdvancedHandPhysics.SetKinematic(handTarget.handRigidbody, true);
                //handTarget.colliders = AdvancedHandPhysics.SetKinematic(handTarget.handRigidbody);

            Joint joint = handTarget.hand.bone.transform.GetComponent<Joint>();
            if (joint != null)
                LetGoJoint(handTarget, joint);
            else
                LetGoParenting(handTarget);

            if (handTarget.humanoid.physics)
                handTarget.colliders = AdvancedHandPhysics.SetKinematic(handTarget.handRigidbody);

            if (handTarget.humanoid.dontDestroyOnLoad) {
                // Prevent this object inherites the dontDestroyOnLoad from the humanoid
                Object.DontDestroyOnLoad(handTarget.grabbedObject);
            }

            LetGoGrabbedObject(handTarget);
        }

        private static void LetGoJoint(HandTarget handTarget, Joint joint) {
            Object.DestroyImmediate(joint);
        }

        private static void LetGoParenting(HandTarget handTarget) {
            if (handTarget.grabbedObject.transform.parent == handTarget.hand.bone.transform || handTarget.grabbedObject.transform.parent == handTarget.handPalm)
                handTarget.grabbedObject.transform.parent = null; // originalParent, see InstantVR

            if (handTarget.humanoid.humanoidNetworking != null)
                handTarget.humanoid.humanoidNetworking.ReenableNetworkSync(handTarget.grabbedObject);
//#if hNW_PHOTON
//            PhotonTransformView transformView = handTarget.grabbedObject.GetComponent<PhotonTransformView>();
//            if (transformView != null) {
//                transformView.m_PositionModel.SynchronizeEnabled = true;
//                transformView.m_RotationModel.SynchronizeEnabled = true;
//            }
//#endif
        }

        private static void LetGoGrabbedObject(HandTarget handTarget) {
            //HandMovements.SetAllColliders(handTarget.grabbedObject, true);
            //if (handTarget.humanoid.physics)
            //    //HumanoidTarget.SetColliderToTrigger(handTarget.grabbedObject, false);
            //    HumanoidTarget.UnsetColliderToTrigger(handTarget.colliders);


            if (handTarget.grabbedRBdata == null)
                LetGoStaticObject(handTarget, handTarget.grabbedObject);
            else
                LetGoRigidbody(handTarget);

            HandTarget.TmpDisableCollisions(handTarget, 0.2F);

            if (handTarget.handPhysics != null)
                handTarget.handPhysics.DeterminePhysicsMode(kinematicMass);

#if hNW_UNET
            NetworkTransform nwTransform = handTarget.grabbedObject.GetComponent<NetworkTransform>();
            if (nwTransform != null)
                nwTransform.sendInterval = 1;
#endif

            handTarget.SendMessage("OnLettingGo", null, SendMessageOptions.DontRequireReceiver);
            handTarget.grabbedObject.SendMessage("OnLetGo", null, SendMessageOptions.DontRequireReceiver);

            handTarget.grabbedObject = null;
            handTarget.grabbedRigidbody = false;
        }

        private static void LetGoRigidbody(HandTarget handTarget) {
            //Debug.Log("LetGoRigidbody");
            Rigidbody grabbedRigidbody = handTarget.grabbedObject.GetComponent<Rigidbody>();
            if (!handTarget.grabbedObject.isStatic && grabbedRigidbody == null) {
                grabbedRigidbody = handTarget.grabbedObject.AddComponent<Rigidbody>();
                if (handTarget.grabbedRBdata != null) {
                    handTarget.grabbedRBdata.CopyToRigidbody(grabbedRigidbody);
                    handTarget.grabbedRBdata = null;
                }
            }

            if (grabbedRigidbody != null) {
                if (handTarget.handRigidbody != null)
                    GrabMassRestoration(handTarget.handRigidbody, grabbedRigidbody);

                Joint[] joints = handTarget.grabbedObject.GetComponents<Joint>();
                for (int i = 0; i < joints.Length; i++) {
                    if (joints[i].connectedBody == handTarget.handRigidbody)
                        Object.Destroy(joints[i]);
                }
                grabbedRigidbody.centerOfMass = handTarget.storedCOM;

                if (handTarget.handRigidbody != null) {
                    if (handTarget.handRigidbody.isKinematic) {
                        grabbedRigidbody.velocity = handTarget.hand.target.velocity;
                        Vector3 targetAngularSpeed = handTarget.hand.target.rotationVelocity.eulerAngles;
                        grabbedRigidbody.angularVelocity = targetAngularSpeed * Mathf.Deg2Rad;
                    }
                    else {
                        grabbedRigidbody.velocity = handTarget.handRigidbody.velocity;
                        grabbedRigidbody.angularVelocity = handTarget.handRigidbody.angularVelocity;
                    }
                }

                if (handTarget.grabbedHandle != null) 
                    LetGoHandle(handTarget, handTarget.grabbedHandle);                
            }
            handTarget.grabbedRigidbody = false;
        }

        private static void LetGoStaticObject(HandTarget handTarget, GameObject obj) {
            //Debug.Log("LetGoStaticObject");
            if (handTarget.grabbedHandle != null)
                LetGoHandle(handTarget, handTarget.grabbedHandle);

            Collider c = obj.GetComponentInChildren<Collider>();
            if (c != null) {
                Collider[] handColliders = handTarget.hand.bone.transform.GetComponentsInChildren<Collider>();
                foreach (Collider handCollider in handColliders) {
                    Physics.IgnoreCollision(handCollider, c, false);
                    //Debug.Log("Ignore Collision: " + handCollider.name + " - " + c.name + " FALSE");
                }
            }
        }

        private static void LetGoHandle(HandTarget handTarget, Handle handle) {
            handle.handTarget = null;
            handTarget.grabbedHandle = null;
            if (handTarget.transform.parent == handle.transform)
                handTarget.transform.parent = handTarget.humanoid.transform;

            if (handle.pose != null) {
                handTarget.poseMixer.SetPoseValue(handle.pose, 0);
            }
        }
        #endregion

        public static void GrabOrLetGo(HandTarget handTarget, GameObject obj, bool rangeCheck = true) {
            if (handTarget.grabbedObject != null)
                LetGo(handTarget);
            else
                Grab(handTarget, obj, rangeCheck);
        }
    }
}