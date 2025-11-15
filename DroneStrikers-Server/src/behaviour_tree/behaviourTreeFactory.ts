import { SequenceNode, PrioritySelectorNode, ActionNode, ConditionNode, SelectorNode, ParallelNode } from "src/behaviour_tree/behaviourNodes";
import { BehaviourNode, NodeType } from "src/types/behaviour_tree/behaviourTree";
import { BehaviourContext, IBlackboardSchema } from "src/types/behaviour_tree/behaviourTreeBlackboard";
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

/**
 * Creates a behaviour tree from the given definition.
 * Does not create the blackboard or context.
 * @see createBehaviourTreeInstance for creating both tree and context.
 * @param definition The behaviour tree definition.
 * @returns The root node of the created behaviour tree.
 */
export function createBehaviourTree<TSchema extends IBlackboardSchema>(definition: BehaviourTreeDefinition<TSchema>): BehaviourNode<TSchema> {
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

export interface BehaviourTreeInstance<Schema extends IBlackboardSchema> {
  tree: BehaviourNode<Schema>;
  context: BehaviourContext<Schema>;
}

/**
 * Creates a behaviour tree instance along with its context and blackboard.
 * @param definition The behaviour tree definition.
 * @returns The behaviour tree instance containing the tree and context.
 */
export function createBehaviourTreeInstance<TSchema extends IBlackboardSchema>(
  definition: BehaviourTreeDefinition<TSchema>
): BehaviourTreeInstance<TSchema> {
  const tree = createBehaviourTree(definition); // Build the behaviour tree
  const blackboard = new Map<keyof TSchema, TSchema[keyof TSchema]>();

  if (definition.initialBlackboard) {
    for (const [key, value] of Object.entries(definition.initialBlackboard) as [keyof TSchema, TSchema[keyof TSchema]][]) {
      blackboard.set(key, value);
    }
  }

  const context: BehaviourContext<TSchema> = {
    blackboard,
    deltaTime: 0, // caller updates each tick before running tree.tick(context)
  };

  return { tree, context };
}
