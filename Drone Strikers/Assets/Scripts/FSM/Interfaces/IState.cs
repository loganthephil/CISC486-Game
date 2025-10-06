namespace DroneStrikers.FSM.Interfaces
{
    public interface IState
    {
        /// <summary>
        ///     Called upon entering the state.
        /// </summary>
        void OnEnter();

        /// <summary>
        ///     Called upon exiting the state.
        /// </summary>
        void OnExit();

        /// <summary>
        ///     Called every update while the state is active.
        /// </summary>
        void Update();

        /// <summary>
        ///     Called every fixed update frame while the state is active.
        /// </summary>
        void FixedUpdate();
    }
}