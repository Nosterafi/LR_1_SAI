using System.Text.Json;
using System.Text.Json.Serialization;

namespace LR1_SAI
{
    public class Node<ValueType>
    {
        [JsonInclude]
        public readonly ValueType Value;

        [JsonInclude]
        public readonly NodeType Type;

        [JsonInclude]
        public int? ParentNodeHash { get; set; }

        [JsonInclude]
        public int? TrueNodeHash { get; set; }

        [JsonInclude]
        public int? FalseNodeHash { get; set; }

        [JsonConstructor]
        public Node(ValueType value,
            NodeType type,
            int? parentNodeHash,
            int? trueNodeHash,
            int? falseNodeHash) : this(value, type)
        {
            ParentNodeHash = parentNodeHash;
            TrueNodeHash = trueNodeHash;
            FalseNodeHash = falseNodeHash;
        }

        public Node(ValueType value, NodeType type)
        {
            Value = value;
            Type = type;
        }

        public override string ToString()
        {
            var typeStr = Type == NodeType.Question ? "вопрос" : "объект";
            return $"{Value} ({typeStr})";
        }

        public override int GetHashCode()
        {
            var valueBytes = JsonSerializer.SerializeToUtf8Bytes(Value);

            unchecked
            {
                var hash = 17;

                foreach (var valueByte in valueBytes)
                    hash = hash * 23 + valueByte;

                hash = hash * 23 + Type.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is Node<ValueType> other)
                return EqualityComparer<ValueType>.Default.Equals(Value, other.Value) && 
                    Type == other.Type;

            return false;
        }
    }

    public enum NodeType
    {
        Question,
        Object
    }
}
