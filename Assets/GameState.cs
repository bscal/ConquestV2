public class GameState
{
    public EGameState state;
    public bool paused;

    public GameState()
    {
        state = EGameState.MENU;
    }
}

public enum EGameState
{
    NONE,
    MENU,
    LOADING,
    GENERATING,
    SIMULATING,
    PLAYING
}