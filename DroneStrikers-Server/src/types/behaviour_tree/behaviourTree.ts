import { IBlackboardSchema, BehaviourContext } from "src/types/behaviour_tree/behaviourTreeBlackboard";
import { NodeConfig } from "src/types/behaviour_tree/nodeConfig";

export type NodeStatus = "SUCCESS" | "FAILURE" | "RUNNING";

export type NodeType = "sequence" | "selector" | "prioritySelector" | "parallel" | "action" | "condition";

export interface BehaviourNode<TSchema extends IBlackboardSchema> {
  tick(context: BehaviourContext<TSchema>): NodeStatus;
  reset(): void;
  config?: NodeConfig;
}

// Strategies
export interface Action<TSchema extends IBlackboardSchema> {
  (context: BehaviourContext<TSchema>): NodeStatus;
}

export interface Condition<TSchema extends IBlackboardSchema> {
  (context: BehaviourContext<TSchema>): boolean;
}
