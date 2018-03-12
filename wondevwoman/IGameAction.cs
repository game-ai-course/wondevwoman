using System;

namespace CG.WondevWoman
{
    public interface IGameAction : IEquatable<IGameAction>
    {
        string Message { get; set; }
        Cancelable ApplyTo(State state);
    }
}
