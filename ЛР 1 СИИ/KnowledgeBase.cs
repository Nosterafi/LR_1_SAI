using System.Text;
using System.Text.Json.Serialization;

namespace LR1_SAI;

public class KnowledgeBase
{
    [JsonInclude]
    private readonly int topNodeHash;

    [JsonInclude]
    private readonly Dictionary<int, Node<string>> nodes = [];

    [JsonInclude]
    private int currentNodeHash;

    private Node<string> CurrentNode => nodes[currentNodeHash];

    public string CurrentValue => CurrentNode.Value;

    public NodeType CurrentType => CurrentNode.Type;

    public KnowledgeBase() { }

    [JsonConstructor]
    public KnowledgeBase(
        int topNodeHash,
        Dictionary<int, Node<string>> nodes,
        int currentNodeHash)
    {
        this.topNodeHash = topNodeHash;
        this.nodes = nodes;
        this.currentNodeHash = currentNodeHash;
    }

    public KnowledgeBase(string topValue, NodeType topNodeType = NodeType.Object)
    {
        var topNode = new Node<string>(topValue, topNodeType);
        topNodeHash = topNode.GetHashCode();
        nodes[topNodeHash] = topNode;

        currentNodeHash = topNodeHash;
    }

    public void MoveTop() => currentNodeHash = topNodeHash;

    public void MoveUp()
    {
        if (CurrentNode.ParentNodeHash == null)
            throw new InvalidOperationException("Текущий узел является корневым");

        currentNodeHash = CurrentNode.ParentNodeHash.Value;
    }

    public bool TryMoveDown(bool answer)
    {
        var newNodeHash = answer ? CurrentNode.TrueNodeHash : CurrentNode.FalseNodeHash;
        if (newNodeHash == null) return false;

        currentNodeHash = newNodeHash.Value;
        return true;
    }

    public void AddNode(string value, NodeType type, bool answer)
    {
        var newNode = new Node<string>(value, type);

        if (answer && CurrentNode.TrueNodeHash == null)
            CurrentNode.TrueNodeHash = newNode.GetHashCode();
        else if (!answer && CurrentNode.FalseNodeHash == null)
            CurrentNode.FalseNodeHash = newNode.GetHashCode();
        else throw new InvalidOperationException("Для обоих вариантов ответа уже определены узлы");

        newNode.ParentNodeHash = currentNodeHash;
        nodes[newNode.GetHashCode()] = newNode;
    }

    public bool Contains(Node<string> node) => nodes.ContainsKey(node.GetHashCode());

    public string GetNodeInfo(Node<string> node)
    {
        if (node.Type != NodeType.Object)
            throw new ArgumentException("Данный узел не представляет объект");

        if (nodes.TryGetValue(node.GetHashCode(), out var currentNode))
        {
            var info = new List<string>();
            var builder = new StringBuilder($"{currentNode.Value}:\n");
            var lastNodeHash = currentNode.GetHashCode();
            
            while (currentNode.ParentNodeHash != null)
            {
                currentNode = nodes[currentNode.ParentNodeHash.Value];
                var correctAnswer = lastNodeHash == currentNode.TrueNodeHash ? "да" : "нет";
                
                if (currentNode.Type == NodeType.Object)
                    info.Add($"Это {currentNode.Value}? -> {correctAnswer}");
                else
                    info.Add($"{currentNode.Value}? -> {correctAnswer}");
            }

            info.Reverse();

            foreach (var note in info)
                builder.AppendLine($"    {note}");

            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        throw new InvalidOperationException("Указанного объекта в базе знаний нет");
    }

    public override string ToString()
    {
        var savedCurrentHash = currentNodeHash;

        var topNode = nodes[topNodeHash];
        var typeStr = topNode.Type == NodeType.Object ? "объект" : "вопрос";

        var builder = new StringBuilder($"{topNode.Value}({typeStr})\n");
        var stack = new Stack<Context>();

        if (topNode.FalseNodeHash != null)
            stack.Push(new Context(1, false, nodes[topNode.FalseNodeHash.Value]));

        if (topNode.TrueNodeHash != null)
            stack.Push(new Context(1, true, nodes[topNode.TrueNodeHash.Value]));

        while (stack.Count > 0)
            AddNodeInfo(stack, builder);

        currentNodeHash = savedCurrentHash;
        return builder.ToString();
    }

    private void AddNodeInfo(Stack<Context> currentStack, StringBuilder builder)
    {
        var context = currentStack.Pop();
        var answerStr = context.Answer ? "да" : "нет";
        var curNode = context.CurrentNode;
        var nodeTypeStr = curNode.Type == NodeType.Object ? "объект" : "вопрос";

        builder.Append(new string('\t', context.Level));
        builder.AppendLine($"{answerStr} -> {curNode.Value}({nodeTypeStr})");

        if (curNode.FalseNodeHash != null)
            currentStack.Push(new Context(context.Level + 1, false, nodes[curNode.FalseNodeHash.Value]));

        if (curNode.TrueNodeHash != null)
            currentStack.Push(new Context(context.Level + 1, true, nodes[curNode.TrueNodeHash.Value]));
    }

    private class Context(int level, bool answer, Node<string> currentNode)
    {
        public readonly int Level = level;
        public readonly bool Answer = answer;
        public readonly Node<string> CurrentNode = currentNode;
    }
}