import { type } from "@colyseus/schema";
import { MovementController } from "@rooms/controllers/movementController";
import { Rigidbody } from "@rooms/controllers/rigidbody";
import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { createPercentageDrop, ExperienceDropStrategy } from "src/types/experience";
import { createDroneStats, DroneStats } from "src/types/stats";
import { DroneTeam } from "src/types/team";
import { CommonUtils, Constants, VectorUtils } from "src/utils";

export class DroneState extends TransformState {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("string") name: string;
  @type("uint8") team: DroneTeam;

  @type("number") experience: number = 0;
  @type("uint8") level: number = 1;

  @type("number") maxHealth: number;
  @type("number") health: number;

  @type("number") lowerRotation: number = 0; // Movement rotation in radians (consider handling only on client side)
  @type("number") upperRotation: number = 0; // Body rotation in radians
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  public experienceDropStrategy: ExperienceDropStrategy = createPercentageDrop(0.5); // Drones always drop 50% of their XP on destruction

  public nextShotAvailableTime: number = 0; // Time in seconds when the drone can next shoot

  private stats: DroneStats;

  private rigidbody: Rigidbody;
  private movementController: MovementController;

  /**
   * Normalized movement vector requested by controlling force.
   */
  private requestedMovement: Vector2 = { x: 0, y: 0 };

  private requestedAim: number = 0; // Radian angle to aim towards

  constructor(name: string, team: DroneTeam, position: Vector2) {
    super(0.5, position);
    this.name = name;
    this.team = team;

    // Initialize stats
    this.stats = createDroneStats();
    this.maxHealth = this.stats.maxHealth;
    this.health = this.stats.maxHealth;

    this.rigidbody = new Rigidbody(this, { mass: 1, drag: 0 });
    this.movementController = new MovementController(this, this.rigidbody, this.stats);
  }

  public update(deltaTime: number) {
    // Apply movement based on requested movement vector and speed stat
    this.movementController.move(this.requestedMovement);
    this.requestedMovement = { x: 0, y: 0 }; // Reset requested movement

    // Update aim rotation
    this.updateAimRotation(deltaTime);

    // Update physics
    this.rigidbody.update(deltaTime);
  }

  /**
   * Sets the specified stats on the drone.\
   * Example: `setStats({ "maxHealth": 150, "speed": 10 })`
   * @param PartialStats Partial stats to set on the drone.
   */
  public setStats(PartialStats: Partial<DroneStats>) {
    this.stats = { ...this.stats, ...PartialStats };
    if (PartialStats.maxHealth !== undefined) {
      this.maxHealth = this.stats.maxHealth;
      if (this.health > this.maxHealth) {
        this.health = this.maxHealth;
      }
    }
  }

  /**
   * Retrieves the stats of the drone.
   * @returns The stats of the drone.
   */
  public getStats(): DroneStats {
    return this.stats;
  }

  public getRigidbody(): Rigidbody {
    return this.rigidbody;
  }

  public takeDamage(amount: number) {
    this.health = Math.max(0, this.health - amount);
    if (this.health <= 0) {
      this.toDespawn = true;
    }
  }

  public setRequestedMovement(movement: Vector2) {
    this.requestedMovement = VectorUtils.normalize(movement);
  }

  public setRequestedAim(direction: Vector2) {
    const normalizedDir = VectorUtils.normalize(direction);
    this.requestedAim = Math.atan2(normalizedDir.x, normalizedDir.y); // Flip x and y for correct angle
  }

  private updateAimRotation(deltaTime: number = Constants.FIXED_TIME_STEP_S) {
    // Smoothly interpolate upper rotation towards requested aim
    this.upperRotation = CommonUtils.radianSmoothStep(this.upperRotation, this.requestedAim, this.stats.aimSpeed * deltaTime);
  }

  /**
   * Calculates and returns the current aim vector based on upper rotation.
   * @returns The current aim vector.
   */
  public getAimVector(): Vector2 {
    return {
      x: Math.sin(this.upperRotation),
      y: Math.cos(this.upperRotation),
    };
  }
}
