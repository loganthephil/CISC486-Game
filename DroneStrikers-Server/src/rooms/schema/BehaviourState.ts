import { Schema } from "@colyseus/schema";

export class BehaviorState extends Schema {
  /**
   * Update the state by the specified delta time.
   * @param deltaTime Time in seconds since the last update.
   */
  public update?(deltaTime: number): void;
}
