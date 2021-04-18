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
    public string[] storageOptions = new string[] {"OFFICIAL", "sd2vita"};
    public string[] storageOptionsVisual = new string[] {"Official Storage", "SD2Vita (Gamecard Adapter)"};
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
        //EditorGUILayout.HelpBox("Remember to save your configuration every time you make a change before doing anything", MessageType.Info);
#endif


        if(File.Exists(Application.dataPath + "/VitaFTPI/OLDLAYOUT"))
        {
            Debug.Log("using old layout!");
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
            if (uploadData == null)
                GetUploadData();
            uploadData.startOnBuildEnd = EditorGUILayout.Toggle("Start on build end", uploadData.startOnBuildEnd);
            if (uploadData.startOnBuildEnd)
            {
                uploadData.UseReplaceInstallOnEnd = EditorGUILayout.Toggle("Use replace install", uploadData.UseReplaceInstallOnEnd);
            }
            uploadData.KeepFolderAfterBuild = EditorGUILayout.Toggle("Keep build folder", uploadData.KeepFolderAfterBuild);
            uploadData.ExtractOnPC = EditorGUILayout.Toggle("Extract VPK on PC", uploadData.ExtractOnPC);

            GUILayout.Label("IP Adress: ", EditorStyles.largeLabel);
            uploadData.IP = EditorGUILayout.TextField(uploadData.IP, EditorStyles.numberField).Split(' ')[0];
            uploadData.UseUSB = EditorGUILayout.Toggle("Use USB", uploadData.UseUSB);
            if (uploadData.UseUSB)
            {
                GUILayout.Label("Storage Type (PSVita)", EditorStyles.largeLabel);
				uploadData.storageIndex = EditorGUILayout.Popup(uploadData.storageIndex, storageOptionsVisual);
				uploadData.storageType = storageOptions[uploadData.storageIndex];
            }

            uploadData.CustomUploaderFolder = EditorGUILayout.Toggle("Custom Uploader Folder", uploadData.CustomUploaderFolder);

            if (uploadData.CustomUploaderFolder)
            {
                if (GUILayout.Button("Browse"))
                {
					uploadData.UploaderFolder = EditorUtility.OpenFolderPanel("Select Uploader Folder", uploadData.UploaderFolder, "");
                }
                EditorGUILayout.Space();
            }

			if(GUI.changed){
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
				AssetDatabase.ImportAsset("Assets/VitaFTPI/SaveConfig.txt");
                Debug.Log("Configuration Saved!");
			}
            if (GUILayout.Button("Save Configuration")){
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
				AssetDatabase.ImportAsset("Assets/VitaFTPI/SaveConfig.txt");
                Debug.Log("Configuration Saved!");
			}

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
			// new style
			
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            GUILayout.Label("Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            scrollView = EditorGUILayout.BeginScrollView(scrollView, false, false);
            if (uploadData == null){
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
			GuiLine(1);
			EditorGUILayout.Space();

			// Other options
            GUILayout.Label("Other Settings", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
			
			// Should save once you change a field.
			if(GUI.changed){
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
				AssetDatabase.ImportAsset("Assets/VitaFTPI/SaveConfig.txt");
                Debug.Log("Configuration Saved!");
			}
			// Just in case manual Save
            if (GUILayout.Button("(Manual)Save Configuration", GUILayout.Width(200)))
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(uploadData));
				AssetDatabase.ImportAsset("Assets/VitaFTPI/SaveConfig.txt");
                Debug.Log("Configuration Saved!");
            }
            GUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			GuiLine(1);
			EditorGUILayout.Space();


			// Install options
			
            GUILayout.Label("Install actions", EditorStyles.boldLabel);
			GUILayout.Space(8);

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


			EditorGUILayout.Space();
			GuiLine(1);
			EditorGUILayout.Space();

			
			EditorGUILayout.Space();
            GUILayout.Label("Credits: ");
            GUILayout.Label("VitaFTPI by Ibrahim778");
            GUILayout.Label("https://github.com/Ibrahim778/VitaFTPI-Core");

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

void GuiLine( int i_height = 1 )

   {

       Rect rect = EditorGUILayout.GetControlRect(false, i_height );

       rect.height = i_height;

       EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );

   }

}

