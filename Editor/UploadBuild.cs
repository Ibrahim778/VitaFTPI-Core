using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
public class UploadBuild 
{
	public static UploadWrapper.UploadData data = null;
	public static string UploaderPath = null;
	public static string LastBuildDirSavePath = Application.dataPath + "/VitaFTPI/LastBuildDir.txt";
	public static string buildDir = null;
	public static string Path = new StackTrace(true).GetFrame(0).GetFileName();
	public static string runFilePath = Application.dataPath + "/VitaFTPI/run";

	public static string GetUploadDir()
    {
		return JsonUtility.FromJson<UploadWrapper.UploadData>(File.ReadAllText(VitaFTPOptions.SavePath)).UploaderFolder;
    }

	public static void sendCommand(string cmd)
	{
		if (loadData() < 0)
			return;
		using (TcpClient client = new TcpClient(data.IP, 1338))
		{
			using (NetworkStream ns = client.GetStream())
			{
				using (StreamWriter sw = new StreamWriter(ns))
				{
					sw.Write(cmd + "\n");
					sw.Flush();
					using (StreamReader sr = new StreamReader(ns))
					{
						UnityEngine.Debug.Log(sr.ReadToEnd());
						sr.Close();
					}
					sw.Close();
				}
				ns.Close();
			}
			client.Close();
		}
	}
	

	[PostProcessBuildAttribute(1)]
	public static void OnBuildEnd(BuildTarget target, string pathToBuiltProject)
	{
		if (!target.Equals(BuildTarget.PSP2))
			return;
		if (pathToBuiltProject.Contains("/data/VitaUnity/build"))
        {
			CustomPrepBuild(true, pathToBuiltProject);
			sendCommand("usb disable -");
			sendCommand("file ux0:data/VitaUnity/build/build.self");
			return;
		}

		File.WriteAllText(LastBuildDirSavePath, pathToBuiltProject);
		if(data == null)
			data = JsonUtility.FromJson<UploadWrapper.UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		if(!data.startOnBuildEnd)
			return;

		UnityEngine.Debug.Log("Autorunning");

		if(loadData() < 0)
			return;

		BuildVPK(true);
		ReplaceInstall();
	}


	static string[] GetDriveLetters()
	{
		List<string> driveNames = new List<string>();
		foreach (DriveInfo drive in DriveInfo.GetDrives()) driveNames.Add(drive.Name);
		return driveNames.ToArray();
	}

	public static void TestBuild()
    {
		if (loadData() < 0)
			return;

		string[] initialDrives = GetDriveLetters();
		sendCommand("usb enable " + data.storageType);
		while (Enumerable.SequenceEqual(initialDrives, GetDriveLetters()))
			Thread.Sleep(1);

		List<string> CurrentDrives = new List<string>();

		foreach (DriveInfo drive in DriveInfo.GetDrives()) CurrentDrives.Add(drive.Name);

		foreach (string drive in initialDrives) CurrentDrives.Remove(drive);

		string driveLetter = CurrentDrives[0].Remove(CurrentDrives[0].Length - 1);

		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
		string[] scenes = new string[sceneCount];
		for (int i = 0; i < sceneCount; i++)
		{
			scenes[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
		}
        BuildPipeline.BuildPlayer(scenes, driveLetter + "/data/VitaUnity/build", BuildTarget.PSP2, BuildOptions.None);
	}

	public static string GetProjectName()
 	{
		 string[] s = Application.dataPath.Split('/');
		 string projectName = s[s.Length - 2];
		 return projectName;
 	}

	public static bool HasStarted;
	static Process process;

	public static void StartDebug()
	{
		if (loadData() < 0) return;
		ProcessStartInfo info = new ProcessStartInfo();
		info.FileName = UploaderPath + "/DebugPortReader.exe";
		info.RedirectStandardOutput = true;
		info.UseShellExecute = false;
		process = new Process();
		process.EnableRaisingEvents = true;
		process.OutputDataReceived += Process_OutputDataReceived;
        process.Exited += OnDebugExit;
		process.StartInfo = info;
		process.Start();
		process.BeginOutputReadLine();


		HasStarted = true;
	}

    private static void OnDebugExit(object sender, System.EventArgs e)
    {
		HasStarted = false;
    }

    public static void StopDebug()
	{
		HasStarted = false;
		process.Kill();
	}

	private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		if (e.Data.Contains("[Unity PSP2]"))
			UnityEngine.Debug.Log(e.Data);
	}


	static int loadData()
	{
		if (data == null && File.Exists(VitaFTPOptions.SavePath))
			data = JsonUtility.FromJson<UploadWrapper.UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		else if (!File.Exists(VitaFTPOptions.SavePath))
		{
			UnityEngine.Debug.Log("Please configure options under VitaFTPI/Options");
			File.WriteAllText(VitaFTPOptions.SavePath, JsonUtility.ToJson(new UploadBuild()));
			return -1;
		}

		if (UploaderPath == null)
		{
			if (!data.CustomUploaderFolder) UploaderPath = Regex.Replace(Application.dataPath, "Assets", "Uploader");
			else UploaderPath = data.UploaderFolder;
		}
		return 0;
	}
	static int PreSetup()
    {
		if (data == null && File.Exists(VitaFTPOptions.SavePath))
			data = JsonUtility.FromJson<UploadWrapper.UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		else if (!File.Exists(VitaFTPOptions.SavePath))
		{
			UnityEngine.Debug.Log("Please configure options under VitaFTPI/Options");
			File.WriteAllText(VitaFTPOptions.SavePath, JsonUtility.ToJson(new UploadBuild()));
			return -1;
		}
		buildDir = File.ReadAllText(LastBuildDirSavePath);
		if(!Directory.Exists(buildDir))
        {
			UnityEngine.Debug.LogError("No build found!");
			return -2;
        }

		if(UploaderPath == null)
        	if(!data.CustomUploaderFolder) UploaderPath = Regex.Replace(Application.dataPath,"Assets","Uploader");
			else UploaderPath = data.UploaderFolder;

		if(Directory.Exists("\"" + UploaderPath + "\"")){}
        if(Directory.Exists(UploaderPath)){}
		else
		{
            UnityEngine.Debug.Log("Uploader folder not found exiting!");
			UnityEngine.Debug.Log(UploaderPath);
            return -1;
		}
		return 0;
    }

	public static void ReplaceInstall()
    {
		if (PreSetup() < 0)
			return;


		if (!File.Exists(UploaderPath + "/" + GetProjectName() + ".vpk") && !data.ExtractOnPC)
		{
			UnityEngine.Debug.Log("No VPK found! Please pack it first");
			return;
		}

        if (data.ExtractOnPC)
        {
			DirectoryInfo buildDirInfo = new DirectoryInfo(buildDir);
			foreach (FileInfo currentFile in buildDirInfo.GetFiles())
			{
				if (currentFile.Extension.Equals(".self"))
				{
					File.Move(currentFile.FullName, Regex.Replace(currentFile.FullName, currentFile.Name, "eboot.bin"));
					break;
				}
			}
		}

		UnityEngine.Debug.Log("Launching VitaFPTI");


		CopyCustomFiles();

		string args = "--vpk \"" + UploaderPath + "\\" + GetProjectName() + ".vpk\" --ip " + data.IP + " --usb " +
			boolToString(data.UseUSB)  + " --storage-type " + data.storageType + " --upload-dir \"" 
			+ data.UploaderFolder + "\"";
		if (data.ExtractOnPC)
		{
			args += " --extract";
			args += " --pre-extract \"" + buildDir + "\"";
		}
		args += " --replace-install";
		args += " --partial";
		if (data.useUDCD)
			args += " --udcd \"" + data.udcdPath + "\"";


		ProcessStartInfo VitaFTPIStartInfo = new ProcessStartInfo();
		VitaFTPIStartInfo.Arguments = args;
		if (File.Exists(UploaderPath + "/Vita-FTPI-Core.exe"))
			VitaFTPIStartInfo.FileName = UploaderPath + "/Vita-FTPI-Core.exe";
		else
			VitaFTPIStartInfo.FileName = UploaderPath + "/VitaFTPI.exe";
		Process VitaFTPI = new Process();
		VitaFTPI.StartInfo = VitaFTPIStartInfo;
		VitaFTPI.Start();
		UnityEngine.Debug.Log("Done!");
	}

	public static void CustomPrepBuild(bool wait, string path)
    {
		if (loadData() < 0)
			return;

		if (!Directory.Exists(path))
			UnityEngine.Debug.Log("No build directory found!");

		string args = "-i \"" + path + "\" -o \"" + UploaderPath + "\\" + GetProjectName() + "\"" + " -f -u -r";
		if (!data.ExtractOnPC)
			args += " -p";
		if (!data.KeepFolderAfterBuild)
			args += " -d";


		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = UploaderPath + "/UnityTools.exe";
		processStartInfo.Arguments = args;
		Process UnityTools = new Process();
		UnityTools.StartInfo = processStartInfo;
		UnityTools.Start();

		if (!wait)
			goto EXIT;

		while (!UnityTools.HasExited)
			Thread.Sleep(1);

		goto EXIT;

	EXIT:
		UnityEngine.Debug.Log("Done!");
	}

	public static void BuildGame()
    {
		if (PreSetup() < 0)
			return;

		int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
		string[] scenes = new string[sceneCount];
		for (int i = 0; i < sceneCount; i++)
		{
			scenes[i] = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
		}
		BuildPipeline.BuildPlayer(scenes, buildDir, BuildTarget.PSP2, BuildOptions.None);
	}

	static void CopyCustomFiles()
    {
		if (Directory.Exists(Application.dataPath + "\\CustomPlugins")) 
		{
			foreach (FileInfo file in new DirectoryInfo(Application.dataPath + "\\CustomPlugins").GetFiles())
			{
				if (file.Extension.Equals(".suprx") || file.Extension.Equals(".skprx")) 
				{
					UnityEngine.Debug.Log("Copying " + file.Name + " to: " + buildDir + "/Media/Plugins/" + file.Name);
					file.CopyTo(buildDir + "\\Media\\Plugins\\" + file.Name, true);
				}
			} 
		}
		if(Directory.Exists(Application.dataPath + "\\CustomSelfs"))
        {
			foreach (FileInfo file in new DirectoryInfo(Application.dataPath + "\\CustomSelfs").GetFiles())
            {
                if (file.Extension.Equals(".self"))
                {
                    if (file.Name.Equals("replace_original_eboot.self"))
                    {
						UnityEngine.Debug.Log("Replacing " + file.Name + " as the original eboot.bin");
						if(File.Exists(buildDir + "\\eboot.bin")) File.Move(buildDir + "\\eboot.bin", buildDir + "\\original_eboot.bin");
						file.CopyTo(buildDir + "\\eboot.bin", true);
					}
                    else
                    {
						UnityEngine.Debug.Log("Copying " + file.Name + " to: " + buildDir + "\\" + file.Name.Replace(".self", ".bin"));
						file.CopyTo(buildDir + "\\" + file.Name.Replace(".self", ".bin"), true);
					}
                }
            }
        }
    }

	[MenuItem("VitaFTPI/Upload VPK")]
	public static void UploadVPK()
	{
		UnityEngine.Debug.Log("Launching VitaFPTI");
		if(PreSetup() < 0)
			return;

		if(!File.Exists(UploaderPath + "/" + GetProjectName() + ".vpk"))
		{
			UnityEngine.Debug.Log("No VPK found! Please pack it first");
			return;
		}
		if (data.ExtractOnPC)
		{
			DirectoryInfo buildDirInfo = new DirectoryInfo(buildDir);
			foreach (FileInfo currentFile in buildDirInfo.GetFiles())
			{
				if (currentFile.Extension.Equals(".self"))
				{
					File.Move(currentFile.FullName, Regex.Replace(currentFile.FullName, currentFile.Name, "eboot.bin"));
					break;
				}
			}
		}

		CopyCustomFiles();

		string args = "--vpk \"" + UploaderPath + "\\" + GetProjectName() + ".vpk\" --ip " + data.IP + " --usb " + boolToString(data.UseUSB) + " --storage-type " + data.storageType;

		if (data.ExtractOnPC)
        {
			args += " --pre-extract \"" + buildDir + "\"";
			args += " --extract";
		}
		else args += " --complete-vita-install";

		if (data.useUDCD)
			args += " --udcd \"" + data.udcdPath + "\"";

		ProcessStartInfo VitaFTPIStartInfo = new ProcessStartInfo();
		VitaFTPIStartInfo.Arguments = args;
		if(File.Exists(UploaderPath + "/Vita-FTPI-Core.exe"))
			VitaFTPIStartInfo.FileName = UploaderPath + "/Vita-FTPI-Core.exe";
		else
			VitaFTPIStartInfo.FileName = UploaderPath + "/VitaFTPI.exe";
		VitaFTPIStartInfo.WorkingDirectory = UploaderPath;
		Process VitaFTPI = new Process();
		VitaFTPI.StartInfo = VitaFTPIStartInfo;
		VitaFTPI.Start();
		UnityEngine.Debug.Log("Done!");
	}

	public static void PackVPK()
    {
		if (PreSetup() < 0)
			return;

		if (!File.Exists(LastBuildDirSavePath))
			return;

		if (!Directory.Exists(File.ReadAllText(LastBuildDirSavePath)))
			UnityEngine.Debug.Log("No build directory found!");

		if (File.Exists(data.UploaderFolder + "/" + GetProjectName() + ".vpk"))
			File.Delete(data.UploaderFolder + "/" + GetProjectName() + ".vpk");

		string args = "-i \"" + buildDir + "\" -o \"" + UploaderPath + "/" + GetProjectName() + "\"" + " -f -u -r -p";


		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = UploaderPath + "/UnityTools.exe";
		processStartInfo.Arguments = args;
		Process UnityTools = new Process();
		UnityTools.StartInfo = processStartInfo;
		UnityTools.Start();

		UnityEngine.Debug.Log("Done!");
	}

	[MenuItem("VitaFTPI/Pack VPK")]
	public static void BuildVPKMenu()
	{
		PackVPK();
	}

	static string boolToString(bool a)
    {
		if (a) return "true";
		return "false";
    }

	
	public static void BuildVPK(bool wait = false)
	{
		if(PreSetup() < 0)
			return;

		if(!File.Exists(LastBuildDirSavePath))
			return;
		
		if(!Directory.Exists(File.ReadAllText(LastBuildDirSavePath)))
			UnityEngine.Debug.Log("No build directory found!");
		
		string args = "-i \"" + buildDir + "\" -o \"" + UploaderPath + "\\" + GetProjectName() + "\"" + " -f -u -r";
		if (!data.ExtractOnPC)
			args += " -p";
		if (!data.KeepFolderAfterBuild)
			args += " -d";
		

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
}
