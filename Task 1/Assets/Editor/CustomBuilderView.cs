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
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android & BuildTargetGroup.iOS, bundleIdentifier);
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
        RenderBuildButton();
    }
    
    void ShowAndroidBuildParams()
    {
        AndroidBuildParams.InstallAfterBuild = EditorGUILayout.Toggle("Install after build", AndroidBuildParams.InstallAfterBuild);
        AndroidBuildParams.RunAfterBuild = EditorGUILayout.Toggle("Run after build", AndroidBuildParams.RunAfterBuild);
    }

    void ShowCurrentlyAttachedDevices()
    {
        if(Time.realtimeSinceStartup - lastUpdate >= deviceUpdateInterval){
            connectedDevices = ADBUtility.GetConnectedDevices();
        }
        if(connectedDevices == null){
            EditorGUILayout.HelpBox("There are no devices connected.", MessageType.Warning);
        }
        else{
            GUILayout.Label("Select devices which you want to build for:");
            foreach(ADBUtility.ConnectedDeviceData device in connectedDevices){
                GUILayout.Label(device.deviceID + "\t" + device.deviceName);
            }
        }
    }

    void SetUpBuildForiOS()
    {
        RenderBuildButton();
    }

    void RenderBuildButton()
    {
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Build for " + selectedBuildPlatform + "!"))
        {
            Debug.Log("Start building process");
        }
        GUILayout.Space(5f);
    }
}
 