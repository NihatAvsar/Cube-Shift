using System;
using System.Collections.Generic;
using System.Linq;
using CubeShift.Core;
using CubeShift.LevelDesign;
using CubeShift.Player;
using CubeShift.Tiles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeShift.EditorTools
{
    public static class LevelDataAssetCreator_21_25
    {
        private const string LevelFolder = "Assets/Levels";
        private const string PrefabFolder = "Assets/Prefabs/Tiles";
        private const string SourceScene = "Assets/Scenes/CubeShift_Mechanics_Test.unity";
        private const string ColoredFaceRootName = "Level21_25_ColoredFaces";

        private const LevelObjective FullObjectives = LevelObjective.RedBreakable
            | LevelObjective.YellowButton
            | LevelObjective.GreenJump
            | LevelObjective.IceSlide
            | LevelObjective.PressurePlate
            | LevelObjective.ToggleBridge
            | LevelObjective.OneTime;

        [MenuItem("Tools/Cube Shift/Create Levels 21-25")]
        public static void CreateLevels21To25()
        {
            EnsureFolder(LevelFolder);
            Dictionary<LevelTileType, GameObject> prefabs = LoadPrefabs();
            List<string> scenePaths = new();

            foreach (LevelSpec spec in CreateSpecs())
            {
                LevelData data = SaveLevelData(spec);
                scenePaths.Add(CreatePlayableScene(spec, data, prefabs));
                Debug.Log($"{spec.Name} generated with {spec.Entries.Count} tiles.");
            }

            AddScenesToBuildSettings(scenePaths);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Level_21 through Level_25 assets and scenes generated.");
        }

        private static List<LevelSpec> CreateSpecs()
        {
            return new List<LevelSpec>
            {
                Level21(),
                Level22(),
                Level23(),
                Level24(),
                Level25()
            };
        }


        private static LevelSpec Level21()
        {
            LevelSpec level = new("Level_21", 24, 14, P(1, 6), P(22, 6), "Renk kilidi tanitim", LevelObjective.YellowButton);
            level.Path(new[] { P(1, 6), P(5, 6), P(5, 3), P(9, 3), P(9, 6), P(13, 6), P(13, 9), P(18, 9), P(18, 6), P(22, 6) });
            level.Line(3, 8, 7, 8);
            
            // Loop 1 near button at (7,3): Add normal tiles at (7,4), (8,4), (8,3) to form a 2x2 loop
            level.Add(7, 4);
            level.Add(8, 4);
            level.Add(8, 3);
            // Extra turning tiles nearby:
            level.Add(6, 4);
            level.Add(9, 4);

            // Loop 2 near button at (16,9): Add normal tiles at (16,10), (17,10), (17,9) to form a 2x2 loop
            level.Add(16, 10);
            level.Add(17, 10);
            level.Add(17, 9);
            // Extra turning tiles nearby:
            level.Add(15, 10);
            level.Add(18, 10);

            level.Special(1, 6, LevelTileType.Start);
            level.Special(7, 3, LevelTileType.YellowButton, "Y21", 0, RequiredCubeFace.Purple);
            level.Special(14, 6, LevelTileType.YellowDoor, "Y21", 0, RequiredCubeFace.Purple);
            level.Special(16, 9, LevelTileType.YellowButton, "G21", 0, RequiredCubeFace.Red);
            level.Special(20, 6, LevelTileType.YellowDoor, "G21", 0, RequiredCubeFace.Red);
            level.Special(22, 6, LevelTileType.Goal);
            level.Notes = "Renkli kup ve iki renkli kilit tanitilir: yellow sonra green. Eklenen looplar sayesinde kup dondurulebilir.";
            level.Solution = Route(P(1, 6), P(5, 6), P(5, 3), P(7, 3), P(9, 3), P(9, 6), P(13, 6), P(13, 9), P(16, 9), P(18, 9), P(18, 6), P(22, 6));
            return level;
        }

        private static LevelSpec Level22()
        {
            LevelSpec level = new("Level_22", 26, 15, P(2, 12), P(24, 3), "Renk hazirlama", LevelObjective.YellowButton | LevelObjective.PressurePlate | LevelObjective.ToggleBridge);
            level.Path(new[] { P(2, 12), P(6, 12), P(6, 8), P(10, 8), P(10, 12), P(14, 12), P(14, 7), P(19, 7), P(19, 3), P(24, 3) });
            level.Line(8, 5, 14, 5);
            level.Line(12, 10, 16, 10);

            // Loop 1 near (6,10): Add normal tiles at (7,10), (7,11), (6,11)
            level.Add(7, 10);
            level.Add(7, 11);
            level.Add(6, 11);
            // Extra turning tiles:
            level.Add(8, 11);
            level.Add(5, 11);

            // Loop 2 near (13,12): Add normal tiles at (13,13), (12,13), (12,12)
            level.Add(13, 13);
            level.Add(12, 13);
            level.Add(12, 12);
            // Extra turning tiles:
            level.Add(14, 13);
            level.Add(11, 13);

            // Loop 3 near (18,7): Add normal tiles at (18,8), (17,8), (17,7)
            level.Add(18, 8);
            level.Add(17, 8);
            level.Add(17, 7);
            // Extra turning tiles:
            level.Add(19, 8);
            level.Add(16, 8);

            level.Special(2, 12, LevelTileType.Start);
            level.Special(6, 10, LevelTileType.YellowButton, "R22", 0, RequiredCubeFace.Blue);
            level.Special(11, 8, LevelTileType.YellowDoor, "R22", 0, RequiredCubeFace.Blue);
            level.Special(13, 12, LevelTileType.PressurePlate, "B22", 0, RequiredCubeFace.Red, PlateActivationMode.ActivateOnly);
            level.Special(16, 10, LevelTileType.ToggleBridge, "B22", 0, RequiredCubeFace.Red, PlateActivationMode.ActivateOnly, false);
            level.Special(18, 7, LevelTileType.YellowButton, "P22", 0, RequiredCubeFace.Purple);
            level.Special(21, 3, LevelTileType.YellowDoor, "P22", 0, RequiredCubeFace.Purple);
            level.Special(24, 3, LevelTileType.Goal);
            level.Notes = "Red button, Blue pressure plate ve Purple button ile renk hazirlama yan yollari. Donus looplari eklendi.";
            level.Solution = Route(P(2, 12), P(6, 12), P(6, 10), P(6, 8), P(10, 8), P(10, 12), P(13, 12), P(14, 12), P(14, 7), P(18, 7), P(19, 7), P(19, 3), P(24, 3));
            return level;
        }

        private static LevelSpec Level23()
        {
            LevelSpec level = new("Level_23", 31, 17, P(1, 15), P(29, 2), "Tum mekanikler 1", FullObjectives);
            level.Path(new[] { P(1, 15), P(7, 15), P(7, 11), P(3, 11), P(3, 5), P(10, 5), P(10, 12), P(15, 12), P(15, 7), P(21, 7), P(21, 14), P(27, 14), P(27, 2), P(29, 2) });
            level.Line(7, 11, 10, 11, 1);
            level.Line(10, 11, 10, 5, 1);

            // Loop near (21,10): Add normal tiles at (22,10), (22,9), (21,9)
            level.Add(22, 10);
            level.Add(22, 9);
            level.Add(21, 9);
            // Extra turning tiles:
            level.Add(23, 10);
            level.Add(20, 9);

            // Loop near (27,6): Add normal tiles at (28,6), (28,5), (27,5)
            level.Add(28, 6);
            level.Add(28, 5);
            level.Add(27, 5);
            // Extra turning tiles:
            level.Add(29, 6);
            level.Add(26, 5);

            level.Special(1, 15, LevelTileType.Start);
            level.Special(8, 11, LevelTileType.PressurePlate, "A23", 1, RequiredCubeFace.Red, PlateActivationMode.ActivateOnly);
            level.Special(15, 7, LevelTileType.ToggleBridge, "A23", 0, RequiredCubeFace.Red, PlateActivationMode.ActivateOnly, false);
            level.Special(11, 5, LevelTileType.RedBreakable);
            level.Special(12, 5, LevelTileType.BreakableExit);
            level.Special(17, 7, LevelTileType.IceDownStart);
            level.Special(18, 7, LevelTileType.IceMiddle);
            level.Special(19, 7, LevelTileType.IceMiddle);
            level.Special(20, 7, LevelTileType.IceLanding);
            level.Special(21, 10, LevelTileType.YellowButton, "G23", 0, RequiredCubeFace.White);
            level.Special(25, 14, LevelTileType.YellowDoor, "G23", 0, RequiredCubeFace.White);
            level.Special(23, 14, LevelTileType.OneTime);
            level.Special(24, 14, LevelTileType.OneTime);
            level.Special(27, 10, LevelTileType.GreenJumpUp);
            level.Special(29, 10, LevelTileType.JumpLanding);
            level.Special(27, 6, LevelTileType.YellowButton, "P23", 0, RequiredCubeFace.Blue);
            level.Special(27, 3, LevelTileType.YellowDoor, "P23", 0, RequiredCubeFace.Blue);
            level.Special(29, 2, LevelTileType.Goal);
            level.Notes = "Level 23 ilk tam kombinasyon: renk kilit, h1 plate, bridge, red break, ice, one-time ve jump. Donus looplari eklendi.";
            level.Solution = Route(P(1, 15), P(7, 15), P(7, 11), P(8, 11), P(10, 11), P(10, 5), P(15, 12), P(15, 7), P(21, 7), P(21, 10), P(21, 14), P(27, 14), P(27, 10), P(29, 10), P(27, 6), P(27, 2), P(29, 2));
            return level;
        }

        private static LevelSpec Level24()
        {
            LevelSpec level = new("Level_24", 33, 18, P(1, 2), P(31, 16), "Tum mekanikler 2", FullObjectives);
            level.Path(new[] { P(1, 2), P(8, 2), P(8, 7), P(4, 7), P(4, 13), P(11, 13), P(11, 6), P(17, 6), P(17, 11), P(23, 11), P(23, 4), P(29, 4), P(29, 16), P(31, 16) });
            level.Line(8, 7, 11, 7, 1);
            level.Line(11, 7, 11, 13, 1);
            level.Line(17, 6, 23, 6, 1);

            // Loop near (8,5): Add normal tiles at (9,5), (9,6), (8,6)
            level.Add(9, 5);
            level.Add(9, 6);
            level.Add(8, 6);
            // Extra turning tiles:
            level.Add(10, 5);
            level.Add(7, 6);

            // Loop near (23,11): Add normal tiles at (24,11), (24,12), (23,12)
            level.Add(24, 11);
            level.Add(24, 12);
            level.Add(23, 12);
            // Extra turning tiles:
            level.Add(25, 11);
            level.Add(22, 12);

            level.Special(1, 2, LevelTileType.Start);
            level.Special(8, 5, LevelTileType.YellowButton, "B24", 0, RequiredCubeFace.Purple);
            level.Special(4, 7, LevelTileType.YellowDoor, "B24", 0, RequiredCubeFace.Purple);
            level.Special(10, 7, LevelTileType.PressurePlate, "R24", 1, RequiredCubeFace.Purple, PlateActivationMode.ActivateOnly);
            level.Special(17, 6, LevelTileType.ToggleBridge, "R24", 0, RequiredCubeFace.Purple, PlateActivationMode.ActivateOnly, false);
            level.Special(12, 13, LevelTileType.RedBreakable);
            level.Special(13, 13, LevelTileType.BreakableExit);
            level.Special(18, 6, LevelTileType.IceDownStart, "", 1);
            level.Special(19, 6, LevelTileType.IceMiddle, "", 1);
            level.Special(20, 6, LevelTileType.IceMiddle, "", 1);
            level.Special(21, 6, LevelTileType.IceLanding, "", 1);
            level.Special(23, 8, LevelTileType.PressurePlate, "P24", 0, RequiredCubeFace.Yellow, PlateActivationMode.Toggle);
            level.Special(26, 4, LevelTileType.ToggleBridge, "P24", 0, RequiredCubeFace.Yellow, PlateActivationMode.ActivateOnly, false);
            level.Special(23, 11, LevelTileType.YellowButton, "Y24", 0, RequiredCubeFace.White);
            level.Special(29, 9, LevelTileType.YellowDoor, "Y24", 0, RequiredCubeFace.White);
            level.Special(27, 4, LevelTileType.OneTime);
            level.Special(28, 4, LevelTileType.OneTime);
            level.Special(29, 12, LevelTileType.GreenJumpUp);
            level.Special(31, 12, LevelTileType.JumpLanding);
            level.Special(31, 16, LevelTileType.Goal);
            level.Notes = "Daha uzun renk sirasi: Blue lock, Red plate bridge, Purple toggle, Yellow door ve final jump. Donus looplari eklendi.";
            level.Solution = Route(P(1, 2), P(8, 2), P(8, 5), P(8, 7), P(10, 7), P(11, 7), P(11, 13), P(17, 6), P(23, 6), P(23, 8), P(23, 11), P(29, 4), P(29, 12), P(31, 12), P(31, 16));
            return level;
        }

        private static LevelSpec Level25()
        {
            LevelSpec level = new("Level_25", 35, 19, P(1, 17), P(33, 2), "Final renk kilitleri", FullObjectives);
            level.Path(new[] { P(1, 17), P(7, 17), P(7, 12), P(3, 12), P(3, 5), P(10, 5), P(10, 15), P(16, 15), P(16, 8), P(22, 8), P(22, 16), P(29, 16), P(29, 10), P(25, 10), P(25, 3), P(33, 3), P(33, 2) });
            level.Line(7, 12, 10, 12, 1);
            level.Line(10, 12, 10, 5, 1);
            level.Line(16, 15, 16, 8, 1);

            // Loop near (7,14): Add normal tiles at (8,14), (8,13), (7,13)
            level.Add(8, 14);
            level.Add(8, 13);
            level.Add(7, 13);
            // Extra turning tiles:
            level.Add(9, 14);
            level.Add(6, 13);

            // Loop near (29,14): Add normal tiles at (30,14), (30,13), (29,13)
            level.Add(30, 14);
            level.Add(30, 13);
            level.Add(29, 13);
            // Extra turning tiles:
            level.Add(31, 14);
            level.Add(28, 13);

            level.Special(1, 17, LevelTileType.Start);
            level.Special(7, 14, LevelTileType.YellowButton, "Y25", 0, RequiredCubeFace.Green);
            level.Special(3, 12, LevelTileType.YellowDoor, "Y25", 0, RequiredCubeFace.Green);
            level.Special(9, 12, LevelTileType.PressurePlate, "B25", 1, RequiredCubeFace.Green, PlateActivationMode.ActivateOnly);
            level.Special(16, 8, LevelTileType.ToggleBridge, "B25", 1, RequiredCubeFace.Green, PlateActivationMode.ActivateOnly, false);
            level.Special(11, 5, LevelTileType.RedBreakable);
            level.Special(12, 5, LevelTileType.BreakableExit);
            level.Special(18, 8, LevelTileType.IceDownStart, "", 1);
            level.Special(19, 8, LevelTileType.IceMiddle, "", 1);
            level.Special(20, 8, LevelTileType.IceMiddle, "", 1);
            level.Special(21, 8, LevelTileType.IceLanding, "", 1);
            level.Special(22, 11, LevelTileType.PressurePlate, "P25", 0, RequiredCubeFace.Blue, PlateActivationMode.Toggle);
            level.Special(26, 16, LevelTileType.ToggleBridge, "P25", 0, RequiredCubeFace.Blue, PlateActivationMode.ActivateOnly, false);
            level.Special(23, 16, LevelTileType.OneTime);
            level.Special(24, 16, LevelTileType.OneTime);
            level.Special(29, 14, LevelTileType.YellowButton, "G25", 0, RequiredCubeFace.Purple);
            level.Special(29, 10, LevelTileType.YellowDoor, "G25", 0, RequiredCubeFace.Purple);
            level.Special(25, 7, LevelTileType.PressurePlate, "R25", 0, RequiredCubeFace.Blue, PlateActivationMode.ActivateOnly);
            level.Special(29, 3, LevelTileType.ToggleBridge, "R25", 0, RequiredCubeFace.Blue, PlateActivationMode.ActivateOnly, false);
            level.Special(31, 3, LevelTileType.GreenJumpUp);
            level.Special(33, 3, LevelTileType.JumpLanding);
            level.Special(33, 2, LevelTileType.Goal);
            level.Notes = "Final: Yellow, Blue, Purple, Green ve Red renk zinciri tum mekaniklerle birlesir. Donus looplari eklendi.";
            level.Solution = Route(P(1, 17), P(7, 17), P(7, 14), P(7, 12), P(9, 12), P(10, 12), P(10, 5), P(16, 15), P(16, 8), P(22, 8), P(22, 11), P(22, 16), P(29, 16), P(29, 14), P(29, 10), P(25, 10), P(25, 7), P(25, 3), P(31, 3), P(33, 3), P(33, 2));
            return level;
        }

        private static LevelData SaveLevelData(LevelSpec spec)
        {
            string path = $"{LevelFolder}/{spec.Name}.asset";
            LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.SetEditorData(
                spec.Name,
                spec.Width,
                spec.Height,
                1f,
                spec.Start,
                spec.Objectives,
                spec.Entries.Values.OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x).ToList(),
                BuildNotes(spec),
                spec.Solution);
            EditorUtility.SetDirty(data);
            return data;
        }

        private static string CreatePlayableScene(LevelSpec spec, LevelData data, Dictionary<LevelTileType, GameObject> prefabs)
        {
            Scene scene = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
            GameObject oldRoot = GameObject.Find("LevelRoot");
            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot);
            }

            PlayerCubeController player = UnityEngine.Object.FindAnyObjectByType<PlayerCubeController>();
            LevelManager manager = UnityEngine.Object.FindAnyObjectByType<LevelManager>();
            GameObject root = new("LevelRoot");
            LevelBuilder builder = root.AddComponent<LevelBuilder>();
            SerializedObject serialized = new(builder);
            serialized.FindProperty("levelData").objectReferenceValue = data;
            serialized.FindProperty("playerTransform").objectReferenceValue = player != null ? player.transform : null;
            serialized.FindProperty("levelManager").objectReferenceValue = manager;
            SetPrefab(serialized, "normalTilePrefab", prefabs[LevelTileType.Normal]);
            SetPrefab(serialized, "startTilePrefab", prefabs[LevelTileType.Start]);
            SetPrefab(serialized, "goalTilePrefab", prefabs[LevelTileType.Goal]);
            SetPrefab(serialized, "redBreakableTilePrefab", prefabs[LevelTileType.RedBreakable]);
            SetPrefab(serialized, "breakableExitTilePrefab", prefabs[LevelTileType.BreakableExit]);
            SetPrefab(serialized, "yellowButtonTilePrefab", prefabs[LevelTileType.YellowButton]);
            SetPrefab(serialized, "yellowDoorTilePrefab", prefabs[LevelTileType.YellowDoor]);
            SetPrefab(serialized, "greenJumpTilePrefab", prefabs[LevelTileType.GreenJumpUp]);
            SetPrefab(serialized, "jumpLandingTilePrefab", prefabs[LevelTileType.JumpLanding]);
            SetPrefab(serialized, "iceSlide1Prefab", prefabs[LevelTileType.IceDownStart]);
            SetPrefab(serialized, "iceSlide2Prefab", prefabs[LevelTileType.IceMiddle]);
            SetPrefab(serialized, "iceSlide3Prefab", prefabs[LevelTileType.IceMiddle]);
            SetPrefab(serialized, "iceLandingPrefab", prefabs[LevelTileType.IceLanding]);
            SetPrefab(serialized, "pressurePlatePrefab", prefabs[LevelTileType.PressurePlate]);
            SetPrefab(serialized, "toggleBridgePrefab", prefabs[LevelTileType.ToggleBridge]);
            SetPrefab(serialized, "oneTimeTilePrefab", prefabs[LevelTileType.OneTime]);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (player != null)
            {
                AddColoredFaces(player.transform);
                builder.BuildLevel();
            }
            else
            {
                Debug.LogError($"{spec.Name} scene could not be built because no PlayerCubeController was found in {SourceScene}.");
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographicSize = spec.Width >= 33 ? 9.5f : 8.8f;
            }

            string path = $"Assets/Scenes/{spec.Name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void AddColoredFaces(Transform player)
        {
            string[] legacyFaceNames =
            {
                "Top_Blue",
                "Bottom_White",
                "Front_Green",
                "Back_Purple",
                "Left_Red",
                "Right_Yellow"
            };

            foreach (string faceName in legacyFaceNames)
            {
                Transform legacyFace = player.Find(faceName);
                if (legacyFace != null)
                {
                    UnityEngine.Object.DestroyImmediate(legacyFace.gameObject);
                }
            }

            Transform existing = player.Find(ColoredFaceRootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new(ColoredFaceRootName);
            root.transform.SetParent(player, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            AddFace(root.transform, "Top_Blue", RequiredCubeFace.Blue, new Vector3(0f, 0.506f, 0f), new Vector3(0.82f, 0.018f, 0.82f));
            AddFace(root.transform, "Bottom_White", RequiredCubeFace.White, new Vector3(0f, -0.506f, 0f), new Vector3(0.82f, 0.018f, 0.82f));
            AddFace(root.transform, "Front_Green", RequiredCubeFace.Green, new Vector3(0f, 0f, 0.506f), new Vector3(0.82f, 0.82f, 0.018f));
            AddFace(root.transform, "Back_Purple", RequiredCubeFace.Purple, new Vector3(0f, 0f, -0.506f), new Vector3(0.82f, 0.82f, 0.018f));
            AddFace(root.transform, "Left_Red", RequiredCubeFace.Red, new Vector3(-0.506f, 0f, 0f), new Vector3(0.018f, 0.82f, 0.82f));
            AddFace(root.transform, "Right_Yellow", RequiredCubeFace.Yellow, new Vector3(0.506f, 0f, 0f), new Vector3(0.018f, 0.82f, 0.82f));
        }

        private static void AddFace(Transform parent, string name, RequiredCubeFace face, Vector3 localPosition, Vector3 localScale)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(parent, false);
            panel.transform.localPosition = localPosition;
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = localScale;

            Collider collider = panel.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer renderer = panel.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(FaceMaterialPath(face));
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        private static Dictionary<LevelTileType, GameObject> LoadPrefabs()
        {
            return new Dictionary<LevelTileType, GameObject>
            {
                [LevelTileType.Normal] = Prefab("NormalTile"),
                [LevelTileType.Start] = Prefab("START"),
                [LevelTileType.Goal] = Prefab("Goal_BehindDoor"),
                [LevelTileType.RedBreakable] = Prefab("LEFT_Red_Breaks"),
                [LevelTileType.BreakableExit] = Prefab("BreakableExit"),
                [LevelTileType.YellowButton] = Prefab("RIGHT_Yellow_OpensDoor"),
                [LevelTileType.YellowDoor] = Prefab("DoorTile_YellowGate"),
                [LevelTileType.GreenJumpUp] = Prefab("UP_Green_Jumps"),
                [LevelTileType.JumpLanding] = Prefab("JumpLanding"),
                [LevelTileType.IceDownStart] = Prefab("DOWN_Ice_Slide_1"),
                [LevelTileType.IceMiddle] = Prefab("Ice_Slide_Middle"),
                [LevelTileType.IceLanding] = Prefab("IceLanding"),
                [LevelTileType.PressurePlate] = Prefab("PressurePlateTile"),
                [LevelTileType.ToggleBridge] = Prefab("ToggleBridgeTile"),
                [LevelTileType.OneTime] = Prefab("OneTimeTile")
            };
        }

        private static GameObject Prefab(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{name}.prefab");
            if (prefab == null)
            {
                Debug.LogError($"Missing prefab: {PrefabFolder}/{name}.prefab");
            }

            return prefab;
        }

        private static void SetPrefab(SerializedObject serialized, string property, GameObject prefab)
        {
            serialized.FindProperty(property).objectReferenceValue = prefab;
        }

        private static void AddScenesToBuildSettings(List<string> newLevelScenes)
        {
            const string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            List<string> orderedLevelScenes = Enumerable.Range(1, 25)
                .Select(number => $"Assets/Scenes/Level_{number:00}.unity")
                .Where(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null || newLevelScenes.Contains(path))
                .ToList();

            List<EditorBuildSettingsScene> otherScenes = EditorBuildSettings.scenes
                .Where(scene => scene.path != mainMenuPath
                    && !orderedLevelScenes.Contains(scene.path)
                    && !newLevelScenes.Contains(scene.path))
                .ToList();

            List<EditorBuildSettingsScene> scenes = new();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(mainMenuPath) != null)
            {
                scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            }

            scenes.AddRange(orderedLevelScenes.Select(path => new EditorBuildSettingsScene(path, true)));
            scenes.AddRange(otherScenes);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static string BuildNotes(LevelSpec spec)
        {
            return
                $"Level adi: {spec.Name}\n" +
                $"Boyut: {spec.Width}x{spec.Height}\n" +
                $"Zorluk: {spec.Difficulty}\n" +
                $"Mekanik ozeti: {spec.Notes}\n" +
                $"Start koordinati: {Format(spec.Start)}\n" +
                $"Goal koordinati: {Format(spec.Goal)}\n" +
                $"Renkli kilitler: {ColorLockSummary(spec)}\n" +
                $"PressurePlate requiredFace bilgisi: {PlateFaceSummary(spec)}\n" +
                $"ToggleBridge bilgisi: {BridgeSummary(spec)}\n" +
                $"OneTimeTile koordinatlari: {Summary(spec, LevelTileType.OneTime)}\n" +
                $"IceSlide rotalari: {IceSummary(spec)}\n" +
                $"GreenJump ve JumpLanding koordinatlari: Jump {Summary(spec, LevelTileType.GreenJumpUp)}; Landing {Summary(spec, LevelTileType.JumpLanding)}\n" +
                $"RedBreakable koordinatlari: {Summary(spec, LevelTileType.RedBreakable)}\n" +
                $"Gelistirici test cozum rotasi: {spec.Solution}\n";
        }

        private static Vector2Int P(int x, int y) => new(x, y);

        private static string Route(params object[] parts)
        {
            List<Vector2Int> points = new();
            List<string> annotations = new();
            foreach (object part in parts)
            {
                if (part is Vector2Int p)
                {
                    points.Add(p);
                }
                else if (part is string text)
                {
                    annotations.Add(text);
                }
            }

            List<string> moves = new();
            for (int index = 1; index < points.Count; index++)
            {
                Vector2Int from = points[index - 1];
                Vector2Int to = points[index];
                Vector2Int delta = to - from;
                Vector2Int step = new(Math.Sign(delta.x), Math.Sign(delta.y));
                int count = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
                for (int i = 0; i < count; i++)
                {
                    moves.Add(DirectionName(step));
                }
            }

            if (annotations.Count > 0)
            {
                moves.AddRange(annotations);
            }

            return string.Join(", ", moves);
        }

        private static string DirectionName(Vector2Int direction)
        {
            if (direction == Vector2Int.right) return "Right";
            if (direction == Vector2Int.left) return "Left";
            if (direction == Vector2Int.up) return "Up";
            return "Down";
        }

        private static string Summary(LevelSpec spec, LevelTileType type)
        {
            IEnumerable<LevelTileEntry> entries = spec.Entries.Values.Where(e => e.TileType == type);
            string text = string.Join("; ", entries.Select(e => $"{Format(e.Coordinate)} h{e.HeightLevel}"));
            return string.IsNullOrWhiteSpace(text) ? "Yok" : text;
        }

        private static string ColorLockSummary(LevelSpec spec)
        {
            string text = string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType is LevelTileType.YellowButton or LevelTileType.YellowDoor)
                .Select(e => $"{e.TileType} {Format(e.Coordinate)} link={e.LinkId} face={e.RequiredFace}"));
            return string.IsNullOrWhiteSpace(text) ? "Yok" : text;
        }

        private static string PlateFaceSummary(LevelSpec spec)
        {
            string text = string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType == LevelTileType.PressurePlate)
                .Select(e => $"{Format(e.Coordinate)} requiredFace={e.RequiredFace}, mode={e.PlateActivationMode}, singleUse={e.SingleUse}"));
            return string.IsNullOrWhiteSpace(text) ? "Yok" : text;
        }

        private static string BridgeSummary(LevelSpec spec)
        {
            string text = string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType == LevelTileType.ToggleBridge)
                .Select(e => $"{Format(e.Coordinate)} link={e.LinkId}, startsActive={e.StartsActive}"));
            return string.IsNullOrWhiteSpace(text) ? "Yok" : text;
        }

        private static string IceSummary(LevelSpec spec)
        {
            List<LevelTileEntry> ice = spec.Entries.Values
                .Where(e => e.TileType is LevelTileType.IceDownStart or LevelTileType.IceMiddle or LevelTileType.IceLanding)
                .OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x).ToList();
            return ice.Count == 0 ? "Yok" : string.Join(" -> ", ice.Select(e => $"{e.TileType}{Format(e.Coordinate)}"));
        }

        private static string Format(Vector2Int point) => $"({point.x},{point.y})";

        private static string FaceMaterialPath(RequiredCubeFace face)
        {
            return face switch
            {
                RequiredCubeFace.White => "Assets/Materials/Player/Face_White.mat",
                RequiredCubeFace.Blue => "Assets/Materials/Player/Face_Blue.mat",
                RequiredCubeFace.Red => "Assets/Materials/Player/Face_Red.mat",
                RequiredCubeFace.Yellow => "Assets/Materials/Player/Face_Yellow.mat",
                RequiredCubeFace.Green => "Assets/Materials/Player/Face_Green.mat",
                RequiredCubeFace.Purple => "Assets/Materials/Player/Face_Purple.mat",
                _ => string.Empty
            };
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private sealed class LevelSpec
        {
            public readonly string Name;
            public readonly int Width;
            public readonly int Height;
            public readonly Vector2Int Start;
            public readonly Vector2Int Goal;
            public readonly string Difficulty;
            public readonly LevelObjective Objectives;
            public readonly Dictionary<Vector2Int, LevelTileEntry> Entries = new();
            public string Notes;
            public string Solution;

            public LevelSpec(string name, int width, int height, Vector2Int start, Vector2Int goal, string difficulty, LevelObjective objectives)
            {
                Name = name;
                Width = width;
                Height = height;
                Start = start;
                Goal = goal;
                Difficulty = difficulty;
                Objectives = objectives;
            }

            public void Path(IReadOnlyList<Vector2Int> points, int height = 0)
            {
                for (int index = 1; index < points.Count; index++)
                {
                    Line(points[index - 1].x, points[index - 1].y, points[index].x, points[index].y, height);
                }
            }

            public void Line(int x1, int y1, int x2, int y2, int height = 0)
            {
                int dx = Math.Sign(x2 - x1);
                int dy = Math.Sign(y2 - y1);
                int x = x1;
                int y = y1;
                while (true)
                {
                    Add(x, y, height);
                    if (x == x2 && y == y2)
                    {
                        break;
                    }

                    x += dx;
                    y += dy;
                }
            }

            public void Add(int x, int y, int height = 0)
            {
                Special(x, y, LevelTileType.Normal, "", height);
            }

            public void Special(
                int x,
                int y,
                LevelTileType type,
                string link = "",
                int height = 0,
                RequiredCubeFace requiredFace = RequiredCubeFace.Any,
                PlateActivationMode mode = PlateActivationMode.ActivateOnly,
                bool startsActive = true,
                bool singleUse = false,
                string note = "")
            {
                Entries[new Vector2Int(x, y)] = new LevelTileEntry(
                    new Vector2Int(x, y),
                    type,
                    link,
                    height,
                    requiredFace,
                    mode,
                    startsActive,
                    singleUse,
                    note);
            }
        }
    }
}
