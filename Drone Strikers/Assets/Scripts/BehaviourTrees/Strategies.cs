using System;

namespace DroneStrikers.BehaviourTrees
{
    public interface IStrategy
    {
        Node.Status Process();

        void Reset() { } // Default implementation does nothing
    }

    // -- Common strategies:

    public class ActionStrategy : IStrategy
    {
        private readonly Action _action;

        /// <summary>
        ///     Creates a simple strategy that runs a given Action.
        ///     The strategy will always return Success after executing the action.
        /// </summary>
        /// <param name="action"> The Action to execute when processed. </param>
        public ActionStrategy(Action action) => _action = action;

        public Node.Status Process()
        {
            _action();
            return Node.Status.Success;
        }
    }

    public class ConditionStrategy : IStrategy
    {
        private readonly Func<bool> _condition;

        /// <summary>
        ///     Creates a simple strategy that evaluates a given condition Func.
        ///     The strategy will return Success if the condition evaluates to true, and Failure if false.
        /// </summary>
        /// <param name="condition"> The bool Func to evaluate when processed. </param>
        public ConditionStrategy(Func<bool> condition) => _condition = condition;

        public Node.Status Process() => _condition() ? Node.Status.Success : Node.Status.Failure;
    }
}