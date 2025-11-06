namespace LR1_SAI
{
    public interface IHierarchicalBase
    {
        public string[] ObjectsNames { get; }

        public string CurrentValue { get; }

        public NodeType CurrentType { get; }

        public void MoveTop();

        public void MoveUp();

        public bool TryMoveDown(bool answer);

        public void AddNode(string value, NodeType type, bool answer);

        public bool Contains(Node<string> node);

        public string GetNodeInfo(Node<string> node);
    }
}
