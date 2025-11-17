import { entity } from "@colyseus/schema";
import { DroneState } from "@rooms/schema/DroneState";
import { GameState } from "@rooms/schema/GameState";
import { TransformState } from "@rooms/schema/TransformState";
import { AIDroneBrain } from "@rooms/systems/aiDroneBrain";
import { AINavigation } from "@rooms/systems/aiNavigation";
import { DetectionSystem } from "@rooms/systems/detectionSystem";
import { Vector2 } from "src/types/commonTypes";
import { DroneTeam } from "src/types/team";
import { CommonUtils, VectorUtils } from "src/utils";

@entity
export class AIDroneState extends DroneState {
  private readonly aiNavigation: AINavigation;
  private readonly aiDroneBrain: AIDroneBrain;

  private readonly gameState: GameState;
  private readonly detectionSystem: DetectionSystem;

  // AI Drone Traits
  private readonly skill: number;
  private readonly aggression: number;

  // Detection state
  private lastDetectionTime: number = 0;
  private detectionInterval: number = 0.3; // seconds between detection scans

  private aimTarget: TransformState | null = null;

  constructor(id: string, name: string, team: DroneTeam, position: Vector2, gameState: GameState, detectionSystem: DetectionSystem) {
    super(id, name, team, position);
    this.gameState = gameState;
    this.detectionSystem = detectionSystem;

    // Initialize random traits
    this.skill = Math.random();
    this.aggression = CommonUtils.clamp01(this.skill + (Math.random() - 0.5));

    this.aiNavigation = new AINavigation(this, this.detectionSystem);
    this.aiDroneBrain = new AIDroneBrain(this, this.aiNavigation);
  }

  public override update(deltaTime: number): void {
    super.update(deltaTime);

    // Update detection at intervals (not every frame for performance)
    this.lastDetectionTime += deltaTime;
    if (this.lastDetectionTime >= this.detectionInterval) {
      this.updateDetection();
      this.lastDetectionTime = 0;
    }

    this.aiDroneBrain.update(deltaTime);

    // Update aim direction
    const trackedAimDirection = this.calculateTrackedAimDirection();
    if (trackedAimDirection) {
      this.setRequestedAim(trackedAimDirection);
      this.gameState.requestDroneShoot(this.id); // Request shooting when aiming at a target
    }
  }

  private updateDetection(): void {
    const position: Vector2 = { x: this.posX, y: this.posY };
    const detectionRadius = 15; // Configurable

    // Update AI brain blackboard
    const priorityTargets = this.detectionSystem.findPriorityTarget(this.id, position, detectionRadius, this.team, { skill: this.skill, aggression: this.aggression });
    this.aiDroneBrain.updateDetectionState(priorityTargets);
  }

  public setAimTarget(target: TransformState | null) {
    this.aimTarget = target;
  }

  private calculateTrackedAimDirection(): Vector2 | null {
    if (!this.aimTarget) return null;
    // For now just aim directly at target position
    const direction: Vector2 = {
      x: this.aimTarget.posX - this.posX,
      y: this.aimTarget.posY - this.posY,
    };
    return VectorUtils.normalize(direction);
  }

  public calculateFleeHealthThreshold(): number {
    return CommonUtils.lerp(0, 0.25, this.skill) + CommonUtils.lerp(0.25, 0, this.aggression);
  }

  public calculateFleeLevelDifferenceThreshold(): number {
    return CommonUtils.lerp(0.05, 0.25, this.aggression);
  }

  public calculateGiveUpDistanceMultiplier(): number {
    return CommonUtils.lerp(1, 2,this.aggression); 
  }

  public calculateAvoidanceWeight(): number {
    return CommonUtils.lerp(0.7, 1.5, this.skill);
  }
}
