using UnityEngine;

namespace CubeShift.Core
{
    public static class ProgressManager
    {
        private const string UnlockedLevelKey = "CubeShift.Progress.UnlockedLevel";
        private const string LastPlayedLevelKey = "CubeShift.Progress.LastPlayedLevel";
        private const string CompletedPrefix = "CubeShift.Progress.Completed.";
        private const string StarsPrefix = "CubeShift.Progress.Stars.";
        private const string BestTimePrefix = "CubeShift.Progress.BestTime.";
        private const string BestMovesPrefix = "CubeShift.Progress.BestMoves.";

        public static int UnlockedLevel => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedLevelKey, 1));
        public static int LastPlayedLevel => Mathf.Max(1, PlayerPrefs.GetInt(LastPlayedLevelKey, UnlockedLevel));

        public static bool IsUnlocked(int levelNumber)
        {
            return levelNumber <= UnlockedLevel;
        }

        public static bool IsCompleted(int levelNumber)
        {
            return PlayerPrefs.GetInt(CompletedPrefix + levelNumber, 0) == 1;
        }

        public static int GetStars(int levelNumber)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(StarsPrefix + levelNumber, 0), 0, 3);
        }

        public static float GetBestTime(int levelNumber)
        {
            return PlayerPrefs.GetFloat(BestTimePrefix + levelNumber, 0f);
        }

        public static int GetBestMoves(int levelNumber)
        {
            return PlayerPrefs.GetInt(BestMovesPrefix + levelNumber, 0);
        }

        public static void MarkLevelStarted(int levelNumber)
        {
            if (levelNumber <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(LastPlayedLevelKey, levelNumber);
            if (levelNumber > UnlockedLevel)
            {
                PlayerPrefs.SetInt(UnlockedLevelKey, levelNumber);
            }

            PlayerPrefs.Save();
        }

        public static void MarkLevelComplete(int levelNumber, int stars)
        {
            MarkLevelComplete(levelNumber, stars, 0f, 0);
        }

        public static void MarkLevelComplete(int levelNumber, int stars, float completionTime, int moves)
        {
            if (levelNumber <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(CompletedPrefix + levelNumber, 1);
            PlayerPrefs.SetInt(StarsPrefix + levelNumber, Mathf.Max(GetStars(levelNumber), Mathf.Clamp(stars, 1, 3)));
            if (completionTime > 0f)
            {
                float bestTime = GetBestTime(levelNumber);
                if (bestTime <= 0f || completionTime < bestTime)
                {
                    PlayerPrefs.SetFloat(BestTimePrefix + levelNumber, completionTime);
                }
            }

            if (moves > 0)
            {
                int bestMoves = GetBestMoves(levelNumber);
                if (bestMoves <= 0 || moves < bestMoves)
                {
                    PlayerPrefs.SetInt(BestMovesPrefix + levelNumber, moves);
                }
            }

            PlayerPrefs.SetInt(UnlockedLevelKey, Mathf.Max(UnlockedLevel, levelNumber + 1));
            PlayerPrefs.SetInt(LastPlayedLevelKey, levelNumber + 1);
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            int maxLevel = Mathf.Max(UnlockedLevel + 5, 100);
            PlayerPrefs.DeleteKey(UnlockedLevelKey);
            PlayerPrefs.DeleteKey(LastPlayedLevelKey);
            for (int level = 1; level <= maxLevel; level++)
            {
                PlayerPrefs.DeleteKey(CompletedPrefix + level);
                PlayerPrefs.DeleteKey(StarsPrefix + level);
                PlayerPrefs.DeleteKey(BestTimePrefix + level);
                PlayerPrefs.DeleteKey(BestMovesPrefix + level);
            }

            PlayerPrefs.Save();
        }

        public static int CountCompleted(int maxLevel)
        {
            int count = 0;
            for (int level = 1; level <= maxLevel; level++)
            {
                if (IsCompleted(level))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
