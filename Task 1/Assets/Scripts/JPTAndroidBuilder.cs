using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class JPTAndroidBuilder {

	public static void StartBuildForAndroid(){
#if UNITY_EDITOR
        EditorBuildSettingsScene[] scenesToBuild = EditorBuildSettings.scenes;
        string apkPath = AndroidBuildParams.apkPath + "/build/" + AndroidBuildParams.AppName + ".apk";
        BuildPipeline.BuildPlayer(
            scenesToBuild,
            apkPath,
            BuildTarget.Android,
            BuildOptions.None);
        // BuildOptions.None all the time. Autorun and installation is done seperately.
        UnityEngine.Debug.Log("Building complete!");

        if (AndroidBuildParams.InstallAfterBuild)
        {
            UnityEngine.Debug.Log("Installing...");
            ADBUtility.InstallOnSelectedDevices(apkPath);
        }
#endif
    }

}
