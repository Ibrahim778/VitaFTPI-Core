using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Diagnostics;


[ExecuteInEditMode]
public class PostBuild {

    public static UploadData data = new UploadData();
    public static string UploaderPath;

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
    {
	    UploaderPath = System.Text.RegularExpressions.Regex.Replace(Application.dataPath,"Assets","Uploader");

        if(!Directory.Exists(UploaderPath))
        {
            UnityEngine.Debug.Log("Uploader folder not found exiting!");
            return;
        }
                
        string Args = "-i \"" + pathToBuiltProject + "\" -o \"" + UploaderPath + "/" + data.File_Name + "\"" + " -f -u -r -p -d";
        UnityEngine.Debug.Log(Args);
        Process UnityTools = new Process();
        UnityTools.StartInfo.FileName = UploaderPath + "/UnityTools.exe";
        UnityTools.StartInfo.Arguments = Args;
        UnityTools.EnableRaisingEvents = true;
        UnityTools.Exited += new System.EventHandler(ProcessExit);
        UnityTools.Start();
    }

    private static void ProcessExit(object sender, System.EventArgs e)
    {
        
        Process VitaFTPI = new Process();
        VitaFTPI.StartInfo.FileName = UploaderPath + "/VitaFTPI.exe";
        string Args = "--ip " + data.IP + " --vpk \"" + UploaderPath + "/" + data.File_Name + ".vpk\"" + " --usb " + boolToString(data.UseUSB) + " --drive-letter " + data.DriveLetter + " --storage-type " + StorageTypeToString(data.storageType);
        UnityEngine.Debug.Log(Args);
        VitaFTPI.StartInfo.Arguments = Args;
        VitaFTPI.Start();
    }

    static string boolToString(bool input)
    {
        if (input)
        {
            return "true";
        }
        else
            return "false";
    }

    static string StorageTypeToString(StorageType st)
    {
        if(st.Equals(StorageType.OFFICIAL))
            return "OFFICIAL";
        
        if(st.Equals(StorageType.sd2vita))
            return "sd2vita";

        return "Unconfigured";
    }
}

public class UploadData
{
    //The only reason I made this class was so you can change the values easily without breaking the other code :)
    public string IP = "192.168.18.8";

    //No real need to change this.
    public string File_Name = "Build";

    //Only use this when UseUSB in set to true. This will transfer the VPK over usb but still install it via ftp so the ftpanywhere plugin is required.
    public string DriveLetter = "D:";

    // Transfer the VPK via USB instead of ftp.
    public bool UseUSB = true;

    //If UseUSB = true you need to set this to your storage (Memory card) type (sd2vita or OFFICIAL) if useUSB is false then you can ignore this.
    public StorageType storageType = StorageType.sd2vita;
}

public enum StorageType
{
    OFFICIAL,
    sd2vita
}
