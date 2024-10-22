using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Graphics.Menus
{
    /// <summary>
    /// Class representing the graphical user interface of Game Over
    /// </summary>
    public class GameOverMenuUI : MonoBehaviour
    {
        [Header("Scores")]
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI lastScoreText;
    
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button startMenuButton;

        private void Awake()
        {
            highScoreText.text = SavesManager.GetHighScore().ToString();
            lastScoreText.text = SavesManager.GetLastScore().ToString();
        
            newGameButton.onClick.AddListener(SceneController.LoadInGameMenu);
            startMenuButton.onClick.AddListener(SceneController.LoadStartMenu);
        }
    }
}
