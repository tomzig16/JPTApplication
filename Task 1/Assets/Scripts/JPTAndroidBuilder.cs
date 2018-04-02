using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class JPTAndroidBuilder {

	public static void StartBuildForAndroid(){
#if UNITY_EDITOR
        EditorBuildSettingsScene[] scenesToBuild = EditorBuildSettings.scenes;
        BuildPipeline.BuildPlayer(
            scenesToBuild,
            AndroidBuildParams.apkPath + "/build/" + AndroidBuildParams.AppName + ".apk",
            BuildTarget.Android,
            BuildOptions.None);
        // BuildOptions.None all the time. Autorun and installation is done seperately.
#endif
    }

}
