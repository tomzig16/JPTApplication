using System.Collections;
using System.Collections.Generic;
public class ConnectedDeviceData {

	public string deviceName;
	public string deviceID;
	private bool isInstallTarget;
	private bool isRunTarget;
	public bool IsInstallTarget
	{
		get { return isInstallTarget; }
		// If we do not install after build, we can not run the app either
		set
		{
			if(value == false)
			{
				isRunTarget = false;
			}
			isInstallTarget = value;
		}
	}
	public bool IsRunTarget
	{
		get { return isRunTarget; }
		// We cannot run the application without installing it first.
		set
		{
			if(value == true)
			{
				isInstallTarget = true;
			}
			isRunTarget = value;
		}
	}
    
}
