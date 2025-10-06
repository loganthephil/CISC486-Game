namespace DroneStrikers.FSM.Interfaces
{
    public interface IPredicate
    {
        /// <summary>
        ///     Evaluates the predicate.
        /// </summary>
        /// <returns> True if the predicate condition is met. False otherwise. </returns>
        bool Evaluate();
    }
}