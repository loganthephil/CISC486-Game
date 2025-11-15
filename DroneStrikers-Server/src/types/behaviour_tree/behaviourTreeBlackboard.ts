export interface IBlackboardSchema {}

export interface BehaviourContext<TSchema extends IBlackboardSchema> {
  blackboard: Map<keyof TSchema, TSchema[keyof TSchema]>;
  deltaTime: number;
}

export interface EmptyBlackboard extends IBlackboardSchema {}
export type EmptyContext = BehaviourContext<EmptyBlackboard>;
