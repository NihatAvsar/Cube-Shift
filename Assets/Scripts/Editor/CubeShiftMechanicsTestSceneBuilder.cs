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
    /// Creates a compact playable scene with every face-tile interaction already wired.
    /// </summary>
    public static class CubeShiftMechanicsTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CubeShift_Mechanics_Test.unity";

        [MenuItem("Tools/Cube Shift/Create Mechanics Test Scene")]
        public static void CreateMechanicsTestScene()
        {
            Material normalMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/NormalTile.mat", new Color(0.32f, 0.34f, 0.38f));
            Material breakableMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/BreakableTile.mat", new Color(0.85f, 0.22f, 0.2f));
            Material buttonMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/ButtonTile.mat", new Color(1f, 0.8f, 0.12f));
            Material doorMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/DoorTile.mat", new Color(0.95f, 0.45f, 0.12f));
            Material iceMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/IceTile.mat", new Color(0.2f, 0.75f, 1f));
            Material jumpMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/JumpTile.mat", new Color(0.2f, 0.95f, 0.38f));
            Material goalMaterial = GetOrCreateMaterial("Assets/Materials/Tiles/GoalTile.mat", new Color(0.25f, 1f, 0.58f));
            Material cubeMaterial = GetOrCreateMaterial("Assets/Materials/Player/CubeCore.mat", new Color(0.9f, 0.9f, 0.9f));
            Material whiteFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_White.mat", Color.white);
            Material blueFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Blue.mat", new Color(0.2f, 0.55f, 1f));
            Material redFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Red.mat", new Color(1f, 0.22f, 0.18f));
            Material yellowFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Yellow.mat", new Color(1f, 0.86f, 0.18f));
            Material greenFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Green.mat", new Color(0.2f, 0.95f, 0.35f));
            Material purpleFaceMaterial = GetOrCreateMaterial("Assets/Materials/Player/Face_Purple.mat", new Color(0.66f, 0.35f, 1f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.42f);

            LevelManager levelManager = CreateGameManager();
            Transform tilesRoot = new GameObject("LevelRoot").transform;

            CreatePath(tilesRoot, normalMaterial, breakableMaterial, buttonMaterial, doorMaterial, iceMaterial, jumpMaterial, goalMaterial);
            PlayerCubeController player = CreatePlayer(
                cubeMaterial,
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
            Debug.Log($"Cube Shift mechanics test scene created at {ScenePath}.");
        }

        private static void CreatePath(
            Transform root,
            Material normalMaterial,
            Material breakableMaterial,
            Material buttonMaterial,
            Material doorMaterial,
            Material iceMaterial,
            Material jumpMaterial,
            Material goalMaterial)
        {
            GameObject tilesObject = new GameObject("Tiles");
            tilesObject.transform.SetParent(root);
            Transform tilesRoot = tilesObject.transform;

            CreateTile<NormalTile>(tilesRoot, "START", new Vector2Int(0, 0), normalMaterial);

            // One move left puts Red on the bottom and immediately breaks this tile.
            BreakableTile breakable = CreateTile<BreakableTile>(
                tilesRoot,
                "LEFT_Red_Breaks",
                new Vector2Int(-1, 0),
                breakableMaterial);
            SerializedObject serializedBreakable = new SerializedObject(breakable);
            serializedBreakable.FindProperty("safeExitDuration").floatValue = 1f;
            serializedBreakable.FindProperty("blinkDuration").floatValue = 3f;
            serializedBreakable.ApplyModifiedPropertiesWithoutUndo();
            CreateTile<NormalTile>(tilesRoot, "BreakableExit", new Vector2Int(-2, 0), normalMaterial);

            // One move right puts Yellow on the bottom and opens the connected door.
            ButtonTile button = CreateTile<ButtonTile>(tilesRoot, "RIGHT_Yellow_OpensDoor", new Vector2Int(1, 0), buttonMaterial);
            DoorTile door = CreateDoorTile(tilesRoot, new Vector2Int(2, 0), normalMaterial, doorMaterial);
            CreateTile<GoalTile>(tilesRoot, "Goal_BehindDoor", new Vector2Int(3, 0), goalMaterial);

            // One move up puts Green on the bottom and jumps across the missing cell.
            CreateTile<JumpTile>(tilesRoot, "UP_Green_Jumps", new Vector2Int(0, 1), jumpMaterial);
            CreateTile<NormalTile>(tilesRoot, "JumpLanding", new Vector2Int(0, 3), normalMaterial);

            // Moving down enters ice with Purple down, so the player slides automatically.
            CreateTile<IceTile>(tilesRoot, "DOWN_Ice_Slide_1", new Vector2Int(0, -1), iceMaterial);
            CreateTile<IceTile>(tilesRoot, "Ice_Slide_2", new Vector2Int(0, -2), iceMaterial);
            CreateTile<IceTile>(tilesRoot, "Ice_Slide_3", new Vector2Int(0, -3), iceMaterial);
            CreateTile<NormalTile>(tilesRoot, "IceLanding", new Vector2Int(0, -4), normalMaterial);

            SerializedObject serializedButton = new SerializedObject(button);
            SerializedProperty doors = serializedButton.FindProperty("controlledDoors");
            doors.arraySize = 1;
            doors.GetArrayElementAtIndex(0).objectReferenceValue = door;
            serializedButton.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T CreateTile<T>(Transform parent, string name, Vector2Int cell, Material material)
            where T : TileBase
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = name;
            tile.transform.SetParent(parent);
            tile.transform.position = new Vector3(cell.x, -0.1f, cell.y);
            tile.transform.localScale = new Vector3(0.96f, 0.2f, 0.96f);
            tile.layer = GroundLayer;
            tile.GetComponent<Renderer>().sharedMaterial = material;
            return tile.AddComponent<T>();
        }

        private static DoorTile CreateDoorTile(Transform parent, Vector2Int cell, Material floorMaterial, Material doorMaterial)
        {
            DoorTile door = CreateTile<DoorTile>(parent, "DoorTile_YellowGate", cell, floorMaterial);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "DoorVisual";
            visual.transform.SetParent(door.transform, false);
            visual.transform.localPosition = new Vector3(0f, 3f, 0f);
            visual.transform.localScale = new Vector3(0.8f, 5f, 0.15f);
            visual.GetComponent<Renderer>().sharedMaterial = doorMaterial;

            Object.DestroyImmediate(visual.GetComponent<Collider>());

            SerializedObject serializedDoor = new SerializedObject(door);
            serializedDoor.FindProperty("doorVisual").objectReferenceValue = visual.transform;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();
            return door;
        }

        private static LevelManager CreateGameManager()
        {
            LevelManager manager = new GameObject("GameManager").AddComponent<LevelManager>();
            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("levelNumberOverride").intValue = 2;
            serializedManager.FindProperty("loadNextSceneWhenAvailable").boolValue = false;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            return manager;
        }

        private static PlayerCubeController CreatePlayer(
            Material material,
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
            playerObject.tag = "Player";
            playerObject.transform.position = new Vector3(0f, 0.5f, 0f);
            playerObject.GetComponent<Renderer>().sharedMaterial = material;

            CreateFaceQuad(playerObject.transform, "Top_Blue", new Vector3(0f, 0.501f, 0f), Quaternion.Euler(-90f, 0f, 0f), blueFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Bottom_White", new Vector3(0f, -0.501f, 0f), Quaternion.Euler(90f, 0f, 0f), whiteFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Front_Green", new Vector3(0f, 0f, 0.501f), Quaternion.identity, greenFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Back_Purple", new Vector3(0f, 0f, -0.501f), Quaternion.Euler(0f, 180f, 0f), purpleFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Left_Red", new Vector3(-0.501f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), redFaceMaterial);
            CreateFaceQuad(playerObject.transform, "Right_Yellow", new Vector3(0.501f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), yellowFaceMaterial);

            CubeFaceTracker tracker = playerObject.AddComponent<CubeFaceTracker>();
            PlayerCubeController controller = playerObject.AddComponent<PlayerCubeController>();
            PlayerInputHandler input = playerObject.AddComponent<PlayerInputHandler>();

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("faceTracker").objectReferenceValue = tracker;
            serializedController.FindProperty("levelManager").objectReferenceValue = levelManager;
            serializedController.FindProperty("groundLayer").intValue = 1 << GroundLayer;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInput = new SerializedObject(input);
            serializedInput.FindProperty("player").objectReferenceValue = controller;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void CreateFaceQuad(Transform parent, string name, Vector3 position, Quaternion rotation, Material material)
        {
            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = name;
            face.transform.SetParent(parent, false);
            face.transform.localPosition = position;
            face.transform.localRotation = rotation;
            face.transform.localScale = Vector3.one * 0.96f;
            face.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(face.GetComponent<Collider>());
        }

        private static void CreateCamera(Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = target.position + new Vector3(7f, 8f, -7f);
            cameraObject.transform.LookAt(target.position);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.04f, 0.045f, 0.055f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();

            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            SerializedObject serializedFollow = new SerializedObject(follow);
            serializedFollow.FindProperty("target").objectReferenceValue = target;
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateLighting()
        {
            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateUi(LevelManager levelManager)
        {
            GameObject canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            TMP_Text levelText = CreateText(canvasObject.transform, "LevelText", "Mechanics Test", 38f);
            RectTransform levelRect = levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0f, 1f);
            levelRect.anchorMax = new Vector2(0f, 1f);
            levelRect.pivot = new Vector2(0f, 1f);
            levelRect.anchoredPosition = new Vector2(30f, -25f);
            levelRect.sizeDelta = new Vector2(420f, 70f);

            GameObject winPanel = new GameObject("WinPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            winPanel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = winPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            winPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            CreateText(winPanel.transform, "CompleteText", "Mechanics Complete", 54f);
            winPanel.SetActive(false);

            SerializedObject serializedManager = new SerializedObject(levelManager);
            serializedManager.FindProperty("levelText").objectReferenceValue = levelText;
            serializedManager.FindProperty("levelCompletePanel").objectReferenceValue = winPanel;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.color = Color.white;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.sizeDelta = new Vector2(600f, 100f);
            return text;
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            string directory = Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == scenePath);
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static int GroundLayer
        {
            get
            {
                int layer = LayerMask.NameToLayer("Ground");
                return layer >= 0 ? layer : 0;
            }
        }
    }
}
