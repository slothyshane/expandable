using System.IO;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Manager;

    private string saveProgressFilePath;
    private string saveSceneFilePath;

    private void Awake()
    {
        if (Manager == null)
        {
            Manager = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes if needed
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        saveProgressFilePath = Path.Combine(Directory.GetCurrentDirectory(), "progressSave.json");
        saveSceneFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sceneSave.json");
    }

    public void SaveProgress(ProgressData dataToSave)
    {

        string jsonData = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(saveProgressFilePath, jsonData);
        Debug.Log("Progress Saved to: " + saveProgressFilePath);
    }

    public ProgressData LoadProgress()
    {
        if (File.Exists(saveProgressFilePath))
        {
            string jsonData = File.ReadAllText(saveProgressFilePath);
            ProgressData loadedData = JsonUtility.FromJson<ProgressData>(jsonData);
            Debug.Log("Progress Loaded from: " + saveProgressFilePath);
            return loadedData;
        }
        else
        {
            Debug.Log("No save file found. ");
            return null;
        }
    }

    public void SaveScene(SceneData sceneData)
    {
        string jsonData = JsonUtility.ToJson(sceneData, true);
        File.WriteAllText(saveSceneFilePath, jsonData);
        Debug.Log("Scene Saved to: " + saveSceneFilePath);
    }

    public SceneData LoadScene()
    {
        if (File.Exists(saveSceneFilePath))
        {
            string jsonData = File.ReadAllText(saveSceneFilePath);
            SceneData loadedData = JsonUtility.FromJson<SceneData>(jsonData);
            Debug.Log("Scene Loaded from: " + saveSceneFilePath);
            return loadedData;
        }
        else
        {
            Debug.Log("No save file found. ");
            return null;
        }
    }
}