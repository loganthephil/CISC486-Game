namespace DroneStrikers.BehaviourTrees
{
    public class Inverter : Node
    {
        public Inverter(string name = "Inverter", int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            // Simply return the inverted status of its single child.
            // If the child is Running, return Running.
            switch (Children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                case Status.Success:
                default:
                    return Status.Failure;
            }
        }
    }
}