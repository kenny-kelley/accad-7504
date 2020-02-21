//#define DEBUG_FORCE
//#define DEBUG_TORQUE
//#define IMPULSE

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer {

    public class BasicHandPhysics : EventTrigger {
        public HandTarget handTarget;

        public virtual void Start() {
            if (!handTarget.physics)
                AdvancedHandPhysics.SetKinematic(handTarget.handRigidbody);
        }

        public virtual void OnCollisionEnter(Collision collision) {
            Rigidbody objRigidbody = collision.rigidbody;
            if (objRigidbody != null)
                HandInteraction.OnTouchStart(handTarget, objRigidbody.gameObject);
            else
                HandInteraction.OnTouchStart(handTarget, collision.gameObject);
        }

        public virtual void OnCollisionExit(Collision collision) {
            Rigidbody objRigidbody = collision.rigidbody;
            if (objRigidbody != null)
                HandInteraction.OnTouchEnd(handTarget, objRigidbody.gameObject);
            else
                HandInteraction.OnTouchEnd(handTarget, collision.gameObject);
        }

        public virtual void OnTriggerEnter(Collider collider) {
            if (collider.isTrigger)
                // We cannot touch trigger colliders
                return;

            Rigidbody objRigidbody = collider.attachedRigidbody;
            if (objRigidbody != null)
                handTarget.touchedObject = objRigidbody.gameObject;
            else
                handTarget.touchedObject = collider.gameObject;

            HandInteraction.OnTouchStart(handTarget, handTarget.touchedObject);
        }

        public virtual void OnTriggerStay(Collider collider) {
            HandInteraction.GrabCheck(handTarget, handTarget.touchedObject);
        }

        public virtual void OnTriggerExit(Collider collider) {
            Rigidbody objRigidbody = collider.attachedRigidbody;
            if (objRigidbody != null)
                HandInteraction.OnTouchEnd(handTarget, objRigidbody.gameObject);
            else
                HandInteraction.OnTouchEnd(handTarget, collider.gameObject);

            handTarget.touchedObject = null;
        }

        // forwarding events to grabbed objects
        // This is needed because grabbed objects can be parented to the hands
        // If we send an event to the hand and we have grabbed on object, we forward the event to the grabbed object
        public override void OnPointerDown(PointerEventData eventata) {
            // disabled because it can result in a cycle in spine
            //if (target != null && target.grabbedObject != null)
            //Debug.Log(target.grabbedObject);
            //ExecuteEvents.ExecuteHierarchy(target.grabbedObject, eventData, ExecuteEvents.pointerDownHandler);
        }
        public override void OnPointerUp(PointerEventData eventData) {
            // disabled because it can result in a cycle in spine
            //if (target != null && target.grabbedObject != null) ;
            //ExecuteEvents.ExecuteHierarchy(target.grabbedObject, eventData, ExecuteEvents.pointerUpHandler);
        }

        [HideInInspector]
        protected Rigidbody handRigidbody;

        protected virtual void Initialize() {
            if (handTarget == null)
                return;

            handRigidbody = GetComponent<Rigidbody>();
        }

        #region Update

        public virtual void FixedUpdate() {            
        }

        public virtual void ManualFixedUpdate(HandTarget _handTarget) {
            handTarget = _handTarget;

            if (handRigidbody == null)
                Initialize();

            if (!handRigidbody.isKinematic)
                UpdateNonKinematicRigidbody();
        }

        protected virtual void UpdateNonKinematicRigidbody() {
            Vector3 torque = CalculateTorque();
            ApplyTorqueAtPosition(torque, handTarget.handPalm.position);

            Vector3 wristTorque = CalculateWristTorque();
            ApplyTorqueAtPosition(wristTorque, handTarget.hand.bone.transform.position);

            Vector3 force = CalculateForce();
            ApplyForceAtPosition(force, handTarget.handPalm.position);

            //if (!hasCollided &&
            //    !handRigidbody.useGravity &&
            //    mode != PhysicsMode.NonKinematic &&
            //    mode != PhysicsMode.ForceLess) {

            //    if (!handRigidbody.isKinematic)
            //        handTarget.colliders = SetKinematic(handRigidbody);
            //}
        }

        #endregion

        #region Force

        protected Vector3 CalculateForce() {
            Vector3 locationDifference = handTarget.stretchlessTarget.position - handRigidbody.position;
            Vector3 force = locationDifference * handTarget.strength;

            //force += CalculateForceDamper();
            return force;
        }

        private const float damping = 30;
        private float lastDistanceTime;
        private Vector3 lastDistanceToTarget;
        private Vector3 CalculateForceDamper() {
            Vector3 distanceToTarget = handTarget.hand.bone.transform.position - handTarget.hand.target.transform.position;

            float deltaTime = Time.fixedTime - lastDistanceTime;

            Vector3 damper = Vector3.zero;
            if (deltaTime < 0.1F) {
                Vector3 velocityTowardsTarget = (distanceToTarget - lastDistanceToTarget) / deltaTime;

                damper = -velocityTowardsTarget * damping;

                //Compensate for absolute rigidbody speed (specifically when on a moving platform)
                Vector3 residualVelocity = handRigidbody.velocity - velocityTowardsTarget;
                damper += residualVelocity * 10;
            }
            lastDistanceToTarget = distanceToTarget;
            lastDistanceTime = Time.fixedTime;

            return damper;
        }

        protected void ApplyForce(Vector3 force) {
            if (float.IsNaN(force.magnitude))
                return;

            /*
            if (contactPoint.sqrMagnitude > 0) {
                // The contact point is OK, but the force here is not OK, because this is the force from the hand
                // The force needs to be projected on the contactPoint !
                //handRigidbody.AddForceAtPosition(force, contactPoint);
                //#if DEBUG_FORCE
                Debug.DrawRay(contactPoint, force / 10, Color.yellow);
                //#endif
            }
            else {
                // The contact point is OK, but the force here is not OK, because this is the force from the hand
                // The force needs to be projected on the contactPoint !
                //handRigidbody.AddForceAtPosition(force, target.handPalm.position);
                handRigidbody.AddForce(force);
                //#if DEBUG_FORCE
                Debug.DrawRay(target.handPalm.position, force / 10, Color.yellow);
                //#endif
            }
            */
            handRigidbody.AddForce(force);
#if DEBUG_FORCE
            Debug.DrawRay(handRigidbody.position, force / 10, Color.yellow);
#endif
        }

        protected void ApplyForceAtPosition(Vector3 force, Vector3 position) {
            if (float.IsNaN(force.magnitude))
                return;

            handRigidbody.AddForceAtPosition(force, position);
#if DEBUG_FORCE
            Debug.DrawRay(position, force / 10, Color.yellow);
#endif
        }
        #endregion

        #region Torque

        protected  Vector3 CalculateTorque() {
            Quaternion sollRotation = handTarget.hand.target.transform.rotation * handTarget.hand.target.toBoneRotation;
            Quaternion istRotation = handTarget.hand.bone.transform.rotation;
            Quaternion dRot = sollRotation * Quaternion.Inverse(istRotation);

            float angle;
            Vector3 axis;
            dRot.ToAngleAxis(out angle, out axis);
            angle = UnityAngles.Normalize(angle);

            Vector3 angleDifference = axis.normalized * (angle * Mathf.Deg2Rad);
            Vector3 torque = angleDifference * handTarget.strength * 0.1F;
            return torque;
        }

        protected Vector3 CalculateWristTorque() {
            //Vector3 wristTension = target.GetWristTension();

            // Not stable
            //Vector3 forces = new Vector3(-(wristTension.x * wristTension.x * 10), -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);
            //Vector3 forces = new Vector3(0, -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);

            Vector3 torque = Vector3.zero; // (0, 0, -wristTension.z * wristTension.z * 10);
            return torque;
        }

        private void ApplyTorque(Vector3 torque) {
            //AddTorqueAtPosition(torque, target.handPalm.position);
            ApplyTorqueAtPosition(torque, handTarget.hand.bone.transform.position);
        }

        protected void ApplyTorqueAtPosition(Vector3 torque, Vector3 posToApply) {
            if (float.IsNaN(torque.magnitude))
                return;

            Vector3 torqueAxis = torque.normalized;
            Vector3 ortho = new Vector3(1, 0, 0);

            // prevent torqueAxis and ortho from pointing in the same direction
            if (((torqueAxis - ortho).sqrMagnitude < Mathf.Epsilon) || ((torqueAxis + ortho).sqrMagnitude < Mathf.Epsilon)) {
                ortho = new Vector3(0, 1, 0);
            }

            ortho = Vector3OrthoNormalize(torqueAxis, ortho);
            // calculate force 
            Vector3 force = Vector3.Cross(0.5f * torque, ortho);

            handRigidbody.AddForceAtPosition(force, posToApply + ortho);
            handRigidbody.AddForceAtPosition(-force, posToApply - ortho);

#if DEBUG_TORQUE
            UnityEngine.Debug.DrawRay(posToApply + ortho / 20, force / 10, Color.yellow);
            UnityEngine.Debug.DrawLine(posToApply + ortho / 20, posToApply - ortho / 20, Color.yellow);
            UnityEngine.Debug.DrawRay(posToApply - ortho / 20, -force / 10, Color.yellow);
#endif
        }

        private Vector3 Vector3OrthoNormalize(Vector3 a, Vector3 b) {
            Vector3 tmp = Vector3.Cross(a.normalized, b).normalized;
            return tmp;
        }
        #endregion

    }

    public class AdvancedHandPhysics : BasicHandPhysics {

        public enum PhysicsMode {
            Kinematic,
            NonKinematic,
            HybridKinematic,
            ForceLess,
        }
        public PhysicsMode mode = PhysicsMode.HybridKinematic;


        private bool colliding;
        public bool hasCollided = false;
        public Vector3 contactPoint;

        public Vector3 force;
        public Vector3 torque;

        protected override void Initialize() {
            if (handTarget == null)
                return;

            if (enabled) {
                handRigidbody = GetComponent<Rigidbody>();
                if (handRigidbody != null) {
                    //Kinematize(handRigidbody, mode);
                    if (handRigidbody != null) {
                        if (handRigidbody.useGravity || mode == PhysicsMode.NonKinematic)
                            //SetKinematic(handRigidbody, false);
                            SetNonKinematic(handRigidbody, handTarget.colliders);
                        else
                            //SetKinematic(rigidbody, true);
                            handTarget.colliders = SetKinematic(handRigidbody);
                    }
                    handRigidbody.maxAngularVelocity = 20;
                }
            }
        }

        #region Update
        public override void FixedUpdate() {
            CalculateVelocity();
        }

        public override void ManualFixedUpdate(HandTarget _handTarget) {
            handTarget = _handTarget;

            if (hasCollided && !colliding) {
                HandInteraction.OnTouchEnd(handTarget, handTarget.touchedObject);
                handTarget.touchedObject = null;
            }

            if (handTarget.touchedObject == null) { // Object may be destroyed
                hasCollided = false;
            }

            if (handRigidbody == null)
                Initialize();

            // Check for stuck hands. Only when hand is kinematic you can pull the hand loose
            // it will then turn into a kinematic hand, which results in snapping the hand back
            // onto the forearm.
            if (handTarget.forearm.bone.transform != null && !handRigidbody.isKinematic) {
                float distance = Vector3.Distance(handTarget.hand.bone.transform.position, handTarget.forearm.bone.transform.position) - handTarget.forearm.bone.length;
                if (distance > 0.05F) {
                    //SetKinematic(handRigidbody, true);
                    handTarget.colliders = SetKinematic(handRigidbody);
                }
            }

            UpdateRigidbody();

            colliding = false;
        }

        public void UpdateRigidbody() {
            if (handRigidbody == null)
                return;

            if ((mode == PhysicsMode.NonKinematic || mode == PhysicsMode.ForceLess) && handRigidbody.isKinematic)
                //SetKinematic(handRigidbody, false);
                SetNonKinematic(handRigidbody, handTarget.colliders);

            Quaternion targetRotation = handTarget.transform.rotation;

            Quaternion rot = Quaternion.Inverse(handRigidbody.rotation) * targetRotation;
            float angle;
            Vector3 axis;
            rot.ToAngleAxis(out angle, out axis);

            if (handRigidbody.isKinematic)
                UpdateKinematicRigidbody();
            else
                UpdateNonKinematicRigidbody();
        }

        private void UpdateKinematicRigidbody() {
            force = Vector3.zero;
            torque = Vector3.zero;
        }

        protected override void UpdateNonKinematicRigidbody() {
            if (mode != PhysicsMode.ForceLess) {
                torque = CalculateTorque();
                ApplyTorqueAtPosition(torque, handTarget.handPalm.position);

                Vector3 wristTorque = CalculateWristTorque();
                ApplyTorqueAtPosition(wristTorque, handTarget.hand.bone.transform.position);

                force = CalculateForce();
                ApplyForceAtPosition(force, handTarget.handPalm.position);

                if (handTarget.humanoid.haptics)
                    handTarget.Vibrate(force.magnitude / 25);
            }
            else {
                force = Vector3.zero;
                torque = Vector3.zero;
            }

            if (!hasCollided &&
                !handRigidbody.useGravity &&
                mode != PhysicsMode.NonKinematic &&
                mode != PhysicsMode.ForceLess) {

                if (!handRigidbody.isKinematic)
                    handTarget.colliders = SetKinematic(handRigidbody);
            }
        }
        #endregion Update

        #region Events

        public override void OnTriggerEnter(Collider collider) {
            bool otherHasKinematicPhysics = false;
            bool otherIsHumanoid = false;

            Rigidbody otherRigidbody = collider.attachedRigidbody;
            if (otherRigidbody != null) {
                AdvancedHandPhysics kp = otherRigidbody.GetComponent<AdvancedHandPhysics>();
                otherHasKinematicPhysics = (kp != null);
                HumanoidControl humanoid = otherRigidbody.GetComponent<HumanoidControl>();
                otherIsHumanoid = (humanoid != null);
            }

            if (handRigidbody != null &&
                handRigidbody.isKinematic &&
                (!collider.isTrigger || otherHasKinematicPhysics) &&
                !otherIsHumanoid) {

                colliding = true;
                hasCollided = true;
                if (otherRigidbody != null) {
                    handTarget.touchedObject = otherRigidbody.gameObject;
                        SetNonKinematic(handRigidbody, handTarget.colliders);
                }
                else {
                    handTarget.touchedObject = collider.gameObject;
                    SetNonKinematic(handRigidbody, handTarget.colliders);
                }

                ProcessFirstCollision(handRigidbody, collider);
            }

            if (hasCollided) {
                Rigidbody objRigidbody = collider.attachedRigidbody;
                if (objRigidbody != null) {
                    HandInteraction.GrabCheck(handTarget, objRigidbody.gameObject);
                }
                else
                    HandInteraction.GrabCheck(handTarget, collider.gameObject);
            }
        }

        public override void OnTriggerExit(Collider collider) {            
        }

        public override void OnCollisionEnter(Collision collision) {
            colliding = true;
            if (collision.contacts.Length > 0)
                contactPoint = collision.contacts[0].point;
            base.OnCollisionEnter(collision);
        }

        public void OnCollisionStay(Collision collision) {
            colliding = true;
            if (collision.contacts.Length > 0)
                contactPoint = collision.contacts[0].point;
        }

        public override void OnCollisionExit(Collision collision) {
            if (handRigidbody != null && !handRigidbody.useGravity) {
                RaycastHit hit;
                if (handRigidbody.SweepTest(handTarget.transform.position - handRigidbody.position, out hit)) {
                    ;
                }
                else {
                    hasCollided = false;
                    contactPoint = Vector3.zero;
                    handTarget.touchedObject = null;
                }
            }
        }

        #endregion

        public void DeterminePhysicsMode(float kinematicMass = 1) {
            mode = DeterminePhysicsMode(handRigidbody, kinematicMass);
        }

        public static PhysicsMode DeterminePhysicsMode(Rigidbody rigidbody, float kinematicMass = 1) {
            if (rigidbody == null)
                return PhysicsMode.Kinematic;

            PhysicsMode physicsMode;
            if (rigidbody.useGravity) {
                physicsMode = PhysicsMode.NonKinematic;
            }
            else {
                float mass = CalculateTotalMass(rigidbody);
                if (mass > kinematicMass)
                    physicsMode = PhysicsMode.NonKinematic;
                else
                    physicsMode = PhysicsMode.HybridKinematic;
            }
            return physicsMode;
        }

        public static float CalculateTotalMass(Rigidbody rigidbody) {
            if (rigidbody == null)
                return 0;

            float mass = rigidbody.gameObject.isStatic ? Mathf.Infinity : rigidbody.mass;
            Joint[] joints = rigidbody.GetComponents<Joint>();
            for (int i = 0; i < joints.Length; i++) {
                // Seems to result in cycle in spine in some cases
                //if (joints[i].connectedBody != null)
                //    mass += CalculateTotalMass(joints[i].connectedBody);
                //else
                mass = Mathf.Infinity;
            }
            return mass;
        }

        public Vector3 boneVelocity;
        private Vector3 lastPosition = Vector3.zero;
        private void CalculateVelocity() {
            if (lastPosition != Vector3.zero) {
                boneVelocity = (handTarget.hand.bone.transform.position - lastPosition) / Time.fixedDeltaTime;
            }
            lastPosition = handTarget.hand.bone.transform.position;
        }
/*
        #region Force
        
        private Vector3 CalculateForce() {
            Vector3 locationDifference = handTarget.stretchlessTarget.position - handRigidbody.position;
            Vector3 force = locationDifference * handTarget.strength;

            //force += CalculateForceDamper();
            return force;
        }

        private const float damping = 30;
        private float lastDistanceTime;
        private Vector3 lastDistanceToTarget;
        private Vector3 CalculateForceDamper() {
            Vector3 distanceToTarget = handTarget.hand.bone.transform.position - handTarget.hand.target.transform.position;

            float deltaTime = Time.fixedTime - lastDistanceTime;

            Vector3 damper = Vector3.zero;
            if (deltaTime < 0.1F) {
                Vector3 velocityTowardsTarget = (distanceToTarget - lastDistanceToTarget) / deltaTime;

                damper = -velocityTowardsTarget * damping;

                //Compensate for absolute rigidbody speed (specifically when on a moving platform)
                Vector3 residualVelocity = handRigidbody.velocity - velocityTowardsTarget;
                damper += residualVelocity * 10;
            }
            lastDistanceToTarget = distanceToTarget;
            lastDistanceTime = Time.fixedTime;

            return damper;
        }

        private void ApplyForce(Vector3 force) {
            if (float.IsNaN(force.magnitude))
                return;

            
            //if (contactPoint.sqrMagnitude > 0) {
            //    // The contact point is OK, but the force here is not OK, because this is the force from the hand
            //    // The force needs to be projected on the contactPoint !
            //    //handRigidbody.AddForceAtPosition(force, contactPoint);
            //    //#if DEBUG_FORCE
            //    Debug.DrawRay(contactPoint, force / 10, Color.yellow);
            //    //#endif
            //}
            //else {
            //    // The contact point is OK, but the force here is not OK, because this is the force from the hand
            //    // The force needs to be projected on the contactPoint !
            //    //handRigidbody.AddForceAtPosition(force, target.handPalm.position);
            //    handRigidbody.AddForce(force);
            //    //#if DEBUG_FORCE
            //    Debug.DrawRay(target.handPalm.position, force / 10, Color.yellow);
            //    //#endif
            //}
            
            handRigidbody.AddForce(force);
#if DEBUG_FORCE
            Debug.DrawRay(handRigidbody.position, force / 10, Color.yellow);
#endif
        }

        private void ApplyForceAtPosition(Vector3 force, Vector3 position) {
            if (float.IsNaN(force.magnitude))
                return;

            handRigidbody.AddForceAtPosition(force, position);
#if DEBUG_FORCE
            Debug.DrawRay(position, force / 10, Color.yellow);
#endif
        }
        #endregion

        #region Torque
        private Vector3 CalculateTorque() {
            Quaternion sollRotation = handTarget.hand.target.transform.rotation * handTarget.hand.target.toBoneRotation;
            Quaternion istRotation = handTarget.hand.bone.transform.rotation;
            Quaternion dRot = sollRotation * Quaternion.Inverse(istRotation);

            float angle;
            Vector3 axis;
            dRot.ToAngleAxis(out angle, out axis);
            angle = UnityAngles.Normalize(angle);

            Vector3 angleDifference = axis.normalized * (angle * Mathf.Deg2Rad);
            Vector3 torque = angleDifference * handTarget.strength * 0.1F;
            return torque;
        }

        private Vector3 CalculateWristTorque() {
            //Vector3 wristTension = target.GetWristTension();

            // Not stable
            //Vector3 forces = new Vector3(-(wristTension.x * wristTension.x * 10), -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);
            //Vector3 forces = new Vector3(0, -(wristTension.y * wristTension.y * 10), -wristTension.z * wristTension.z * 10);

            Vector3 torque = Vector3.zero; // (0, 0, -wristTension.z * wristTension.z * 10);
            return torque;
        }

        private void ApplyTorque(Vector3 torque) {
            //AddTorqueAtPosition(torque, target.handPalm.position);
            ApplyTorqueAtPosition(torque, handTarget.hand.bone.transform.position);
        }

        private void ApplyTorqueAtPosition(Vector3 torque, Vector3 posToApply) {
            if (float.IsNaN(torque.magnitude))
                return;

            Vector3 torqueAxis = torque.normalized;
            Vector3 ortho = new Vector3(1, 0, 0);

            // prevent torqueAxis and ortho from pointing in the same direction
            if (((torqueAxis - ortho).sqrMagnitude < Mathf.Epsilon) || ((torqueAxis + ortho).sqrMagnitude < Mathf.Epsilon)) {
                ortho = new Vector3(0, 1, 0);
            }

            ortho = Vector3OrthoNormalize(torqueAxis, ortho);
            // calculate force 
            Vector3 force = Vector3.Cross(0.5f * torque, ortho);

            handRigidbody.AddForceAtPosition(force, posToApply + ortho);
            handRigidbody.AddForceAtPosition(-force, posToApply - ortho);

#if DEBUG_TORQUE
            UnityEngine.Debug.DrawRay(posToApply + ortho / 20, force / 10, Color.yellow);
            UnityEngine.Debug.DrawLine(posToApply + ortho / 20, posToApply - ortho / 20, Color.yellow);
            UnityEngine.Debug.DrawRay(posToApply - ortho / 20, -force / 10, Color.yellow);
#endif
        }

        private Vector3 Vector3OrthoNormalize(Vector3 a, Vector3 b) {
            Vector3 tmp = Vector3.Cross(a.normalized, b).normalized;
            return tmp;
        }
        #endregion
*/
        public void ProcessFirstCollision(Rigidbody rigidbody, Collider otherCollider) {

#if IMPULSE
		CalculateCollisionImpuls(rigidbody, otherRigidbody, collisionPoint);
#endif
        }

#if IMPULSE
	private static void CalculateCollisionImpuls(Rigidbody rigidbody, Rigidbody otherRigidbody, Vector3 collisionPoint) {
		if (otherRigidbody != null) {
			Vector3 myImpuls = (rigidbody.mass / 10) * rigidbody.velocity;
			otherRigidbody.AddForceAtPosition(myImpuls, collisionPoint, ForceMode.Impulse);
		}
	}
#endif

        public static void SetNonKinematic(Rigidbody rigidbody, List<Collider> colliders) {
            if (rigidbody == null)
                return;

            //Debug.Log("SetNonKinematic " + rigidbody.name);
            GameObject obj = rigidbody.gameObject;
            if (obj.isStatic == false) {
                rigidbody.isKinematic = false;
                HumanoidTarget.UnsetColliderToTrigger(colliders);
            }
        }

        public static List<Collider> SetKinematic(Rigidbody rigidbody) {
            if (rigidbody == null)
                return null;

            //Debug.Log("SetKinematic " + rigidbody.name + " " + rigidbody.isKinematic);
            GameObject obj = rigidbody.gameObject;
            if (obj.isStatic == false) {
                rigidbody.isKinematic = true;
                return HumanoidTarget.SetColliderToTrigger(obj);
            }
            return null;
        }
    }
}