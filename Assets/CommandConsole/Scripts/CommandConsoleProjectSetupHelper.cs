#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public class CommandConsoleProjectSetupHelper : EditorWindow {

    //the libraries that c# runtime compilation depends on are not availible in .net 2.0 subset.
    //so, chis editor changes all of the buildTargetGroups to use full .net 2.0.

    bool done = false;

    [MenuItem("Tools/Command Console Project Setup Helper")]
    public static void ShowWindow() {
        GetWindow(typeof(CommandConsoleProjectSetupHelper));
    }

    void OnGUI() {
        if (GUILayout.Button("Set up project for Command Console")) {
            foreach (BuildTargetGroup buildTargetGroup in Enum.GetValues(typeof(BuildTargetGroup))) {
                PlayerSettings.SetApiCompatibilityLevel(buildTargetGroup, ApiCompatibilityLevel.NET_2_0);
            }
            done = true;
        }
        if (done)
            EditorGUILayout.LabelField("Done!");
    }
}

#endif
