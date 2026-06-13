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
    public static class LevelDataAssetCreator_11_20
    {
        private const string LevelFolder = "Assets/Levels";
        private const string PrefabFolder = "Assets/Prefabs/Tiles";
        private const string MaterialFolder = "Assets/Materials/Tiles";
        private const string SourceScene = "Assets/Scenes/CubeShift_Mechanics_Test.unity";

        private const LevelObjective NewObjectives = LevelObjective.RedBreakable
            | LevelObjective.YellowButton
            | LevelObjective.GreenJump
            | LevelObjective.IceSlide
            | LevelObjective.PressurePlate
            | LevelObjective.ToggleBridge
            | LevelObjective.OneTime;

        [MenuItem("Tools/Cube Shift/Create Levels 11-20")]
        public static void CreateLevels11To20()
        {
            EnsureFolder(LevelFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);
            CreateNewMechanicPrefabs();

            Dictionary<LevelTileType, GameObject> prefabs = LoadPrefabs();
            List<string> scenePaths = new();
            foreach (LevelSpec spec in CreateSpecs())
            {
                LevelData data = SaveLevelData(spec);
                scenePaths.Add(CreatePlayableScene(spec, data, prefabs));
                Debug.Log($"{spec.Name} asset generated with {spec.Entries.Count} tiles.");
            }

            AddScenesToBuildSettings(scenePaths);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Level_11 through Level_20 assets and scenes generated.");
        }

        private static List<LevelSpec> CreateSpecs()
        {
            return new List<LevelSpec>
            {
                Level11(),
                Level12(),
                Level13(),
                Level14(),
                Level15(),
                Level16(),
                Level17(),
                Level18(),
                Level19(),
                Level20()
            };
        }

        private static LevelSpec Level11()
        {
            LevelSpec level = new("Level_11", 22, 13, new Vector2Int(1, 6), new Vector2Int(20, 7), "Orta");
            level.Path(new[] { P(1, 6), P(5, 6), P(5, 3), P(9, 3), P(9, 6), P(13, 6), P(13, 9), P(20, 9), P(20, 7) });
            level.Line(6, 8, 8, 8);
            level.Special(1, 6, LevelTileType.Start);
            level.Special(7, 3, LevelTileType.PressurePlate, "A", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(14, 9, LevelTileType.ToggleBridge, "A", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(10, 6, LevelTileType.OneTime, "", 0, note: "First fragile warning.");
            level.Special(11, 6, LevelTileType.OneTime);
            level.Special(20, 7, LevelTileType.Goal);
            level.Notes = "Tek Any plate, tek kapali bridge ve iki OneTime tile ile yeni mekanikleri tanitir. Kisa h1 rampasi icin batida yan kol bulunur.";
            level.Solution = Route(P(1, 6), P(5, 6), P(5, 3), P(7, 3), P(9, 3), P(9, 6), P(13, 6), P(13, 9), P(20, 9), P(20, 7), "Right(bridge A)");
            return level;
        }

        private static LevelSpec Level12()
        {
            LevelSpec level = new("Level_12", 24, 14, new Vector2Int(2, 11), new Vector2Int(22, 4), "Orta");
            level.Path(new[] { P(2, 11), P(6, 11), P(6, 7), P(10, 7), P(10, 11), P(14, 11), P(14, 8), P(18, 8), P(18, 4), P(22, 4) });
            level.Line(8, 5, 12, 5, 1);
            level.Special(2, 11, LevelTileType.Start);
            level.Special(6, 9, LevelTileType.IceDownStart);
            level.Special(6, 8, LevelTileType.IceMiddle);
            level.Special(6, 7, LevelTileType.IceLanding);
            level.Special(10, 9, LevelTileType.PressurePlate, "B", 0, RequiredCubeFace.Yellow, PlateActivationMode.ActivateOnly, false, false, "Yellow face required.");
            level.Special(18, 8, LevelTileType.ToggleBridge, "B", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(12, 11, LevelTileType.OneTime);
            level.Special(22, 4, LevelTileType.Goal);
            level.Notes = "Yellow requiredFace plate ve kisa dikey ice hattindan sonra bridge ustunden final koridora gecilir.";
            level.Solution = Route(P(2, 11), P(6, 11), P(6, 7), P(10, 7), P(10, 9), P(10, 11), P(14, 11), P(14, 8), P(22, 4), "Down(ice), Up/Down face setup, Right(bridge B)");
            return level;
        }

        private static LevelSpec Level13()
        {
            LevelSpec level = new("Level_13", 25, 14, new Vector2Int(1, 2), new Vector2Int(23, 12), "Orta-zor");
            level.Path(new[] { P(1, 2), P(7, 2), P(7, 6), P(4, 6), P(4, 10), P(11, 10), P(11, 5), P(16, 5), P(16, 9), P(21, 9), P(21, 12), P(23, 12) });
            level.Line(12, 2, 17, 2);
            level.Special(1, 2, LevelTileType.Start);
            level.Special(7, 4, LevelTileType.PressurePlate, "C", 0, RequiredCubeFace.Any, PlateActivationMode.Toggle);
            level.Special(11, 5, LevelTileType.ToggleBridge, "C", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(16, 7, LevelTileType.PressurePlate, "D", 0, RequiredCubeFace.Any, PlateActivationMode.DeactivateOnly, true);
            level.Special(13, 2, LevelTileType.ToggleBridge, "D", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, true);
            level.Special(5, 10, LevelTileType.OneTime);
            level.Special(6, 10, LevelTileType.OneTime);
            level.Special(21, 12, LevelTileType.OneTime);
            level.Special(23, 12, LevelTileType.Goal);
            level.Notes = "Toggle plate C ana bridge'i acar; single-use D yan bridge'i kapatarak sirayi onemli hale getirir. OneTime donusu kisitlar.";
            level.Solution = Route(P(1, 2), P(7, 2), P(7, 4), P(7, 6), P(4, 6), P(4, 10), P(11, 10), P(11, 5), P(16, 5), P(16, 9), P(23, 12), "Down/Up(toggle C), Right(bridge C), avoid D until exit");
            return level;
        }

        private static LevelSpec Level14()
        {
            LevelSpec level = new("Level_14", 26, 15, new Vector2Int(2, 13), new Vector2Int(24, 3), "Orta-zor");
            level.Path(new[] { P(2, 13), P(5, 13), P(5, 9), P(9, 9), P(9, 4), P(14, 4), P(14, 10), P(19, 10), P(19, 3), P(24, 3) });
            level.Line(5, 9, 9, 9, 1);
            level.Line(9, 9, 9, 4, 1);
            level.Special(2, 13, LevelTileType.Start);
            level.Special(8, 9, LevelTileType.PressurePlate, "E", 1, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(18, 10, LevelTileType.ToggleBridge, "E", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(10, 4, LevelTileType.IceDownStart, "", 1);
            level.Special(11, 4, LevelTileType.IceMiddle, "", 1);
            level.Special(12, 4, LevelTileType.IceMiddle, "", 1);
            level.Special(13, 4, LevelTileType.IceLanding, "", 1);
            level.Special(15, 10, LevelTileType.OneTime);
            level.Special(24, 3, LevelTileType.Goal);
            level.Notes = "Plate h1 platformundadir; h1 ice oyuncuyu bridge bolgesine tasir, final h0 koridoruna inilerek gidilir.";
            level.Solution = Route(P(2, 13), P(5, 13), P(5, 9), P(8, 9), P(9, 9), P(9, 4), P(14, 4), P(14, 10), P(24, 3), "Up(climb), Right(plate E), Right(ice), Down(descend), Right(bridge E)");
            return level;
        }

        private static LevelSpec Level15()
        {
            LevelSpec level = new("Level_15", 27, 15, new Vector2Int(1, 7), new Vector2Int(25, 13), "Zor");
            level.Path(new[] { P(1, 7), P(5, 7), P(5, 2), P(10, 2), P(10, 7), P(15, 7), P(15, 11), P(20, 11), P(20, 13), P(25, 13) });
            level.Line(3, 10, 8, 10);
            level.Line(18, 4, 23, 4);
            level.Special(1, 7, LevelTileType.Start);
            level.Special(5, 4, LevelTileType.PressurePlate, "F", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(10, 7, LevelTileType.ToggleBridge, "F", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(15, 9, LevelTileType.PressurePlate, "G", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(20, 12, LevelTileType.ToggleBridge, "G", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(7, 2, LevelTileType.YellowButton, "Gate15");
            level.Special(19, 11, LevelTileType.YellowDoor, "Gate15");
            level.Special(12, 7, LevelTileType.RedBreakable);
            level.Special(13, 7, LevelTileType.BreakableExit);
            level.Special(16, 11, LevelTileType.OneTime);
            level.Special(17, 11, LevelTileType.OneTime);
            level.Special(25, 13, LevelTileType.Goal);
            level.Notes = "A sonra B plate sirasi, YellowDoor ve RedBreakable ana hatta kullanilir. OneTime ikilisi final donusunu kapatir.";
            level.Solution = Route(P(1, 7), P(5, 7), P(5, 4), P(5, 2), P(10, 2), P(10, 7), P(15, 7), P(15, 9), P(15, 11), P(25, 13), "Plate F, bridge F, YellowButton, RedBreakable, Plate G, door, bridge G");
            return level;
        }

        private static LevelSpec Level16()
        {
            LevelSpec level = new("Level_16", 28, 16, new Vector2Int(2, 2), new Vector2Int(26, 14), "Zor");
            level.Path(new[] { P(2, 2), P(8, 2), P(8, 6), P(4, 6), P(4, 11), P(11, 11), P(11, 5), P(17, 5), P(17, 10), P(22, 10), P(22, 14), P(26, 14) });
            level.Line(8, 6, 11, 6, 1);
            level.Line(11, 6, 11, 11, 1);
            level.Special(2, 2, LevelTileType.Start);
            level.Special(9, 6, LevelTileType.PressurePlate, "H", 1, RequiredCubeFace.Purple, PlateActivationMode.ActivateOnly, false, false, "Purple face check after climb.");
            level.Special(17, 5, LevelTileType.ToggleBridge, "H", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(14, 5, LevelTileType.GreenJumpUp);
            level.Special(16, 5, LevelTileType.JumpLanding);
            level.Special(19, 10, LevelTileType.YellowButton, "Gate16");
            level.Special(22, 12, LevelTileType.YellowDoor, "Gate16");
            level.Special(20, 10, LevelTileType.OneTime);
            level.Special(26, 14, LevelTileType.Goal);
            level.Notes = "Purple requiredFace, h1 tirmanma/descend ve GreenJump zinciriyle bridge+door finali acilir.";
            level.Solution = Route(P(2, 2), P(8, 2), P(8, 6), P(9, 6), P(11, 6), P(11, 5), P(14, 5), P(16, 5), P(17, 5), P(22, 10), P(26, 14), "Face setup Purple, jump east, bridge H, YellowButton, door");
            return level;
        }

        private static LevelSpec Level17()
        {
            LevelSpec level = new("Level_17", 29, 16, new Vector2Int(1, 14), new Vector2Int(27, 3), "Zor");
            level.Path(new[] { P(1, 14), P(6, 14), P(6, 9), P(3, 9), P(3, 4), P(10, 4), P(10, 9), P(15, 9), P(15, 13), P(21, 13), P(21, 6), P(27, 6), P(27, 3) });
            level.Line(12, 2, 18, 2);
            level.Special(1, 14, LevelTileType.Start);
            level.Special(4, 9, LevelTileType.PressurePlate, "I", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(21, 8, LevelTileType.ToggleBridge, "I", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(6, 12, LevelTileType.OneTime);
            level.Special(6, 11, LevelTileType.OneTime);
            level.Special(3, 7, LevelTileType.OneTime);
            level.Special(10, 6, LevelTileType.OneTime);
            level.Special(15, 11, LevelTileType.OneTime);
            level.Special(18, 13, LevelTileType.OneTime);
            level.Special(12, 9, LevelTileType.IceDownStart);
            level.Special(13, 9, LevelTileType.IceMiddle);
            level.Special(14, 9, LevelTileType.IceMiddle);
            level.Special(15, 9, LevelTileType.IceLanding);
            level.Special(27, 3, LevelTileType.Goal);
            level.Notes = "OneTime ana zorluktur; plate bridge'i acar ama kirilan geri donusler sirayi cezalandirir. Ice yanlis zamanda kullanilirsa restart dogaldir.";
            level.Solution = Route(P(1, 14), P(6, 14), P(6, 9), P(4, 9), P(3, 9), P(3, 4), P(10, 4), P(10, 9), P(21, 13), P(21, 6), P(27, 3), "Plate I first, commit through fragile corridor, ice east, bridge I");
            return level;
        }

        private static LevelSpec Level18()
        {
            LevelSpec level = new("Level_18", 30, 17, new Vector2Int(2, 15), new Vector2Int(28, 2), "Cok zor");
            level.Path(new[] { P(2, 15), P(7, 15), P(7, 10), P(4, 10), P(4, 5), P(11, 5), P(11, 12), P(16, 12), P(16, 7), P(22, 7), P(22, 14), P(27, 14), P(27, 2), P(28, 2) });
            level.Line(7, 10, 11, 10, 1);
            level.Line(11, 10, 11, 5, 1);
            level.Special(2, 15, LevelTileType.Start);
            level.Special(10, 10, LevelTileType.PressurePlate, "J", 1, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(16, 7, LevelTileType.ToggleBridge, "J", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(22, 9, LevelTileType.PressurePlate, "K", 0, RequiredCubeFace.Red, PlateActivationMode.Toggle);
            level.Special(24, 14, LevelTileType.ToggleBridge, "K", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, true);
            level.Special(12, 5, LevelTileType.RedBreakable);
            level.Special(13, 5, LevelTileType.BreakableExit);
            level.Special(18, 7, LevelTileType.IceDownStart);
            level.Special(19, 7, LevelTileType.IceMiddle);
            level.Special(20, 7, LevelTileType.IceMiddle);
            level.Special(21, 7, LevelTileType.IceLanding);
            level.Special(23, 14, LevelTileType.YellowButton, "Gate18");
            level.Special(27, 6, LevelTileType.YellowDoor, "Gate18");
            level.Special(25, 14, LevelTileType.OneTime);
            level.Special(28, 2, LevelTileType.Goal);
            level.Notes = "Altı bolgeli harita: baslangic, h1 tirmanma, plate, ice, door/bridge ve goal. Red requiredFace plate rotayi yeniden acar/kapatir.";
            level.Solution = Route(P(2, 15), P(7, 15), P(7, 10), P(10, 10), P(11, 10), P(11, 5), P(16, 7), P(22, 7), P(22, 9), P(22, 14), P(28, 2), "Plate J, descend, red break, ice, Red face Plate K, YellowButton, door");
            return level;
        }

        private static LevelSpec Level19()
        {
            LevelSpec level = new("Level_19", 31, 17, new Vector2Int(1, 1), new Vector2Int(29, 15), "Cok zor");
            level.Path(new[] { P(1, 1), P(8, 1), P(8, 6), P(3, 6), P(3, 13), P(10, 13), P(10, 8), P(16, 8), P(16, 3), P(23, 3), P(23, 10), P(28, 10), P(28, 15), P(29, 15) });
            level.Special(1, 1, LevelTileType.Start);
            level.Special(8, 4, LevelTileType.PressurePlate, "L", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly);
            level.Special(10, 8, LevelTileType.ToggleBridge, "L", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(16, 6, LevelTileType.PressurePlate, "M", 0, RequiredCubeFace.Green, PlateActivationMode.Toggle);
            level.Special(23, 6, LevelTileType.ToggleBridge, "M", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(23, 10, LevelTileType.PressurePlate, "N", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, true);
            level.Special(28, 12, LevelTileType.ToggleBridge, "N", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(5, 6, LevelTileType.OneTime);
            level.Special(3, 9, LevelTileType.OneTime);
            level.Special(13, 8, LevelTileType.OneTime);
            level.Special(25, 10, LevelTileType.OneTime);
            level.Special(12, 8, LevelTileType.GreenJumpUp);
            level.Special(14, 8, LevelTileType.JumpLanding);
            level.Special(18, 3, LevelTileType.IceDownStart);
            level.Special(19, 3, LevelTileType.IceMiddle);
            level.Special(20, 3, LevelTileType.IceMiddle);
            level.Special(21, 3, LevelTileType.IceLanding);
            level.Special(29, 15, LevelTileType.Goal);
            level.Notes = "Sirali aktivasyon: L sonra M sonra N. OneTime geri donusleri sinirlar; GreenJump ve IceSlide orta bolgede birlikte kullanilir.";
            level.Solution = Route(P(1, 1), P(8, 1), P(8, 4), P(8, 6), P(3, 6), P(3, 13), P(10, 13), P(10, 8), P(12, 8), P(14, 8), P(16, 8), P(16, 6), P(16, 3), P(23, 3), P(23, 10), P(29, 15), "Plate L, bridge L, GreenJump, Green face Plate M, ice, Plate N, bridge N");
            return level;
        }

        private static LevelSpec Level20()
        {
            LevelSpec level = new("Level_20", 32, 18, new Vector2Int(1, 16), new Vector2Int(30, 2), "Final / cok zor");
            level.Path(new[] { P(1, 16), P(7, 16), P(7, 11), P(3, 11), P(3, 5), P(10, 5), P(10, 13), P(15, 13), P(15, 7), P(21, 7), P(21, 15), P(27, 15), P(27, 9), P(23, 9), P(23, 3), P(30, 3), P(30, 2) });
            level.Line(7, 11, 10, 11, 1);
            level.Line(10, 11, 10, 5, 1);
            level.Line(15, 13, 15, 7, 1);
            level.Special(1, 16, LevelTileType.Start);
            level.Special(8, 11, LevelTileType.PressurePlate, "O", 1, RequiredCubeFace.Yellow, PlateActivationMode.ActivateOnly);
            level.Special(15, 7, LevelTileType.ToggleBridge, "O", 1, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(21, 10, LevelTileType.PressurePlate, "P", 0, RequiredCubeFace.Purple, PlateActivationMode.Toggle);
            level.Special(24, 15, LevelTileType.ToggleBridge, "P", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(27, 11, LevelTileType.PressurePlate, "Q", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, true);
            level.Special(23, 7, LevelTileType.ToggleBridge, "Q", 0, RequiredCubeFace.Any, PlateActivationMode.ActivateOnly, false);
            level.Special(11, 5, LevelTileType.RedBreakable);
            level.Special(12, 5, LevelTileType.BreakableExit);
            level.Special(18, 7, LevelTileType.IceDownStart, "", 1);
            level.Special(19, 7, LevelTileType.IceMiddle, "", 1);
            level.Special(20, 7, LevelTileType.IceMiddle, "", 1);
            level.Special(21, 7, LevelTileType.IceLanding);
            level.Special(22, 15, LevelTileType.YellowButton, "Gate20");
            level.Special(23, 5, LevelTileType.YellowDoor, "Gate20");
            level.Special(5, 11, LevelTileType.OneTime);
            level.Special(6, 11, LevelTileType.OneTime);
            level.Special(14, 13, LevelTileType.OneTime);
            level.Special(25, 15, LevelTileType.OneTime);
            level.Special(28, 3, LevelTileType.GreenJumpUp);
            level.Special(30, 3, LevelTileType.JumpLanding);
            level.Special(30, 2, LevelTileType.Goal);
            level.Notes = "Final bolum: yuz ayarlama, h1 platform, uc plate/bridge zinciri, ice labirenti, OneTime gecidi, YellowDoor, RedBreakable ve GreenJump finali.";
            level.Solution = Route(P(1, 16), P(7, 16), P(7, 11), P(8, 11), P(10, 11), P(10, 5), P(15, 13), P(15, 7), P(21, 7), P(21, 10), P(21, 15), P(27, 15), P(27, 11), P(27, 9), P(23, 9), P(23, 3), P(28, 3), P(30, 3), P(30, 2), "Yellow face Plate O, bridge O, red break, ice, Purple face Plate P, YellowButton, Plate Q, door+bridge final, GreenJump");
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
            else
            {
                Debug.LogWarning($"{spec.Name} already exists and will be updated.", data);
            }

            string notes = BuildNotes(spec);
            data.SetEditorData(
                spec.Name,
                spec.Width,
                spec.Height,
                1f,
                spec.Start,
                NewObjectives,
                spec.Entries.Values.OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x).ToList(),
                notes,
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
                builder.BuildLevel();
            }
            else
            {
                Debug.LogError($"{spec.Name} scene could not be built because no PlayerCubeController was found in {SourceScene}.");
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographicSize = spec.Width >= 30 ? 9f : 8f;
            }

            string path = $"Assets/Scenes/{spec.Name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
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
            List<string> orderedLevelScenes = Enumerable.Range(1, 20)
                .Select(number => $"Assets/Scenes/Level_{number:00}.unity")
                .Where(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
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
                $"Normal tile koordinatlari: {Summary(spec, LevelTileType.Normal)}\n" +
                $"HeightLevel bilgileri: {Heights(spec)}\n" +
                $"PressurePlate koordinatlari: {Summary(spec, LevelTileType.PressurePlate)}\n" +
                $"PressurePlate requiredFace bilgisi: {PlateFaceSummary(spec)}\n" +
                $"PressurePlate linkId bilgisi: {LinkSummary(spec, LevelTileType.PressurePlate)}\n" +
                $"ToggleBridge koordinatlari: {Summary(spec, LevelTileType.ToggleBridge)}\n" +
                $"ToggleBridge startsActive bilgisi: {BridgeSummary(spec)}\n" +
                $"OneTimeTile koordinatlari: {Summary(spec, LevelTileType.OneTime)}\n" +
                $"YellowDoor / YellowButton koordinatlari: Buttons {Summary(spec, LevelTileType.YellowButton)}; Doors {Summary(spec, LevelTileType.YellowDoor)}\n" +
                $"RedBreakable koordinatlari: {Summary(spec, LevelTileType.RedBreakable)}\n" +
                $"IceSlide rotalari: {IceSummary(spec)}\n" +
                $"GreenJump ve JumpLanding koordinatlari: Jump {Summary(spec, LevelTileType.GreenJumpUp)}; Landing {Summary(spec, LevelTileType.JumpLanding)}\n" +
                $"Tirmanma noktalari: {ClimbSummary(spec)}\n" +
                $"Asagi inis noktalari: {DescendSummary(spec)}\n" +
                $"Yaklasik hamle sayisi: {ApproxMoveCount(spec.Solution)}\n" +
                $"Gelistirici test cozum rotasi: {spec.Solution}\n";
        }

        private static void CreateNewMechanicPrefabs()
        {
            Material plate = Material("PressurePlateTile", new Color(0.05f, 0.7f, 0.95f));
            Material bridge = Material("ToggleBridgeTile", new Color(0.45f, 0.55f, 0.65f));
            Material fragile = Material("OneTimeTile", new Color(0.95f, 0.55f, 0.25f));

            SaveTilePrefab<PressurePlateTile>("PressurePlateTile", plate);
            SaveTilePrefab<ToggleBridgeTile>("ToggleBridgeTile", bridge);
            SaveTilePrefab<OneTimeTile>("OneTimeTile", fragile);
        }

        private static GameObject SaveTilePrefab<T>(string name, Material material) where T : TileBase
        {
            string path = $"{PrefabFolder}/{name}.prefab";
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = name;
            tile.transform.localScale = new Vector3(0.96f, 0.2f, 0.96f);
            tile.layer = Mathf.Max(0, LayerMask.NameToLayer("Ground"));
            tile.GetComponent<Renderer>().sharedMaterial = material;
            tile.AddComponent<T>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, path);
            UnityEngine.Object.DestroyImmediate(tile);
            return prefab;
        }

        private static Material Material(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            return material;
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

        private static string LinkSummary(LevelSpec spec, LevelTileType type)
        {
            string text = string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType == type)
                .Select(e => $"{Format(e.Coordinate)} link={e.LinkId}"));
            return string.IsNullOrWhiteSpace(text) ? "Yok" : text;
        }

        private static string PlateFaceSummary(LevelSpec spec)
        {
            return string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType == LevelTileType.PressurePlate)
                .Select(e => $"{Format(e.Coordinate)} requiredFace={e.RequiredFace}, mode={e.PlateActivationMode}, singleUse={e.SingleUse}"));
        }

        private static string BridgeSummary(LevelSpec spec)
        {
            return string.Join("; ", spec.Entries.Values
                .Where(e => e.TileType == LevelTileType.ToggleBridge)
                .Select(e => $"{Format(e.Coordinate)} link={e.LinkId}, startsActive={e.StartsActive}"));
        }

        private static string IceSummary(LevelSpec spec)
        {
            List<LevelTileEntry> ice = spec.Entries.Values
                .Where(e => e.TileType is LevelTileType.IceDownStart or LevelTileType.IceMiddle or LevelTileType.IceLanding)
                .OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x).ToList();
            return ice.Count == 0 ? "Yok" : string.Join(" -> ", ice.Select(e => $"{e.TileType}{Format(e.Coordinate)}"));
        }

        private static string Heights(LevelSpec spec)
        {
            string text = string.Join("; ", spec.Entries.Values
                .Where(e => e.HeightLevel > 0)
                .OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x)
                .Select(e => $"{Format(e.Coordinate)} h{e.HeightLevel}"));
            return string.IsNullOrWhiteSpace(text) ? "Tum tile'lar h0" : text;
        }

        private static string ClimbSummary(LevelSpec spec) => HeightTransitionSummary(spec, 1);
        private static string DescendSummary(LevelSpec spec) => HeightTransitionSummary(spec, -1);

        private static string HeightTransitionSummary(LevelSpec spec, int sign)
        {
            List<string> transitions = new();
            foreach (LevelTileEntry entry in spec.Entries.Values)
            {
                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = entry.Coordinate + direction;
                    if (!spec.Entries.TryGetValue(next, out LevelTileEntry other))
                    {
                        continue;
                    }

                    int delta = other.HeightLevel - entry.HeightLevel;
                    if (Math.Sign(delta) == sign)
                    {
                        transitions.Add($"{Format(entry.Coordinate)} -> {Format(next)}");
                    }
                }
            }

            return transitions.Count == 0 ? "Yok" : string.Join("; ", transitions.Take(12));
        }

        private static int ApproxMoveCount(string solution)
        {
            return solution.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static string Format(Vector2Int point) => $"({point.x},{point.y})";

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
        };

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
            public readonly Dictionary<Vector2Int, LevelTileEntry> Entries = new();
            public string Notes;
            public string Solution;

            public LevelSpec(string name, int width, int height, Vector2Int start, Vector2Int goal, string difficulty)
            {
                Name = name;
                Width = width;
                Height = height;
                Start = start;
                Goal = goal;
                Difficulty = difficulty;
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
