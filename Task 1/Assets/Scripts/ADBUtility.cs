using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class ADBUtility
{
    public static string GetConnectedDevices()
    {
        string cmdFileName;
        // Add check for Linux?
        if(Environment.OSVersion.Platform == PlatformID.Win32Windows){
            cmdFileName = "cmd.exe";
        }
        else{
            cmdFileName = "/AndroidStuff/sdk/platform-tools/adb";
        }

        Process process = new Process();
        process.StartInfo = new ProcessStartInfo(){
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = cmdFileName,
            Arguments = "devices",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        //process.OutputDataReceived += (sender, args) => Output = args.Data;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        //string error = process.StandardError.ReadToEnd();
        process.WaitForExit(10000);
        return output;
    }
}
