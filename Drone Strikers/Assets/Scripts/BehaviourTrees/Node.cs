using System.Collections.Generic;

namespace DroneStrikers.BehaviourTrees
{
    // Base Node class for Behaviour Tree system.
    // Useless on its own. Should be extended to create specific node behaviours.
    public abstract class Node
    {
        public enum Status
        {
            Running,
            Failure,
            Success
        }

        public readonly string Name;
        public readonly int Priority;

        public readonly List<Node> Children = new();
        protected int _currentChild;

        public Node(string name = "Node", int priority = 0)
        {
            Name = name;
            Priority = priority;
        }

        /// <summary>
        ///     Adds a child node to this node.
        /// </summary>
        /// <param name="child"> The node to add as a child to this node. </param>
        public void AddChild(Node child) => Children.Add(child);

        public virtual Status Process() => Children[_currentChild].Process();

        public virtual void Reset()
        {
            _currentChild = 0;
            foreach (Node child in Children) child.Reset();
        }
    }
}