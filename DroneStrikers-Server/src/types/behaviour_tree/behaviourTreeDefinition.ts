import { SequenceNodeConfig, BaseNodeConfig, PrioritySelectorNodeConfig, ParallelNodeConfig, ActionNodeConfig, ConditionNodeConfig } from "./nodeConfig";
import { IBlackboardSchema } from "./behaviourTreeBlackboard";
import { Action, Condition } from "src/types/behaviour_tree/behaviourTree";

export interface SequenceNodeDefinition {
  type: "sequence";
  name: string;
  children?: NodeDefinition[];
  config?: SequenceNodeConfig;
}

export interface SelectorNodeDefinition {
  type: "selector";
  name: string;
  children?: NodeDefinition[];
  config?: BaseNodeConfig;
}

export interface PrioritySelectorNodeDefinition {
  type: "prioritySelector";
  name: string;
  children?: NodeDefinition[];
  config?: PrioritySelectorNodeConfig;
}

export interface ParallelNodeDefinition {
  type: "parallel";
  name: string;
  children?: NodeDefinition[];
  config?: ParallelNodeConfig;
}

export interface ActionNodeDefinition {
  type: "action";
  name: string;
  config: ActionNodeConfig;
}

export interface ConditionNodeDefinition {
  type: "condition";
  name: string;
  config: ConditionNodeConfig;
}

// Union type for all node definitions
export type NodeDefinition = SequenceNodeDefinition | SelectorNodeDefinition | PrioritySelectorNodeDefinition | ParallelNodeDefinition | ActionNodeDefinition | ConditionNodeDefinition;

export interface BehaviourTreeDefinition<TSchema extends IBlackboardSchema> {
  name: string;
  root: NodeDefinition;
  actions: Record<string, Action<TSchema>>;
  conditions: Record<string, Condition<TSchema>>;
  initialBlackboard?: Partial<TSchema>;
}
