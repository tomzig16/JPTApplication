using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class ADBUtility
{
    public struct ConnectedDeviceData{
        string deviceName;
        string deviceID;
    }
    public static string[] GetConnectedDevices()
    {
        Process process = new Process();
        process.StartInfo = new ProcessStartInfo(){
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
        return ids;
    }

    private static string GetADBPath(){
        // TODO test if this works on both OSes
        return UnityEditor.EditorPrefs.GetString("AndroidSdkRoot") + "/platform-tools/adb";
    }

    private static string[] GetConnectedDeviceIDs(string adbDevicesOutput)
    {
        List<string> connectedDevices = new List<string>();
        string[] lines = adbDevicesOutput.Split('\n');
        foreach(string line in lines){
            if(line.Contains("device") && !line.Contains("List")){
                string[] id = line.Split('\t');
                connectedDevices.Add(id[0]);
            }
        }
        return connectedDevices.ToArray();
    }
}
