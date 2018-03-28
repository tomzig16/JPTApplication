﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomBuilderView : EditorWindow {

    static string buildName;
    static string bundleIdentifier;

    [MenuItem("JPT Application/Task 1/Custom Build Window")]
	static void OpenBuildWindow(){
        SetUpGlobalVariables();

        EditorWindow.GetWindow(typeof(CustomBuilderView));
	}

    static void SetUpGlobalVariables()
    {
        buildName = Application.productName;
        bundleIdentifier = Application.identifier;
    }

	void OnGUI(){
        buildName = EditorGUILayout.TextField("Build Name:", buildName);
        bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier:", bundleIdentifier);
        ValidateBundleIdentifier();
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
}
 