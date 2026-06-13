using System.Collections.Generic;
using System.IO;
using CubeShift.Cameras;
using CubeShift.Core;
using CubeShift.Player;
using CubeShift.Tiles;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace CubeShift.EditorTools
{
    /// <summary>
    /// Editor-only helper that creates a complete MVP test scene for the first playable prototype.
    /// </summary>
    public static class CubeShiftMvpSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CubeShift_MVP_Test.unity";
        private const int LevelSize = 6;

        [MenuItem("Tools/Cube Shift/Create MVP Test Scene")]
        public static void CreateMvpTestScene()
        {
            EnsureProjectFolders();

            Material normalTileMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/NormalTile.mat", new Color(0.32f, 0.34f, 0.38f));
            Material goalTileMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/GoalTile.mat", new Color(0.25f, 1f, 0.58f));
            Material cubeCoreMaterial = GetOrCreateMaterial("Assets/Materials/Player/CubeCore.mat", new Color(0.9f, 0.9f, 0.9f));
            Material whiteFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_White.mat", Color.white);
            Material blueFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Blue.mat", new Color(0.2f, 0.55f, 1f));
            Material redFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Red.mat", new Color(1f, 0.22f, 0.18f));
            Material yellowFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Yellow.mat", new Color(1f, 0.86f, 0.18f));
            Material greenFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Green.mat", new Color(0.2f, 0.95f, 0.35f));
            Material purpleFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Purple.mat", new Color(0.66f, 0.35f, 1f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.42f);

            LevelManager levelManager = CreateGameManager();
            Transform tilesRoot = CreateLevelTiles(normalTileMaterial, goalTileMaterial);
            PlayerCubeController player = CreatePlayer(
                cubeCoreMaterial,
                whiteFaceMaterial,
                blueFaceMaterial,
                redFaceMaterial,
                yellowFaceMaterial,
                greenFaceMaterial,
                purpleFaceMaterial,
                levelManager);

            CreateCamera(player.transform);
            CreateLighting();
            CreateUi(levelManager);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeTransform = player.transform;
            Debug.Log($"Cube Shift MVP test scene created at {ScenePath}. Level root: {tilesRoot.name}");
        }

        private static LevelManager CreateGameManager()
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            LevelManager levelManager = gameManagerObject.AddComponent<LevelManager>();

            SerializedObject serializedLevelManager = new SerializedObject(levelManager);
            serializedLevelManager.FindProperty("levelNumberOverride").intValue = 1;
            serializedLevelManager.FindProperty("loadNextSceneWhenAvailable").boolValue = false;
            serializedLevelManager.ApplyModifiedPropertiesWithoutUndo();

            return levelManager;
        }

        private static Transform CreateLevelTiles(Material normalTileMaterial, Material goalTileMaterial)
        {
            GameObject levelRoot = new GameObject("LevelRoot");
            GameObject tilesRoot = new GameObject("Tiles");
            tilesRoot.transform.SetParent(levelRoot.transform);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0)
            {
                groundLayer = 0;
            }

            HashSet<Vector2Int> holes = new HashSet<Vector2Int>
            {
                new Vector2Int(1, 2),
                new Vector2Int(2, 4),
                new Vector2Int(3, 1),
                new Vector2Int(4, 3)
            };

            for (int x = 0; x < LevelSize; x++)
            {
                for (int z = 0; z < LevelSize; z++)
                {
                    Vector2Int cell = new Vector2Int(x, z);
                    if (holes.Contains(cell))
                    {
                        continue;
                    }

                    bool isGoal = x == LevelSize - 1 && z == LevelSize - 1;
                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = isGoal ? $"GoalTile_{x}_{z}" : $"NormalTile_{x}_{z}";
                    tile.transform.SetParent(tilesRoot.transform);
                    tile.transform.position = new Vector3(x, -0.1f, z);
                    tile.transform.localScale = new Vector3(0.96f, 0.2f, 0.96f);
                    tile.layer = groundLayer;

                    Renderer renderer = tile.GetComponent<Renderer>();
                    renderer.sharedMaterial = isGoal ? goalTileMaterial : normalTileMaterial;

                    if (isGoal)
                    {
                        tile.AddComponent<GoalTile>();
                    }
                    else
                    {
                        tile.AddComponent<NormalTile>();
                    }
                }
            }

            return tilesRoot.transform;
        }

        private static PlayerCubeController CreatePlayer(
            Material cubeCoreMaterial,
            Material whiteFaceMaterial,
            Material blueFaceMaterial,
            Material redFaceMaterial,
            Material yellowFaceMaterial,
            Material greenFaceMaterial,
            Material purpleFaceMaterial,
            LevelManager levelManager)
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerObject.name = "Player";
            playerObject.transform.position = new Vector3(0f, 0.5f, 0f);
            playerObject.tag = "Player";

            Renderer renderer = playerObject.GetComponent<Renderer>();
            renderer.sharedMaterial = cubeCoreMaterial;

            CreateFaceQuad(playerObject.transform, "Face_Top_Blue", new Vector3(0f, 0.501f, 0f), Quaternion.Euler(-90f, 0f, 0f), blueFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Face_Bottom_White", new Vector3(0f, -0.501f, 0f), Quaternion.Euler(90f, 0f, 0f), whiteFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Face_Front_Green", new Vector3(0f, 0f, 0.501f), Quaternion.identity, greenFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Face_Back_Purple", new Vector3(0f, 0f, -0.501f), Quaternion.Euler(0f, 180f, 0f), purpleFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Face_Left_Red", new Vector3(-0.501f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), redFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Face_Right_Yellow", new Vector3(0.501f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), yellowFaceMaterial);

            CubeFaceTracker faceTracker = playerObject.AddComponent<CubeFaceTracker>();
            PlayerCubeController controller = playerObject.AddComponent<PlayerCubeController>();
            PlayerInputHandler inputHandler = playerObject.AddComponent<PlayerInputHandler>();

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("faceTracker").objectReferenceValue = faceTracker;
            serializedController.FindProperty("levelManager").objectReferenceValue = levelManager;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                serializedController.FindProperty("groundLayer").intValue = 1 << groundLayer;
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInput = new SerializedObject(inputHandler);
            serializedInput.FindProperty("player").objectReferenceValue = controller;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        private static void CreateFaceQuad(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Material material)
        {
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = name;
            face.transform.SetParent(parent, false);
            face.transform.localPosition = localPosition;
            face.transform.localRotation = localRotation;
            face.transform.localScale = Vector3.one * 0.96f;

            Collider faceCollider = face.GetComponent<Collider>();
            if (faceCollider != null)
            {
                Object.DestroyImmediate(faceCollider);
            }

            Renderer renderer = face.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
        }

        private static void CreateCamera(Transform player)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = player.position + new Vector3(6f, 7f, -6f);
            cameraObject.transform.LookAt(player.position);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.045f, 0.055f);

            cameraObject.AddComponent<AudioListener>();
            CameraFollow cameraFollow = cameraObject.AddComponent<CameraFollow>();

            SerializedObject serializedCameraFollow = new SerializedObject(cameraFollow);
            serializedCameraFollow.FindProperty("target").objectReferenceValue = player;
            serializedCameraFollow.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.15f;
        }

        private static void CreateUi(LevelManager levelManager)
        {
            GameObject canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            TMP_Text levelText = CreateText(canvasObject.transform, "LevelText", "Level 1", 42f, TextAlignmentOptions.TopLeft);
            RectTransform levelRect = levelText.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0f, 1f);
            levelRect.anchorMax = new Vector2(0f, 1f);
            levelRect.pivot = new Vector2(0f, 1f);
            levelRect.anchoredPosition = new Vector2(32f, -28f);
            levelRect.sizeDelta = new Vector2(320f, 80f);

            Button restartButton = CreateRestartButton(canvasObject.transform, levelManager);
            RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-32f, -28f);
            buttonRect.sizeDelta = new Vector2(190f, 58f);

            GameObject panel = CreateCompletePanel(canvasObject.transform);
            panel.SetActive(false);

            SerializedObject serializedLevelManager = new SerializedObject(levelManager);
            serializedLevelManager.FindProperty("levelText").objectReferenceValue = levelText;
            serializedLevelManager.FindProperty("levelCompletePanel").objectReferenceValue = panel;
            serializedLevelManager.ApplyModifiedPropertiesWithoutUndo();

            CreateEventSystem();
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = alignment;
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = 18f;
            textComponent.fontSizeMax = fontSize;

            return textComponent;
        }

        private static Button CreateRestartButton(Transform parent, LevelManager levelManager)
        {
            GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.14f, 0.18f, 0.86f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            UnityEventTools.AddPersistentListener(button.onClick, levelManager.RestartLevel);

            TMP_Text label = CreateText(buttonObject.transform, "Label", "Restart", 30f, TextAlignmentOptions.Center);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        private static GameObject CreateCompletePanel(Transform parent)
        {
            GameObject panelObject = new GameObject("WinPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image image = panelObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);

            TMP_Text completeText = CreateText(panelObject.transform, "LevelCompleteText", "Level Complete", 58f, TextAlignmentOptions.Center);
            RectTransform textRect = completeText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(620f, 120f);

            return panelObject;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(path)
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureProjectFolders()
        {
            EnsureProjectFolder("Assets/Scripts");
            EnsureProjectFolder("Assets/Scripts/Player");
            EnsureProjectFolder("Assets/Scripts/Core");
            EnsureProjectFolder("Assets/Scripts/Tiles");
            EnsureProjectFolder("Assets/Scripts/Camera");
            EnsureProjectFolder("Assets/Scripts/UI");
            EnsureProjectFolder("Assets/Prefabs");
            EnsureProjectFolder("Assets/Prefabs/Player");
            EnsureProjectFolder("Assets/Prefabs/Tiles");
            EnsureProjectFolder("Assets/Materials");
            EnsureProjectFolder("Assets/Materials/Player");
            EnsureProjectFolder("Assets/Materials/Tiles");
            EnsureProjectFolder("Assets/Scenes");
            EnsureProjectFolder("Assets/Art");
            EnsureProjectFolder("Assets/Audio");
        }

        private static void EnsureProjectFolder(string path)
        {
            if (path == "Assets" || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(parent))
            {
                return;
            }

            EnsureProjectFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int index = scenes.Count - 1; index >= 0; index--)
            {
                if (scenes[index].path == scenePath)
                {
                    scenes.RemoveAt(index);
                }
            }

            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
