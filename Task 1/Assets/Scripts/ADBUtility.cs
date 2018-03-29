using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class ADBUtility
{
    public struct ConnectedDeviceData{
        public string deviceName;
        public string deviceID;
    }
    public static List<ConnectedDeviceData> GetConnectedDevices()
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
        if(ids.Length < 1){
            // No devices found
            return null;
        }
        List<ConnectedDeviceData> connectedDevices = new List<ConnectedDeviceData>();
        foreach(string id in ids){
            ConnectedDeviceData deviceData = new ConnectedDeviceData();
            deviceData.deviceName = GetDeviceName(id);
            deviceData.deviceID = id;
            connectedDevices.Add(deviceData);
        }
        return connectedDevices;
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

    private static string GetDeviceName(string connectedDeviceID)
    {
        Process process = new Process();
        string argument = "-s " + connectedDeviceID + " shell getprop ro.product.model";
        process.StartInfo = new ProcessStartInfo(){
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = GetADBPath(),
            Arguments = argument,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.Start();
        string deviceModel = process.StandardOutput.ReadToEnd();
        //string error = process.StandardError.ReadToEnd();
        process.WaitForExit(10000);

        // ADB returns with \r\n at the end. Removing those
        deviceModel = deviceModel.Remove(deviceModel.Length - 2, 2);

        // CSV file source:
        // https://support.google.com/googleplay/android-developer/answer/6154891?hl=en
        StreamReader file = new StreamReader(UnityEngine.Application.dataPath + "/Editor/Resources/supported_devices.csv", System.Text.Encoding.Unicode);
        string line;
        string[] deviceName = null;
        int counter = 0;
        while((line = file.ReadLine()) != null){
            if(line.Contains(deviceModel)){
                deviceName = line.Split(',');
                break;
            }
            counter++;
        }

        if(deviceName == null){
            return "DEVICE NOT FOUND";
        }

        return deviceName[0] + " " + deviceName[1];
    }
}
