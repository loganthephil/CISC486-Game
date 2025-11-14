import { DroneState } from "@rooms/schema/DroneState";
import { Rigidbody } from "@rooms/systems/rigidbody";
import { Vector2 } from "src/types/commonTypes";
import { Constants, VectorUtils } from "src/utils";

export class MovementController {
  private parentDrone: DroneState;
  private attachedRigidbody: Rigidbody;

  constructor(drone: DroneState, rigidbody: Rigidbody) {
    this.parentDrone = drone;
    this.attachedRigidbody = rigidbody;
  }

  /**
   * Applies the specified movement vector to the attached rigidbody.
   * @param movement The direction to move in (should be normalized).
   * @param deltaTime Time elapsed since last update (in seconds). Defaults to fixed time step.
   */
  public move(movement: Vector2, deltaTime: number = Constants.FIXED_TIME_STEP_S) {
    const moveSpeedStat = this.parentDrone.getStatValue("moveSpeed");

    // Determine target velocity
    const targetVelocity: Vector2 = VectorUtils.isNegligible(movement)
      ? { x: 0, y: 0 } // Target stopping if no movement input
      : VectorUtils.scale(movement, moveSpeedStat); // Target max speed in movement direction

    const currentVelocity: Vector2 = { x: this.parentDrone.velX, y: this.parentDrone.velY };

    if (VectorUtils.equals(targetVelocity, currentVelocity)) {
      return; // Already at target velocity
    }

    // Check if we need to slow down or speed up
    const currentSpeed = VectorUtils.magnitude(currentVelocity);
    const shouldSlowDown = currentSpeed > VectorUtils.magnitude(targetVelocity);

    const moveAccelerationStat = this.parentDrone.getStatValue("moveAcceleration");
    const moveDecelerationStat = this.parentDrone.getStatValue("moveDeceleration");

    // Determine acceleration or deceleration to apply
    // TODO: Add "dynamic" deceleration to speed down much faster if max move speed is exceeded
    const acceleration = shouldSlowDown ? moveDecelerationStat : moveAccelerationStat;

    // Smoothly interpolate current speed towards target movement direction
    const smoothedTargetVelocity = VectorUtils.moveTowards(currentVelocity, targetVelocity, acceleration * deltaTime);

    // Calculate needed acceleration to reach desired velocity
    const neededAcceleration: Vector2 = VectorUtils.divide(VectorUtils.subtract(smoothedTargetVelocity, currentVelocity), deltaTime);

    // Add force to rigidbody
    this.attachedRigidbody.applyForce(neededAcceleration);
  }

  public isOutOfBounds(): boolean {
    const maxCoord = Constants.MAP_MAX_COORDINATE;
    const p = this.parentDrone;
    return p.posX < -maxCoord || p.posX > maxCoord || p.posY < -maxCoord || p.posY > maxCoord;
  }
}
