namespace Dev.Infrastructure
{
    public interface IGameEndedListener
    {
        void OnGameEnded(GameEndContext context);
    }
}