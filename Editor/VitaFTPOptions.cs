using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class VitaFTPOptions : EditorWindow 
{
    public static bool UploadOnBuildEnd = false;
    public static string SavePath;
    public static Vector2 scrollView;
    private static UploadData uploadData;
    private static List<char> violatedLetters = new List<char>()
    {
        '\'',
        ']',
        '[',
        '{',
        '}',
        ':',
        '-',
        '+',
        '=',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        '0',
        '`',
        '~',
        '+',
        '_',
        '/',
        '\\',
        '|',
        ':',
        ';',
        '"',
        ',',
        '.',
        '<',
        '>'
    };

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
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        scrollView = EditorGUILayout.BeginScrollView(scrollView,false,false);
        if(uploadData == null)
            GetUploadData();
        uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end: ", uploadData.startOnBuildEnd);

        uploadData.KeepFolderAfterBuild = EditorGUILayout.Toggle("Keep build folder", uploadData.KeepFolderAfterBuild);
        uploadData.ExtractOnPC = EditorGUILayout.Toggle("Extract VPK on PC", uploadData.ExtractOnPC);

        GUILayout.Label("IP Adress: ", EditorStyles.largeLabel);
        uploadData.IP = EditorGUILayout.TextField(uploadData.IP,EditorStyles.numberField).Split(' ')[0];

        uploadData.UseUSB = EditorGUILayout.Toggle("Use USB: ",uploadData.UseUSB);

        if(uploadData.UseUSB)
        {
            GUILayout.Label("Drive Letter: ", EditorStyles.largeLabel);
            uploadData.DriveLetter = EditorGUILayout.TextField(uploadData.DriveLetter).Split(' ')[0];

            if(violatedLetters.Contains(uploadData.DriveLetter[0]))
            {
                Debug.Log("Invalid letter given changing to default");
                uploadData.DriveLetter = "D:";
            }
            else
            {
                uploadData.DriveLetter = uploadData.DriveLetter[0].ToString() + ":";
            }
            
            GUILayout.Label("Storage Type: ", EditorStyles.largeLabel);
            uploadData.storageType = GUILayout.TextField(uploadData.storageType,EditorStyles.textField);
            uploadData.storageType = uploadData.storageType.Split(' ')[0];
            
            if(uploadData.storageType.ToLower().Equals("official"))
            {
                uploadData.storageType = "OFFICIAL";
            }
            else if(uploadData.storageType.ToLower().Equals("sd2vita"))
            {
                uploadData.storageType = "sd2vita";
            }
        }

        uploadData.CustomUploaderFolder = EditorGUILayout.Toggle("Custom Uploader Folder: ", uploadData.CustomUploaderFolder);

        if(uploadData.CustomUploaderFolder)
        {
            if(GUILayout.Button("Browse"))
            {
                ThreadStart start = new ThreadStart(ShowDialog);
                Thread thread = new Thread(start);
                thread.Start();
            }
            EditorGUILayout.Space();
        }

        if(GUILayout.Button("Save Configuration"))
            File.WriteAllText(SavePath,JsonUtility.ToJson(uploadData));
        

        if(GUILayout.Button("Build VPK"))
            UploadBuild.BuildVPKMenu();
        if(GUILayout.Button("Upload VPK"))
            UploadBuild.UploadVPK();
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void ShowDialog()
    {
        System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
        System.Windows.Forms.DialogResult dialogResult = browserDialog.ShowDialog();
        if(dialogResult == System.Windows.Forms.DialogResult.OK)
        {
            uploadData.UploaderFolder = browserDialog.SelectedPath;
            UnityEngine.Debug.Log("Set Path to : " + uploadData.UploaderFolder);
        }
    }
}

