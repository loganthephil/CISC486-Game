import { AIDroneState } from "@rooms/schema/AIDroneState";
import { TransformState } from "@rooms/schema/TransformState";
import { DetectionSystem } from "@rooms/systems/detectionSystem";
import { Vector2 } from "src/types/commonTypes";
import { DetectionResult } from "src/types/detection";
import { CommonUtils, VectorUtils } from "src/utils";

type NavigationMode = "None" | "Wander" | "Follow" | "Flee";

interface Threat {
  timeToClosestApproach: number;
  distanceAtClosestApproach: number;
  awayFromThreat: Vector2;
  urgency: number;
}

const MIN_RANDOM_DIRECTION_CHANGE_INTERVAL = 2.0;
const MAX_RANDOM_DIRECTION_CHANGE_INTERVAL = 5.0;
const WANDER_BOUNDARY_RADIUS = 50.0;
const CENTER_RADIUS = 10.0;

const MIN_DISTANCE_TO_TARGET = 5.0;
const MAX_DISTANCE_TO_TARGET = 10.0;

const MIN_STEERING_UPDATE_INTERVAL = 0.1; // Minimum time between steering updates
const MAX_STEERING_UPDATE_INTERVAL = 0.2; // Maximum time between steering updates
const OBSTACLE_DETECTION_RADIUS = 10.0; // Radius to scan for obstacles
const MAX_TIME_LOOKAHEAD = 1.5; // How far ahead to predict collisions
const MAX_THREATS_CONSIDERED = 3; // Max number of threats to consider when steering

export class AINavigation {
  private readonly aiDroneState: AIDroneState;
  private readonly detectionSystem: DetectionSystem;

  private navigationMode: NavigationMode = "None";
  private targetState: TransformState | null = null;
  private desiredDirection: Vector2 = { x: 0, y: 0 };
  private movementDirection: Vector2 = { x: 0, y: 0 };

  private withinRange: boolean = false; // Whether the drone is within a certain range of the target and should stop moving

  private nextRandomDirectionChangeTimer: number = 0;
  private nextSteeringUpdateTimer: number = 0;

  private threats: Threat[] = [];

  constructor(aiDroneState: AIDroneState, detectionSystem: DetectionSystem) {
    this.aiDroneState = aiDroneState;
    this.detectionSystem = detectionSystem;
  }

  public update(deltaTime: number) {
    this.desiredDirection = this.getDesiredDirection(deltaTime);
    let movementDirection: Vector2 = { ...this.desiredDirection };

    movementDirection = this.steerMovement(movementDirection, deltaTime);
    // console.log(
    //   `Desired Direction: (${this.desiredDirection.x.toFixed(2)}, ${this.desiredDirection.y.toFixed(2)}) | Movement Direction: (${movementDirection.x.toFixed(2)}, ${movementDirection.y.toFixed(2)})`
    // );

    this.aiDroneState.setRequestedMovement(movementDirection);
    this.movementDirection = movementDirection;
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

  private steerMovement(desiredDirection: Vector2, deltaTime: number): Vector2 {
    // Get current position and velocity
    const selfPosition: Vector2 = { x: this.aiDroneState.posX, y: this.aiDroneState.posY };
    const selfVelocity: Vector2 = { x: this.aiDroneState.velX, y: this.aiDroneState.velY };
    const selfRadius: number = this.aiDroneState.collisionRadius;

    // Gather perceived potential obstacles
    const hits = this.detectionSystem.scanArea(this.aiDroneState.id, selfPosition, OBSTACLE_DETECTION_RADIUS, this.aiDroneState.team);

    this.threats = [];
    for (const hit of hits) {
      // If the state is a projectile on the same team, ignore
      if (hit.objectType === "Projectile" && hit.team === this.aiDroneState.team) continue;

      const hitState = hit.object;

      const hitRadius = hitState.collisionRadius;
      const hitClosestPoint: Vector2 = CommonUtils.closestPointOnCircle({ x: hitState.posX, y: hitState.posY }, hitRadius, selfPosition);
      const hitVelocity: Vector2 = { x: hitState.velX, y: hitState.velY };

      const relativePosition: Vector2 = VectorUtils.subtract(hitClosestPoint, selfPosition);
      const relativeVelocity: Vector2 = VectorUtils.subtract(hitVelocity, selfVelocity);

      // Get how long until closest approach
      const relativeSpeedSq = VectorUtils.sqrMagnitude(relativeVelocity);
      let timeToClosestApproach = relativeSpeedSq > 0 ? -VectorUtils.dot(relativePosition, relativeVelocity) / relativeSpeedSq : 0;
      timeToClosestApproach = CommonUtils.clamp(timeToClosestApproach, 0, MAX_TIME_LOOKAHEAD); // Treat anything beyond max lookahead as being at max lookahead

      // Get distance at closest approach
      const separationAtClosestApproach = VectorUtils.add(relativePosition, VectorUtils.scale(relativeVelocity, timeToClosestApproach));
      const distanceAtClosestApproach = VectorUtils.magnitude(separationAtClosestApproach);

      const combinedRadii = Math.max(0.001, selfRadius + hitRadius); // Prevent division by zero

      // If relative speed is zero (parallel movement or stationary), simply push away if already within combined radii
      if (relativeSpeedSq === 0) {
        const distance = VectorUtils.magnitude(relativePosition);
        if (distance < combinedRadii) {
          const awayFromThreat = VectorUtils.normalize(VectorUtils.scale(relativePosition, -1));
          this.threats.push({
            timeToClosestApproach: 0,
            distanceAtClosestApproach: distance,
            awayFromThreat: awayFromThreat,
            urgency: CommonUtils.clamp01((combinedRadii - distance) / combinedRadii), // 0 = at or beyond safe distance, 1 = collision
          });
          continue;
        }
      }

      // If we will be within the combined radius at closest approach, consider this a threat
      if (distanceAtClosestApproach >= combinedRadii) continue;
      const awayFromThreat = VectorUtils.normalize(VectorUtils.scale(separationAtClosestApproach, -1));

      // Calculate urgency
      const proximityFactor = CommonUtils.clamp01((combinedRadii - distanceAtClosestApproach) / combinedRadii); // 0 = at or beyond safe distance, 1 = collision
      const timeFactor = 1 - timeToClosestApproach / MAX_TIME_LOOKAHEAD; // 0 = at max lookahead, 1 = immediate
      this.threats.push({
        timeToClosestApproach: timeToClosestApproach,
        distanceAtClosestApproach: distanceAtClosestApproach,
        awayFromThreat: awayFromThreat,
        urgency: CommonUtils.clamp01(proximityFactor * timeFactor),
      });
    }

    // Sort threats by urgency
    this.threats.sort((a, b) => {
      const comparison = b.urgency - a.urgency;
      if (comparison !== 0) return comparison;
      return a.timeToClosestApproach - b.timeToClosestApproach; // If equal urgency, prioritize earlier collisions
    });

    let rawAvoidanceVector: Vector2 = { x: 0, y: 0 };
    const numThreatsToConsider = Math.min(this.threats.length, MAX_THREATS_CONSIDERED);
    for (let i = 0; i < numThreatsToConsider; i++) {
      const threat = this.threats[i];
      const weight = CommonUtils.lerp(0.25, 1.0, threat.urgency); // Weight avoidance more heavily for more urgent threats
      rawAvoidanceVector = VectorUtils.add(rawAvoidanceVector, VectorUtils.scale(threat.awayFromThreat, weight));
    }

    // Avoid map boundaries
    // TODO: Implement map boundary avoidance

    const rawAvoidanceSqrMagnitude = VectorUtils.sqrMagnitude(rawAvoidanceVector);
    if (rawAvoidanceSqrMagnitude === 0) return desiredDirection; // No adjustment needed
    rawAvoidanceVector = VectorUtils.normalize(rawAvoidanceVector);

    // TODO: Smooth the avoidance vector

    // Combine desired direction and avoidance
    const avoidanceWeight = this.aiDroneState.calculateAvoidanceWeight();
    const steeredMovement = VectorUtils.normalize(VectorUtils.add(desiredDirection, VectorUtils.scale(rawAvoidanceVector, avoidanceWeight)));
    return steeredMovement;
  }
}
