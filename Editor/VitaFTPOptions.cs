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
#if UNITY_PSP2
        if (!Directory.Exists(Application.dataPath + "/VitaFTPI"))
        {
            Directory.CreateDirectory(Application.dataPath + "/VitaFTPI");
        }
        SavePath = Application.dataPath + "/VitaFTPI/SaveConfig.txt";
        GetUploadData();
#endif
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
        GUILayoutOption width112 = GUILayout.Width(112);
#if !UNITY_PSP2
        EditorGUILayout.HelpBox("This is a PSVITA only tool, To use it set your build target to PSVITA!", MessageType.Warning);
        return;
#else
        EditorGUILayout.HelpBox("Remember to save your configuration every time you make a change before doing anything", MessageType.Info);
#endif

        if(File.Exists(Application.dataPath + "/VitaFTPI/OLDLAYOUT"))
        {
            Debug.Log("using old layout!");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
            if (uploadData == null)
                GetUploadData();
            uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end: ", uploadData.startOnBuildEnd);
            if (uploadData.startOnBuildEnd)
            {
                uploadData.UseReplaceInstallOnEnd = EditorGUILayout.Toggle("Use replace install", uploadData.UseReplaceInstallOnEnd);
            }
            uploadData.KeepFolderAfterBuild = EditorGUILayout.Toggle("Keep build folder", uploadData.KeepFolderAfterBuild);
            uploadData.ExtractOnPC = EditorGUILayout.Toggle("Extract VPK on PC", uploadData.ExtractOnPC);

            GUILayout.Label("IP Adress: ", EditorStyles.largeLabel);
            uploadData.IP = EditorGUILayout.TextField(uploadData.IP, EditorStyles.numberField).Split(' ')[0];
            uploadData.UseUSB = EditorGUILayout.Toggle("Use USB: ", uploadData.UseUSB);
            if (uploadData.UseUSB)
            {
                GUILayout.Label("Storage Type: ", EditorStyles.largeLabel);
                uploadData.storageType = GUILayout.TextField(uploadData.storageType, EditorStyles.textField);
                uploadData.storageType = uploadData.storageType.Split(' ')[0];

                if (uploadData.storageType.ToLower().Equals("official"))
                {
                    uploadData.storageType = "OFFICIAL";
                }
                else if (uploadData.storageType.ToLower().Equals("sd2vita"))
                {
                    uploadData.storageType = "sd2vita";
                }
            }

            uploadData.CustomUploaderFolder = EditorGUILayout.Toggle("Custom Uploader Folder: ", uploadData.CustomUploaderFolder);

            if (uploadData.CustomUploaderFolder)
            {
                if (GUILayout.Button("Browse"))
                {
                    ThreadStart start = new ThreadStart(ShowDialog);
                    Thread thread = new Thread(start);
                    thread.Start();
                }
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Save Configuration"))
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));


            if (GUILayout.Button("Build VPK"))
                UploadBuild.BuildVPKMenu();
            if (GUILayout.Button("Complete Install"))
                UploadBuild.UploadVPK();
            if (GUILayout.Button("Replace Install"))
                UploadBuild.ReplaceInstall();
            if (!UploadBuild.HasStarted)
            {
                if (GUILayout.Button("Start Debug"))
                    UploadBuild.StartDebug();
            }
            else if(UploadBuild.HasStarted)
            {
                if (GUILayout.Button("Stop Debug"))
                    UploadBuild.StopDebug();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
            if (uploadData == null)
                GetUploadData();
            uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end: ", uploadData.startOnBuildEnd);
            if (uploadData.startOnBuildEnd)
            {
                uploadData.UseReplaceInstallOnEnd = EditorGUILayout.Toggle("Use replace install", uploadData.UseReplaceInstallOnEnd);
            }
            uploadData.KeepFolderAfterBuild = EditorGUILayout.Toggle("Keep build folder", uploadData.KeepFolderAfterBuild);
            uploadData.ExtractOnPC = EditorGUILayout.Toggle("Extract VPK on PC", uploadData.ExtractOnPC);
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP Adress: ", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            uploadData.IP = EditorGUILayout.TextField(uploadData.IP, EditorStyles.numberField, width112).Split(' ')[0];
            GUILayout.EndHorizontal();
            uploadData.UseUSB = EditorGUILayout.Toggle("Use USB: ", uploadData.UseUSB);
            if (uploadData.UseUSB)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Storage Type: ", EditorStyles.largeLabel);
                uploadData.storageType = GUILayout.TextField(uploadData.storageType, EditorStyles.textField, width112);
                uploadData.storageType = uploadData.storageType.Split(' ')[0];

                if (uploadData.storageType.ToLower().Equals("official"))
                {
                    uploadData.storageType = "OFFICIAL";
                }
                else if (uploadData.storageType.ToLower().Equals("sd2vita"))
                {
                    uploadData.storageType = "sd2vita";
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            uploadData.CustomUploaderFolder = EditorGUILayout.Toggle("Custom Uploader Folder: ", uploadData.CustomUploaderFolder);

            if (uploadData.CustomUploaderFolder)
            {
                if (GUILayout.Button("Browse", width112))
                {
                    ThreadStart start = new ThreadStart(ShowDialog);
                    Thread thread = new Thread(start);
                    thread.Start();
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            else GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save Configuration", GUILayout.Width(120)))
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
                Debug.Log("Configuration Saved!");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(12);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Complete Install"))
                UploadBuild.UploadVPK();
            if (GUILayout.Button("Replace Install"))
                UploadBuild.ReplaceInstall();
            if (!UploadBuild.HasStarted)
            {
                if (GUILayout.Button("Start Debug"))
                    UploadBuild.StartDebug();
            }
            else if(UploadBuild.HasStarted)
            {
                if (GUILayout.Button("Stop Debug"))
                    UploadBuild.StopDebug();
            }
            GUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    void ShowDialog()
    {
        System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
        System.Windows.Forms.DialogResult dialogResult = browserDialog.ShowDialog();
        if(dialogResult == System.Windows.Forms.DialogResult.OK)
        {
            uploadData.UploaderFolder = browserDialog.SelectedPath;
            Debug.Log("Set Path to : " + uploadData.UploaderFolder);
        }
    }
}

