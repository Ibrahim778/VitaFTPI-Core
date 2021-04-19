using UnityEditor;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public class VitaFTPOptions : EditorWindow 
{
    public static bool UploadOnBuildEnd = false;
    public static string SavePath;
    public static Vector2 scrollView;
    public string[] storageOptions = new string[] {"OFFICIAL", "sd2vita"};
    public string[] storageOptionsVisual = new string[] {"Official Storage", "SD2Vita (Gamecard Adapter)"};
    private static UploadWrapper.UploadData uploadData;
    public static string Path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();

    [MenuItem("VitaFTPI/Options")]
    public static void ShowWindow()
    {
        GetWindow<VitaFTPOptions>("VitaFTPI Options");
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
            File.WriteAllText(SavePath,JsonUtility.ToJson(new UploadWrapper.UploadData()));
        }
        uploadData = JsonUtility.FromJson<UploadWrapper.UploadData>(File.ReadAllText(SavePath));
    }

    void OnGUI()
    {
        GUILayoutOption width112 = GUILayout.Width(112);
#if !UNITY_PSP2
        EditorGUILayout.HelpBox("This is a PSVITA only tool, To use it set your build target to PSVITA!", MessageType.Warning);
        return;
#endif
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
        if (uploadData == null)
        {
            GetUploadData();
        }

        uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end", uploadData.startOnBuildEnd);
        if (uploadData.startOnBuildEnd)
        {
            uploadData.UseReplaceInstallOnEnd = EditorGUILayout.Toggle("Use replace install", uploadData.UseReplaceInstallOnEnd);
        }

        GUILayout.Space(7);
        uploadData.KeepFolderAfterBuild = EditorGUILayout.Toggle("Keep build folder", uploadData.KeepFolderAfterBuild);
        GUILayout.Space(5);
        uploadData.ExtractOnPC = EditorGUILayout.Toggle("Extract VPK on PC", uploadData.ExtractOnPC);

        EditorGUILayout.Space();
        GuiLine(1);
        EditorGUILayout.Space();

        GUILayout.Label("Connection Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("IP Adress");
        GUILayout.FlexibleSpace();
        uploadData.IP = EditorGUILayout.TextField(uploadData.IP, EditorStyles.numberField, width112).Split(' ')[0];
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        uploadData.UseUSB = EditorGUILayout.Toggle("Use USB", uploadData.UseUSB);
        if (uploadData.UseUSB)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Storage Type (PSVita)");
            uploadData.storageIndex = EditorGUILayout.Popup(uploadData.storageIndex, storageOptionsVisual);
            uploadData.storageType = storageOptions[uploadData.storageIndex];
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        uploadData.CustomUploaderFolder = EditorGUILayout.Toggle("Custom Uploader Folder", uploadData.CustomUploaderFolder);

        if (uploadData.CustomUploaderFolder)
        {
            if (GUILayout.Button("Browse", width112))
            {
                uploadData.UploaderFolder = EditorUtility.OpenFolderPanel("Select Uploader Folder", uploadData.UploaderFolder, "");
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        else GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Should save once you change a field.
        if (GUI.changed)
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
            AssetDatabase.ImportAsset("Assets/VitaFTPI/SaveConfig.txt");
            Debug.Log("Configuration Saved!");
        }


        EditorGUILayout.Space();
        GuiLine(1);
        EditorGUILayout.Space();


        // Install options

        GUILayout.Label("Actions", EditorStyles.boldLabel);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Game"))
        {
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            UploadBuild.BuildGame();
        }
        if (GUILayout.Button("Complete Install"))
            UploadBuild.UploadVPK();
        if (GUILayout.Button("Replace Install"))
            UploadBuild.ReplaceInstall();
        if (GUILayout.Button("Pack VPK"))
            UploadBuild.PackVPK();
        GUILayout.EndHorizontal();


        EditorGUILayout.Space();
        GuiLine(1);
        EditorGUILayout.Space();

        GUILayout.Label("Other", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Update"))
            VitaFTPIUpdater.Update();
        if (GUILayout.Button("Launch Game"))
            UploadBuild.sendCommand("launch " + Regex.Match(PlayerSettings.PSVita.contentID, "([A-Z][A-Z][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9])").Value);
        if (!UploadBuild.HasStarted)
        {
            if (GUILayout.Button("Start Debug"))
                UploadBuild.StartDebug();
        }
        else if (UploadBuild.HasStarted)
        {
            if (GUILayout.Button("Stop Debug"))
                UploadBuild.StopDebug();
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GuiLine(1);
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        GUILayout.Label("Made with <3 by Ibrahim");
        GUILayout.Label("Layout by Bizzy and iDevOnAParkingLot");

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

    }

    void GuiLine( int i_height = 1 )
    {
       Rect rect = EditorGUILayout.GetControlRect(false, i_height );
       rect.height = i_height;
       EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
    }

}

