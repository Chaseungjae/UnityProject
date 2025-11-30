using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SettingToStart : MonoBehaviour
{
#if UNITY_EDITOR
    public SceneAsset sceneLoader;  // Inspector에서 이동할 씬 드롭
#endif
    private string sceneName;
    void Awake()
    {
#if UNITY_EDITOR
        if (sceneLoader != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneLoader);
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
#endif
    }

    public void OnButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
