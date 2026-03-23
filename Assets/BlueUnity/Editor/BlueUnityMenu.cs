using UnityEngine;
using UnityEditor;
using System.IO;

public static class BlueUnityMenu
{
    private const string BASE_PATH = "Assets/BlueUnity/Plugins/Android/";
    private const string SOURCE_MANIFEST = BASE_PATH + "AndroidManifest.xml";
    private const string TARGET_FOLDER = "Assets/Plugins/Android";
    private const string TARGET_MANIFEST = TARGET_FOLDER + "/AndroidManifest.xml";

    private static readonly string[] RequiredFiles =
    {
        "blueunity-release.aar",
        "kotlin-stdlib-1.9.24.jar",
        "gson-2.10.1.jar",
        "AndroidManifest.xml",
        "CallbackProxy.cs",
        "BluetoothHandler.cs"
    };


    [MenuItem("BlueUnity/Validate Plugin Files", priority = 0)]
    public static void ValidateFiles()
    {
        bool allGood = true;

        foreach (string filePath in RequiredFiles)
        {
            string path = Path.Combine(BASE_PATH, filePath);

            if (!File.Exists(path))
            {
                Debug.LogError("[BlueUnity] Missing file: " + path);
                allGood = false;
            }
            else
            {
                Debug.Log("<color=green>[BlueUnity] OK</color>: " + path);
            }
        }

        if (allGood)
        {
            Debug.Log("<color=green>[BlueUnity] All required files are present</color>");
            CopyManifest();
        }
        else
        {
            Debug.LogError("[BlueUnity] Some required files are missing");
        }
    }

    [MenuItem("BlueUnity/Setup Android Manifest", priority = 1)]
    public static void CopyManifest()
    {
        if (!Directory.Exists(TARGET_FOLDER))
            Directory.CreateDirectory(TARGET_FOLDER);

        if (!File.Exists(SOURCE_MANIFEST))
        {
            Debug.LogError("[BlueUnity] Source manifest not found at: " + SOURCE_MANIFEST);
            return;
        }

        File.Copy(SOURCE_MANIFEST, TARGET_MANIFEST, true);
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[BlueUnity] AndroidManifest copied successfully.</color>");
    }

}
