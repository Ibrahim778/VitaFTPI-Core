using UnityEditor;
using System.IO;
using UnityEngine;

public class VitaFTPOptions : EditorWindow 
{
    public static bool UploadOnBuildEnd = false;
    public static string SavePath;
    private static UploadData uploadData;

    [MenuItem("VitaFTPI/Options")]
    public static void ShowWindow()
    {
        GetWindow<VitaFTPOptions>("Upload Options");
    }

    void OnEnable()
    {
        SavePath = Application.dataPath + "\\SaveConfig.txt";
        GetUploadData();
    }

    static void GetUploadData()
    {
        if(!File.Exists(SavePath))
        {
            File.WriteAllText(SavePath,JsonUtility.ToJson(new UploadData()));
        }
        uploadData = JsonUtility.FromJson<UploadData>(File.ReadAllText(SavePath));
    }

    void OnGUI()
    {
        if(uploadData == null)
            GetUploadData();
        uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end: ", uploadData.startOnBuildEnd);

        GUILayout.Label("IP Adress: ", EditorStyles.largeLabel);
        uploadData.IP = EditorGUILayout.TextField(uploadData.IP,EditorStyles.numberField).Split(' ')[0];

        uploadData.UseUSB = EditorGUILayout.Toggle("Use USB: ",uploadData.UseUSB);

        if(uploadData.UseUSB)
        {
            GUILayout.Label("Drive Letter: ", EditorStyles.largeLabel);
            uploadData.DriveLetter = EditorGUILayout.TextField(uploadData.DriveLetter).Split(' ')[0];
            
            GUILayout.Label("Storage Type: ", EditorStyles.largeLabel);
            uploadData.storageType = GUILayout.TextField(uploadData.storageType,EditorStyles.textField);
            if(GetWordLength(uploadData.storageType) != 1)
                uploadData.storageType = uploadData.storageType.Split(' ')[0];
        }

        if(GUILayout.Button("Save Configuration"))
            File.WriteAllText(SavePath,JsonUtility.ToJson(uploadData));
        

        if(GUILayout.Button("Build VPK"))
            UploadBuild.BuildVPKMenu();
        if(GUILayout.Button("Upload VPK"))
            UploadBuild.UploadVPK();
    }

    int GetWordLength(string words)
    {
        int WordCount = 0;
        int index = 0;
        while(index < words.Length && char.IsWhiteSpace(words[index]))
            index++;

        while(index < words.Length)
        {
            while(index < words.Length && !char.IsWhiteSpace(words[index]))
                index++;
            
            WordCount++;

            while(index < words.Length && char.IsWhiteSpace(words[index]))
                index++;
        }

        return WordCount;
    }
}

