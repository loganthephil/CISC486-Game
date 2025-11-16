import { Schema, type } from "@colyseus/schema";
import { BehaviorState } from "@rooms/schema/BehaviourState";
import { Collider } from "@rooms/systems/collider";
import { ObjectType, Vector2 } from "src/types/commonTypes";

export abstract class TransformState extends BehaviorState {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("number") posX: number;
  @type("number") posY: number;

  @type("number") velX: number;
  @type("number") velY: number;

  @type("number") collisionRadius: number; // Radius in units for collision
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  public readonly objectType: ObjectType;

  /**
   * Indicates whether this object is marked for despawning/removal from the game.
   */
  public toDespawn: boolean = false;

  constructor(objectType: ObjectType, collisionRadius: number, position?: Vector2, velocity?: Vector2) {
    super();

    this.objectType = objectType;

    this.posX = position?.x ?? 0;
    this.posY = position?.y ?? 0;

    this.velX = velocity?.x ?? 0;
    this.velY = velocity?.y ?? 0;

    this.collisionRadius = collisionRadius;
  }

  // Solid collisions
  public onCollisionEnter?(self: Collider, other: Collider): void;
  public onCollisionStay?(self: Collider, other: Collider): void;
  public onCollisionExit?(self: Collider, other: Collider): void;

  // Trigger overlaps
  public onTriggerEnter?(self: Collider, other: Collider): void;
  public onTriggerStay?(self: Collider, other: Collider): void;
  public onTriggerExit?(self: Collider, other: Collider): void;
}
