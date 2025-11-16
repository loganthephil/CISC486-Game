import { SequenceNode, PrioritySelectorNode, ActionNode, ConditionNode, SelectorNode, ParallelNode } from "src/behaviour_tree/behaviourNodes";
import { BehaviourNode, NodeType } from "src/types/behaviour_tree/behaviourTree";
import { BehaviourContext, IBlackboardSchema, createBlackboard } from "src/types/behaviour_tree/behaviourTreeBlackboard";
import {
  ActionNodeDefinition,
  BehaviourTreeDefinition,
  ConditionNodeDefinition,
  NodeDefinition,
  ParallelNodeDefinition,
  PrioritySelectorNodeDefinition,
  SelectorNodeDefinition,
  SequenceNodeDefinition,
} from "src/types/behaviour_tree/behaviourTreeDefinition";

export interface BehaviourTreeInstance<Schema extends IBlackboardSchema> {
  tree: BehaviourNode<Schema>;
  context: BehaviourContext<Schema>;
}

/**
 * Creates a behaviour tree instance along with its context and blackboard.
 * @param definition The behaviour tree definition.
 * @returns The behaviour tree instance containing the tree and context.
 */
export function createBehaviourTreeInstance<TSchema extends IBlackboardSchema>(definition: BehaviourTreeDefinition<TSchema>): BehaviourTreeInstance<TSchema> {
  const tree = createBehaviourTree(definition); // Build the behaviour tree
  const blackboard = createBlackboard(definition.initialBlackboard);

  const context: BehaviourContext<TSchema> = {
    blackboard,
    deltaTime: 0, // caller updates each tick before running tree.tick(context)
  };

  return { tree, context };
}

function createBehaviourTree<TSchema extends IBlackboardSchema>(definition: BehaviourTreeDefinition<TSchema>): BehaviourNode<TSchema> {
  const createNode = (def: NodeDefinition): BehaviourNode<TSchema> => {
    const nodeFactory: Record<NodeType, (def: NodeDefinition) => BehaviourNode<TSchema>> = {
      sequence: (def) => new SequenceNode<TSchema>(def as SequenceNodeDefinition, createNode),
      selector: (def) => new SelectorNode<TSchema>(def as SelectorNodeDefinition, createNode),
      prioritySelector: (def) => new PrioritySelectorNode<TSchema>(def as PrioritySelectorNodeDefinition, createNode),
      parallel: (def) => new ParallelNode<TSchema>(def as ParallelNodeDefinition, createNode),
      action: (def) => new ActionNode<TSchema>(def as ActionNodeDefinition, definition.actions),
      condition: (def) => new ConditionNode<TSchema>(def as ConditionNodeDefinition, definition.conditions),
    };

    const factory = nodeFactory[def.type];
    if (!factory) throw new Error(`Unknown node type: ${def.type}`);

    return factory(def);
  };

  return createNode(definition.root);
}
