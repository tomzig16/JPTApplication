public class AndroidBuildParams
{
    #region Singleton pattern
    private static AndroidBuildParams instance;
    public static AndroidBuildParams Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AndroidBuildParams();
            }
            return instance;
        }
    }
    private AndroidBuildParams()
    {
        AppName = "";
        InstallAfterBuild = false;
        RunAfterBuild = false;
    }
    #endregion

    public string AppName = "";

    private bool installAfterBuild;
    public bool InstallAfterBuild
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

    private bool runAfterBuild;
    public bool RunAfterBuild
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

}

