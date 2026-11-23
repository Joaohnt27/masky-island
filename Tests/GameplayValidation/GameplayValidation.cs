using UnityEditor;
using UnityEditor.SceneManagement;

public static class GameplayValidation
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var settings = UnityEngine.Object.FindAnyObjectByType<SettingsMenuUI>();
        var serialized = new SerializedObject(settings);
        if (serialized.FindProperty("settingsPanel").objectReferenceValue == null)
            throw new System.Exception("SettingsPanel must be saved and editable before Play Mode.");
        EditorSceneManager.OpenScene("Assets/Scenes/Splash.unity");
        EditorApplication.EnterPlaymode();
    }
}
