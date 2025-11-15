import { Action, BehaviourNode, Condition, NodeStatus } from "src/types/behaviour_tree/behaviourTree";
import { ParallelPolicies, ParallelPolicy } from "src/types/behaviour_tree/parallelPolicy";
import { NodeConfig } from "src/types/behaviour_tree/nodeConfig";
import { BehaviourContext, IBlackboardSchema } from "src/types/behaviour_tree/behaviourTreeBlackboard";
import {
  ActionNodeDefinition,
  ConditionNodeDefinition,
  NodeDefinition,
  ParallelNodeDefinition,
  PrioritySelectorNodeDefinition,
  SelectorNodeDefinition,
  SequenceNodeDefinition,
} from "src/types/behaviour_tree/behaviourTreeDefinition";

export abstract class BaseNode<TSchema extends IBlackboardSchema> implements BehaviourNode<TSchema> {
  public name: string;
  public config?: NodeConfig;
  protected currentChild: number = 0;
  protected children: BehaviourNode<TSchema>[] = [];

  constructor(definition: NodeDefinition & { children?: NodeDefinition[] }, createNode?: (def: NodeDefinition) => BehaviourNode<TSchema>) {
    this.name = definition.name;
    this.config = definition.config;

    // Create children if a factory function is provided
    if (createNode && definition.children) {
      this.children = definition.children.map((childDef) => createNode(childDef));
    }
  }

  public abstract tick(context: BehaviourContext<TSchema>): NodeStatus;

  public reset(): void {
    this.currentChild = 0;
    this.children.forEach((child) => child.reset());
  }

  protected addChild(child: BehaviourNode<TSchema>): void {
    this.children.push(child);
  }
}

export class SequenceNode<TSchema extends IBlackboardSchema> extends BaseNode<TSchema> {
  private processMultiple: boolean;

  constructor(definition: SequenceNodeDefinition, createNode?: (def: NodeDefinition) => BehaviourNode<TSchema>) {
    super(definition, createNode);
    this.processMultiple = definition.config?.processMultiple ?? false;
  }

  public override tick(context: BehaviourContext<TSchema>): NodeStatus {
    while (this.currentChild < this.children.length) {
      const status = this.children[this.currentChild].tick(context);

      switch (status) {
        case "RUNNING":
          return "RUNNING";
        case "FAILURE":
          this.currentChild = 0;
          return "FAILURE";
        case "SUCCESS":
        default:
          this.currentChild++;
          if (this.processMultiple) break; // Continue processing next child if allowed
          if (this.currentChild === this.children.length) {
            // If all children succeeded
            this.reset();
            return "SUCCESS";
          }
          return "RUNNING"; // More processing needed
      }
    }

    // All children succeeded (only reached if processMultiple is true)
    this.reset();
    return "SUCCESS";
  }
}

//#region Composite Nodes

export class SelectorNode<TSchema extends IBlackboardSchema> extends BaseNode<TSchema> {
  constructor(definition: SelectorNodeDefinition, createNode?: (def: NodeDefinition) => BehaviourNode<TSchema>) {
    super(definition, createNode);
  }

  public override tick(context: BehaviourContext<TSchema>): NodeStatus {
    if (this.currentChild < this.children.length) {
      const status = this.children[this.currentChild].tick(context);

      switch (status) {
        case "RUNNING":
          return "RUNNING";
        case "SUCCESS":
          this.reset();
          return "SUCCESS";
        case "FAILURE":
        default:
          this.currentChild++;
          return "RUNNING";
      }
    }

    this.reset();
    return "FAILURE";
  }
}

export class PrioritySelectorNode<TSchema extends IBlackboardSchema> extends BaseNode<TSchema> {
  private sortOnReset: boolean;
  private sortedChildren: BehaviourNode<TSchema>[] = [];

  constructor(definition: PrioritySelectorNodeDefinition, createNode?: (def: NodeDefinition) => BehaviourNode<TSchema>) {
    super(definition, createNode);
    this.sortOnReset = definition.config?.sortOnReset ?? false;
    this.sortChildren();
  }

  private sortChildren(): void {
    this.sortedChildren = [...this.children].sort((a, b) => {
      // Access priority from each child's config
      const priorityA = a.config?.priority ?? 0;
      const priorityB = b.config?.priority ?? 0;
      return priorityB - priorityA; // Descending order
    });
  }

  public override tick(context: BehaviourContext<TSchema>): NodeStatus {
    for (const child of this.sortedChildren) {
      const status = child.tick(context);
      if (status !== "FAILURE") {
        if (status === "SUCCESS") this.reset();
        return status;
      }
    }

    this.reset();
    return "FAILURE";
  }

  public override reset(): void {
    super.reset();
    if (this.sortOnReset) {
      this.sortChildren();
    }
  }
}

export class ParallelNode<TSchema extends IBlackboardSchema> extends BaseNode<TSchema> {
  private policy: ParallelPolicy;
  private synchronizedChildren: BehaviourNode<TSchema>[] = [];
  private successCount: number = 0;
  private failureCount: number = 0;

  constructor(definition: ParallelNodeDefinition, createNode?: (def: NodeDefinition) => BehaviourNode<TSchema>) {
    super(definition, createNode);
    this.policy = definition.config?.policy ? ParallelPolicies[definition.config.policy === "successOnAll" ? "SuccessOnAll" : "SuccessOnOne"] : ParallelPolicies.SuccessOnAll;
    this.synchronizedChildren = [...this.children];
  }

  public override tick(context: BehaviourContext<TSchema>): NodeStatus {
    const childrenToRemove: BehaviourNode<TSchema>[] = [];

    // Process all synchronized children
    for (const child of this.synchronizedChildren) {
      const status = child.tick(context);

      switch (status) {
        case "RUNNING":
          break;
        case "SUCCESS":
          this.successCount++;
          childrenToRemove.push(child);
          break;
        case "FAILURE":
        default:
          this.failureCount++;
          childrenToRemove.push(child);
          break;
      }
    }

    // Remove completed children
    this.synchronizedChildren = this.synchronizedChildren.filter((child) => !childrenToRemove.includes(child));

    // Check policy
    const policyResult = this.policy.shouldReturn(this.successCount, this.failureCount, this.children.length);

    if (policyResult.shouldReturn) {
      this.reset();
      return policyResult.status;
    }

    return "RUNNING";
  }

  public override reset(): void {
    super.reset();
    this.synchronizedChildren = [...this.children];
    this.successCount = 0;
    this.failureCount = 0;
  }
}

// #endregion
//#region Leaf Nodes

export class ActionNode<TSchema extends IBlackboardSchema> implements BehaviourNode<TSchema> {
  constructor(private definition: ActionNodeDefinition, private actions: Record<string, Action<TSchema>>) {}

  public tick(context: BehaviourContext<TSchema>): NodeStatus {
    const actionName = this.definition.config.action;
    const action = this.actions[actionName];

    if (!action) {
      console.warn(`Action not found: ${actionName}`);
      return "FAILURE";
    }

    return action(context);
  }

  reset(): void {} // Action nodes typically don't have state to reset
}

export class ConditionNode<TSchema extends IBlackboardSchema> implements BehaviourNode<TSchema> {
  constructor(private definition: ConditionNodeDefinition, private conditions: Record<string, Condition<TSchema>>) {}

  tick(context: BehaviourContext<TSchema>): NodeStatus {
    const conditionName = this.definition.config.condition;
    const condition = this.conditions[conditionName];

    if (!conditionName || !condition) {
      console.warn(`Condition not found: ${conditionName}`);
      return "FAILURE";
    }

    return condition(context) ? "SUCCESS" : "FAILURE";
  }

  reset(): void {} // Condition nodes typically don't have state to reset
}

//#endregion
