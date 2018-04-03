using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CustomBuilderView : EditorWindow
{
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
        AndroidBuildParams.apkBundleID = Application.identifier;
    }

    void OnGUI()
    {
        AndroidBuildParams.AppName = EditorGUILayout.TextField("Build Name:", AndroidBuildParams.AppName);
        AndroidBuildParams.apkBundleID = EditorGUILayout.TextField("Bundle Identifier:", AndroidBuildParams.apkBundleID);
        ShowPathSelectionButton();
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidBuildParams.apkBundleID);
        ValidateBundleIdentifier();
        selectedBuildPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("Select platform", selectedBuildPlatform);

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

    void ShowPathSelectionButton(){
        GUILayout.BeginHorizontal();
        string oldPath = AndroidBuildParams.apkPath;
        AndroidBuildParams.apkPath = EditorGUILayout.TextField("Choose location:", AndroidBuildParams.apkPath);
        if(GUILayout.Button("Choose path")){
            GetApkPath(oldPath);
        }
        if(!IsBuildPathValid()){
            if(EditorUtility.DisplayDialog("Check build path", "Selected path is not valid. Please select any path outside Assets folder. Do you want to change it now?", "OK", "Change later")){
                GetApkPath(oldPath);
                // Could cause stack overflow but what maniac would have enough patience to select asset folder and click on "OK" enormous amount of time 
            }
        }
        GUILayout.EndHorizontal();
    }

    bool IsBuildPathValid(){
        //Debug.Log(AndroidBuildParams.apkPath.Contains(UnityEngine.Application.dataPath));
        return !(AndroidBuildParams.apkPath.Contains(UnityEngine.Application.dataPath));
    }

    void GetApkPath(string oldPath){
        AndroidBuildParams.apkPath = EditorUtility.SaveFolderPanel("Choose location of built game", "", "");
        if(!IsBuildPathValid()){
            AndroidBuildParams.apkPath = oldPath;
            if(EditorUtility.DisplayDialog("Check build path", "Selected path is not valid. Please select any path outside Assets folder. Do you want to change it now?", "OK", "Change later")){
                GetApkPath(oldPath);
                // Could potentially cause stack overflow but what maniac would have enough 
                // patience to select asset folder and click on "OK" enormous amount of time 
            }
        }
        
    }

    void ValidateBundleIdentifier()
    {
        string defaultBundleID = "com.Company.ProductName";
        if (AndroidBuildParams.apkBundleID == defaultBundleID ||
            AndroidBuildParams.apkBundleID == "")
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
        ShowAdditionalFlagsOptions();
        ShowBuildButton();
    }

    void ShowAndroidBuildParams()
    {
        bool lastScriptingBackend = AndroidBuildParams.isIL2CPPBuild;
        AndroidBuildParams.isIL2CPPBuild = EditorGUILayout.Toggle("IL2CPP build", AndroidBuildParams.isIL2CPPBuild);
        if (lastScriptingBackend != AndroidBuildParams.isIL2CPPBuild)
        {
            Debug.Log("Scripting backend has changed. IL2CPP: " + AndroidBuildParams.isIL2CPPBuild);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, 
                AndroidBuildParams.isIL2CPPBuild ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
        }

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
                GUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label(device.deviceID + "\t" + device.deviceName + "\t");
                device.isBuildTarget = EditorGUILayout.ToggleLeft("", device.isBuildTarget);
                
                GUILayout.EndHorizontal();
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

    void ShowAdditionalFlagsOptions(){
        AndroidBuildParams.additionalArguments = EditorGUILayout.TextField("Additional arguments:", AndroidBuildParams.additionalArguments);
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
            Debug.Log("Starting building process");
            switch(selectedBuildPlatform){
                case BuildPlatform.Android:
                    JPTAndroidBuilder.StartBuildForAndroid();
                    break;
            }
        }
        GUILayout.Space(5f);
    }
}
