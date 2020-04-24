using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Passer.Humanoid {
#if UNITY_2018_3_OR_NEWER
    public class HumanoidSettings : ScriptableObject {
        public const string settingsPath = "Assets/Humanoid/HumanoidSettings.asset";

        public Configuration configuration;

        internal static HumanoidSettings GetOrCreateSettings() {
            string humanoidPath = Configuration_Editor.FindHumanoidFolder();
            humanoidPath = humanoidPath.Substring(0, humanoidPath.Length - 1); // strip last /
            humanoidPath = humanoidPath.Substring(0, humanoidPath.LastIndexOf('/') + 1); // strip Scripts;

            var settings = AssetDatabase.LoadAssetAtPath<HumanoidSettings>("Assets" + humanoidPath + "HumanoidSettings.asset");
            SerializedObject serializedSettings = new SerializedObject(settings);
            if (settings == null) {
                settings = CreateInstance<HumanoidSettings>();

                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.SaveAssets();
            }
            if (settings.configuration == null) {
                string configurationString = EditorPrefs.GetString("HumanoidConfigurationKey", "DefaultConfiguration");
                Configuration configuration = Configuration_Editor.LoadConfiguration(configurationString);
                if (configuration == null) {
                    configurationString = "DefaultConfiguration";
                    Configuration_Editor.LoadConfiguration(configurationString);
                    if (configuration == null) {
                        Debug.Log("Created new Default Configuration");
                        // Create new Default Configuration
                        configuration = CreateInstance<Configuration>();
                        string path = "Assets" + humanoidPath + configurationString + ".asset";
                        AssetDatabase.CreateAsset(configuration, path);
                        AssetDatabase.SaveAssets();
                    }
                }
                SerializedProperty configurationProp = serializedSettings.FindProperty("configuration");
                configurationProp.objectReferenceValue = configuration;
            }
            serializedSettings.ApplyModifiedProperties();
            return (HumanoidSettings)serializedSettings.targetObject;//settings;
        }
    }
#endif
    static class HumanoidSettingsIMGUIRegister {
#if UNITY_2018_3_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateHumanoidSettingsProvider() {
            var provider = new SettingsProvider("Preferences/HumanoidControlSettings", SettingsScope.User) {
                label = "Humanoid Control",
                guiHandler = (searchContext) => {
                    HumanoidSettings settings = HumanoidSettings.GetOrCreateSettings();
                    bool anyChanged = false;

                    Configuration oldConfiguration = settings.configuration;
                    settings.configuration = (Configuration)EditorGUILayout.ObjectField("Configuration", settings.configuration, typeof(Configuration), false);
                    SerializedObject serializedConfiguration = new SerializedObject(settings.configuration);
                    anyChanged |= (settings.configuration != oldConfiguration);
                    anyChanged |= Configuration_Editor.ConfigurationGUI(settings.configuration, serializedConfiguration);
                    serializedConfiguration.ApplyModifiedProperties();

                    if (anyChanged)
                        Configuration_Editor.CheckExtensions(settings.configuration);
                },
                keywords = new HashSet<string>(
                    new[] { "Humanoid", "Oculus", "SteamVR" }
                    )
            };
            return provider;
        }
#endif

    }

}