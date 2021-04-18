using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

[ExecuteInEditMode]
public class UploadBuild 
{
	public static UploadData data = null;
	public static string UploaderPath = null;
	public static string LastBuildDirSavePath = Application.dataPath + "/VitaFTPI/LastBuildDir.txt";
	public static string buildDir = null;
	

	[PostProcessBuildAttribute(1)]
	public static void OnBuildEnd(BuildTarget target, string pathToBuiltProject)
	{
		if (!target.Equals(BuildTarget.PSP2))
			return;

		File.WriteAllText(LastBuildDirSavePath,pathToBuiltProject);
		if(data == null)
			data = JsonUtility.FromJson<UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		if(!data.startOnBuildEnd)
			return;

		UnityEngine.Debug.Log("Autorunning");

		if(PreSetup() < 0)
			return;
		
		BuildVPK(true);
		if (!data.UseReplaceInstallOnEnd) UploadVPK();
		else ReplaceInstall();
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
		if (PreSetup() < 0) return;
		ProcessStartInfo info = new ProcessStartInfo();
		info.FileName = UploaderPath + "/DebugPortReader.exe";
		info.RedirectStandardOutput = true;
		info.UseShellExecute = false;
		process = new Process();
		process.EnableRaisingEvents = true;
		process.OutputDataReceived += Process_OutputDataReceived;
		process.StartInfo = info;
		process.Start();
		process.BeginOutputReadLine();


		HasStarted = true;
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


	static int PreSetup()
    {
		if(buildDir == null)
			buildDir = File.ReadAllText(LastBuildDirSavePath);
		if(!Directory.Exists(buildDir))
        {
			UnityEngine.Debug.LogError("No build found!");
			return -1;
        }
		if(data == null && File.Exists(VitaFTPOptions.SavePath))
			data = JsonUtility.FromJson<UploadData>(File.ReadAllText(VitaFTPOptions.SavePath));
		else if(!File.Exists(VitaFTPOptions.SavePath))
		{
			UnityEngine.Debug.Log("Please configure options under VitaFTPI/Options");
			File.WriteAllText(VitaFTPOptions.SavePath,JsonUtility.ToJson(new UploadBuild()));
			return -1;
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
			UnityEngine.Debug.Log("No VPK found! Please build it first");
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
			UnityEngine.Debug.Log("No VPK found! Please build it first");
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

	[MenuItem("VitaFTPI/Run UnityTools")]
	public static void BuildVPKMenu()
	{
		BuildVPK(false);
	}

	static string boolToString(bool a)
    {
		if (a) return "true";
		return "false";
    }

	static void BuildVPK(bool wait = false)
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
