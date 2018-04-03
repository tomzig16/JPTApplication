using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class ADBUtility
{
    public class ConnectedDeviceData
    {
        public string deviceName;
        public string deviceID;
        public bool isBuildTarget;
    }

    private static string RunADBCommand(string command, string deviceID)
    {
        Process process = new Process();
        string argument = "-s " + deviceID + " " + command;
        process.StartInfo = new ProcessStartInfo()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = GetADBPath(),
            Arguments = argument,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit(10000);
        if (String.IsNullOrEmpty(output))
        {
            output = "[THERE WAS AN ERROR]: " + error;
        }
        return output;
    }

    public static List<ConnectedDeviceData> GetConnectedDevices()
    {
        // Can't use RunADBCommand() as adb devices does not require deviceID
        Process process = new Process();
        process.StartInfo = new ProcessStartInfo()
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = GetADBPath(),
            Arguments = "devices",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        //string error = process.StandardError.ReadToEnd();
        process.WaitForExit(10000);
        string[] ids = GetConnectedDeviceIDs(output);
        if (ids.Length < 1)
        {
            // No devices found
            return null;
        }
        List<ConnectedDeviceData> connectedDevices = new List<ConnectedDeviceData>();
        foreach (string id in ids)
        {
            ConnectedDeviceData deviceData = new ConnectedDeviceData();
            deviceData.deviceName = GetDeviceName(id);
            deviceData.deviceID = id;
            deviceData.isBuildTarget = false;
            connectedDevices.Add(deviceData);
        }
        return connectedDevices;
    }

    private static string GetADBPath()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorPrefs.GetString("AndroidSdkRoot") + "/platform-tools/adb";
#else
        // Should not be called.
        return "";
#endif
    }

    private static string[] GetConnectedDeviceIDs(string adbDevicesOutput)
    {
        List<string> connectedDevices = new List<string>();
        string[] lines = adbDevicesOutput.Split('\n');
        foreach (string line in lines)
        {
            if (line.Contains("device") && !line.Contains("List"))
            {
                string[] id = line.Split('\t');
                connectedDevices.Add(id[0]);
            }
        }
        return connectedDevices.ToArray();
    }

    private static string GetDeviceName(string connectedDeviceID)
    {
        string deviceModel = RunADBCommand("shell getprop ro.product.model", connectedDeviceID);
        if (String.IsNullOrEmpty(deviceModel))
        {
            return "DEVICE NOT FOUND";
        }
        // ADB returns with \r\n (\r\r\n on Windows apparently) at the end. Removing those
        deviceModel = deviceModel.Trim();

        // CSV file source:
        // https://support.google.com/googleplay/android-developer/answer/6154891?hl=en
        StreamReader file = new StreamReader(UnityEngine.Application.dataPath + "/Editor/Resources/supported_devices.csv",
                                                System.Text.Encoding.Unicode);
        string line;
        string[] deviceName = null;
        while ((line = file.ReadLine()) != null)
        {
            if (line.Contains(deviceModel))
            {
                deviceName = line.Split(',');
                break;
            }
        }

        if (deviceName == null)
        {
            return "DEVICE NOT FOUND";
        }
        return deviceName[0] + " " + deviceName[1];
    }

    public static void InstallOnSelectedDevices(string apkPath)
    {
        foreach (string deviceId in AndroidBuildParams.targetedDevices)
        {
            UnityEngine.Debug.Log("Installing apk on deviceid: " + deviceId);
            RunADBCommand("install -r " + apkPath, deviceId);
        }
    }

    public static void RunOnSelectedDevices()
    {
        foreach(string deviceID in AndroidBuildParams.targetedDevices)
        {
            RunADBCommand("shell am start -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -f 0x10200000 -n " + AndroidBuildParams.apkBundleID + "/com.unity3d.player.UnityPlayerActivity", deviceID);
        }
    }
}
