using System.Collections.Generic;

public class AndroidBuildParams
{
    public static string AppName = UnityEngine.Application.productName;
    public static string apkBundleID = UnityEngine.Application.identifier;
    public static string apkPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
    public static string additionalArguments = "";
    public static bool isIL2CPPBuild = false;
    public static bool isDevelopmentBuild = true;

    // Key: Device unique ID
    // Value: Device name
    public static List<ConnectedDeviceData> connectedDevices = new List<ConnectedDeviceData>();

    public static ConnectedDeviceData FindInList(string id){
        foreach(ConnectedDeviceData device in connectedDevices){
            if (device.deviceID == id){
                return device;
            }
        }
        return null;
    }
}

