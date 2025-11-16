import { AIDroneState } from "@rooms/schema/AIDroneState";
import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { CommonUtils, VectorUtils } from "src/utils";

type NavigationMode = "None" | "Wander" | "Follow" | "Flee";

const MIN_RANDOM_DIRECTION_CHANGE_INTERVAL = 2.0;
const MAX_RANDOM_DIRECTION_CHANGE_INTERVAL = 5.0;
const WANDER_BOUNDARY_RADIUS = 50.0;
const CENTER_RADIUS = 10.0;

const MIN_DISTANCE_TO_TARGET = 5.0;
const MAX_DISTANCE_TO_TARGET = 10.0;

export class AINavigation {
  private aiDroneState: AIDroneState;

  private navigationMode: NavigationMode = "None";
  private desiredDirection: Vector2 = { x: 0, y: 0 };
  private targetState: TransformState | null = null;

  private withinRange: boolean = false; // Whether the drone is within a certain range of the target and should stop moving

  private nextRandomDirectionChangeTimer: number = 0;

  constructor(aiDroneState: AIDroneState) {
    this.aiDroneState = aiDroneState;
  }

  public update(deltaTime: number) {
    let movementDirection: Vector2 = this.getDesiredDirection(deltaTime);
    this.desiredDirection = { ...movementDirection }; // Store for next frame

    this.aiDroneState.setRequestedMovement(movementDirection);
  }

  public stop() {
    this.navigationMode = "None";
    this.targetState = null;
  }

  public wander() {
    if (this.navigationMode === "Wander") return; // Already wandering, don't reset timer
    this.navigationMode = "Wander";
    this.targetState = null;
    this.nextRandomDirectionChangeTimer = 0;
  }

  public followTarget(targetState: TransformState) {
    if (this.targetState === targetState && this.navigationMode === "Follow") return; // Already following this target
    this.navigationMode = "Follow";
    this.targetState = targetState;
    this.withinRange = false;
  }

  public fleeTarget(targetState: TransformState) {
    if (this.targetState === targetState && this.navigationMode === "Flee") return; // Already fleeing this target
    this.navigationMode = "Flee";
    this.targetState = targetState;
  }

  private getDesiredDirection(deltaTime: number): Vector2 {
    switch (this.navigationMode) {
      case "Wander":
        this.nextRandomDirectionChangeTimer -= deltaTime;

        // If it's time to change direction, get a new random direction
        if (this.nextRandomDirectionChangeTimer <= 0) {
          this.nextRandomDirectionChangeTimer = CommonUtils.randomRange(MIN_RANDOM_DIRECTION_CHANGE_INTERVAL, MAX_RANDOM_DIRECTION_CHANGE_INTERVAL);
          return this.getCenterWeightedRandomDirection();
        }

        // Otherwise, keep current desired direction
        return this.desiredDirection;

      case "Follow":
        if (!this.targetState) return { x: 0, y: 0 };

        const directionToTarget: Vector2 = {
          x: this.targetState.posX - this.aiDroneState.posX,
          y: this.targetState.posY - this.aiDroneState.posY,
        };
        const distanceToTarget = VectorUtils.magnitude(directionToTarget);

        if (distanceToTarget <= MIN_DISTANCE_TO_TARGET) this.withinRange = true;
        else if (distanceToTarget >= MAX_DISTANCE_TO_TARGET) this.withinRange = false;

        // If within range, stop moving; otherwise, move towards target
        return this.withinRange ? { x: 0, y: 0 } : VectorUtils.normalize(directionToTarget);
      case "Flee":
        if (!this.targetState) return { x: 0, y: 0 };

        const directionAwayFromTarget: Vector2 = {
          x: this.aiDroneState.posX - this.targetState.posX,
          y: this.aiDroneState.posY - this.targetState.posY,
        };

        return VectorUtils.normalize(directionAwayFromTarget);
      case "None":
        return { x: 0, y: 0 };
    }
  }

  // Using a maximum distance to consider from the center, return a new normalized direction vector weighted towards the center.
  // The further away from the center, the more likely to return a direction towards the center.
  // If within a small radius of the center, return a completely random direction.
  private getCenterWeightedRandomDirection(): Vector2 {
    const curretPosition: Vector2 = { x: this.aiDroneState.posX, y: this.aiDroneState.posY };
    const center: Vector2 = { x: 0, y: 0 };
    const toCenter: Vector2 = VectorUtils.subtract(center, curretPosition);
    const distanceToCenter = VectorUtils.magnitude(toCenter);

    // Random direction in unit circle
    const randomAngle = Math.random() * Math.PI * 2;
    const randomDirection: Vector2 = {
      x: Math.cos(randomAngle),
      y: Math.sin(randomAngle),
    };

    // If within the center radius, return a random direction
    if (distanceToCenter < CENTER_RADIUS) {
      return randomDirection; // already normalized
    }

    // Calculate weight towards center based on distance
    const weightTowardsCenter = CommonUtils.clamp01((distanceToCenter - CENTER_RADIUS) / (WANDER_BOUNDARY_RADIUS - CENTER_RADIUS));

    // Blend random direction and direction to center based on weight
    const directionToCenter = VectorUtils.normalize(toCenter);
    const weightedRandomDirection = VectorUtils.slerp(randomDirection, directionToCenter, weightTowardsCenter);
    return VectorUtils.normalize(weightedRandomDirection);
  }
}
