using UnityEngine;

namespace DroneStrikers.BehaviourTrees
{
    public class Leaf : Node
    {
        private readonly IStrategy _strategy;

        /// <summary>
        ///     Creates a Leaf node that uses the given strategy.
        ///     A leaf node has no children and will perform logic according to its strategy.
        /// </summary>
        /// <param name="name"> The name of the leaf node </param>
        /// <param name="strategy"> The strategy this leaf node uses when processed </param>
        /// <param name="priority"> The priority of this node </param>
        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority) => _strategy = strategy;

        public override Status Process()
        {
            // Debug.Log("Processing Leaf Node: " + Name); // Debug to track leaf node
            return _strategy.Process();
        }

        public override void Reset() => _strategy.Reset();
    }
}