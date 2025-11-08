using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Core;

namespace DroneStrikers.BehaviourTrees
{
    public class Sequence : Node
    {
        private readonly bool _processMultiple;

        /// <summary>
        ///     Creates a Sequence node that processes its children in the order they were added.
        ///     The Sequence node will process each child sequentially as long as they continue to return Success.
        ///     If any child returns Failure, the Sequence node will immediately stop and return Failure.
        ///     If all children succeed, the Sequence node will return Success.
        /// </summary>
        /// <param name="name"> The name of the sequence node </param>
        /// <param name="processMultiple"> Whether to process multiple children per tick if they return Success </param>
        /// <param name="priority"> The priority of the sequence node </param>
        public Sequence(string name, bool processMultiple = false, int priority = 0) : base(name, priority) => _processMultiple = processMultiple;

        public override Status Process()
        {
            while (_currentChild < Children.Count)
                switch (Children[_currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        _currentChild = 0;
                        return Status.Failure;
                    case Status.Success:
                    default:
                        _currentChild++;
                        if (_processMultiple) break; // Continue processing next child if allowed
                        if (_currentChild == Children.Count) // If all children have succeeded, reset and return Success
                        {
                            Reset();
                            return Status.Success;
                        }

                        return Status.Running; // Otherwise, return Running to indicate more processing is needed
                }

            // All children have succeeded (reached only if _processMultiple is true)
            Reset();
            return Status.Success;
        }
    }

    public class Selector : Node
    {
        /// <summary>
        ///     Creates a Selector node that processes its children in the order they were added.
        ///     The Selector node will process each child sequentially until one returns Success.
        ///     If a child returns Failure, the Selector node will move on to the next child.
        ///     If a child returns Success, the Selector node will immediately return Success.
        ///     If none of the children succeed, the Selector node will return Failure.
        /// </summary>
        /// <param name="name"> The name of the selector node </param>
        /// <param name="priority"> The priority of the selector node </param>
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (_currentChild < Children.Count)
                switch (Children[_currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Failure:
                    default:
                        _currentChild++;
                        return Status.Running;
                }

            Reset();
            return Status.Failure;
        }
    }

    public class PrioritySelector : Selector
    {
        private List<Node> _sortedChildren;
        private List<Node> SortedChildren => _sortedChildren ??= SortChildren();
        private readonly bool _sortOnReset;

        protected virtual List<Node> SortChildren() => Children.OrderByDescending(child => child.Priority).ToList();

        /// <summary>
        ///     Creates a Priority Selector node that processes its children based on their priority.
        ///     Always processes each child in order of priority, from highest to lowest.
        ///     In other words, this selector will always attempt to run higher priority children before lower priority ones,
        ///     allowing higher priority tasks to interrupt lower priority tasks.
        ///     Returns Success immediately if any child succeeds.
        ///     If none of the children succeed, returns Failure.
        /// </summary>
        /// <param name="name"> The name of the priority selector node </param>
        /// <param name="sortOnReset">
        ///     Whether to re-sort children every time the selector is processed.
        ///     Set to true if priorities will change at runtime.
        /// </param>
        /// <param name="priority"> The priority of the priority selector node </param>
        public PrioritySelector(string name, bool sortOnReset = false, int priority = 0) : base(name, priority) => _sortOnReset = sortOnReset;

        public override Status Process()
        {
            foreach (Node child in SortedChildren)
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    case Status.Failure:
                    default:
                        continue;
                }

            Reset();
            return Status.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            if (_sortOnReset) _sortedChildren = null;
        }
    }

    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> SortChildren() => Children.Shuffle().ToList();

        /// <summary>
        ///     Creates a Random Selector node that processes its children in a random order.
        ///     Always processes each child in a random order each time it is run.
        ///     Returns Success immediately if any child succeeds.
        ///     If none of the children succeed, returns Failure.
        /// </summary>
        /// <param name="name"> The name of the random selector node </param>
        /// <param name="priority"> The priority of the random selector node </param>
        public RandomSelector(string name, int priority = 0) : base(name, true, priority) { }
    }

    public class Parallel : Node
    {
        private readonly IParallelPolicy _policy;

        private List<Node> _synchronizedChildren;
        private List<Node> SynchronizedChildren => _synchronizedChildren ??= Children.ToList();

        private readonly HashSet<Node> _childrenToRemove = new();

        private int _successCount;
        private int _failureCount;

        /// <summary>
        ///     Creates a Parallel node that processes all of its children in the order they were added,
        ///     regardless of their individual statuses. The specific behavior of when the Parallel node returns
        ///     Success or Failure is determined by the provided policy.
        /// </summary>
        /// <param name="name"> The name of the parallel node </param>
        /// <param name="policy"> The policy that determines when the parallel node should return Success or Failure </param>
        /// <param name="priority"> The priority of the parallel node </param>
        public Parallel(string name, IParallelPolicy policy, int priority = 0) : base(name, priority) => _policy = policy;

        public override Status Process()
        {
            _childrenToRemove.Clear();

            foreach (Node child in SynchronizedChildren)
                switch (child.Process())
                {
                    case Status.Running:
                        break;
                    case Status.Success:
                        _successCount++;
                        _childrenToRemove.Add(child);
                        break;
                    case Status.Failure:
                    default:
                        _failureCount++;
                        _childrenToRemove.Add(child);
                        break;
                }

            // Remove completed children from the synchronized list
            foreach (Node child in _childrenToRemove) SynchronizedChildren.Remove(child);

            // Check if the policy indicates we should return Success or Failure
            if (_policy.ShouldReturn(_successCount, _failureCount, Children.Count, out Status returnStatus))
            {
                Reset();
                return returnStatus;
            }

            // Otherwise, still running
            return Status.Running;
        }

        public override void Reset()
        {
            base.Reset();
            _synchronizedChildren = null;
            _successCount = 0;
            _failureCount = 0;
        }
    }
}