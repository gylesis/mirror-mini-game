using System.Collections.Generic;

namespace Dev.Infrastructure
{
    public class GameStateDispatcher
    {
        private readonly List<IGameEndedListener> _listeners = new List<IGameEndedListener>();

        public void Add(IGameEndedListener listener)
        {
            _listeners.Add(listener);
        }

        public void EndGame(GameEndContext context)
        {
            foreach (IGameEndedListener listener in _listeners)
            {
                listener.OnGameEnded(context);
            }
        }
    }
}