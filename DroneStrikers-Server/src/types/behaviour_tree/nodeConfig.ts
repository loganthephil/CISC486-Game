export interface BaseNodeConfig {
  priority?: number;
}

export interface SequenceNodeConfig extends BaseNodeConfig {
  processMultiple?: boolean;
}

export interface PrioritySelectorNodeConfig extends BaseNodeConfig {
  sortOnReset?: boolean;
}

export interface ParallelNodeConfig extends BaseNodeConfig {
  policy: "successOnAll" | "successOnOne";
}

export interface ActionNodeConfig extends BaseNodeConfig {
  action: string;
}

export interface ConditionNodeConfig extends BaseNodeConfig {
  condition: string;
}

// Union type for all possible configs
export type NodeConfig = SequenceNodeConfig | PrioritySelectorNodeConfig | ParallelNodeConfig | ActionNodeConfig | ConditionNodeConfig | BaseNodeConfig;
