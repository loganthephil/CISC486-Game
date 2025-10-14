namespace DroneStrikers.Core.Types
{
    /// <summary>
    ///     Context describing the destruction of an object and passed to IDestructionContextReceiver.
    /// </summary>
    public struct ObjectDestructionContext
    {
        /// <summary>
        ///     The amount of experience to award to the destroyer.
        /// </summary>
        public float ExperienceToAward { get; }

        public ObjectDestructionContext(float experienceToAward) => ExperienceToAward = experienceToAward;
    }
}