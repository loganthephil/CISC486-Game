import { type } from "@colyseus/schema";
import { DroneState } from "@rooms/schema/DroneState";
import { TransformState } from "@rooms/schema/TransformState";
import { Collider } from "@rooms/systems/collider";
import { Vector2 } from "src/types/commonTypes";
import { isDamageable } from "src/types/interfaces/damageableInterface";
import { Team } from "src/types/team";

export class ProjectileState extends TransformState {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("uint8") team: Team; // Team that fired the projectile
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  public firedBy: DroneState; // Reference to the drone that fired the projectile

  public contactDamage: number;

  private pierceRemaining: number;
  private remainingLifeTimeSeconds: number = 2.0; // Projectiles expire after 2 seconds

  constructor(firedBy: DroneState, damage: number, pierce: number, team: Team, position: Vector2, velocity: Vector2) {
    super("Projectile", 0.3, position, velocity);
    this.firedBy = firedBy;
    this.contactDamage = damage;
    this.team = team;
    this.pierceRemaining = pierce;
  }

  public override update(deltaTime: number) {
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
    if (other.team === this.team) return; // Ignore collisions with same team
    const target = other.transform;

    if (isDamageable(target)) {
      // Apply damage to the target
      if (target.takeDamage(this.contactDamage)) {
        // If the target was destroyed, award experience to the firing drone
        const expDrop = target.getExperienceDrop();
        this.firedBy.awardExperience(expDrop);
      }
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
