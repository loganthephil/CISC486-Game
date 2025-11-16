import { type } from "@colyseus/schema";
import { TransformState } from "@rooms/schema/TransformState";
import { Rigidbody } from "@rooms/systems/rigidbody";
import { Vector2 } from "src/types/commonTypes";
import { IDamageable } from "src/types/interfaces/damageableInterface";
import { ObjectTeam } from "src/types/team";

export type ArenaObjectType = "small" | "medium" | "large";
const ARENA_OBJECT_CONFIG: Record<ArenaObjectType, { health: number; expDrop: number; radius: number }> = {
  small: { health: 5, expDrop: 5, radius: 0.4 },
  medium: { health: 10, expDrop: 10, radius: 0.6 },
  large: { health: 50, expDrop: 50, radius: 0.8 },
};

export class ArenaObjectState extends TransformState implements IDamageable {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("string") arenaObjectType: ArenaObjectType;

  @type("uint8") team: ObjectTeam = 0; // Arena objects are neutral

  @type("number") maxHealth: number = 50; // Might set on drone spawn
  @type("number") health: number = 50; // Might set on drone spawn
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  public readonly rigidbody: Rigidbody = new Rigidbody(this, { mass: 10, drag: 10 });
  private onDestroyAction: () => void;

  constructor(arenaObjectType: ArenaObjectType, position: Vector2, onDestroy?: () => void) {
    const cfg = ARENA_OBJECT_CONFIG[arenaObjectType];
    super("ArenaObject", cfg.radius, position);

    this.arenaObjectType = arenaObjectType;
    this.maxHealth = cfg.health;
    this.health = cfg.health;
    this.onDestroyAction = onDestroy ?? (() => {});
  }

  public override update(deltaTime: number) {
    // Update physics
    this.rigidbody.updatePhysics(deltaTime);
  }

  public onDestroy() {
    this.onDestroyAction();
  }

  public takeDamage(amount: number): boolean {
    this.health = Math.max(0, this.health - amount);
    if (this.health <= 0) {
      this.toDespawn = true;
    }
    return this.toDespawn;
  }

  public getExperienceDrop(): number {
    const cfg = ARENA_OBJECT_CONFIG[this.arenaObjectType];
    return cfg.expDrop;
  }

  public getRigidbody(): Rigidbody {
    return this.rigidbody;
  }
}
