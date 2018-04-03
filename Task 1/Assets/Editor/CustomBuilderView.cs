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
        // As iOS will not use anything special, I will take settings from AndroidBuildParams.
        // However, in real world, this should be changed and iOS should be given its own params class.
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, AndroidBuildParams.apkBundleID); 
        ValidateBundleIdentifier();
        PlayerSettings.productName = AndroidBuildParams.AppName;
        AndroidBuildParams.isDevelopmentBuild = EditorGUILayout.Toggle("Development build", AndroidBuildParams.isDevelopmentBuild);
        EditorUserBuildSettings.development = AndroidBuildParams.isDevelopmentBuild;
        
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
                SetUpBuildForAndroid();
                // iOS does not have anthing important, so skiping it.
                break;
        }
    }

    void OnEnable()
    {
        //SetUpGlobalVariables();
        //OnGUI();
        AndroidBuildParams.connectedDevices = ADBUtility.GetConnectedDevices();
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
    }

    void ShowCurrentlyAttachedDevices()
    {
        if (Time.realtimeSinceStartup - lastUpdate >= deviceUpdateInterval)
        {
            AndroidBuildParams.connectedDevices = ADBUtility.GetConnectedDevices();
            lastUpdate = Time.realtimeSinceStartup;
        }
        if (AndroidBuildParams.connectedDevices == null)
        {
            EditorGUILayout.HelpBox("There are no devices connected.", MessageType.Warning);
        }
        else
        {
            ShowLabels();
            foreach(ConnectedDeviceData device in AndroidBuildParams.connectedDevices)
            {
                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleLeft;
                EditorGUILayout.LabelField(device.deviceID, GUILayout.MinWidth(0));
                EditorGUILayout.LabelField(device.deviceName, GUILayout.MinWidth(0));
                device.IsInstallTarget = EditorGUILayout.ToggleLeft("", device.IsInstallTarget, GUILayout.Width(50));
                device.IsRunTarget = EditorGUILayout.ToggleLeft("", device.IsRunTarget, GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            ShowSelectAndDeselectAllButton_Install();
            ShowSelectAndDeselectAllButton_Run();
            GUILayout.EndHorizontal();
        }
    }

    void ShowLabels(){
        GUILayout.Label("Select devices which you want to build for:");
        GUILayout.BeginHorizontal();
        // 20
        EditorGUILayout.LabelField("Device ID", GUILayout.MinWidth(0));
        EditorGUILayout.LabelField("Device name", GUILayout.MinWidth(0));
        EditorGUILayout.LabelField("Install", GUILayout.Width(50));
        EditorGUILayout.LabelField("Run", GUILayout.Width(50));      

        GUILayout.EndHorizontal();
    }

    void ShowSelectAndDeselectAllButton_Install()
    {
        int cForInstallationTargets = AndroidBuildParams.connectedDevices.Where(x => x.IsInstallTarget == true).Count();
        if(AndroidBuildParams.connectedDevices.Count > cForInstallationTargets){
            if (GUILayout.Button("Select all installation targets"))
            {
                foreach (ConnectedDeviceData device in AndroidBuildParams.connectedDevices)
                {
                    device.IsInstallTarget = true;
                }
            }
        }
        else
        {
            if (GUILayout.Button("Deselect all installation targets"))
            {
                foreach (ConnectedDeviceData device in AndroidBuildParams.connectedDevices)
                {
                    device.IsInstallTarget = false;
                }
            }
        }
    }

        void ShowSelectAndDeselectAllButton_Run()
    {
        int cForRunTargets = AndroidBuildParams.connectedDevices.Where(x => x.IsRunTarget == true).Count();
        if(AndroidBuildParams.connectedDevices.Count > cForRunTargets){
            if (GUILayout.Button("Select all run targets"))
            {
                foreach (ConnectedDeviceData device in AndroidBuildParams.connectedDevices)
                {
                    device.IsRunTarget = true;
                }
            }
        }

        else
        {
            if (GUILayout.Button("Deselect all run targets"))
            {
                foreach (ConnectedDeviceData device in AndroidBuildParams.connectedDevices)
                {
                    device.IsRunTarget = false;
                }
            }
        }
    }

    void ShowAdditionalFlagsOptions(){
        AndroidBuildParams.additionalArguments = EditorGUILayout.TextField("Additional arguments:", AndroidBuildParams.additionalArguments);
    }

    void SetUpBuildForiOS()
    {
        GUILayout.Label("Just press build button:");
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
                case BuildPlatform.iOS:
                    JPTAndroidBuilder.StartBuildForiOS();
                    break;
                case BuildPlatform.Both:
                    JPTAndroidBuilder.StartBuildForAndroid();
                    JPTAndroidBuilder.StartBuildForiOS();
                    break;
            }
        }
        GUILayout.Space(5f);
    }
}
