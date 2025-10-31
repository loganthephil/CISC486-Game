namespace DroneStrikers.BehaviourTrees
{
    public interface IPolicy
    {
        bool ShouldReturn(Node.Status status);
    }

    public static class Policies
    {
        public static readonly IPolicy RunForever = new RunForeverPolicy();
        public static readonly IPolicy RunUntilSuccess = new RunUntilSuccessPolicy();
        public static readonly IPolicy RunUntilFailure = new RunUntilFailurePolicy();

        private class RunForeverPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => false;
        }

        private class RunUntilSuccessPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Success;
        }

        private class RunUntilFailurePolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Failure;
        }
    }

    public interface IParallelPolicy
    {
        bool ShouldReturn(int successCount, int failureCount, int totalChildren, out Node.Status returnStatus);
    }

    public static class ParallelPolicies
    {
        public static readonly IParallelPolicy SuccessOnAll = new SuccessOnAllPolicy();
        public static readonly IParallelPolicy SuccessOnOne = new SuccessOnOnePolicy();

        private class SuccessOnAllPolicy : IParallelPolicy
        {
            public bool ShouldReturn(int successCount, int failureCount, int totalChildren, out Node.Status returnStatus)
            {
                if (successCount == totalChildren)
                {
                    returnStatus = Node.Status.Success;
                    return true;
                }

                if (failureCount > 0)
                {
                    returnStatus = Node.Status.Failure;
                    return true;
                }

                returnStatus = Node.Status.Running;
                return false;
            }
        }

        private class SuccessOnOnePolicy : IParallelPolicy
        {
            public bool ShouldReturn(int successCount, int failureCount, int totalChildren, out Node.Status returnStatus)
            {
                if (successCount > 0)
                {
                    returnStatus = Node.Status.Success;
                    return true;
                }

                if (failureCount == totalChildren)
                {
                    returnStatus = Node.Status.Failure;
                    return true;
                }

                returnStatus = Node.Status.Running;
                return false;
            }
        }
    }
}