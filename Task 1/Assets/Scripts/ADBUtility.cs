using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class ADBUtility
{
    public static string GetConnectedDevices()
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        process.StartInfo = startInfo;
        process.Start();

        process.StandardInput.WriteLine("/c adb devices");
        process.StandardInput.Flush();
        process.StandardInput.Close();
        process.WaitForExit();
        return process.StandardOutput.ReadToEnd();
    }
}
