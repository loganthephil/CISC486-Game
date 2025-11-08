using DroneStrikers.FSM.Interfaces;

namespace DroneStrikers.FSM
{
    /// <summary>
    ///     The base state class. Specific state implementations should inherit from this class.
    /// </summary>
    public abstract class BaseState : IState
    {
        protected FiniteStateMachine _machine;

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }
}