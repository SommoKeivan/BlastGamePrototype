using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Static class that provides methods for saving game scores and methods for get them
    /// </summary>
    public static class SavesManager
    {
        private const string HighScoreText = "HighScore";
        private const string LastScoreText = "LastScore";
    
    
    
        public static void SaveHighScore(int newHighScore) => PlayerPrefs.SetInt(HighScoreText, newHighScore);
        public static int GetHighScore() => PlayerPrefs.GetInt(HighScoreText, 0);

        public static void SaveLastScore(int newLastScore) => PlayerPrefs.SetInt(LastScoreText, newLastScore);
        public static int GetLastScore() => PlayerPrefs.GetInt(LastScoreText, 0);
    }
}
