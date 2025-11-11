import { Rigidbody } from "@rooms/controllers/rigidbody";
import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { Team } from "src/types/team";

export type CollisionPhase = "enter" | "stay" | "exit";

export enum CollisionLayer {
  Drone = 1 << 0,
  Projectile = 1 << 1,
  ArenaObject = 1 << 2,
}

export type ColliderID = string;

export interface ICollisionHandler {
  // Solid collisions
  onCollisionEnter?(self: Collider, other: Collider): void;
  onCollisionStay?(self: Collider, other: Collider): void;
  onCollisionExit?(self: Collider, other: Collider): void;

  // Trigger overlaps
  onTriggerEnter?(self: Collider, other: Collider): void;
  onTriggerStay?(self: Collider, other: Collider): void;
  onTriggerExit?(self: Collider, other: Collider): void;
}

export interface ColliderProperties {
  id: ColliderID;
  transform: TransformState;
  layer: CollisionLayer;
  mask: number; // Layers this collider can collide with
  isTrigger?: boolean; // Skip physical response if true
  team?: Team;
  handler?: ICollisionHandler;
  rigidbody?: Rigidbody;
}

export class Collider {
  public readonly id: ColliderID;
  public readonly transform: TransformState;
  public readonly layer: CollisionLayer;
  public readonly mask: number;
  public readonly isTrigger: boolean;
  public readonly team?: Team;

  public readonly handler?: ICollisionHandler;
  public readonly rigidbody?: Rigidbody;

  constructor(properties: ColliderProperties) {
    this.id = properties.id;
    this.transform = properties.transform;
    this.layer = properties.layer;
    this.mask = properties.mask;
    this.isTrigger = properties.isTrigger ?? false;
    this.handler = properties.handler;
    this.rigidbody = properties.rigidbody;
  }

  public get radius(): number {
    return this.transform.collisionRadius;
  }

  /**
   * Returns a vector representing the position of the collider.
   */
  public get position(): Vector2 {
    return { x: this.transform.posX, y: this.transform.posY };
  }
}
