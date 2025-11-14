import { type } from "@colyseus/schema";
import { TransformState } from "@rooms/schema/TransformState";
import { DroneStats } from "@rooms/systems/droneStats";
import { DroneUpgrader } from "@rooms/systems/droneUpgrader";
import { MovementController } from "@rooms/systems/movementController";
import { Rigidbody } from "@rooms/systems/rigidbody";
import { Vector2 } from "src/types/commonTypes";
import { IDamageable } from "src/types/interfaces/damageableInterface";
import { StatType } from "src/types/stats";
import { DroneTeam } from "src/types/team";
import { CommonUtils, Constants, VectorUtils } from "src/utils";

export class DroneState extends TransformState implements IDamageable {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("string") name: string;
  @type("uint8") team: DroneTeam;

  @type("number") experience: number = 0;
  @type("uint8") level: number = 1;
  @type("uint8") upgradePoints: number = 0;
  @type("number") progressToNextLevel: number = 0;

  @type("string") lastTurretUpgradeId: string = "";
  @type("string") lastBodyUpgradeId: string = "";
  @type("string") lastMovementUpgradeId: string = "";

  @type("number") maxHealth: number;
  @type("number") health: number;

  @type("number") lowerRotation: number = 0; // Movement rotation in radians (consider handling only on client side)
  @type("number") upperRotation: number = 0; // Body rotation in radians
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  public nextShotAvailableTime: number = 0; // Time in seconds when the drone can next shoot

  public readonly rigidbody: Rigidbody = new Rigidbody(this, { mass: 1, drag: 0 });
  public readonly movementController: MovementController = new MovementController(this, this.rigidbody);
  public readonly droneStats: DroneStats = new DroneStats();
  public readonly droneUpgrader: DroneUpgrader = new DroneUpgrader(this, this.droneStats);

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
    this.maxHealth = this.droneStats.getValue("maxHealth");
    this.health = this.maxHealth;
  }

  public override update(deltaTime: number) {
    // Apply movement based on requested movement vector and speed stat
    this.movementController.move(this.requestedMovement);
    this.requestedMovement = { x: 0, y: 0 }; // Reset requested movement

    // Update aim rotation
    this.updateAimRotation(deltaTime);

    // Update physics
    this.rigidbody.updatePhysics(deltaTime);
  }

  /**
   * Retrieves the stats of the drone.
   */
  public getStatValue(stat: StatType): number {
    return this.droneStats.getValue(stat);
  }

  /**
   * Retrieves the Rigidbody associated with this drone.
   */
  public getRigidbody(): Rigidbody {
    return this.rigidbody;
  }

  /**
   * Applies damage to the drone.
   * @param amount The amount of damage to apply.
   * @returns Whether the drone was destroyed.
   */
  public takeDamage(amount: number): boolean {
    this.health = Math.max(0, this.health - amount);
    if (this.health <= 0) {
      this.toDespawn = true;
    }
    return this.toDespawn;
  }

  /**
   * Calculates and returns the amount of experience to drop upon destruction.
   * @returns The amount of experience to drop.
   */
  public getExperienceDrop(): number {
    // Drops 50% of current experience on destruction
    return this.experience * 0.5;
  }

  /**
   * Awards experience to the drone.
   * @param amount The amount of experience to award.
   */
  public awardExperience(amount: number) {
    this.droneUpgrader.awardExperience(amount);
  }

  /**
   * Sets the requested movement vector for this drone.
   * @param movement The movement vector requested by the controlling force.
   */
  public setRequestedMovement(movement: Vector2) {
    this.requestedMovement = VectorUtils.normalize(movement);
  }

  /**
   * Sets the requested aim direction for this drone.
   * @param direction The direction vector to aim towards.
   */
  public setRequestedAim(direction: Vector2) {
    const normalizedDir = VectorUtils.normalize(direction);
    this.requestedAim = Math.atan2(normalizedDir.x, normalizedDir.y); // Flip x and y for correct angle
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

  private updateAimRotation(deltaTime: number = Constants.FIXED_TIME_STEP_S) {
    // Smoothly interpolate upper rotation towards requested aim
    this.upperRotation = CommonUtils.radianSmoothStep(this.upperRotation, this.requestedAim, this.getStatValue("aimSpeed") * deltaTime);
  }
}
