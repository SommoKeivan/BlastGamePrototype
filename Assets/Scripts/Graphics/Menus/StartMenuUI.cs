using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Graphics.Menus
{
    /// <summary>
    /// Class representing the graphical user interface of the start menu
    /// </summary>
    public class StartMenuUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button newGameButton;

        private void Awake()
        {
            scoreText.text = SavesManager.GetHighScore().ToString();
            newGameButton.onClick.AddListener(SceneController.LoadInGameMenu);
        }
    }
}
