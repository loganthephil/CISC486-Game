import { Schema, type } from "@colyseus/schema";
import { Collider } from "@rooms/controllers/collider";
import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { DroneState } from "@rooms/schema/DroneState";
import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { Team } from "src/types/team";
import { Constants } from "src/utils";

export class ProjectileState extends TransformState {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("uint8") team: Team; // Team that fired the projectile
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --
  public contactDamage: number;

  private pierceRemaining: number;
  private remainingLifeTimeSeconds: number = 3;

  constructor(damage: number, pierce: number, team: Team, position: Vector2, velocity: Vector2) {
    super(0.3, position, velocity);
    this.contactDamage = damage;
    this.team = team;
    this.pierceRemaining = pierce;
  }

  public update(deltaTime: number) {
    // Update position based on velocity
    this.posX += this.velX * deltaTime;
    this.posY += this.velY * deltaTime;

    // Decrease remaining lifetime
    this.remainingLifeTimeSeconds -= deltaTime;
    if (this.remainingLifeTimeSeconds < 0) {
      this.toDespawn = true;
    }
  }

  public override onTriggerEnter(self: Collider, other: Collider) {
    const target = other.transform;

    if (target instanceof DroneState || target instanceof ArenaObjectState) {
      target.takeDamage(this.contactDamage);
    }

    if (!this.reducePierce()) {
      this.toDespawn = true;
    }
  }

  /**
   * Checks if the projectile has expired.
   * @returns True if the projectile has expired, false otherwise.
   */
  public isExpired(): boolean {
    return this.remainingLifeTimeSeconds <= 0;
  }

  /**
   * Reduces the pierce count of the projectile.
   * @returns Whether the projectile still has pierce remaining.
   */
  public reducePierce(): boolean {
    this.pierceRemaining = Math.max(0, this.pierceRemaining - 1);

    return this.pierceRemaining > 0;
  }
}
