using System;
using System.Collections.Generic;
using System.Linq;
using DroneStrikers.FSM.Interfaces;

namespace DroneStrikers.FSM
{
    public class FiniteStateMachine
    {
        private StateNode _currentNode;
        private readonly Dictionary<Type, StateNode> _nodes = new(); // Maps state classes to their nodes
        private readonly HashSet<ITransition> _anyTransitions = new();

        // Nested class that represents a state and its out-going transitions
        private class StateNode
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState to, IPredicate condition) => Transitions.Add(new Transition(to, condition));
        }

        /// <summary>
        ///     Returns the type of the current state.
        /// </summary>
        public Type CurrentState => _currentNode?.State.GetType();

        /// <summary>
        ///     Should be called once per update cycle.
        ///     Checks for valid transitions and runs the current state's update logic.
        /// </summary>
        public void Update()
        {
            // Check if any transition conditions are met
            ITransition transition = GetTransition();
            if (transition != null) TransitionState(transition.To);

            _currentNode.State.Update(); // Run the state's update logic
        }

        /// <summary>
        ///     Should be called once per fixed update cycle.
        ///     Runs the current state's fixed update logic.
        /// </summary>
        public void FixedUpdate() => _currentNode.State.FixedUpdate();

        /// <summary>
        ///     Changes the current state to the given state, ignoring any transitions.
        /// </summary>
        /// <param name="state"> The state to change to. </param>
        public void SetState(IState state)
        {
            _currentNode = _nodes[state.GetType()];
            _currentNode.State.OnEnter();
        }

        /// <summary>
        ///     Adds a transition between two states with the given condition.
        /// </summary>
        /// <param name="from"> The state to transition from. </param>
        /// <param name="to"> The state to transition to. </param>
        /// <param name="condition"> The condition that must be met. </param>
        public void AddTransition(IState from, IState to, IPredicate condition) => GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);

        /// <summary>
        ///     Adds a transition from any state to the given state with the specified condition.
        /// </summary>
        /// <param name="to"> The state to transition to. </param>
        /// <param name="condition"> The condition that must be met. </param>
        public void AddAnyTransition(IState to, IPredicate condition) => _anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));

        private void TransitionState(IState state)
        {
            if (_currentNode.State == state) return; // Do nothing if already in the target state

            // Get previous state before transition
            IState previousState = _currentNode.State;

            // Get the next state after transition
            StateNode nextNode = _nodes[state.GetType()];
            IState nextState = nextNode.State;

            previousState.OnExit(); // Exit previous state
            nextState.OnEnter(); // Enter next state
            _currentNode = nextNode; // Update current node
        }

        private ITransition GetTransition()
        {
            // Check any transitions first
            ITransition any = _anyTransitions.FirstOrDefault(t => t.Condition.Evaluate());

            // If no any transitions are valid, check current state's transitions
            return any ?? _currentNode.Transitions.FirstOrDefault(t => t.Condition.Evaluate());
        }

        // Either gets an existing node for a state or adds a new one if it doesn't exist.
        private StateNode GetOrAddNode(IState state)
        {
            StateNode node = _nodes.GetValueOrDefault(state.GetType());

            if (node == null)
            {
                node = new StateNode(state);
                _nodes.Add(state.GetType(), node);
            }

            return node;
        }
    }
}