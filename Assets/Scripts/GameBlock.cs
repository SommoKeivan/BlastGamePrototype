using Random = System.Random;

/// <summary>
///  Class representing a block in the game. The block has a Main type and a Sub type
/// </summary>
public class GameBlock
{
    public GameBlockMainType MainType { get; }
    public GameBlockSubType SubType { get; }

    public GameBlock(GameBlockMainType gameBlockMainType)
    {
        MainType = gameBlockMainType;
        SubType = GameBlockSubType.None;
    }
    
    public GameBlock(GameBlockMainType gameBlockMainType, GameBlockSubType subType) : this(gameBlockMainType)
    {
        SubType = subType;
    }
}

public enum GameBlockMainType
{
    Basic, Bomb, Stripe
}

public enum GameBlockSubType
{
    None, Type1, Type2, Type3, Type4, Type5, Type6
}

/// <summary>
/// Static class that extends the GameBlockSubType enum, providing it with methods
/// </summary>
public static class GameBlockSubTypeExtensions
{
    /// <summary>
    /// Static method that returns a random subtype (excluding the None type)
    /// </summary>
    /// <returns>Game block sub type</returns>
    public static GameBlockSubType GetRandomType()
    {
        // Generate a random int between 1 and 6
        var random = new Random();
        var randomNumber = random.Next(1, 7);
        
        return randomNumber switch
        {
            1 => GameBlockSubType.Type1,
            2 => GameBlockSubType.Type2,
            3 => GameBlockSubType.Type3,
            4 => GameBlockSubType.Type4,
            5 => GameBlockSubType.Type5,
            6 => GameBlockSubType.Type6,
            _ => GameBlockSubType.None  // Theoretically, None is never returned
        };
    }
}