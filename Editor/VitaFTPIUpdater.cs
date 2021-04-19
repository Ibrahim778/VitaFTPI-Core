using UnityEngine;
using System.Net;
using System.IO;
using System;
using System.Threading;

[ExecuteInEditMode]
public class VitaFTPIUpdater
{
    static string UploadBuildRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/UploadBuild.cs";
    static string VitaFTPIOptionsRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/VitaFTPOptions.cs";
    static string UploadDataRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/UploadData.cs";
    static string ApiQuery = "https://api.github.com/repos/Ibrahim778/VitaFTPI-Core/releases/latest";

    public static void UpdateThread(object PersistentPath)
    {
        Debug.Log("Updating....");
        
        using (WebClient client = new WebClient())
        {
            client.DownloadProgressChanged += Progress;
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            Debug.Log("Downloading scripts...");
            File.WriteAllText(UploadBuild.Path, client.DownloadString(new Uri(UploadBuildRemotePath)));
            File.WriteAllText(VitaFTPOptions.Path, client.DownloadString(VitaFTPIOptionsRemotePath));
            File.WriteAllText(UploadWrapper.path, client.DownloadString(UploadDataRemotePath));
            
            client.Headers.Add("user-agent", "VitaFTPI Updater");
            Debug.Log("Downloading Uploader...");
            
            client.DownloadFile("https://github.com/Ibrahim778/VitaFTPI-Core/releases/download/" + GetVersionTag(client.DownloadString(ApiQuery)) + "/Tools.zip", (string)PersistentPath + "/tempfile.zip");
        }
        Debug.Log("Extracting...");
        System.Diagnostics.ProcessStartInfo extStartInfo = new System.Diagnostics.ProcessStartInfo();
        extStartInfo.FileName = "\"" + new FileInfo(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName()).DirectoryName + "\\Extractor\\Extractor.exe" + "\"";
        extStartInfo.Arguments = "\"" + (string)PersistentPath + "/tempfile.zip\"" + " \"" + (string)PersistentPath + "/Extracted\"";
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo = extStartInfo;
        process.Start();
        while (!process.HasExited)
            Thread.Sleep(1);
        Debug.Log("Extracted!");
        File.Delete(PersistentPath + "/tempfile.zip");
        
        DirectoryInfo newUploader = new DirectoryInfo(PersistentPath + "/Extracted/Tools/Uploader");
        DirectoryInfo oldUploader = new DirectoryInfo(UploadBuild.GetUploadDir());
        if (!Directory.Exists(oldUploader.FullName)) Directory.CreateDirectory(oldUploader.FullName);
        foreach (FileInfo file in newUploader.GetFiles())
        {
            Debug.Log("Copying : " + file.Name + " to :" + Path.Combine(oldUploader.FullName, file.Name));
            file.CopyTo(Path.Combine(oldUploader.FullName, file.Name), true);
        }
        Debug.Log("Done!");
    }

    private static void Progress(object sender, DownloadProgressChangedEventArgs e)
    {
        Debug.Log(e.ProgressPercentage.ToString());
    }

    public static void Update()
    {
        new Thread(UpdateThread).Start(Application.persistentDataPath);
    }

    static string GetVersionTag(string response)
    {
        string Ret = response.Split(new string[] { "\"name\":" }, StringSplitOptions.None)[1].Split(',')[0];
        return Ret.Trim('\"');
    }
}


