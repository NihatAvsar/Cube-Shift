using System.Collections.Generic;
using System.Linq;
using CubeShift.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeShift.EditorTools
{
    public static class CubeShiftMainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Tools/Cube Shift/Create Main Menu")]
        public static void CreateMainMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("MainMenuController").AddComponent<MainMenuController>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(item => item.path != ScenePath)
                .ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
