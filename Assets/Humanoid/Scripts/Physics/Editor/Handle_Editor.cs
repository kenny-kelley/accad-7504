using UnityEditor;
using UnityEngine;

namespace Passer {

    [CustomEditor(typeof(Handle))]
    public class Handle_Editor : Editor {
        protected Handle handle;

        protected SerializedProperty handlePositionProp;
        protected SerializedProperty handleRotationProp;
        protected SerializedProperty handleHandProp;
        protected SerializedProperty grabTypeProp;
        protected SerializedProperty rangeProp;
        protected SerializedProperty poseProp;

        public void OnEnable() {
            handle = (Handle)target;

            handlePositionProp = serializedObject.FindProperty("position");
            handleRotationProp = serializedObject.FindProperty("rotation");
            handleHandProp = serializedObject.FindProperty("hand");
            grabTypeProp = serializedObject.FindProperty("grabType");
            rangeProp = serializedObject.FindProperty("range");
            poseProp = serializedObject.FindProperty("pose");

            InitHandPoses(handle);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            handlePositionProp.vector3Value = EditorGUILayout.Vector3Field("Position", handle.position);
            handleRotationProp.vector3Value = EditorGUILayout.Vector3Field("Rotation", handle.rotation);
            handleHandProp.intValue = (int)(Handle.Hand)EditorGUILayout.EnumPopup("Hand", handle.hand);
            grabTypeProp.intValue = (int)(Handle.GrabType)EditorGUILayout.EnumPopup("Grab type", handle.grabType);
            rangeProp.floatValue = EditorGUILayout.FloatField("Range", handle.range);

            HandPoseInspector(handle);
            CheckHandTarget(handle);
            SceneView.RepaintAll();

            serializedObject.ApplyModifiedProperties();
        }


        private string[] handPoseNames;
        private void InitHandPoses(Handle handle) {
        }

        private bool showHandPoseInspector;
        private void HandPoseInspector(Handle handle) {
            poseProp.objectReferenceValue = (Humanoid.Pose)EditorGUILayout.ObjectField("Hand Pose", handle.pose, typeof(Humanoid.Pose), false);
#if hNEARHANDLE
            EditorGUILayout.BeginHorizontal();
            SphereCollider collider = handle.gameObject.GetComponent<SphereCollider>();
            bool useNearPose = EditorGUILayout.ToggleLeft("Near Pose", handle.useNearPose);
            if (useNearPose && !handle.useNearPose)
                AddNearTrigger(handle, collider);
            else if (!useNearPose && handle.useNearPose)
                RemoveNearTrigger(handle, collider);

            handle.useNearPose = useNearPose;
            if (handle.useNearPose) {
                EditorGUI.indentLevel--;
                handle.nearPose = EditorGUILayout.Popup(handle.nearPose, handPoseNames);
                collider.radius = handle.range;
                EditorGUI.indentLevel++;
            }
            EditorGUILayout.EndHorizontal();
#endif
        }

        private void AddNearTrigger(Handle handle, SphereCollider collider) {
            if (collider == null || !collider.isTrigger) {
                collider = handle.gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = handle.range;
            }
        }

        private void RemoveNearTrigger(Handle handle, SphereCollider collider) {
            if (collider != null && collider.isTrigger) {
                DestroyImmediate(collider, true);
            }
        }

        public static void CheckHandTarget(Handle handle) {
            HandTarget handTarget = (HandTarget)EditorGUILayout.ObjectField("Hand Target", handle.handTarget, typeof(HandTarget), true);
            if (handTarget != handle.handTarget) {
                if (handTarget != null) {
                    if (handle.handTarget != null)
                        HandInteraction.LetGo(handle.handTarget);
                    if (handTarget.grabbedObject != null)
                        HandInteraction.LetGo(handTarget);

                    HandInteraction.MoveAndGrabHandle(handTarget, handle);
                    handTarget.transform.parent = handle.transform;
                }
                else {
                    HandInteraction.LetGo(handle.handTarget);
                }
            }
        }

        #region Scene
        public void OnSceneGUI() {
            Handle handle = (Handle)target;

            if (handle.handTarget == null)
                return;

            if (!Application.isPlaying) {
                handle.handTarget.poseMixer.ShowPose(handle.handTarget.humanoid, handle.handTarget.side);
                HandInteraction.MoveHandTargetToHandle(handle.handTarget, handle);

                ArmMovements.Update(handle.handTarget);
                FingerMovements.Update(handle.handTarget);
                handle.handTarget.MatchTargetsToAvatar();
            }
        }
        #endregion
    }
}