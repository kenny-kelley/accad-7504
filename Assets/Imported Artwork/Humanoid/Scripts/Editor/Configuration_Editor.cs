using System.IO;
using UnityEditor;
using UnityEngine;
#if !UNITY_2019_1_OR_NEWER
using UnityEngine.Networking;
#endif
#if hPHOTON2
using Photon.Pun;
#endif

namespace Passer.Humanoid {

    //[InitializeOnLoad]
    //public class LoadPreferences {
    //    static LoadPreferences() {
    //        Debug.Log("StartupPreferences");
    //        string humanoidPath = Configuration_Editor.FindHumanoidFolder();
    //        string configurationString = EditorPrefs.GetString("HumanoidConfigurationKey");
    //        Debug.Log("I " + configurationString);

    //        string[] foundAssets = AssetDatabase.FindAssets(configurationString + " t:Configuration");
    //        if (foundAssets.Length == 0)
    //            return;

    //        string path = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
    //        Debug.Log(path);
    //        Configuration configuration = (Configuration)AssetDatabase.LoadAssetAtPath(path, typeof(Configuration));

    //        Debug.Log("I- " + configuration.name);
    //        Debug.Log("I- " + configuration.steamVRSupport);
    //        Debug.Log("I- " + configuration.viveTrackerSupport);
    //        Debug.Log("I- " + configuration.networkingSupport);
    //        Configuration_Editor.CheckExtensions(configuration);
    //    }
    //}

    [InitializeOnLoad]
    public class ConfigurationCheck {
        static ConfigurationCheck() {
            CheckXrSdks();
        }

        public static void CheckXrSdks() {
            Configuration_Editor.FindHumanoidFolder();
            //string configurationString = EditorPrefs.GetString("HumanoidConfigurationKey", "DefaultConfiguration");

            Configuration configuration = Configuration_Editor.GetConfiguration();

            bool anyChanged = false;

#if (UNITY_STANDALONE_WIN || UNITY_ANDROID)
            bool oculusSupported = Oculus_Editor.OculusSupported();
            if (oculusSupported && !configuration.oculusSupport) {
                configuration.oculusSupport = true;
                anyChanged = true;
            }
#if !hOCULUS
            if (oculusSupported)
                anyChanged = true;
#endif
#endif

#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX)
            bool steamVrSupported = SteamVR_Editor.SteamVRSupported();
            if (steamVrSupported && !configuration.steamVRSupport && !configuration.openVRSupport) {
                configuration.openVRSupport = true;
                anyChanged = true;
            }
#if !hSTEAMVR && !hOPENVR
            if (steamVrSupported)
                anyChanged = true;
#endif
#endif

#if (UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0)
            bool windowsMrSupported = WindowsMR_Editor.MixedRealitySupported();
            if (windowsMrSupported && !configuration.windowsMRSupport) {
                configuration.windowsMRSupport = true;
                anyChanged = true;
            }
#if !hWINDOWSMR
            if (windowsMrSupported)
                anyChanged = true;
#endif
#endif

            if (anyChanged)
                Configuration_Editor.CheckExtensions(configuration);
        }
    }

    [CustomEditor(typeof(Configuration))]
    public class Configuration_Editor : Editor {
        private Configuration configuration;

        private static string humanoidPath;

        private const string vivetrackerPath = "Extensions/ViveTrackers/ViveTracker.cs";
        private const string steamVRPath = "Extensions/SteamVR/SteamVR.cs";
        private const string openVRPath = "Extensions/OpenVR/OpenVR.cs";
        private const string oculusPath = "Extensions/Oculus/Oculus.cs";
        private const string windowsMRPath = "Extensions/WindowsMR/WindowsMR.cs";
        private const string vrtkPath = "Extensions/VRTK/Vrtk.cs";
        private const string neuronPath = "Extensions/PerceptionNeuron/PerceptionNeuron.cs";
        private const string realsensePath = "Extensions/IntelRealsense/IntelRealsense.cs";
        private const string leapPath = "Extensions/LeapMotion/LeapMotion.cs";
        private const string kinect1Path = "Extensions/MicrosoftKinect1/MicrosoftKinect1.cs";
        private const string kinect2Path = "Extensions/MicrosoftKinect2/MicrosoftKinect2.cs";
        private const string astraPath = "Extensions/OrbbecAstra/OrbbecAstra.cs";
        private const string hydraPath = "Extensions/RazerHydra/RazerHydra.cs";
        private const string tobiiPath = "Extensions/Tobii/Tobii.cs";
        private const string optitrackPath = "Extensions/OptiTrack/OptiTrack.cs";
        private const string pupilPath = "Extensions/Pupil/PupilTracker.cs";

        private const string facePath = "FaceControl/EyeTarget.cs";

        public static Configuration CreateDefaultConfiguration() {
            Configuration configuration;

            Debug.Log("Created new Default Configuration");
            // Create new Default Configuration
            configuration = CreateInstance<Configuration>();
            configuration.oculusSupport = true;
            configuration.openVRSupport = true;
            configuration.windowsMRSupport = true;

            string path = humanoidPath.Substring(0, humanoidPath.Length - 1); // strip last /
            path = path.Substring(0, path.LastIndexOf('/') + 1); // strip Scripts;
            path = "Assets" + path + "DefaultConfiguration.asset";
            AssetDatabase.CreateAsset(configuration, path);
            AssetDatabase.SaveAssets();
            return configuration;
        }

        public static Configuration GetConfiguration() {
            string humanoidPath = FindHumanoidFolder();
            humanoidPath = humanoidPath.Substring(0, humanoidPath.Length - 1); // strip last /
            humanoidPath = humanoidPath.Substring(0, humanoidPath.LastIndexOf('/') + 1); // strip Scripts;

            string configurationString = EditorPrefs.GetString("HumanoidConfigurationKey", "DefaultConfiguration");
#if UNITY_2018_3_OR_NEWER
            var settings = AssetDatabase.LoadAssetAtPath<HumanoidSettings>("Assets" + humanoidPath + "HumanoidSettings.asset");
            if (settings == null) {
                settings = CreateInstance<HumanoidSettings>();

                AssetDatabase.CreateAsset(settings, HumanoidSettings.settingsPath);
                AssetDatabase.SaveAssets();
            }

            SerializedObject serializedSettings = new SerializedObject(settings);
            SerializedProperty configurationProp = serializedSettings.FindProperty("configuration");
            if (settings.configuration == null) {
#endif
            Configuration configuration = LoadConfiguration(configurationString);
            if (configuration == null) {
                configurationString = "DefaultConfiguration";
                LoadConfiguration(configurationString);
                if (configuration == null) {
                    Debug.Log("Created new Default Configuration");
                    // Create new Default Configuration
                    configuration = CreateInstance<Configuration>();
                    configuration.oculusSupport = true;
                    configuration.openVRSupport = true;
                    configuration.windowsMRSupport = true;
                    string path = "Assets" + humanoidPath + configurationString + ".asset";
                    AssetDatabase.CreateAsset(configuration, path);
                    AssetDatabase.SaveAssets();
                }
            }
#if UNITY_2018_3_OR_NEWER
                configurationProp.objectReferenceValue = configuration;
            }
            serializedSettings.ApplyModifiedProperties();
            return (Configuration)configurationProp.objectReferenceValue;
#else
            
            return configuration;
#endif
        }

#region Enable 
        public void OnEnable() {
            configuration = (Configuration)target;

            humanoidPath = FindHumanoidFolder();
        }

        public static string FindHumanoidFolder() {
            // Path is correct
            if (IsFileAvailable("HumanoidControl.cs"))
                return humanoidPath;

            // Determine in which (sub)folder HUmanoid Control has been placed
            // This makes it possible to place Humanoid Control is a different folder
            string[] hcScripts = AssetDatabase.FindAssets("HumanoidControl");
            for (int i = 0; i < hcScripts.Length; i++) {
                string assetPath = AssetDatabase.GUIDToAssetPath(hcScripts[i]);
                if (assetPath.Length > 36 && assetPath.Substring(assetPath.Length - 27, 27) == "/Scripts/HumanoidControl.cs") {
                    humanoidPath = assetPath.Substring(6, assetPath.Length - 24);
                    return humanoidPath;
                }
            }

            // Defaulting to standard folder
            humanoidPath = "/Humanoid/Scripts/";
            return humanoidPath;
        }
#endregion

        public override void OnInspectorGUI() {
            serializedObject.Update();

            bool anyChanged = ConfigurationGUI(configuration, serializedObject);
            if (GUI.changed || anyChanged) {
                EditorUtility.SetDirty(configuration);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static bool ConfigurationGUI(Configuration configuration, SerializedObject serializedConfiguration) {
            bool anyChanged = false;

            //anyChanged |= SteamVRSettingUI(configuration);
            anyChanged |= SteamVrConfigurationGUI(configuration);
            anyChanged |= OculusSettingUI(configuration);
            anyChanged |= WindowsMRSettingUI(configuration);
            anyChanged |= VrtkSettingUI(configuration);

            anyChanged |= LeapSettingUI(configuration);
            anyChanged |= Kinect1SettingUI(configuration);
            anyChanged |= Kinect2SettingUI(configuration);
            anyChanged |= AstraSettingUI(configuration);

            anyChanged |= RealsenseSettingUI(configuration);
            anyChanged |= HydraSettingUI(configuration);
            anyChanged |= TobiiSettingUI(configuration);
            anyChanged |= PupilSettingUI(configuration);
            anyChanged |= NeuronSettingUI(configuration);
            anyChanged |= OptitrackSettingUI(configuration);

            anyChanged |= NetworkingSettingUI(configuration, serializedConfiguration);

            return anyChanged;
        }

#region SettingUI

        public static bool SteamVRSettingUI(Configuration configuration) {
            bool anyChanged = false;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            bool steamVrSupported = SteamVR_Editor.SteamVRSupported();
            if (steamVrSupported) {
                configuration.steamVRSupport = isSteamVrSupportAvailable();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("SteamVR Support", configuration.steamVRSupport);
                EditorGUI.EndDisabledGroup();
            }
            else if (!isSteamVrSupportAvailable())
                configuration.steamVRSupport = false;
            else
                configuration.steamVRSupport = EditorGUILayout.Toggle("SteamVR Support", configuration.steamVRSupport);

            ViveTrackerSettingUI(configuration);
#else
            if (configuration.steamVRSupport | configuration.viveTrackerSupport)
                anyChanged = true;
            configuration.steamVRSupport = false;
            configuration.viveTrackerSupport = false;
#endif
            return anyChanged;
        }

        public static bool ViveTrackerSettingUI(Configuration configuration) {
            EditorGUI.BeginDisabledGroup(!configuration.steamVRSupport);
            configuration.viveTrackerSupport = isViveTrackerSupportAvailable() && EditorGUILayout.Toggle("Vive Tracker Support", configuration.viveTrackerSupport);
            EditorGUI.EndDisabledGroup();
            return false;
        }

        public static bool OculusSettingUI(Configuration configuration) {
            bool anyChanged = false;

#if UNITY_STANDALONE_WIN || UNITY_ANDROID
            bool oculusSupported = Oculus_Editor.OculusSupported();
            if (oculusSupported) {
                configuration.oculusSupport = isOculusSupportAvailable();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Oculus Support", configuration.oculusSupport);
                EditorGUI.EndDisabledGroup();
            }
            else if (!isOculusSupportAvailable())
                configuration.oculusSupport = false;
            else
                configuration.oculusSupport = EditorGUILayout.Toggle("Oculus Support", configuration.oculusSupport);
#else
            if (configuration.oculusSupport)
                anyChanged = true;
            configuration.oculusSupport = false;
#endif
            return anyChanged;
        }

        public static bool WindowsMRSettingUI(Configuration configuration) {
            bool anyChanged = false;

#if UNITY_WSA_10_0
            bool windowsMrSupported = WindowsMR_Editor.MixedRealitySupported();
            if (windowsMrSupported) {
                configuration.windowsMRSupport = isWindowsMrAvailable();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Windows MR Support", configuration.windowsMRSupport);
                EditorGUI.EndDisabledGroup();
            }
            else if (!isWindowsMrAvailable())
                configuration.windowsMRSupport = false;
            else
                configuration.windowsMRSupport = EditorGUILayout.Toggle("Windows MR Support", configuration.windowsMRSupport);
#else
            if (configuration.windowsMRSupport)
                anyChanged = true;
            configuration.windowsMRSupport = false;
#endif
            return anyChanged;
        }

        public static bool VrtkSettingUI(Configuration configuration) {
            if (!isVrtkSupportAvailable())
                configuration.vrtkSupport = false;

            else if (!isVrtkAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("VRTK Supoort", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("VRTK is not available. Please download it from the Asset Store.", MessageType.Warning, true);
            }
            else
                configuration.vrtkSupport = EditorGUILayout.Toggle("VRTK Support", configuration.vrtkSupport);
            return false;
        }

        public static bool LeapSettingUI(Configuration configuration) {
            bool oldLeapSupport = configuration.leapSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isLeapSupportAvailable())
                configuration.leapSupport = false;

            else if (!isLeapAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                configuration.leapSupport = EditorGUILayout.Toggle("Leap Motion Support", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Leap Motion Core Assets are not available. Please download the Core Assets using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Leap Motion Unity Core Assets"))
                    Application.OpenURL("https://developer.leapmotion.com/unity");
            }

            else
                configuration.leapSupport = EditorGUILayout.Toggle("Leap Motion Support", configuration.leapSupport);
#else
            configuration.leapSupport = false;
#endif
            return (configuration.leapSupport != oldLeapSupport);
        }

        public static bool Kinect1SettingUI(Configuration configuration) {
            bool oldKinectSupport = configuration.kinect1Support;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isKinect1SupportAvailable())
                configuration.kinect1Support = false;
            else
                configuration.kinect1Support = EditorGUILayout.Toggle("Kinect 1 Support", configuration.kinect1Support);
#else
            configuration.kinect1Support = false;
#endif
            return (configuration.kinect1Support != oldKinectSupport);
        }

        public static bool Kinect2SettingUI(Configuration configuration) {
            bool oldKinectSupport = configuration.kinectSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isKinect2SupportAvailable())
                configuration.kinectSupport = false;
            else
                configuration.kinectSupport = EditorGUILayout.Toggle("Kinect 2 Support", configuration.kinectSupport);
#else
            configuration.kinectSupport = false;
#endif
            return (configuration.kinectSupport != oldKinectSupport);
        }

        public static bool AstraSettingUI(Configuration configuration) {
            bool oldAstraSupport = configuration.astraSupport;
#if UNITY_STANDALONE_WIN
            if (!isAstraSupportAvailable())
                configuration.astraSupport = false;

            else if (!isAstraAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                configuration.astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Astra SDK is not available. Please download the Astra Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Orbbec Astra SDK"))
                    Application.OpenURL("https://orbbec3d.com/develop/");
            }
            else
                configuration.astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", configuration.astraSupport);
#else
            configuration.astraSupport = false;
#endif
            return (configuration.astraSupport != oldAstraSupport);
        }

        public static bool RealsenseSettingUI(Configuration configuration) {
            bool oldRealsenseSupport = configuration.realsenseSupport;
#if UNITY_STANDALONE_WIN
            if (!isRealsenseSupportAvailable())
                configuration.realsenseSupport = false;
            else
                configuration.realsenseSupport = EditorGUILayout.Toggle("Intel RealSense Support", configuration.realsenseSupport);
#else
            configuration.realsenseSupport = false;
#endif
            return (configuration.realsenseSupport != oldRealsenseSupport);
        }

        public static bool HydraSettingUI(Configuration configuration) {
            bool oldHydraSupport = configuration.hydraSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isHydraSupportAvailable())
                configuration.hydraSupport = false;
            else
                configuration.hydraSupport = EditorGUILayout.Toggle("Hydra Support", configuration.hydraSupport);
#else
            configuration.hydraSupport = false;
#endif
            return (configuration.hydraSupport != oldHydraSupport);
        }

        public static bool TobiiSettingUI(Configuration configuration) {
            bool oldTobiiSupport = configuration.tobiiSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isFaceSupportAvailable() || !isTobiiSupportAvailable())
                configuration.tobiiSupport = false;

            else if (!isTobiiAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                configuration.tobiiSupport = EditorGUILayout.Toggle("Tobii Support", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Tobii Framework is not available. Please download the Tobii Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Tobii Unity SDK"))
                    Application.OpenURL("http://developer.tobii.com/tobii-unity-sdk/");
            }
            else
                configuration.tobiiSupport = EditorGUILayout.Toggle("Tobii Support", configuration.tobiiSupport);
#else
            configuration.tobiiSupport = false;
#endif
            return (configuration.tobiiSupport != oldTobiiSupport);
        }

        public static bool PupilSettingUI(Configuration configuration) {
            bool oldPupilSupport = configuration.pupilSupport;
#if UNITY_STANDALONE_WIN
            if (!isPupilSupportAvailable())
                configuration.pupilSupport = false;

            else if (!isPupilAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                configuration.pupilSupport = EditorGUILayout.Toggle("Pupil Labs Support", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Pupil Labs plugin is not available. Please download the plugin using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Pupil Unity Plugin"))
                    Application.OpenURL("https://github.com/pupil-labs/hmd-eyes/releases/tag/v0.5.1");
            }
            else
                configuration.pupilSupport = EditorGUILayout.Toggle("Pupil Labs Support", configuration.pupilSupport);
#else
            configuration.pupilSupport = false;
#endif
            return (configuration.pupilSupport != oldPupilSupport);
        }

        public static bool NeuronSettingUI(Configuration configuration) {
            bool oldNeuronSupport = configuration.neuronSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isNeuronSupportAvailable())
                configuration.neuronSupport = false;

            else
                configuration.neuronSupport = EditorGUILayout.Toggle("Perception Neuron Support", configuration.neuronSupport);
#else
            configuration.neuronSupport = false;
#endif
            return (configuration.neuronSupport != oldNeuronSupport);
        }

        public static bool OptitrackSettingUI(Configuration configuration) {
            bool oldOptitrackSupport = configuration.optitrackSupport;
#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            if (!isOptitrackSupportAvailable())
                configuration.optitrackSupport = false;

            else if (!isOptitrackAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                configuration.optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("OptiTrack Unity plugin is not available. Please download the plugin using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download OptiTrack Unity Plugin"))
                    Application.OpenURL("https://optitrack.com/downloads/plugins.html#unity-plugin");
            }
            else
                configuration.optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", configuration.optitrackSupport);
#else
            configuration.optitrackSupport = false;
#endif
            return (configuration.optitrackSupport != oldOptitrackSupport);
        }

        private static bool NetworkingSettingUI(Configuration configuration, SerializedObject serializedConfiguration) {
            SerializedProperty networkingSupportProp = serializedConfiguration.FindProperty("networkingSupport");
            int oldNetworkingSupport = networkingSupportProp.intValue;
            networkingSupportProp.intValue = (int)(NetworkingSystems)EditorGUILayout.EnumPopup("Networking Support", (NetworkingSystems)networkingSupportProp.intValue);
            return (oldNetworkingSupport != networkingSupportProp.intValue);
        }

#endregion

        public static bool ConfigurationGUI_old(Configuration configuration) {
            bool anyChanged = false;

            FindHumanoidFolder();

            // Preferences GUI
            anyChanged |= SteamVrConfigurationGUI(configuration);

#if UNITY_STANDALONE_WIN || UNITY_ANDROID
            configuration.oculusSupport = IsFileAvailable(oculusPath) && EditorGUILayout.Toggle("Oculus Support", configuration.oculusSupport);
#else
            if (configuration.oculusSupport)
                anyChanged = true;
            configuration.oculusSupport = false;
#endif

#if UNITY_WSA_10_0
            configuration.windowsMRSupport = IsFileAvailable(windowsMRPath) && EditorGUILayout.Toggle("Windows MR Support", configuration.windowsMRSupport);
#else
            if (configuration.windowsMRSupport)
                anyChanged = true;
            configuration.windowsMRSupport = false;
#endif
            configuration.vrtkSupport = IsFileAvailable(vrtkPath) && VrtkConfiguration(configuration.vrtkSupport);

#if UNITY_STANDALONE_WIN
            configuration.astraSupport = IsFileAvailable(astraPath) && AstraConfiguration(configuration.astraSupport);
            configuration.realsenseSupport = IsFileAvailable(realsensePath) && EditorGUILayout.Toggle("Intel RealSense Support", configuration.realsenseSupport);
            configuration.pupilSupport = IsFileAvailable(pupilPath) && PupilConfiguration(configuration.pupilSupport);
#else
            if (configuration.realsenseSupport || configuration.pupilSupport)
                anyChanged = true;
            configuration.realsenseSupport = false;
            configuration.pupilSupport = false;
#endif

#if UNITY_STANDALONE_WIN || UNITY_WSA_10_0
            configuration.neuronSupport = IsFileAvailable(neuronPath) && NeuronConfiguration(configuration.neuronSupport);

            configuration.leapSupport = IsFileAvailable(leapPath) && LeapConfiguration(configuration.leapSupport);
            configuration.kinect1Support = IsFileAvailable(kinect1Path) && Kinect1Configuration(configuration.kinect1Support);
            configuration.kinectSupport = IsFileAvailable(kinect2Path) && KinectConfiguration(configuration.kinectSupport);
            configuration.hydraSupport = IsFileAvailable(hydraPath) && HydraConfiguration(configuration.hydraSupport);
            configuration.tobiiSupport = IsFileAvailable(facePath) && IsFileAvailable(tobiiPath) && TobiiConfiguration(configuration.tobiiSupport);
            configuration.optitrackSupport = IsFileAvailable(optitrackPath) && OptitrackConfiguration(configuration.optitrackSupport);
#else
            if (configuration.neuronSupport ||
                configuration.realsenseSupport ||
                configuration.leapSupport ||
                configuration.kinectSupport ||
                configuration.hydraSupport || 
                configuration.optitrackSupport) {

                anyChanged = true;
            }
            configuration.neuronSupport = false;

            configuration.leapSupport = false;
            configuration.kinectSupport = false;
            configuration.hydraSupport = false;
            configuration.tobiiSupport = false;
            configuration.optitrackSupport = false;
#endif
            configuration.networkingSupport = (NetworkingSystems)EditorGUILayout.EnumPopup("Networking Support", configuration.networkingSupport);

            return anyChanged;
        }

        private enum SteamVrOptions {
            Disabled = 0,
            OpenVr1 = 1,
            OpenVr2 = 2,
        };
        private static bool SteamVrConfigurationGUI(Configuration configuration) {
            bool anyChanged = false;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            bool oldSteamVRSupport = configuration.steamVRSupport;
            bool oldOpenVRSupport = configuration.openVRSupport;
            bool oldViveTrackerSupport = configuration.viveTrackerSupport;
            SteamVrOptions steamVrOption = configuration.openVRSupport ? SteamVrOptions.OpenVr2 : (configuration.steamVRSupport ? SteamVrOptions.OpenVr1 : SteamVrOptions.Disabled);
            steamVrOption = (SteamVrOptions)EditorGUILayout.EnumPopup("SteamVR Support", steamVrOption);
            if (steamVrOption == SteamVrOptions.OpenVr2 && IsFileAvailable(openVRPath)) {
                configuration.steamVRSupport = false;
                configuration.openVRSupport = true;
            }
            else if (steamVrOption == SteamVrOptions.OpenVr1 && IsFileAvailable(steamVRPath)) {
                configuration.steamVRSupport = true;
                configuration.openVRSupport = false;
            }
            else {
                configuration.steamVRSupport = false;
                configuration.openVRSupport = false;
            }
            EditorGUI.BeginDisabledGroup(!configuration.steamVRSupport && !configuration.openVRSupport);
            configuration.viveTrackerSupport = IsFileAvailable(vivetrackerPath) && EditorGUILayout.Toggle("Vive Tracker Support", configuration.viveTrackerSupport);
            if (configuration.steamVRSupport)
                EditorGUILayout.HelpBox("OpenVR 2 support is recommendeed for Vive Trackers", MessageType.Warning, true);
            EditorGUI.EndDisabledGroup();
            if (configuration.steamVRSupport != oldSteamVRSupport ||
                configuration.openVRSupport != oldOpenVRSupport ||
                configuration.viveTrackerSupport != oldViveTrackerSupport) {

                anyChanged = true;
            }
#else
            if (configuration.steamVRSupport | configuration.openVRSupport | configuration.viveTrackerSupport)
                anyChanged = true;
            configuration.steamVRSupport = false;
            configuration.openVRSupport = false;
            configuration.viveTrackerSupport = false;
#endif
            return anyChanged;
        }

        private static bool IsFileAvailable(string filePath) {
            string path = Application.dataPath + humanoidPath + filePath;
            bool fileAvailable = File.Exists(path);
            return fileAvailable;
        }

#region Configurations

        public static bool NeuronConfiguration(bool neuronSupport) {
            return EditorGUILayout.Toggle("Perception Neuron Support", neuronSupport);
        }

        public static bool LeapConfiguration(bool leapSupport) {
            if (!isLeapAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                leapSupport = false;
                leapSupport = EditorGUILayout.Toggle("Leap Motion Support", leapSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Leap Motion Core Assets are not available. Please download the Core Assets using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Leap Motion Unity Core Assets"))
                    Application.OpenURL("https://developer.leapmotion.com/unity");
            }
            else
                leapSupport = EditorGUILayout.Toggle("Leap Motion Support", leapSupport);
            return leapSupport;
        }

        public static bool Kinect1Configuration(bool kinect1Support) {
            return EditorGUILayout.Toggle("Kinect 1 Support", kinect1Support);
        }

        public static bool KinectConfiguration(bool kinectSupport) {
            return EditorGUILayout.Toggle("Kinect 2 Support", kinectSupport);
        }

        public static bool AstraConfiguration(bool astraSupport) {
            if (!isAstraAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                astraSupport = false;
                astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", astraSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Astra SDK is not available. Please download the Astra Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Orbbec Astra SDK"))
                    Application.OpenURL("https://orbbec3d.com/develop/");
            }
            else
                astraSupport = EditorGUILayout.Toggle("Orbbec Astra Support", astraSupport);
            return astraSupport;
        }

        public static bool HydraConfiguration(bool hydraSupport) {
            return EditorGUILayout.Toggle("Hydra Support", hydraSupport);
        }
        public static bool TobiiConfiguration(bool tobiiSupport) {
            if (!isTobiiAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                tobiiSupport = false;
                tobiiSupport = EditorGUILayout.Toggle("Tobii Support", tobiiSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Tobii Framework is not available. Please download the Tobii Unity SDK using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download Tobii Unity SDK"))
                    Application.OpenURL("http://developer.tobii.com/tobii-unity-sdk/");
            }
            else if (IsFileAvailable(facePath)) //(isFaceTrackingAvailable())
                tobiiSupport = EditorGUILayout.Toggle("Tobii Support", tobiiSupport);
            return tobiiSupport;
        }

        public static bool OptitrackConfiguration(bool optitrackSupport) {
            if (!isOptitrackAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                optitrackSupport = false;
                optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", optitrackSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("OptiTrack Unity plugin is not available. Please download the plugin using the button below and import them into this project.", MessageType.Warning, true);
                if (GUILayout.Button("Download OptiTrack Unity Plugin"))
                    Application.OpenURL("https://optitrack.com/downloads/plugins.html#unity-plugin");
            }
            else
                optitrackSupport = EditorGUILayout.Toggle("OptiTrack Support", optitrackSupport);
            return optitrackSupport;
        }

        public static bool PupilConfiguration(bool pupilSupport) {
            if (!isPupilAvailable()) {
                pupilSupport = false;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Pupil Labs Support", GUILayout.Width(197));
                if (GUILayout.Button("Download Unity Plugin"))
                    Application.OpenURL("https://github.com/pupil-labs/hmd-eyes/releases/tag/v0.5.1");
                EditorGUILayout.EndHorizontal();
            }
            else
                pupilSupport = EditorGUILayout.Toggle("Pupil Labs Support", pupilSupport);
            return pupilSupport;
        }

        public static bool VrtkConfiguration(bool vrtkSupport) {
            if (!isVrtkAvailable()) {
                EditorGUI.BeginDisabledGroup(true);
                vrtkSupport = false;
                EditorGUILayout.Toggle("VRTK Supoort", vrtkSupport);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("VRTK is not available. Please download it from the Asset Store.", MessageType.Warning, true);
            }
            else
                vrtkSupport = EditorGUILayout.Toggle("VRTK Support", vrtkSupport);
            return vrtkSupport;
        }

#endregion

#region Availability

        private static bool isSteamVrSupportAvailable() {
            return IsFileAvailable(steamVRPath);
        }

        private static bool isViveTrackerSupportAvailable() {
            return IsFileAvailable(vivetrackerPath);
        }

        private static bool isOculusSupportAvailable() {
            return IsFileAvailable(oculusPath);
        }

        private static bool isWindowsMrAvailable() {
            return IsFileAvailable(windowsMRPath);
        }

        private static bool isVrtkSupportAvailable() {
            return IsFileAvailable(vrtkPath);
        }

        private static bool isVrtkAvailable() {
            string path1 = Application.dataPath + "/VRTK/Scripts/Utilities/SDK/VRTK_SDKManager.cs"; // v3.2.0
            string path2 = Application.dataPath + "/VRTK/Source/Scripts/Utilities/SDK/VRTK_SDKManager.cs"; // v3.3.0
            return File.Exists(path1) || File.Exists(path2);
        }

        private static bool isLeapAvailable() {
            // Location for the Leap Core Assets < v4.2
            string path1 = Application.dataPath + "/Plugins/x86/LeapC.dll";
            string path2 = Application.dataPath + "/Plugins/x86_64/LeapC.dll";
            if (File.Exists(path1) || File.Exists(path2))
                return true;

            // Location for Leap Core Assets >= v4.2
            path1 = Application.dataPath + "/LeapMotion/Core/Plugins/x86/LeapC.dll";
            path2 = Application.dataPath + "/LeapMotion/Core/Plugins/x86_64/LeapC.dll";
            return (File.Exists(path1) || File.Exists(path2));
        }

        private static bool isLeapSupportAvailable() {
            return IsFileAvailable(leapPath);
        }

        private static bool isKinect1SupportAvailable() {
            return IsFileAvailable(kinect1Path);
        }

        private static bool isKinect2SupportAvailable() {
            return IsFileAvailable(kinect2Path);
        }

        public static bool isAstraSupportAvailable() {
            return IsFileAvailable(astraPath);
        }

        public static bool isAstraAvailable() {
            string path1 = Application.dataPath + "/Plugins/x86_64/astra.dll";
            return File.Exists(path1);
        }

        public static bool isRealsenseSupportAvailable() {
            return IsFileAvailable(realsensePath);
        }

        private static bool isHydraSupportAvailable() {
            return IsFileAvailable(hydraPath);
        }

        private static bool isTobiiSupportAvailable() {
            return IsFileAvailable(tobiiPath);
        }

        private static bool isTobiiAvailable() {
            string path1 = Application.dataPath + "/Tobii/Framework/TobiiAPI.cs";
            string path2 = Application.dataPath + "/Tobii/Plugins/x64/Tobii.GameIntegration.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        private static bool isPupilSupportAvailable() {
            return IsFileAvailable(pupilPath);
        }

        private static bool isPupilAvailable() {
            string path1 = Application.dataPath + "/pupil_plugin/Scripts/Networking/PupilTools.cs";
            string path2 = Application.dataPath + "/pupil_plugin/Plugins/x86_64/NetMQ.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        public static bool isNeuronSupportAvailable() {
            return IsFileAvailable(neuronPath);
        }

        private static bool isOptitrackSupportAvailable() {
            return Configuration_Editor.IsFileAvailable(optitrackPath);
        }

        private static bool isOptitrackAvailable() {
            string path1 = Application.dataPath + "/OptiTrack/Scripts/OptitrackStreamingClient.cs";
            string path2 = Application.dataPath + "/OptiTrack/Plugins/x86_64/NatNetLib.dll";
            return File.Exists(path1) && File.Exists(path2);
        }

        //private static bool isAstraAvailable() {
        //    string path1 = Application.dataPath + "/Plugins/x86_64/astra.dll";
        //    return File.Exists(path1);
        //}

        //private static bool isTobiiAvailable() {
        //    string path1 = Application.dataPath + "/Tobii/Framework/TobiiAPI.cs";
        //    string path2 = Application.dataPath + "/Tobii/Plugins/x64/Tobii.GameIntegration.dll";
        //    return File.Exists(path1) && File.Exists(path2);
        //}

        //private static bool isOptitrackAvailable() {
        //    string path1 = Application.dataPath + "/OptiTrack/Scripts/OptitrackStreamingClient.cs";
        //    string path2 = Application.dataPath + "/OptiTrack/Plugins/x86_64/NatNetLib.dll";
        //    return File.Exists(path1) && File.Exists(path2);
        //}

        //private static bool isPupilAvailable() {
        //    string path1 = Application.dataPath + "/pupil_plugin/Scripts/Networking/PupilTools.cs";
        //    string path2 = Application.dataPath + "/pupil_plugin/Plugins/x86_64/NetMQ.dll";
        //    return File.Exists(path1) && File.Exists(path2);
        //}

        private static bool isPhotonAvailable() {
            string path = Application.dataPath + "/Plugins/Photon3Unity3D.dll";
            return File.Exists(path);
        }

        private static bool isPhoton2Available() {
            string path = Application.dataPath + "/Photon/PhotonUnityNetworking/Code/PunClasses.cs";
            return File.Exists(path);
        }

        public static bool isFaceSupportAvailable() {
            return IsFileAvailable(facePath);
        }

#endregion

#region Extension Checks   

        public static void CheckExtensions(Configuration configuration) {
            configuration.steamVRSupport = CheckExtensionSteamVR(configuration);
            configuration.openVRSupport = CheckExtensionOpenVR(configuration);
            configuration.viveTrackerSupport = CheckExtensionViveTracker(configuration);
            configuration.oculusSupport = CheckExtensionOculus(configuration);
            configuration.windowsMRSupport = CheckExtensionWindowsMR(configuration);
            configuration.vrtkSupport = CheckExtensionVRTK(configuration);
            configuration.neuronSupport = CheckExtensionNeuron(configuration);
            configuration.realsenseSupport = CheckExtensionRealsense(configuration);
            configuration.leapSupport = CheckExtensionLeap(configuration);
            configuration.kinect1Support = CheckExtensionKinect1(configuration);
            configuration.kinectSupport = CheckExtensionKinect2(configuration);
            configuration.astraSupport = CheckExtensionAstra(configuration);
            configuration.hydraSupport = CheckExtensionHydra(configuration);
            configuration.tobiiSupport = CheckExtensionTobii(configuration);
            configuration.optitrackSupport = CheckExtensionOptitrack(configuration);
            configuration.pupilSupport = CheckExtensionPupil(configuration);

            CheckExtensionNetworking(configuration);
            CheckFaceTracking(configuration);
        }

        public static bool CheckExtensionSteamVR(Configuration configuration) {
            bool enabled = configuration.steamVRSupport;
            return CheckExtension(enabled, steamVRPath, "hSTEAMVR");
        }

        public static bool CheckExtensionOpenVR(Configuration configuration) {
            bool enabled = configuration.openVRSupport;
            return CheckExtension(enabled, openVRPath, "hOPENVR");
        }

        public static bool CheckExtensionViveTracker(Configuration configuration) {
            bool enabled = configuration.viveTrackerSupport;
            return CheckExtension(enabled, vivetrackerPath, "hVIVETRACKER");
        }

        public static bool CheckExtensionOculus(Configuration configuration) {
            bool enabled = configuration.oculusSupport;
            return CheckExtension(enabled, oculusPath, "hOCULUS");
        }

        public static bool CheckExtensionWindowsMR(Configuration configuration) {
            bool enabled = configuration.windowsMRSupport;
            return CheckExtension(enabled, windowsMRPath, "hWINDOWSMR");
        }

        public static bool CheckExtensionVRTK(Configuration configuration) {
            bool enabled = configuration.vrtkSupport && isVrtkAvailable();
            return CheckExtension(enabled, vrtkPath, "hVRTK");
        }

        public static bool CheckExtensionNeuron(Configuration configuration) {
            bool enabled = configuration.neuronSupport;
            return CheckExtension(enabled, neuronPath, "hNEURON");
        }

        public static bool CheckExtensionRealsense(Configuration configuration) {
            bool enabled = configuration.realsenseSupport;
            return CheckExtension(enabled, realsensePath, "hREALSENSE");
        }

        public static bool CheckExtensionLeap(Configuration configuration) {
            bool enabled = configuration.leapSupport && isLeapAvailable();
            return CheckExtension(enabled, leapPath, "hLEAP");
        }

        public static bool CheckExtensionKinect1(Configuration configuration) {
            bool enabled = configuration.kinect1Support;
            return CheckExtension(enabled, kinect1Path, "hKINECT1");
        }

        public static bool CheckExtensionKinect2(Configuration configuration) {
            bool enabled = configuration.kinectSupport;
            return CheckExtension(enabled, kinect2Path, "hKINECT2");
        }

        public static bool CheckExtensionAstra(Configuration configuration) {
            bool enabled = configuration.astraSupport && isAstraAvailable();
            return CheckExtension(enabled, astraPath, "hORBBEC");
        }

        public static bool CheckExtensionHydra(Configuration configuration) {
            bool enabled = configuration.hydraSupport;
            return CheckExtension(enabled, hydraPath, "hHYDRA");
        }

        public static bool CheckExtensionTobii(Configuration configuration) {
            bool enabled = configuration.tobiiSupport && isTobiiAvailable();
            return CheckExtension(enabled, tobiiPath, "hTOBII");
        }

        public static bool CheckExtensionOptitrack(Configuration configuration) {
            bool enabled = configuration.optitrackSupport && isOptitrackAvailable();
            return CheckExtension(enabled, optitrackPath, "hOPTITRACK");
        }

        public static bool CheckExtensionPupil(Configuration configuration) {
            bool enabled = configuration.pupilSupport && isPupilAvailable();
            return CheckExtension(enabled, pupilPath, "hPUPIL");
        }

        private static void CheckExtensionNetworking(Configuration configuration) {
            if (isPhoton2Available()) {
                GlobalDefine("hPHOTON2");
                GlobalUndefine("hPHOTON1");
            }
            else if (isPhotonAvailable()) {
                GlobalDefine("hPHOTON1");
                GlobalUndefine("hPHOTON2");
            }
            else {
                GlobalUndefine("hPHOTON1");
                GlobalUndefine("hPHOTON2");
            }

#if !UNITY_2019_1_OR_NEWER
            if (configuration.networkingSupport == NetworkingSystems.UnityNetworking)
                GlobalDefine("hNW_UNET");
            else
                GlobalUndefine("hNW_UNET");

            if (configuration.networkingSupport == NetworkingSystems.UnityNetworking)
                CheckUnetHasNetworkIdentity();
#endif

#if hPHOTON1 || hPHOTON2
            if (configuration.networkingSupport == NetworkingSystems.PhotonNetworking)
                GlobalDefine("hNW_PHOTON");
            else
                GlobalUndefine("hNW_PHOTON");

            if (configuration.networkingSupport == NetworkingSystems.PhotonNetworking)
                CheckPunHasPhotonView();
#endif
        }

#if hPHOTON1 || hPHOTON2
        private static void CheckPunHasPhotonView() {
            // Check HumanoidPun has a PhotonView
#if UNITY_2018_3_OR_NEWER
            string humanoidPathWithoutScripts = humanoidPath.Substring(0, humanoidPath.Length - 8);
            string humanoidPunPath = "Assets" + humanoidPathWithoutScripts + "Prefabs/Networking/Resources/HumanoidPUN.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(humanoidPunPath);
            if (prefab != null) {
                try {
                    PhotonView photonView = prefab.GetComponent<PhotonView>();
                    if (photonView == null) 
                        photonView = prefab.AddComponent<PhotonView>();

                    photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
#if hPHOTON2
                    photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
#else
                    photonView.synchronization = ViewSynchronization.UnreliableOnChange;
#endif

                    HumanoidPun humanoidPun = prefab.GetComponent<HumanoidPun>();
                    if (humanoidPun != null)
                        photonView.ObservedComponents.Add(humanoidPun);
                    PrefabUtility.SaveAsPrefabAsset(prefab, humanoidPunPath);
                }
                finally {
                    PrefabUtility.UnloadPrefabContents(prefab);
                }
            }
#else
            GameObject prefab = (GameObject)Resources.Load("HumanoidPUN");
            if (prefab != null) {
                PhotonView photonView = prefab.GetComponent<PhotonView>();
                if (photonView == null) {
                    photonView = prefab.AddComponent<PhotonView>();
                    photonView.ObservedComponents = new System.Collections.Generic.List<Component>();
#if hPHOTON2
                    photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
#else
                        photonView.synchronization = ViewSynchronization.UnreliableOnChange;
#endif

                    HumanoidPun humanoidPun = prefab.GetComponent<HumanoidPun>();
                    if (humanoidPun != null)
                        photonView.ObservedComponents.Add(humanoidPun);
                }
            }
#endif
        }
#endif

        private static void CheckUnetHasNetworkIdentity() {
#if !UNITY_2019_1_OR_NEWER && hNW_UNET
#if UNITY_2018_3_OR_NEWER
            string humanoidPathWithoutScripts = humanoidPath.Substring(0, humanoidPath.Length - 8);
            string humanoidUnetPath = "Assets" + humanoidPathWithoutScripts + "Prefabs/Networking/Resources/HumanoidUnet.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(humanoidUnetPath);
            if (prefab == null)
                return;

            try {
                NetworkIdentity nwId = prefab.GetComponent<NetworkIdentity>();
                if (nwId != null)
                    return;
                nwId = prefab.AddComponent<NetworkIdentity>();
                PrefabUtility.SaveAsPrefabAsset(prefab, humanoidUnetPath);
            }
            finally {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
#else
            GameObject prefab = (GameObject)Resources.Load("HumanoidUnet");
            if (prefab == null)
                return;

            NetworkIdentity nwId = prefab.GetComponent<NetworkIdentity>();
            if (nwId != null)
                return;

            nwId = prefab.AddComponent<NetworkIdentity>();
#endif
#endif
        }

        private static void CheckFaceTracking(Configuration configuration) {
            if (IsFileAvailable(facePath)) {
                GlobalDefine("hFACE");
            }
            else {
                GlobalUndefine("hFACE");
            }
        }

        private static bool CheckExtension(bool enabled, string filePath, string define) {
            if (enabled) {
                if (IsFileAvailable(filePath)) {
                    GlobalDefine(define);
                    return true;
                }
                else {
                    GlobalUndefine(define);
                    return false;
                }

            }
            else {
                GlobalUndefine(define);
                return false;
            }
        }

        public static void GlobalDefine(string name) {
            //Debug.Log("Define " + name);
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!scriptDefines.Contains(name)) {
                string newScriptDefines = scriptDefines + " " + name;
                if (EditorUserBuildSettings.selectedBuildTargetGroup != 0)
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }
        }

        public static void GlobalUndefine(string name) {
            //Debug.Log("Undefine " + name);
            string scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (scriptDefines.Contains(name)) {
                int playMakerIndex = scriptDefines.IndexOf(name);
                string newScriptDefines = scriptDefines.Remove(playMakerIndex, name.Length);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newScriptDefines);
            }

        }

        public static Configuration LoadConfiguration(string configurationName) {
            string[] foundAssets = AssetDatabase.FindAssets(configurationName + " t:Configuration");
            if (foundAssets.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(foundAssets[0]);
            Configuration configuration = AssetDatabase.LoadAssetAtPath<Configuration>(path);
            return configuration;
        }

#endregion
    }
    public static class CustomAssetUtility {
        public static void CreateAsset<T>() where T : ScriptableObject {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetTypeName = typeof(T).ToString();
            int dotIndex = assetTypeName.LastIndexOf('.');
            if (dotIndex >= 0)
                assetTypeName = assetTypeName.Substring(dotIndex + 1); // leave just text behind '.'
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + assetTypeName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

}