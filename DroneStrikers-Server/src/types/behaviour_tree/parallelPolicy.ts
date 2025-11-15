import { NodeStatus } from "src/types/behaviour_tree/behaviourTree";

export interface ParallelPolicy {
  shouldReturn(
    successCount: number,
    failureCount: number,
    totalChildren: number
  ): {
    shouldReturn: boolean;
    status: NodeStatus;
  };
}

export const ParallelPolicies = {
  SuccessOnAll: {
    shouldReturn(successCount: number, failureCount: number, totalChildren: number) {
      if (successCount === totalChildren) {
        return { shouldReturn: true, status: "SUCCESS" as NodeStatus };
      }
      if (failureCount > 0) {
        return { shouldReturn: true, status: "FAILURE" as NodeStatus };
      }
      return { shouldReturn: false, status: "RUNNING" as NodeStatus };
    },
  },
  SuccessOnOne: {
    shouldReturn(successCount: number, failureCount: number, totalChildren: number) {
      if (successCount > 0) {
        return { shouldReturn: true, status: "SUCCESS" as NodeStatus };
      }
      if (failureCount === totalChildren) {
        return { shouldReturn: true, status: "FAILURE" as NodeStatus };
      }
      return { shouldReturn: false, status: "RUNNING" as NodeStatus };
    },
  },
};
