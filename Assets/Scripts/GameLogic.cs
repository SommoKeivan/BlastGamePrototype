using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graphics.Menus;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

/// <summary>
/// Class representing logic for a "Blast" type game with 2D grid and blocks of various types.
/// </summary>
public class GameLogic : MonoBehaviour
{
    [Header("Grid Info")]
    [SerializeField] private uint height = 8;
    [SerializeField] private uint width = 8;
    
    [Header("Game Info")]
    [SerializeField] private float initialSeconds = 120.0f;
    [SerializeField] private float basicBlockSpawnPercentage= 0.95f;

    [Header("UI")]
    [SerializeField] private GameUI gameUI;
    
    private uint _score;
    private float _seconds;
    private bool _isTimerRunning = true;
    private bool _isInteracting;
    private GameBlock[,] _gameBlocks;

    private const uint BombRadius = 1;
    
    public delegate void InteractionAction((uint, uint) coord);
    
    
    /// <summary>
    /// It initializes the game.
    /// </summary>
    private void Awake()
    {
        InitializeGame();
    }

    /// <summary>
    /// It updates the timer and if it reaches zero, call GameOver()
    /// </summary>
    private void Update()
    {
        if (!_isTimerRunning) return;
        
        _seconds -= Time.deltaTime;
        gameUI.SetTimer((int)_seconds);
        if (_seconds <= 0)
            GameOver();
    }



    /// <summary>
    /// Method that sets the new game, resetting the score and timer, and generating the grid of blocks.
    /// </summary>
    private void InitializeGame()
    {
        _score = 0;
        _seconds = initialSeconds;
        _gameBlocks = new GameBlock[height, width];
        _isInteracting = false;
        _isTimerRunning = true;
        
        void GenerateGameBlocksGrid()
        {
            for (var row = 0; row < height; row++)
                for (var col = 0; col < width; col++)
                    _gameBlocks[row, col] = GenerateGameBlock();
        }

        // It generates the grid until it has at least one valid move
        do
        {
            GenerateGameBlocksGrid();
        } while (!CheckIfThereArePossibleMoves());
        
        // Initializes the graphic appearance of the grid
        gameUI.InitializeGame(height, width, _gameBlocks, Interact);
    }

    /// <summary>
    /// Method that takes as input a coordinate representing a position within the Game Block grid and
    /// handles interaction with it, including checking whether the user can interact with it and
    /// whether the coordinate is valid.
    /// </summary>
    /// <param name="coord">Game Block coordinate with which to interact</param>
    private void Interact((uint, uint) coord)
    {
        if (_isInteracting || !_isTimerRunning ) return;
        
        if (coord.Item1 >= height || coord.Item2 >= width)
        {
            Debug.LogWarning("Invalid row index");
            return;
        }

        StartCoroutine(ManageInteraction(coord));
    }
    
    /// <summary>
    /// Method that handles interaction with a Game Block ensuring that no other interaction is handled in the meantime.
    /// </summary>
    /// <param name="coord">Game Block coordinate with which to interact</param>
    /// <returns>An IEnumerator to manage the interaction</returns>
    private IEnumerator ManageInteraction((uint, uint) coord)
    {
        if (_isInteracting || !_isTimerRunning ) yield break;

        _isInteracting = true;
        
        var interactionCoordinates = new List<(uint, uint)>();
        switch (_gameBlocks[coord.Item1, coord.Item2].MainType)
        {
            case GameBlockMainType.Basic:
                FindConnectedBlocks(coord, interactionCoordinates);
                
                // If the block has no other blocks of the same type connected to it, return.
                if (interactionCoordinates.Count <= 1)
                {
                    _isInteracting = false;
                    yield break;
                }
                break;
            case GameBlockMainType.Bomb:
                FindBombArea(coord, interactionCoordinates);
                break;
            case GameBlockMainType.Stripe:
                FindRowBlocks(coord.Item1, interactionCoordinates);
                break;
            default:
                _isInteracting = false;
                yield break;
        }
        
        _seconds += CalculateTimeIncrement((uint) interactionCoordinates.Count);
        gameUI.SetTimer((int)_seconds);
        _score += CalculateScoreIncrement((uint)interactionCoordinates.Count);
        gameUI.SetScore(_score);
        
        interactionCoordinates.ForEach(tuple => _gameBlocks[tuple.Item1, tuple.Item2] = GenerateGameBlock());
        yield return StartCoroutine(gameUI.UpdateBoard(interactionCoordinates, coord, _gameBlocks, Interact));

        // If there are no more possible moves, it's game over.
        if (!CheckIfThereArePossibleMoves())
            GameOver();
        
        _isInteracting = false;
    }
    
    /// <summary>
    /// Method that handles the game over situation.
    /// Stops the timer, saves the score (possibly updating the high score), and updates the UI.
    /// </summary>
    private void GameOver()
    {
        _isTimerRunning = false;
        
        // Score data update
        SavesManager.SaveLastScore((int)_score);
        if (_score > SavesManager.GetHighScore())
            SavesManager.SaveHighScore((int)_score);
        
        StartCoroutine(gameUI.GameOver());
    }

    /// <summary>
    /// Method that generates a Game Block based on the spawn rates of a Basic type block.
    /// </summary>
    /// <returns>A randomly generated Game Block</returns>
    private GameBlock GenerateGameBlock()
    {
        var randomValue = Random.value; // Random float between 0 and 1
        if (randomValue < basicBlockSpawnPercentage)
            return new GameBlock(GameBlockMainType.Basic, GameBlockSubTypeExtensions.GetRandomType());
        
        return randomValue < basicBlockSpawnPercentage + (1 - basicBlockSpawnPercentage) / 2
            ? new GameBlock(GameBlockMainType.Bomb)
            : new GameBlock(GameBlockMainType.Stripe);
    }

    /// <summary>
    /// DFS-based (Depth-First Search) method to find a connected blocks of the same type as the root Game Block.
    /// The method searches for connected blocks by exploring 4 directions: up, down, left, right.
    /// </summary>
    /// <param name="root">It is the coordinate of the root block</param>
    /// <param name="result">It is the collection where the output is stored</param>
    private void FindConnectedBlocks((uint, uint) root, ICollection<(uint, uint)> result)
    {
        
        var rootMainType = _gameBlocks[root.Item1, root.Item2].MainType;
        var rootSubType = _gameBlocks[root.Item1, root.Item2].SubType;

        // Matrix used to keep track of blocks already visited
        var visited = new bool[height][];
        for (var index = 0; index < height; index++)
            visited[index] = new bool[width];

        // These arrays are used to define possible neighbor directions during depth-first search (up, down, left, right).
        int[] deltaRow = { -1, 1, 0, 0 };
        int[] deltaCol = { 0, 0, -1, 1 };

        void Dfs(uint row, uint col)
        {
            // If the indexes exceed the size of the grid or if it has already visited that location, it returns
            if(row >= height || col >= width || visited[row][col]) return;
            
            visited[row][col] = true;   // Mark the location as visited

            // If the type of the visited block is not the same as the root, it returns
            var gameBlock = _gameBlocks[row, col];
            if (gameBlock == null || gameBlock.MainType != rootMainType || gameBlock.SubType != rootSubType) return;

            result.Add((row, col)); // The block is valid and then adds it to the result

            // Explore neighboring cells
            for (var dir = 0; dir < 4; dir++)
            {
                var newRow = row + deltaRow[dir];
                var newCol = col + deltaCol[dir];
                if(newRow < 0 || newCol < 0) continue;  // In this case, they would not be valid uint
                
                Dfs((uint)newRow, (uint)newCol);    // Recursive call of DFS
            }
        }

        // Start DFS from the input coordinates
        Dfs(root.Item1, root.Item2);
    }
    
    /// <summary>
    /// Method that, given the index of a row, it returns all the positions of that row.
    /// </summary>
    /// <param name="rowIndex">The row index</param>
    /// <param name="result">It is the collection where the output is stored</param>
    private void  FindRowBlocks(uint rowIndex, ICollection<(uint, uint)> result)
    {
        for (uint i = 0; i < width; i++)
            result.Add((rowIndex, i));
    }

    /// <summary>
    /// Method that, given a coordinate, returns all positions found within the bomb's area of action
    /// </summary>
    /// <param name="coord">Bomb coordinate</param>
    /// <param name="result">It is the collection where the output is stored</param>
    private void FindBombArea((uint, uint) coord, ICollection<(uint, uint)> result)
    {
        // Calculation of the initial and final coordinates of the bomb action area
        var initRow = coord.Item1 > BombRadius - 1 ? coord.Item1 - BombRadius : 0;
        var initCol = coord.Item2 > BombRadius - 1 ? coord.Item2 - BombRadius : 0;
        var finalRow = coord.Item1 < width - BombRadius ? coord.Item1 + BombRadius : width - 1;
        var finalCol = coord.Item2 < height - BombRadius ? coord.Item2 + BombRadius : height - 1;

        for (var row = initRow; row <= finalRow; row++)
            for (var col  = initCol; col <= finalCol; col++)
                result.Add((row, col));
    }

    /// <summary>
    /// Method that returns whether there are possible valid moves within the Game Block grid.
    /// </summary>
    /// <returns>True if there are possible moves</returns>
    private bool CheckIfThereArePossibleMoves()
    {
        // Checks the grid for blocks of non-Basic type, and if there are returns true.
        if (_gameBlocks.Cast<GameBlock>().Any(block => block.MainType != GameBlockMainType.Basic)) return true;

        // Checks if there is at least one connected basic block sequence, and if it finds one, it returns true.
        var connectedBlocks = new List<(uint, uint)>();
        for (uint row = 0; row < height; row++)
        {
            for (uint col = 0; col < width; col++)
            {
                if (_gameBlocks[row, col].MainType != GameBlockMainType.Basic) continue;
                
                FindConnectedBlocks((row, col), connectedBlocks);
                if (connectedBlocks.Count > 1) return true;
                
                connectedBlocks.Clear();
            }
        }
        return false;
    }

    /// <summary>
    /// Static method that calculates the time increment
    /// </summary>
    /// <param name="x">Number of game blocks replaced</param>
    /// <returns>Time increment</returns>
    private static uint CalculateTimeIncrement(uint x) =>  x < 2 ? 0 : (uint)(10.0 + Math.Pow((x - 2) / 3.0, 2) * 20.0);
    
    /// <summary>
    /// Static method that calculates the score increment
    /// </summary>
    /// <param name="x">Number of game blocks replaced</param>
    /// <returns>Score increment</returns>
    private static uint CalculateScoreIncrement(uint x) =>  x < 2 ? 0 : (uint)((x-1) * 80 + Math.Pow((x - 2) / 5.0, 2));

}