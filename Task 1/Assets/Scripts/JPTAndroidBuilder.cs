using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class JPTAndroidBuilder {

	public static void StartBuildForAndroid(){
#if UNITY_EDITOR
        EditorBuildSettingsScene[] scenesToBuild = EditorBuildSettings.scenes;
        string apkPath = AndroidBuildParams.apkPath + "/build/" + AndroidBuildParams.AppName + ".apk";
        BuildOptions options = AndroidBuildParams.isDevelopmentBuild ? BuildOptions.Development : BuildOptions.None;
        BuildPipeline.BuildPlayer(
            scenesToBuild,
            apkPath,
            BuildTarget.Android,
            options);
        // BuildOptions.None all the time. Autorun and installation is done seperately.
        UnityEngine.Debug.Log("Android building complete!");

        ADBUtility.InstallOnSelectedDevices(apkPath);
        ADBUtility.RunOnSelectedDevices();
#endif
    }

    public static void StartBuildForiOS(){
        // Did not test, but it should work, right?
#if UNITY_EDITOR
        EditorBuildSettingsScene[] scenesToBuild = EditorBuildSettings.scenes;
        string apkPath = AndroidBuildParams.apkPath + "/build/" + AndroidBuildParams.AppName + "-iOS";
        BuildOptions options = AndroidBuildParams.isDevelopmentBuild ? BuildOptions.Development : BuildOptions.None;
        BuildPipeline.BuildPlayer(
            scenesToBuild,
            apkPath,
            BuildTarget.iOS,
            options);
        // BuildOptions.None all the time. Autorun and installation is done seperately.
        UnityEngine.Debug.Log("iOS building complete!");
#endif
    }

}
