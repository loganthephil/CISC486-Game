using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.BehaviourTrees
{
    [CreateAssetMenu(fileName = "BlackboardData_", menuName = "Blackboard/Blackboard Data")]
    public class BlackboardDataSO : ScriptableObject
    {
        public List<BlackboardEntryData> Entries = new();

        public void SetValuesOnBlackboard(Blackboard blackboard)
        {
            foreach (BlackboardEntryData entry in Entries) entry.SetValueOnBlackboard(blackboard);
        }
    }

    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver
    {
        public string KeyName;
        public AnyValue.ValueType ValueType;
        public AnyValue Value;

        public void SetValueOnBlackboard(Blackboard blackboard)
        {
            BlackboardKey key = blackboard.GetOrRegisterKey(KeyName);
            _setValueDispatchTable[Value.Type](blackboard, key, Value);
        }

        private static Dictionary<AnyValue.ValueType, Action<Blackboard, BlackboardKey, AnyValue>> _setValueDispatchTable = new()
        {
            { AnyValue.ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue) }, { AnyValue.ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue) },
            { AnyValue.ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue) }, { AnyValue.ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<string>(key, anyValue) },
            { AnyValue.ValueType.Vector3, (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue) }, { AnyValue.ValueType.GameObject, (blackboard, key, anyValue) => blackboard.SetValue<GameObject>(key, anyValue) },
            { AnyValue.ValueType.Transform, (blackboard, key, anyValue) => blackboard.SetValue<Transform>(key, anyValue) }
        };

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => Value.Type = ValueType;
    }

    [Serializable]
    public struct AnyValue
    {
        public enum ValueType
        {
            Bool,
            Int,
            Float,
            String,
            Vector3,
            GameObject,
            Transform
        }

        public ValueType Type;

        // Storage for different types of values
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue;
        public Vector3 Vector3Value;
        public GameObject GameObjectValue;
        public Transform TransformValue;

        // Note: If adding more types, remember to update the dispatch table in BlackboardEntryData and the editor code.
        public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
        public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
        public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
        public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
        public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();
        public static implicit operator GameObject(AnyValue value) => value.ConvertValue<GameObject>();
        public static implicit operator Transform(AnyValue value) => value.ConvertValue<Transform>();

        private T ConvertValue<T>()
        {
            return Type switch
            {
                ValueType.Bool => AsBool<T>(BoolValue),
                ValueType.Int => AsInt<T>(IntValue),
                ValueType.Float => AsFloat<T>(FloatValue),
                ValueType.String => (T)(object)StringValue,
                ValueType.Vector3 => AsVector3<T>(Vector3Value),
                ValueType.GameObject => (T)(object)GameObjectValue,
                ValueType.Transform => (T)(object)TransformValue,
                _ => throw new NotSupportedException($"Unsupported value type: {typeof(T)}")
            };
        }

        // Correctly cast value types to avoid boxing
        private T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
        private T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
        private T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
        private T AsVector3<T>(Vector3 value) => typeof(T) == typeof(Vector3) && value is T correctType ? correctType : default;
    }
}