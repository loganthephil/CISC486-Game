using System;

namespace DroneStrikers.BehaviourTrees
{
    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }
        
        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }
        
        public override bool Equals(object obj) => obj is BlackboardEntry<T> other && Key == other.Key;
        
        public override int GetHashCode() => Key.GetHashCode();
    }
}