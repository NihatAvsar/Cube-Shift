using UnityEditor;
using UnityEditor.SceneManagement;

namespace CubeShift.EditorTools
{
    /// <summary>Always starts editor Play Mode from the main menu, regardless of the open scene.</summary>
    [InitializeOnLoad]
    public static class CubeShiftPlayFromLevelOne
    {
        private const string StartScenePath = "Assets/Scenes/MainMenu.unity";

        static CubeShiftPlayFromLevelOne()
        {
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
        }

        [MenuItem("Tools/Cube Shift/Play From Main Menu")]
        public static void PlayFromMainMenu()
        {
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
