﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;
using System.Threading;

[ExecuteInEditMode]
public class UploadBuild 
{
	public static UploadData data = null;
	public static string UploaderPath = null;
	public static string LastBuildDirSavePath = Application.dataPath + "/LastBuildDir.txt";
	public static string buildDir = null;

	[PostProcessBuildAttribute(1)]
	public static void OnBuildEnd(BuildTarget target, string pathToBuiltProject)
	{
		File.WriteAllText(LastBuildDirSavePath,pathToBuiltProject);
		if(data == null)
			data = JsonUtility.FromJson<UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		if(!data.startOnBuildEnd)
			return;

		UnityEngine.Debug.Log("Autorunning");

		if(PreSetup() < 0)
			return;
		
		BuildVPK(true);
		UploadVPK();
	}

    static int PreSetup()
    {
		if(buildDir == null)
			buildDir = File.ReadAllText(LastBuildDirSavePath);

		if(data == null && File.Exists(VitaFTPOptions.SavePath))
			data = JsonUtility.FromJson<UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		else if(!File.Exists(VitaFTPOptions.SavePath))
		{
			UnityEngine.Debug.Log("Please configure options under VitaFTPI/Options");
			File.WriteAllText(VitaFTPOptions.SavePath,JsonUtility.ToJson(new UploadBuild()));
			return -1;
		}

		if(UploaderPath == null)
        	UploaderPath = System.Text.RegularExpressions.Regex.Replace(Application.dataPath,"Assets","Uploader");

        if(!Directory.Exists(UploaderPath))
        {
            UnityEngine.Debug.Log("Uploader folder not found exiting!");
            return -1;
        }
		return 0;
    }

	[MenuItem("VitaFTPI/Upload VPK")]
	public static void UploadVPK()
	{
		UnityEngine.Debug.Log("Launching VitaFPTI");
		if(PreSetup() < 0)
			return;

		if(!File.Exists(UploaderPath + "/" + data.File_Name + ".vpk"))
		{
			UnityEngine.Debug.Log("No VPK found! Please build it first");
			return;
		}

		string args = "--vpk \"" + UploaderPath + "/" + data.File_Name + ".vpk\" --ip " + data.IP + " --usb " + boolToString(data.UseUSB) + " --drive-letter " + data.DriveLetter + " --storage-type " + data.storageType;
		ProcessStartInfo VitaFTPIStartInfo = new ProcessStartInfo();
		VitaFTPIStartInfo.Arguments = args;
		if(File.Exists(UploaderPath + "/Vita-FTPI-Core.exe"))
			VitaFTPIStartInfo.FileName = UploaderPath + "/Vita-FTPI-Core.exe";
		else
			VitaFTPIStartInfo.FileName = UploaderPath + "/VitaFTPI.exe";
		Process VitaFTPI = new Process();
		VitaFTPI.StartInfo = VitaFTPIStartInfo;
		VitaFTPI.Start();
		UnityEngine.Debug.Log("Done!");
	}

	[MenuItem("VitaFTPI/Run UnityTools")]
	public static void BuildVPKMenu()
	{
		BuildVPK(false);
	}

	static void BuildVPK(bool wait = false)
	{
		if(PreSetup() < 0)
			return;

		if(!File.Exists(LastBuildDirSavePath))
			return;
		
		if(!Directory.Exists(File.ReadAllText(LastBuildDirSavePath)))
			UnityEngine.Debug.Log("No build directory found!");
		
		string args = "-i \"" + buildDir + "\" -o \"" + UploaderPath + "/" + data.File_Name + "\"" + " -f -u -r -p -d";
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = UploaderPath + "/UnityTools.exe";
		processStartInfo.Arguments = args;
		Process UnityTools = new Process();
		UnityTools.StartInfo = processStartInfo;
		UnityTools.Start();

		if(!wait)
			goto EXIT;
		
		while(!UnityTools.HasExited)
			Thread.Sleep(1);
		
		goto EXIT;
		
		EXIT:
			UnityEngine.Debug.Log("Done!");
	}

	static string boolToString(bool val)
	{
		if(val) return "true";
		else return "false";
	}
}