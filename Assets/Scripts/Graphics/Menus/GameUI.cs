using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utilities;

namespace Graphics.Menus
{
    /// <summary>
    /// Class representing the graphical part of the game.
    /// The class requires the AudioSource component.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class GameUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Transform blocksSpawnPoint;
        [SerializeField] private float blocksDistance = 50f;    // Distance between different GameBlocksUI
    
        [Header("Blocks Prefabs")]
        [SerializeField] private GameBlockUI colorBlockPrefab;
        [SerializeField] private GameBlockUI bombBlockPrefab;
        [SerializeField] private GameBlockUI stripeBlockPrefab;
    
        [Header("Colors Material")]
        [SerializeField] private Color red = Color.red;
        [SerializeField] private Color blue = Color.blue;
        [SerializeField] private Color green = Color.green;
        [SerializeField] private Color yellow = Color.yellow;
        [SerializeField] private Color violet = Color.magenta;
        [SerializeField] private Color orange = new (255f / 255f, 165f / 255f, 0f / 255f);

        private GameBlockUI[,] _blocks;
        private AudioSource _audioSource;

        private const float TimeToWaitAtGameOver = 1.5f;

        public void SetTimer(int seconds) => timerText.text = ConvertSecondsToTimeText(seconds);
        public void SetScore(uint score) => scoreText.text = score.ToString();
    
    
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }


        /// <summary>
        /// Method that initializes the graphical part of the game.
        /// Starting from the grid of GameBlocks taken as input, creates a grid of GameBlockUI.
        /// </summary>
        /// <param name="height">Grid height</param>
        /// <param name="width">Grid width</param>
        /// <param name="gameBlocks">Game blocks grid</param>
        /// <param name="interactionAction">Method that handles the interaction caused by mashing the GameBlockUI</param>
        public void InitializeGame(uint height, uint width, GameBlock[,] gameBlocks, GameLogic.InteractionAction interactionAction)
        {
            blocksSpawnPoint.localPosition =
                new Vector3(-SpawnPointOffset(width), SpawnPointOffset(height), 0);
        
            _blocks = new GameBlockUI[height, width];

            for (uint row = 0; row < height; row++)
                for (uint col = 0; col < width; col++)
                    _blocks[row, col] = InstantiateGameBlockUI(gameBlocks[row, col], row, col, interactionAction);
        }

        /// <summary>
        /// Method that updates the block grid, removing old blocks and instantiating new ones in their place.
        /// </summary>
        /// <param name="coords">Coordinates of blocks to be updated</param>
        /// <param name="rootCoord">Coordinate of the block from which the interaction started</param>
        /// <param name="gameBlocks">Game blocks grid</param>
        /// <param name="interactionAction">Method that handles the interaction caused by mashing the GameBlockUI</param>
        /// <returns>An IEnumerator to manage the board update </returns>
        public IEnumerator UpdateBoard(List<(uint, uint)> coords, (uint, uint) rootCoord, GameBlock[,] gameBlocks, GameLogic.InteractionAction interactionAction)
        {
            // Plays the sound of the GameBlockUI with which the player interacted
            _audioSource.clip = _blocks[rootCoord.Item1, rootCoord.Item2].interactionAudioClip;
            _audioSource.Play();
            
            // Destroys old blocks
            DestroyBlocks(coords, rootCoord);
            
            // Wait until all the old blocks are destroyed
            yield return new WaitWhile(() => coords.Any(coord => _blocks[coord.Item1, coord.Item2]));
            
            // Instantiates new GameBlockUIs
            coords.ForEach(coord =>
                _blocks[coord.Item1, coord.Item2] = InstantiateGameBlockUI(gameBlocks[coord.Item1, coord.Item2],
                    coord.Item1, coord.Item2, interactionAction));
        }

        /// <summary>
        /// Method that handles the game over situation by waiting for a few seconds and then changing the scene.
        /// </summary>
        /// <returns>An IEnumerator to manage the game over</returns>
        public IEnumerator GameOver()
        {
            yield return new WaitForSeconds(TimeToWaitAtGameOver);
            SceneController.LoadGameOverMenu();
        }

        /// <summary>
        /// Method that instantiates a GameBlockUI and sets its various parameters.
        /// </summary>
        /// <param name="gameBlock">Game Block</param>
        /// <param name="row">Row index</param>
        /// <param name="col">Col index</param>
        /// <param name="interactionAction">Method that handles the interaction caused by mashing the GameBlockUI</param>
        /// <returns>The new GameBlockUI instantiated</returns>
        private GameBlockUI InstantiateGameBlockUI(GameBlock gameBlock, uint row, uint col, GameLogic.InteractionAction interactionAction)
        {
            // Instantiates in the spawn point a new GameBlockUI based on the MainType of gameBlock and locates it
            var newBlockUI = Instantiate(GetBlockUIPrefab(gameBlock.MainType), blocksSpawnPoint);
            newBlockUI.transform.localPosition = new Vector3(col * blocksDistance, -row * blocksDistance, 0);

            // Set newBlockUI params
            var color = GetGameBlockSubTypeMaterial(gameBlock.SubType);
            if (color != null)
                newBlockUI.SetImageColor(color.Value);
            newBlockUI.SetOnClickListener(() => interactionAction((row, col)));
        
            return newBlockUI;
        }
    
        /// <summary>
        /// Method that takes as input a list of coordinates and destroys blocks at the corresponding locations.
        /// </summary>
        /// <param name="coords">The list of coordinates</param>
        /// <param name="rootCoord">Coordinate of the block from which the interaction started</param>
        private void DestroyBlocks(List<(uint, uint)> coords, (uint, uint) rootCoord)
        {
            foreach (var coord in coords)
            {
                var block = _blocks[coord.Item1, coord.Item2];
                if (block is SpecialGameBlockUI && coord == rootCoord)
                    StartCoroutine(block.GetComponent<SpecialGameBlockUI>().ActiveBlockDestruction());
                else
                    StartCoroutine(block.BlockDestruction());
            }
        }
        
        /// <summary>
        /// Method that given a main type of block as input, returns the corresponding prefab.
        /// </summary>
        /// <param name="blockMainType">The block main type</param>
        /// <returns>The corresponding GameBlockUI prefab</returns>
        private GameBlockUI GetBlockUIPrefab(GameBlockMainType blockMainType)
        {
            return blockMainType switch
            {
                GameBlockMainType.Bomb => bombBlockPrefab,
                GameBlockMainType.Stripe => stripeBlockPrefab,
                _ => colorBlockPrefab
            };
        }

        /// <summary>
        /// Method that given a sub type as input, returns the corresponding color.
        /// </summary>
        /// <param name="type">The block sub type</param>
        /// <returns>The corresponding Color</returns>
        private Color? GetGameBlockSubTypeMaterial(GameBlockSubType type)
        {
            return type switch
            {
                GameBlockSubType.Type1 => red,
                GameBlockSubType.Type2 => blue,
                GameBlockSubType.Type3 => green,
                GameBlockSubType.Type4 => yellow,
                GameBlockSubType.Type5 => violet,
                GameBlockSubType.Type6 => orange,
                _ => null
            };
        }

        /// <summary>
        /// Method that returns the spawn block offset of the blocks so that the grid is always centered at the game screen.
        /// </summary>
        /// <param name="dimension">Grid size</param>
        /// <returns>The corresponding offset</returns>
        private float SpawnPointOffset(uint dimension) => (dimension - 1) * blocksDistance / 2;

        /// <summary>
        /// Method that takes as input an integer representing seconds, and returns a string in the format "mm:ss".
        /// </summary>
        /// <param name="totalSeconds">The integer representing seconds</param>
        /// <returns>String in the format "mm:ss"</returns>
        private static string ConvertSecondsToTimeText(int totalSeconds) =>
            $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";

    }
}
