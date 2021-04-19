using UnityEngine;
using System.Net;
using System.IO;

[ExecuteInEditMode]
public class VitaFTPIUpdater
{
    static string UploadBuildRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/UploadBuild.cs";
    static string VitaFTPIOptionsRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/VitaFTPOptions.cs";
    static string UploadDataRemotePath = "https://raw.githubusercontent.com/Ibrahim778/VitaFTPI-Core/master/Editor/UploadData.cs";

    public static void Update()
    {
        using (WebClient client = new WebClient())
        {
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            File.WriteAllText(UploadBuild.Path, client.DownloadString(UploadBuildRemotePath));
            File.WriteAllText(VitaFTPOptions.Path, client.DownloadString(VitaFTPIOptionsRemotePath));
            File.WriteAllText(UploadWrapper.path, client.DownloadString(UploadDataRemotePath));
        }
    }
}
