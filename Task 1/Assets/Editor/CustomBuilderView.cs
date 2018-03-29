﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomBuilderView : EditorWindow
{
    static string bundleIdentifier;
    // Could possibly use BuildTargetGroup enum, but I want to limit users only for Android and iOS (or both)
    enum BuildPlatform
    {
        Android,
        iOS,
        Both
    };
    BuildPlatform selectedBuildPlatform = BuildPlatform.Android;

    float deviceUpdateInterval = 10f;
    float lastUpdate = -10f;
    List<ADBUtility.ConnectedDeviceData> connectedDevices;


    [MenuItem("JPT Application/Task 1/Custom Build Window")]
    static void OpenBuildWindow()
    {
        SetUpGlobalVariables();
        EditorWindow.GetWindow(typeof(CustomBuilderView));
    }

    static void SetUpGlobalVariables()
    {
        AndroidBuildParams.AppName = Application.productName;
        bundleIdentifier = Application.identifier;
    }

    void OnGUI()
    {
        AndroidBuildParams.AppName = EditorGUILayout.TextField("Build Name:", AndroidBuildParams.AppName);
        bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier:", bundleIdentifier);
        // TODO test if this works in the real world for both platforms
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleIdentifier);
        ValidateBundleIdentifier();
        selectedBuildPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("Select platform 2", selectedBuildPlatform);

        switch (selectedBuildPlatform)
        {
            case BuildPlatform.Android:
                SetUpBuildForAndroid();
                break;
            case BuildPlatform.iOS:
                SetUpBuildForiOS();
                break;
            case BuildPlatform.Both:

                break;
        }
    }

    void ValidateBundleIdentifier()
    {
        string defaultBundleID = "com.Company.ProductName";
        if (bundleIdentifier == defaultBundleID ||
            bundleIdentifier == "")
        {
            // TODO add more checks (for example for special characters)
            // Find out what characters bundle ID does not accept
            EditorGUILayout.HelpBox("Build will fail with default or empty bundle identifier. Please, make sure it is not \"" + defaultBundleID + "\" or empty", MessageType.Warning);
        }
    }

    void SetUpBuildForAndroid()
    {
        GUILayout.Space(5f);
        GUILayout.Label("Here you can select additional options");
        ShowAndroidBuildParams();
        GUILayout.Space(3f);
        ShowCurrentlyAttachedDevices();
        ShowBuildButton();
    }

    void ShowAndroidBuildParams()
    {
        AndroidBuildParams.InstallAfterBuild = EditorGUILayout.Toggle("Install after build", AndroidBuildParams.InstallAfterBuild);
        AndroidBuildParams.RunAfterBuild = EditorGUILayout.Toggle("Run after build", AndroidBuildParams.RunAfterBuild);
    }

    void ShowCurrentlyAttachedDevices()
    {
        if (Time.realtimeSinceStartup - lastUpdate >= deviceUpdateInterval)
        {
            connectedDevices = ADBUtility.GetConnectedDevices();
            lastUpdate = Time.realtimeSinceStartup;
        }
        if (connectedDevices == null)
        {
            EditorGUILayout.HelpBox("There are no devices connected.", MessageType.Warning);
        }
        else
        {
            GUILayout.Label("Select devices which you want to build for:");
            foreach (ADBUtility.ConnectedDeviceData device in connectedDevices)
            {
                device.isBuildTarget = EditorGUILayout.Toggle(device.deviceID + "\t" + device.deviceName, device.isBuildTarget);
                if (device.isBuildTarget && !AndroidBuildParams.targetedDevices.Contains(device.deviceID))
                {
                    AndroidBuildParams.targetedDevices.Add(device.deviceID);
                }
                else if (!device.isBuildTarget && AndroidBuildParams.targetedDevices.Contains(device.deviceID))
                {
                    AndroidBuildParams.targetedDevices.Remove(device.deviceID);
                }
            }
            ShowSelectAndDeselectAllButton();
        }
    }

    void ShowSelectAndDeselectAllButton()
    {
        if(connectedDevices.Count > AndroidBuildParams.targetedDevices.Count){
            if (GUILayout.Button("Select all devices"))
            {
                foreach (ADBUtility.ConnectedDeviceData device in connectedDevices)
                {
                    device.isBuildTarget = true;
                    if (device.isBuildTarget && !AndroidBuildParams.targetedDevices.Contains(device.deviceID))
                    {
                        AndroidBuildParams.targetedDevices.Add(device.deviceID);
                    }
                }
            }
        }
        // If device was disconnected
        else if(connectedDevices.Count < AndroidBuildParams.targetedDevices.Count)
        {
            List<string> devicesToRemoveFromList = new List<string>();
            foreach(string deviceID in AndroidBuildParams.targetedDevices){
                bool exists = false;
                foreach (ADBUtility.ConnectedDeviceData device in connectedDevices)
                {
                    if(device.deviceID == deviceID){
                        exists = true;
                        break;
                    }
                }
                if(!exists) {devicesToRemoveFromList.Add(deviceID);}
            }
            foreach(string id in devicesToRemoveFromList){
                AndroidBuildParams.targetedDevices.Remove(id);
            }
        }
        else
        {
            if (GUILayout.Button("Deselect all devices"))
            {
                foreach (ADBUtility.ConnectedDeviceData device in connectedDevices)
                {
                    device.isBuildTarget = false;
                    if (!device.isBuildTarget && AndroidBuildParams.targetedDevices.Contains(device.deviceID))
                    {
                        AndroidBuildParams.targetedDevices.Remove(device.deviceID);
                    }
                }
            }
        }
    }

    void SetUpBuildForiOS()
    {
        ShowBuildButton();
    }

    void ShowBuildButton()
    {
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Build for " + selectedBuildPlatform + "!"))
        {
            Debug.Log("Start building process");
        }
        GUILayout.Space(5f);
    }
}
