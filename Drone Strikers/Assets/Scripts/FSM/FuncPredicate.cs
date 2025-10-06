using System;
using DroneStrikers.FSM.Interfaces;

namespace DroneStrikers.FSM
{
    public class FuncPredicate : IPredicate
    {
        private readonly Func<bool> _func;

        /// <summary>
        ///     Creates a predicate based on a function delegate.
        /// </summary>
        /// <param name="func"> The delegate that evaluates the predicate. </param>
        public FuncPredicate(Func<bool> func) => _func = func;

        public bool Evaluate() => _func();
    }
}