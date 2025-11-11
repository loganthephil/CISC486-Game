import { Rigidbody } from "@rooms/controllers/rigidbody";
import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { DroneStats } from "src/types/stats";
import { Constants, VectorUtils } from "src/utils";

type MovementStats = Pick<DroneStats, "moveAcceleration" | "moveDeceleration" | "moveSpeed">;

export class MovementController {
  private parentTransform: TransformState;
  private attachedRigidbody: Rigidbody;

  private movementStats: MovementStats;

  constructor(transform: TransformState, rigidbody: Rigidbody, stats: DroneStats) {
    this.parentTransform = transform;
    this.attachedRigidbody = rigidbody;

    this.movementStats = {
      moveAcceleration: stats.moveAcceleration,
      moveDeceleration: stats.moveDeceleration,
      moveSpeed: stats.moveSpeed,
    };
  }

  /**
   * Applies the specified movement vector to the attached rigidbody.
   * @param movement The direction to move in (should be normalized).
   * @param deltaTime Time elapsed since last update (in seconds). Defaults to fixed time step.
   */
  public move(movement: Vector2, deltaTime: number = Constants.FIXED_TIME_STEP_S) {
    // Determine target velocity
    const targetVelocity: Vector2 = VectorUtils.isNegligible(movement)
      ? { x: 0, y: 0 } // Target stopping if no movement input
      : VectorUtils.scale(movement, this.movementStats.moveSpeed); // Target max speed in movement direction

    const currentVelocity: Vector2 = { x: this.parentTransform.velX, y: this.parentTransform.velY };

    if (VectorUtils.equals(targetVelocity, currentVelocity)) {
      return; // Already at target velocity
    }

    // Check if we need to slow down or speed up
    const currentSpeed = VectorUtils.magnitude(currentVelocity);
    const shouldSlowDown = currentSpeed > VectorUtils.magnitude(targetVelocity);

    // Determine acceleration or deceleration to apply
    // TODO: Add "dynamic" deceleration to speed down much faster if max move speed is exceeded
    const acceleration = shouldSlowDown ? this.movementStats.moveDeceleration : this.movementStats.moveAcceleration;

    // Smoothly interpolate current speed towards target movement direction
    const smoothedTargetVelocity = VectorUtils.moveTowards(currentVelocity, targetVelocity, acceleration * deltaTime);

    // Calculate needed acceleration to reach desired velocity
    const neededAcceleration: Vector2 = VectorUtils.divide(VectorUtils.subtract(smoothedTargetVelocity, currentVelocity), deltaTime);

    // Add force to rigidbody
    this.attachedRigidbody.applyForce(neededAcceleration);
  }

  public isOutOfBounds(): boolean {
    const maxCoord = Constants.MAP_MAX_COORDINATE;
    const p = this.parentTransform;
    return p.posX < -maxCoord || p.posX > maxCoord || p.posY < -maxCoord || p.posY > maxCoord;
  }
}
