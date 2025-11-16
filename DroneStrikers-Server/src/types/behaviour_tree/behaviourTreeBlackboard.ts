export interface IBlackboardSchema {}

export interface TypedBlackboard<TSchema extends IBlackboardSchema> {
  get<TKey extends keyof TSchema>(key: TKey): TSchema[TKey];
  set<TKey extends keyof TSchema>(key: TKey, value: TSchema[TKey]): void;
}

export class MapBackedBlackboard<TSchema extends IBlackboardSchema> implements TypedBlackboard<TSchema> {
  private readonly storage = new Map<keyof TSchema, TSchema[keyof TSchema]>();

  constructor(initialState: TSchema) {
    if (!initialState) return;

    for (const [key, value] of Object.entries(initialState) as [keyof TSchema, TSchema[keyof TSchema]][]) {
      this.storage.set(key, value);
    }
  }

  public get<TKey extends keyof TSchema>(key: TKey): TSchema[TKey] {
    return this.storage.get(key) as TSchema[TKey];
  }

  public set<TKey extends keyof TSchema>(key: TKey, value: TSchema[TKey]): void {
    this.storage.set(key, value);
  }
}

export const createBlackboard = <TSchema extends IBlackboardSchema>(initialState: TSchema): TypedBlackboard<TSchema> => new MapBackedBlackboard(initialState);

export interface BehaviourContext<TSchema extends IBlackboardSchema> {
  blackboard: TypedBlackboard<TSchema>;
  deltaTime: number;
}

export interface EmptyBlackboard extends IBlackboardSchema {}
export type EmptyContext = BehaviourContext<EmptyBlackboard>;
