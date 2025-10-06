namespace DroneStrikers.FSM.Interfaces
{
    public interface ITransition
    {
        /// <summary>
        ///     The state to transition to when the condition is met.
        /// </summary>
        IState To { get; }

        /// <summary>
        ///     The condition that must be met to trigger the transition.
        /// </summary>
        IPredicate Condition { get; }
    }
}