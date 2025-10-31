using UnityEngine;
#if UNITY_EDITOR
using System.Text;
#endif

namespace DroneStrikers.BehaviourTrees
{
    public class BehaviourTree : Node
    {
        private readonly IPolicy _policy;

        /// <summary>
        ///     Creates a Behaviour Tree node that processes its children according to the given policy.
        ///     Should be used as the root node of a behaviour tree.
        /// </summary>
        /// <param name="name"> The name of the behaviour tree </param>
        /// <param name="policy">The policy that determines how the tree processes its children. Defaults to RunForever policy.</param>
        public BehaviourTree(string name, IPolicy policy = null) : base(name) => _policy = policy ?? Policies.RunForever;

        public override Status Process()
        {
            Status status = Children[_currentChild].Process();
            if (_policy.ShouldReturn(status)) return status;

            _currentChild = (_currentChild + 1) % Children.Count;
            return Status.Running;
        }

#if UNITY_EDITOR
        public void PrintTree()
        {
            StringBuilder sb = new();
            PrintNode(this, 0, sb);
            Debug.Log(sb.ToString());
        }

        private static void PrintNode(Node node, int indentLevel, StringBuilder sb)
        {
            sb.Append(' ', indentLevel * 2).Append(indentLevel).Append(" ").AppendLine(node.Name);
            foreach (Node child in node.Children) PrintNode(child, indentLevel + 1, sb);
        }
#else
        public static void PrintTree() { }
#endif
    }
}