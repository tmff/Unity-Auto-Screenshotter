using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabImageCapture : EditorWindow
{
    private string path = "Assets/PathToPrefabs";
    private string savePath = "Assets/PathToSaveImages";
    private float cameraSize = 1.1f;

    [MenuItem("Tools/Prefab Image Capture")]
    public static void ShowWindow()
    {
        GetWindow<PrefabImageCapture>("Prefab Image Capture");
    }

    void OnGUI()
    {
        GUILayout.Label("Capture Prefab Images", EditorStyles.boldLabel);
        path = EditorGUILayout.TextField("Prefab Path", path);
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        cameraSize = EditorGUILayout.FloatField("Camera Size", cameraSize);

        if (GUILayout.Button("Capture Images"))
        {
            CaptureImages();
        }
    }

    void CaptureImages()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { path });
        Debug.Log("Found " + guids.Length + " prefabs");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // Set up camera and capture image
            Texture2D image = CaptureImage(instance);
            
            // Save image
            byte[] imageBytes = image.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(savePath, prefab.name + ".png"), imageBytes);

            DestroyImmediate(instance);
        }

        AssetDatabase.Refresh();
    }


    Texture2D CaptureImage(GameObject prefab)
    {
        Vector3 pos = new Vector3(0, -500, 0);
        // Set up camera and render texture logic here
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
        instance.transform.Rotate(new Vector3(0, 180, 0));
        //instance.transform.position = pos;

        // Example:
        RenderTexture renderTexture = new RenderTexture(256, 256, 24);
        Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, false);

        GameObject camObject = new GameObject("Camera");
        Camera camera = camObject.AddComponent<Camera>();
        camera.transform.position = pos + new Vector3(0, 0.5f, -10);
        camera.orthographic = true;
        camera.orthographicSize = cameraSize;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;


        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        DestroyImmediate(camObject);
        DestroyImmediate(instance);

        return texture2D;
    }
}
