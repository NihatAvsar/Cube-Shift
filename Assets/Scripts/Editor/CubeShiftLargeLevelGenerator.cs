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
    /// <summary>Creates reusable tile prefabs, validated LevelData assets, and three playable scenes.</summary>
    public static class CubeShiftLargeLevelGenerator
    {
        private const string SourceScene = "Assets/Scenes/CubeShift_Mechanics_Test.unity";
        private const string LevelFolder = "Assets/Levels";
        private const string PrefabFolder = "Assets/Prefabs/Tiles";
        private const LevelObjective AllObjectives = LevelObjective.RedBreakable
            | LevelObjective.YellowButton | LevelObjective.GreenJump | LevelObjective.IceSlide;

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down
        };

        [MenuItem("Tools/Cube Shift/Generate Large Levels")]
        public static void GenerateLargeLevels()
        {
            EnsureFolder(LevelFolder);
            EnsureFolder(PrefabFolder);
            Dictionary<LevelTileType, GameObject> prefabs = CreatePrefabs();

            List<LevelSpec> specs = new()
            {
                CreateLevel01(),
                CreateLevel02(),
                CreateLevel03(),
                CreateLevel04(),
                CreateLevel05(),
                CreateLevel06(),
                CreateLevel07(),
                CreateLevel08(),
                CreateLevel09(),
                CreateLevel10()
            };

            List<string> scenePaths = new();
            foreach (LevelSpec spec in specs)
            {
                ValidationResult result = ValidateAndSolve(spec);
                if (!result.Success)
                {
                    Debug.LogError($"{spec.Name} was not generated: {result.Error}");
                    return;
                }

                spec.Solution = string.Join(", ", result.Solution.Select(DirectionName));
                LevelData data = SaveLevelData(spec);
                scenePaths.Add(CreatePlayableScene(spec, data, prefabs));
                Debug.Log($"{spec.Name}: validated solution with {result.Solution.Count} inputs.");
            }

            AddScenesToBuildSettings(scenePaths);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Cube Shift large levels generated and validated successfully.");
        }

        private static LevelSpec CreateLevel01()
        {
            LevelSpec level = new("Level_01", 18, 11, new Vector2Int(1, 1));
            level.Line(1, 1, 8, 1);
            level.Line(8, 1, 8, 4);
            level.Line(3, 4, 14, 4);
            level.Line(11, 1, 11, 4);
            level.Line(14, 4, 14, 5);
            level.Add(14, 7);
            level.Line(14, 7, 14, 8);
            level.Line(14, 8, 17, 8);
            level.Line(6, 1, 6, 3);
            level.Line(6, 3, 8, 3);

            level.Special(1, 1, LevelTileType.Start);
            level.Special(4, 4, LevelTileType.RedBreakable);
            level.Special(3, 4, LevelTileType.BreakableExit);
            level.Special(9, 4, LevelTileType.IceDownStart);
            level.Special(10, 4, LevelTileType.IceMiddle);
            level.Special(11, 4, LevelTileType.IceLanding);
            level.Special(14, 5, LevelTileType.GreenJumpUp);
            level.Special(14, 7, LevelTileType.JumpLanding);
            level.Special(11, 1, LevelTileType.YellowButton, "Gate01");
            level.Special(16, 8, LevelTileType.YellowDoor, "Gate01");
            level.Special(17, 8, LevelTileType.Goal);
            level.Notes = "18x11 orta seviye. Red yan kolu, yatay ice koridoru, dikey jump ve kapı öncesi geri dönüşlü YellowButton içerir.";
            return level;
        }

        private static LevelSpec CreateLevel02()
        {
            LevelSpec level = new("Level_02", 22, 13, new Vector2Int(1, 11));
            level.Line(1, 7, 1, 11);
            level.Line(1, 7, 16, 7);
            level.Line(4, 2, 4, 7);
            level.Line(4, 2, 8, 2);
            level.Line(8, 2, 8, 5);
            level.Line(8, 5, 10, 5);
            level.Line(10, 5, 10, 7);
            level.Line(15, 6, 17, 6);
            level.Line(15, 6, 15, 8);
            level.Line(17, 6, 17, 8);
            level.Line(15, 8, 18, 8);
            level.Add(20, 8);
            level.Line(20, 8, 20, 11);
            level.Line(20, 11, 21, 11);
            level.Line(2, 9, 5, 9);
            level.Line(5, 9, 5, 11);

            level.Special(1, 11, LevelTileType.Start);
            level.Special(7, 7, LevelTileType.RedBreakable);
            level.Special(8, 7, LevelTileType.BreakableExit);
            level.Special(11, 7, LevelTileType.IceDownStart);
            level.Special(12, 7, LevelTileType.IceMiddle);
            level.Special(13, 7, LevelTileType.IceMiddle);
            level.Special(14, 7, LevelTileType.IceMiddle);
            level.Special(15, 7, LevelTileType.IceMiddle);
            level.Special(16, 7, LevelTileType.IceLanding);
            level.Special(8, 2, LevelTileType.YellowButton, "Gate02");
            level.Special(18, 8, LevelTileType.GreenJumpUp);
            level.Special(20, 8, LevelTileType.JumpLanding);
            level.Special(20, 10, LevelTileType.YellowDoor, "Gate02");
            level.Special(20, 11, LevelTileType.Goal);
            level.Notes = "22x13 orta-zor seviye. Oyuncu alt kola inip butonu açar, ana yola döner, uzun ice zincirini ve riskli jump boşluğunu geçer.";
            return level;
        }

        private static LevelSpec CreateLevel03()
        {
            LevelSpec level = new("Level_03", 24, 15, new Vector2Int(1, 2));
            level.Line(1, 2, 7, 2);
            level.Line(4, 2, 4, 6);
            level.Line(4, 6, 9, 6);
            level.Line(7, 2, 7, 4);
            level.Line(7, 4, 12, 4);
            level.Line(12, 4, 12, 8);
            level.Line(12, 8, 19, 8);
            level.Line(19, 8, 19, 11);
            level.Line(15, 8, 15, 10);
            level.Line(15, 10, 17, 10);
            level.Line(19, 11, 21, 11);
            level.Add(23, 11);
            level.Line(23, 11, 23, 13);
            level.Line(20, 13, 23, 13);
            level.Line(2, 4, 4, 4);
            level.Line(2, 4, 2, 7);
            level.Line(2, 7, 6, 7);
            level.Line(9, 5, 9, 7);

            level.Special(1, 2, LevelTileType.Start);
            level.Special(9, 6, LevelTileType.YellowButton, "Gate03");
            level.Special(10, 4, LevelTileType.YellowDoor, "Gate03");
            level.Special(13, 8, LevelTileType.IceDownStart);
            level.Special(14, 8, LevelTileType.IceMiddle);
            level.Special(15, 8, LevelTileType.IceMiddle);
            level.Special(16, 8, LevelTileType.IceMiddle);
            level.Special(17, 8, LevelTileType.IceMiddle);
            level.Special(18, 8, LevelTileType.IceMiddle);
            level.Special(19, 8, LevelTileType.IceLanding);
            level.Special(19, 10, LevelTileType.RedBreakable);
            level.Special(19, 11, LevelTileType.BreakableExit);
            level.Special(21, 11, LevelTileType.GreenJumpUp);
            level.Special(23, 11, LevelTileType.JumpLanding);
            level.Special(20, 13, LevelTileType.Goal);
            level.Notes = "24x15 zor S/L biçimli seviye. Zorunlu sıra YellowDoor, uzun IceSlide, RedBreakable, GreenJump ve Goal alanıdır.";
            return level;
        }

        private static LevelSpec CreateLevel04()
        {
            LevelSpec source = CreateLevel03();
            LevelSpec level = new("Level_04", 26, 16, new Vector2Int(24, source.Start.y + 1));

            foreach (LevelTileEntry entry in source.Entries.Values)
            {
                Vector2Int mirrored = new Vector2Int(25 - entry.Coordinate.x, entry.Coordinate.y + 1);
                level.Special(mirrored.x, mirrored.y, entry.TileType, entry.LinkId);
            }

            level.Notes = "26x16 zor final remix. Level 3 yapısının yatay aynasıdır; yaklaşım yönleri ve küp rotası tersine çevrilmiştir.";
            return level;
        }

        private static LevelSpec CreateLevel05()
        {
            LevelSpec level = new("Level_05", 20, 12, new Vector2Int(1, 1));
            level.Line(1, 1, 5, 1, 0);
            level.Line(5, 1, 5, 5, 1);
            level.Line(5, 5, 10, 5, 1);
            level.Line(10, 5, 10, 7, 0);
            level.Line(10, 7, 15, 7, 0);
            level.Line(15, 7, 15, 9, 1);
            level.Line(15, 9, 17, 9, 1);
            level.Add(19, 9, 0);
            level.Line(19, 9, 19, 10, 0);
            level.Line(3, 1, 3, 3, 0);
            level.Line(12, 7, 12, 5, 0);
            level.Special(1, 1, LevelTileType.Start);
            level.Special(4, 1, LevelTileType.RedBreakable);
            level.Special(5, 3, LevelTileType.BreakableExit, "", 1);
            level.Special(6, 5, LevelTileType.YellowButton, "Gate05", 1);
            level.Special(7, 5, LevelTileType.IceDownStart, "", 1);
            level.Special(8, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(9, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(10, 5, LevelTileType.IceLanding, "", 1);
            level.Special(15, 9, LevelTileType.YellowDoor, "Gate05", 1);
            level.Special(17, 9, LevelTileType.GreenJumpUp, "", 1);
            level.Special(19, 9, LevelTileType.JumpLanding);
            level.Special(19, 10, LevelTileType.Goal);
            level.Notes = "Tırmanışı tanıtan zikzak rota; yüksek ice köprüsünden alçak avluya ve ikinci yüksek kapıya geçilir.";
            return level;
        }

        private static LevelSpec CreateLevel06()
        {
            LevelSpec level = new("Level_06", 22, 13, new Vector2Int(20, 1));
            level.Line(20, 1, 15, 1, 0);
            level.Line(15, 1, 15, 5, 1);
            level.Line(15, 5, 10, 5, 1);
            level.Line(10, 5, 10, 9, 0);
            level.Line(10, 9, 5, 9, 0);
            level.Line(5, 9, 5, 5, 1);
            level.Line(5, 5, 3, 5, 1);
            level.Add(1, 5, 0);
            level.Line(1, 5, 1, 7, 0);
            level.Line(18, 1, 18, 3, 0);
            level.Line(8, 9, 8, 11, 0);
            level.Special(20, 1, LevelTileType.Start);
            level.Special(17, 1, LevelTileType.RedBreakable);
            level.Special(15, 3, LevelTileType.BreakableExit, "", 1);
            level.Special(14, 5, LevelTileType.IceDownStart, "", 1);
            level.Special(13, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(12, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(11, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(10, 5, LevelTileType.IceLanding, "", 1);
            level.Special(8, 9, LevelTileType.YellowButton, "Gate06");
            level.Special(5, 7, LevelTileType.YellowDoor, "Gate06", 1);
            level.Special(3, 5, LevelTileType.GreenJumpUp, "", 1);
            level.Special(1, 5, LevelTileType.JumpLanding);
            level.Special(1, 7, LevelTileType.Goal);
            level.Notes = "Ters U rotası; sağdan başlayan oyuncu yüksek ice hattını geçip alt avludan kapıya geri döner.";
            return level;
        }

        private static LevelSpec CreateLevel07()
        {
            LevelSpec level = new("Level_07", 24, 14, new Vector2Int(2, 12));
            level.Line(2, 12, 2, 8, 0);
            level.Line(2, 8, 7, 8, 1);
            level.Line(7, 8, 7, 3, 1);
            level.Line(7, 3, 13, 3, 0);
            level.Line(13, 3, 13, 10, 0);
            level.Line(13, 10, 18, 10, 1);
            level.Line(18, 10, 18, 6, 1);
            level.Line(18, 6, 21, 6, 1);
            level.Add(23, 6, 0);
            level.Line(23, 6, 23, 4, 0);
            level.Line(4, 8, 4, 11, 1);
            level.Line(13, 6, 10, 6, 0);
            level.Special(2, 12, LevelTileType.Start);
            level.Special(2, 9, LevelTileType.RedBreakable);
            level.Special(5, 8, LevelTileType.YellowButton, "Gate07", 1);
            level.Special(7, 6, LevelTileType.IceDownStart, "", 1);
            level.Special(7, 5, LevelTileType.IceMiddle, "", 1);
            level.Special(7, 4, LevelTileType.IceMiddle, "", 1);
            level.Special(7, 3, LevelTileType.IceLanding, "", 1);
            level.Special(13, 8, LevelTileType.BreakableExit);
            level.Special(18, 8, LevelTileType.YellowDoor, "Gate07", 1);
            level.Special(21, 6, LevelTileType.GreenJumpUp, "", 1);
            level.Special(23, 6, LevelTileType.JumpLanding);
            level.Special(23, 4, LevelTileType.Goal);
            level.Notes = "İki kuleli S rotası; dikey ice inişi, alçak merkez ve yüksek doğu yolu farklı katmanlar oluşturur.";
            return level;
        }

        private static LevelSpec CreateLevel08()
        {
            LevelSpec level = new("Level_08", 26, 15, new Vector2Int(1, 7));
            level.Line(1, 7, 6, 7, 0);
            level.Line(6, 7, 6, 12, 1);
            level.Line(6, 12, 13, 12, 1);
            level.Line(13, 12, 13, 5, 0);
            level.Line(13, 5, 20, 5, 0);
            level.Line(20, 5, 20, 10, 1);
            level.Line(20, 10, 23, 10, 1);
            level.Add(25, 10, 0);
            level.Line(25, 10, 25, 13, 0);
            level.Line(3, 7, 3, 3, 0);
            level.Line(9, 12, 9, 9, 1);
            level.Line(16, 5, 16, 2, 0);
            level.Special(1, 7, LevelTileType.Start);
            level.Special(4, 7, LevelTileType.RedBreakable);
            level.Special(6, 10, LevelTileType.BreakableExit, "", 1);
            level.Special(8, 12, LevelTileType.IceDownStart, "", 1);
            level.Special(9, 12, LevelTileType.IceMiddle, "", 1);
            level.Special(10, 12, LevelTileType.IceMiddle, "", 1);
            level.Special(11, 12, LevelTileType.IceMiddle, "", 1);
            level.Special(12, 12, LevelTileType.IceMiddle, "", 1);
            level.Special(13, 12, LevelTileType.IceLanding, "", 1);
            level.Special(16, 5, LevelTileType.YellowButton, "Gate08");
            level.Special(20, 8, LevelTileType.YellowDoor, "Gate08", 1);
            level.Special(23, 10, LevelTileType.GreenJumpUp, "", 1);
            level.Special(25, 10, LevelTileType.JumpLanding);
            level.Special(25, 13, LevelTileType.Goal);
            level.Notes = "Büyük C-kancası; kuzey yüksek ice hattı, güney buton çıkmazı ve doğu kapı kulesi bulunur.";
            return level;
        }

        private static LevelSpec CreateLevel09()
        {
            LevelSpec level = new("Level_09", 28, 16, new Vector2Int(2, 2));
            level.Line(2, 2, 9, 2, 0);
            level.Line(9, 2, 9, 7, 1);
            level.Line(9, 7, 4, 7, 1);
            level.Line(4, 7, 4, 13, 0);
            level.Line(4, 13, 15, 13, 0);
            level.Line(15, 13, 15, 8, 1);
            level.Line(15, 8, 22, 8, 1);
            level.Line(22, 8, 22, 3, 0);
            level.Line(22, 3, 25, 3, 0);
            level.Add(27, 3, 0);
            level.Line(27, 3, 27, 6, 0);
            level.Line(6, 13, 6, 10, 0);
            level.Line(18, 8, 18, 12, 1);
            level.Special(2, 2, LevelTileType.Start);
            level.Special(6, 2, LevelTileType.RedBreakable);
            level.Special(9, 5, LevelTileType.BreakableExit, "", 1);
            level.Special(8, 7, LevelTileType.IceDownStart, "", 1);
            level.Special(7, 7, LevelTileType.IceMiddle, "", 1);
            level.Special(6, 7, LevelTileType.IceMiddle, "", 1);
            level.Special(5, 7, LevelTileType.IceMiddle, "", 1);
            level.Special(4, 7, LevelTileType.IceLanding, "", 1);
            level.Special(8, 13, LevelTileType.YellowButton, "Gate09");
            level.Special(15, 10, LevelTileType.YellowDoor, "Gate09", 1);
            level.Special(25, 3, LevelTileType.GreenJumpUp);
            level.Special(27, 3, LevelTileType.JumpLanding);
            level.Special(27, 6, LevelTileType.Goal);
            level.Notes = "Spiral benzeri uzun rota; batı ve merkezde iki ayrı tırmanış, yüksek geri dönüş koridoru ve doğu finali vardır.";
            return level;
        }

        private static LevelSpec CreateLevel10()
        {
            LevelSpec level = new("Level_10", 30, 17, new Vector2Int(1, 15));
            level.Line(1, 15, 7, 15, 0);
            level.Line(7, 15, 7, 10, 1);
            level.Line(7, 10, 3, 10, 1);
            level.Line(3, 10, 3, 5, 0);
            level.Line(3, 5, 12, 5, 0);
            level.Line(12, 5, 12, 12, 1);
            level.Line(12, 12, 20, 12, 1);
            level.Line(20, 12, 20, 6, 0);
            level.Line(20, 6, 26, 6, 0);
            level.Line(26, 6, 26, 13, 1);
            level.Line(26, 13, 27, 13, 1);
            level.Add(29, 13, 0);
            level.Line(29, 13, 29, 15, 0);
            level.Line(5, 15, 5, 12, 0);
            level.Line(9, 5, 9, 2, 0);
            level.Line(16, 12, 16, 15, 1);
            level.Line(23, 6, 23, 3, 0);
            level.Special(1, 15, LevelTileType.Start);
            level.Special(5, 15, LevelTileType.RedBreakable);
            level.Special(7, 12, LevelTileType.BreakableExit, "", 1);
            level.Special(6, 10, LevelTileType.IceDownStart, "", 1);
            level.Special(5, 10, LevelTileType.IceMiddle, "", 1);
            level.Special(4, 10, LevelTileType.IceMiddle, "", 1);
            level.Special(3, 10, LevelTileType.IceLanding, "", 1);
            level.Special(9, 5, LevelTileType.YellowButton, "Gate10");
            level.Special(12, 9, LevelTileType.YellowDoor, "Gate10", 1);
            level.Special(27, 13, LevelTileType.GreenJumpUp, "", 1);
            level.Special(29, 13, LevelTileType.JumpLanding);
            level.Special(29, 15, LevelTileType.Goal);
            level.Notes = "Final zikzak; batı ice terası, merkez kapı kulesi, uzun doğu inişi ve yüksek final jump bölgesinden oluşur.";
            return level;
        }

        private static ValidationResult ValidateAndSolve(LevelSpec level)
        {
            if (level.Width == level.Height || (level.Width < 16 && level.Height < 10))
            {
                return ValidationResult.Fail("Map must be non-square and meet minimum dimensions.");
            }

            if (level.Count(LevelTileType.Start) != 1 || level.Count(LevelTileType.Goal) != 1)
            {
                return ValidationResult.Fail("Map must contain exactly one Start and one Goal.");
            }

            foreach (LevelTileEntry button in level.Entries.Values.Where(e => e.TileType == LevelTileType.YellowButton))
            {
                if (string.IsNullOrWhiteSpace(button.LinkId)
                    || !level.Entries.Values.Any(e => e.TileType == LevelTileType.YellowDoor && e.LinkId == button.LinkId))
                {
                    return ValidationResult.Fail($"Button at {button.Coordinate} has no linked door.");
                }
            }

            foreach (LevelTileEntry jump in level.Entries.Values.Where(e => e.TileType == LevelTileType.GreenJumpUp))
            {
                bool hasLanding = Directions.Any(direction =>
                    level.TypeAt(jump.Coordinate + direction) == LevelTileType.Empty
                    && level.TypeAt(jump.Coordinate + direction * 2) == LevelTileType.JumpLanding);
                if (!hasLanding)
                {
                    return ValidationResult.Fail($"Jump at {jump.Coordinate} has no aligned landing across one empty cell.");
                }
            }

            foreach (LevelTileEntry iceStart in level.Entries.Values.Where(e => e.TileType == LevelTileType.IceDownStart))
            {
                if (!HasIceLanding(level, iceStart.Coordinate))
                {
                    return ValidationResult.Fail($"Ice start at {iceStart.Coordinate} has no straight chain ending at IceLanding.");
                }
            }

            if (CanWalkToGoalWithDoorsRemoved(level))
            {
                return ValidationResult.Fail("Goal is reachable without crossing a YellowDoor.");
            }

            Queue<SearchNode> queue = new();
            HashSet<SearchState> visited = new();
            SearchState initial = SearchState.Initial(level.Start);
            queue.Enqueue(new SearchNode(initial, new List<Vector2Int>()));
            visited.Add(initial);

            while (queue.Count > 0 && visited.Count < 300000)
            {
                SearchNode node = queue.Dequeue();
                foreach (Vector2Int direction in Directions)
                {
                    if (!TryMove(level, node.State, direction, out SearchState next))
                    {
                        continue;
                    }

                    List<Vector2Int> route = new(node.Route) { direction };
                    if (level.TypeAt(next.Position) == LevelTileType.Goal && next.Objectives == AllObjectives)
                    {
                        return ValidationResult.Pass(route);
                    }

                    if (visited.Add(next))
                    {
                        queue.Enqueue(new SearchNode(next, route));
                    }
                }
            }

            return ValidationResult.Fail($"No complete solution found after checking {visited.Count} states.");
        }

        private static bool HasIceLanding(LevelSpec level, Vector2Int start)
        {
            foreach (Vector2Int direction in Directions)
            {
                Vector2Int position = start + direction;
                bool foundIce = false;
                for (int step = 0; step < 20; step++)
                {
                    LevelTileType type = level.TypeAt(position);
                    if (type == LevelTileType.IceMiddle)
                    {
                        foundIce = true;
                        position += direction;
                        continue;
                    }

                    if (foundIce && type == LevelTileType.IceLanding)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private static bool CanWalkToGoalWithDoorsRemoved(LevelSpec level)
        {
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new() { level.Start };
            queue.Enqueue(level.Start);

            while (queue.Count > 0)
            {
                Vector2Int position = queue.Dequeue();
                if (level.TypeAt(position) == LevelTileType.Goal)
                {
                    return true;
                }

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = position + direction;
                    LevelTileType type = level.TypeAt(next);
                    if (type == LevelTileType.Empty || type == LevelTileType.YellowDoor || !visited.Add(next))
                    {
                        continue;
                    }

                    queue.Enqueue(next);
                }
            }

            return false;
        }

        private static bool TryMove(LevelSpec level, SearchState state, Vector2Int direction, out SearchState result)
        {
            result = state;
            Vector2Int nextPosition = state.Position + direction;
            LevelTileType nextType = level.TypeAt(nextPosition);
            if (nextType == LevelTileType.Empty || (nextType == LevelTileType.YellowDoor && !state.YellowOpen))
            {
                return false;
            }

            if (Math.Abs(level.HeightAt(nextPosition) - level.HeightAt(state.Position)) > 1)
            {
                return false;
            }

            result = state.RollTo(nextPosition, direction);
            return ResolveLanding(level, direction, ref result, 0);
        }

        private static bool ResolveLanding(LevelSpec level, Vector2Int direction, ref SearchState state, int depth)
        {
            if (depth > 30)
            {
                return false;
            }

            LevelTileEntry entry = level.EntryAt(state.Position);
            if (entry == null)
            {
                return false;
            }

            if (entry.TileType == LevelTileType.RedBreakable)
            {
                state = state.WithObjective(LevelObjective.RedBreakable);
            }
            else if (entry.TileType == LevelTileType.YellowButton)
            {
                state = state.WithObjective(LevelObjective.YellowButton);
            }
            else if (entry.TileType is LevelTileType.IceDownStart or LevelTileType.IceMiddle)
            {
                state = state.WithObjective(LevelObjective.IceSlide);
                Vector2Int next = state.Position + direction;
                LevelTileType nextType = level.TypeAt(next);
                if (nextType == LevelTileType.Empty || (nextType == LevelTileType.YellowDoor && !state.YellowOpen))
                {
                    return true;
                }

                if (Math.Abs(level.HeightAt(next) - level.HeightAt(state.Position)) > 1)
                {
                    return true;
                }

                state = state.RollTo(next, direction);
                return ResolveLanding(level, direction, ref state, depth + 1);
            }
            else if (entry.TileType == LevelTileType.GreenJumpUp)
            {
                state = state.WithObjective(LevelObjective.GreenJump);
                for (int step = 0; step < 2; step++)
                {
                    state = state.RollTo(state.Position + direction, direction);
                }

                return level.TypeAt(state.Position) != LevelTileType.Empty
                    && ResolveLanding(level, direction, ref state, depth + 1);
            }

            return true;
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

            string notes = $"{spec.Notes}\n\nSpecial tiles:\n{spec.SpecialSummary()}\n\nVerified solution inputs: {spec.Solution.Split(',').Length}";
            data.SetEditorData(spec.Name, spec.Width, spec.Height, 1f, spec.Start, AllObjectives,
                spec.Entries.Values.OrderBy(e => e.Coordinate.y).ThenBy(e => e.Coordinate.x).ToList(),
                notes, spec.Solution);
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
            serialized.FindProperty("playerTransform").objectReferenceValue = player.transform;
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
            serialized.ApplyModifiedPropertiesWithoutUndo();
            builder.BuildLevel();

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographicSize = spec.Width >= 24 ? 8f : 7f;
            }

            string path = $"Assets/Scenes/{spec.Name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static Dictionary<LevelTileType, GameObject> CreatePrefabs()
        {
            Material normal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/NormalTile.mat");
            Material goal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/GoalTile.mat");
            Material red = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/BreakableTile.mat");
            Material yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/ButtonTile.mat");
            Material door = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/DoorTile.mat");
            Material green = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/JumpTile.mat");
            Material ice = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tiles/IceTile.mat");

            Dictionary<LevelTileType, GameObject> result = new();
            result[LevelTileType.Normal] = SaveTilePrefab<NormalTile>("NormalTile", normal);
            result[LevelTileType.Start] = SaveTilePrefab<NormalTile>("START", normal);
            result[LevelTileType.Goal] = SaveTilePrefab<GoalTile>("Goal_BehindDoor", goal);
            result[LevelTileType.RedBreakable] = SaveTilePrefab<BreakableTile>("LEFT_Red_Breaks", red);
            result[LevelTileType.BreakableExit] = SaveTilePrefab<NormalTile>("BreakableExit", normal);
            result[LevelTileType.YellowButton] = SaveTilePrefab<ButtonTile>("RIGHT_Yellow_OpensDoor", yellow);
            result[LevelTileType.YellowDoor] = SaveDoorPrefab(normal, door);
            result[LevelTileType.GreenJumpUp] = SaveTilePrefab<JumpTile>("UP_Green_Jumps", green);
            result[LevelTileType.JumpLanding] = SaveTilePrefab<NormalTile>("JumpLanding", normal);
            result[LevelTileType.IceDownStart] = SaveTilePrefab<IceTile>("DOWN_Ice_Slide_1", ice);
            result[LevelTileType.IceMiddle] = SaveTilePrefab<IceTile>("Ice_Slide_Middle", ice);
            result[LevelTileType.IceLanding] = SaveTilePrefab<NormalTile>("IceLanding", normal);
            return result;
        }

        private static GameObject SaveTilePrefab<T>(string name, Material material) where T : TileBase
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = name;
            tile.transform.localScale = new Vector3(0.96f, 0.2f, 0.96f);
            tile.layer = GroundLayer;
            tile.GetComponent<Renderer>().sharedMaterial = material;
            T component = tile.AddComponent<T>();
            SerializedObject serialized = new(component);
            SerializedProperty yellowRequirement = serialized.FindProperty("requireYellowBottomFace");
            if (yellowRequirement != null)
            {
                yellowRequirement.boolValue = false;
            }
            SerializedProperty redRequirement = serialized.FindProperty("requireRedBottomFace");
            if (redRequirement != null)
            {
                redRequirement.boolValue = false;
            }
            SerializedProperty greenRequirement = serialized.FindProperty("requireGreenBottomFace");
            if (greenRequirement != null)
            {
                greenRequirement.boolValue = false;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            string path = $"{PrefabFolder}/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tile, path);
            UnityEngine.Object.DestroyImmediate(tile);
            return prefab;
        }

        private static GameObject SaveDoorPrefab(Material floor, Material doorMaterial)
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "DoorTile_YellowGate";
            root.transform.localScale = new Vector3(0.96f, 0.2f, 0.96f);
            root.layer = GroundLayer;
            root.GetComponent<Renderer>().sharedMaterial = floor;
            DoorTile tile = root.AddComponent<DoorTile>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "DoorVisual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 15f, 0f);
            visual.transform.localScale = new Vector3(0.83f, 25f, 0.15f);
            visual.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            SerializedObject serialized = new(tile);
            serialized.FindProperty("doorVisual").objectReferenceValue = visual.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/DoorTile_YellowGate.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AddScenesToBuildSettings(List<string> paths)
        {
            const string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            List<EditorBuildSettingsScene> otherScenes = EditorBuildSettings.scenes
                .Where(scene => !paths.Contains(scene.path) && scene.path != mainMenuPath).ToList();
            List<EditorBuildSettingsScene> scenes = new();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(mainMenuPath) != null)
            {
                scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            }
            scenes.AddRange(paths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .Concat(otherScenes));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetPrefab(SerializedObject serialized, string property, GameObject prefab)
        {
            serialized.FindProperty(property).objectReferenceValue = prefab;
        }

        private static string DirectionName(Vector2Int direction)
        {
            if (direction == Vector2Int.right) return "Right";
            if (direction == Vector2Int.left) return "Left";
            if (direction == Vector2Int.up) return "Up";
            return "Down";
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

        private static int GroundLayer => Mathf.Max(0, LayerMask.NameToLayer("Ground"));

        private sealed class LevelSpec
        {
            public readonly string Name;
            public readonly int Width;
            public readonly int Height;
            public readonly Vector2Int Start;
            public readonly Dictionary<Vector2Int, LevelTileEntry> Entries = new();
            public string Notes;
            public string Solution;

            public LevelSpec(string name, int width, int height, Vector2Int start)
            {
                Name = name;
                Width = width;
                Height = height;
                Start = start;
            }

            public void Add(int x, int y, int height = 0) => Special(x, y, LevelTileType.Normal, "", height);
            public void Special(int x, int y, LevelTileType type, string link = "", int height = 0)
                => Entries[new Vector2Int(x, y)] = new LevelTileEntry(new Vector2Int(x, y), type, link, height);

            public void Line(int x1, int y1, int x2, int y2, int height = 0)
            {
                int dx = Math.Sign(x2 - x1);
                int dy = Math.Sign(y2 - y1);
                int x = x1;
                int y = y1;
                while (true)
                {
                    Add(x, y, height);
                    if (x == x2 && y == y2) break;
                    x += dx;
                    y += dy;
                }
            }

            public int Count(LevelTileType type) => Entries.Values.Count(entry => entry.TileType == type);
            public LevelTileEntry EntryAt(Vector2Int position) => Entries.TryGetValue(position, out LevelTileEntry entry) ? entry : null;
            public LevelTileType TypeAt(Vector2Int position) => EntryAt(position)?.TileType ?? LevelTileType.Empty;
            public int HeightAt(Vector2Int position) => EntryAt(position)?.HeightLevel ?? 0;
            public string SpecialSummary() => string.Join("\n", Entries.Values
                .Where(entry => entry.TileType is not LevelTileType.Normal)
                .OrderBy(entry => entry.TileType)
                .Select(entry => $"- {entry.TileType}: ({entry.Coordinate.x}, {entry.Coordinate.y}, h{entry.HeightLevel})"));
        }

        private readonly struct SearchNode
        {
            public readonly SearchState State;
            public readonly List<Vector2Int> Route;
            public SearchNode(SearchState state, List<Vector2Int> route) { State = state; Route = route; }
        }

        private struct SearchState : IEquatable<SearchState>
        {
            public Vector2Int Position;
            public CubeFace Top, Bottom, Front, Back, Left, Right;
            public LevelObjective Objectives;
            public bool YellowOpen => (Objectives & LevelObjective.YellowButton) != 0;

            public static SearchState Initial(Vector2Int position) => new()
            {
                Position = position, Top = CubeFace.Blue, Bottom = CubeFace.White,
                Front = CubeFace.Green, Back = CubeFace.Purple, Left = CubeFace.Red, Right = CubeFace.Yellow
            };

            public SearchState WithObjective(LevelObjective objective) { SearchState copy = this; copy.Objectives |= objective; return copy; }

            public SearchState RollTo(Vector2Int position, Vector2Int direction)
            {
                SearchState copy = this;
                copy.Position = position;
                CubeFace top = Top, bottom = Bottom, front = Front, back = Back, left = Left, right = Right;
                if (direction == Vector2Int.up) { copy.Top = back; copy.Bottom = front; copy.Front = top; copy.Back = bottom; }
                else if (direction == Vector2Int.down) { copy.Top = front; copy.Bottom = back; copy.Front = bottom; copy.Back = top; }
                else if (direction == Vector2Int.left) { copy.Top = right; copy.Bottom = left; copy.Left = top; copy.Right = bottom; }
                else { copy.Top = left; copy.Bottom = right; copy.Left = bottom; copy.Right = top; }
                return copy;
            }

            public bool Equals(SearchState other) => Position == other.Position && Top == other.Top && Bottom == other.Bottom
                && Front == other.Front && Back == other.Back && Left == other.Left && Right == other.Right && Objectives == other.Objectives;
            public override bool Equals(object obj) => obj is SearchState other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Position, Top, Bottom, Front, Back, Left, Right, Objectives);
        }

        private sealed class ValidationResult
        {
            public bool Success;
            public string Error;
            public List<Vector2Int> Solution;
            public static ValidationResult Pass(List<Vector2Int> route) => new() { Success = true, Solution = route };
            public static ValidationResult Fail(string error) => new() { Error = error, Solution = new List<Vector2Int>() };
        }
    }
}
