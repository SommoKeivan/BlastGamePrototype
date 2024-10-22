using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities
{
    /// <summary>
    /// Class that provides methods for changing the game scene
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        private const string StartMenuSceneName = "MenuScene";
        private const string InGameMenuSceneName = "GameScene";
        private const string GameOverMenuSceneName = "GameOverScene";


        public static void LoadStartMenu() => LoadScene(StartMenuSceneName);

        public static void LoadInGameMenu() => LoadScene(InGameMenuSceneName);

        public static void LoadGameOverMenu() => LoadScene(GameOverMenuSceneName);

        private static void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    }
}

