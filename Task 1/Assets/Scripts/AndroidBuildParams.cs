using System.Collections.Generic;

public class AndroidBuildParams
{
    static public string AppName = "";

    private static bool installAfterBuild = false;
    public static bool InstallAfterBuild
    {
        get { return installAfterBuild; }
        // If we do not install after build, we can not run the app either
        set
        {
            if(value == false)
            {
                runAfterBuild = false;
            }
            installAfterBuild = value;
        }
    }

    private static bool runAfterBuild = false;
    public static bool RunAfterBuild
    {
        get { return runAfterBuild; }
        // We cannot run the application without installing it first.
        set
        {
            if(value == true)
            {
                installAfterBuild = true;
            }
            runAfterBuild = value;
        }
    }

    // Key: Device unique ID
    // Value: Device name
    public static HashSet<string> targetedDevices = new HashSet<string>();

}

