using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class ADBUtility
{
    public static string GetConnectedDevices()
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
        return output;
    }

    private static string GetADBPath(){
        // TODO test if this works on both OSes
        return UnityEditor.EditorPrefs.GetString("AndroidSdkRoot") + "/platform-tools/adb";
    }
}
