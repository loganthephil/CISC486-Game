import { entity } from "@colyseus/schema";
import { DroneState } from "@rooms/schema/DroneState";
import { GameState } from "@rooms/schema/GameState";
import { TransformState } from "@rooms/schema/TransformState";
import { AIDroneBrain } from "@rooms/systems/aiDroneBrain";
import { AINavigation } from "@rooms/systems/aiNavigation";
import { DetectionSystem } from "@rooms/systems/detectionSystem";
import { Vector2 } from "src/types/commonTypes";
import { UpgradeType } from "src/types/droneUpgrade";
import { DroneTeam } from "src/types/team";
import { CommonUtils, VectorUtils } from "src/utils";

const MIN_UPGRADE_SELECTION_DELAY = 2.0; // seconds
const MAX_UPGRADE_SELECTION_DELAY_NEW = 15.0; // seconds
const MAX_UPGRADE_SELECTION_DELAY_REPEATED = 8.0; // seconds

// Artificial input latency for AI, in seconds
const AI_INPUT_LATENCY_SECONDS = 0.12; // ~120 ms; tweak as desired
const AI_INPUT_LATENCY_JITTER = 0.01; // +/- 10 ms random jitter

interface AIDroneInputCommand {
  applyAtTime: number; // absolute game time (seconds) when this should be applied
  movement?: Vector2; // requested movement direction
  aim?: Vector2; // requested aim direction
}

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

  // Aiming state
  // TODO: Prevent aiming at target that is too far away (currently makes AI seem omniscient)
  private aimTarget: TransformState | null = null;

  // Upgrade state
  private nextUpgradeSelectTimer: number = 0;
  private exhaustedAllUpgrades: boolean = false;

  // Artificial latency state
  private pendingCommands: AIDroneInputCommand[] = [];

  constructor(id: string, name: string, team: DroneTeam, position: Vector2, gameState: GameState, detectionSystem: DetectionSystem) {
    super(id, name, team, position);
    this.gameState = gameState;
    this.detectionSystem = detectionSystem;

    // Initialize random traits
    this.skill = CommonUtils.randomRangeBiased(0, 1, 1.5); // Bias towards lower skill
    this.aggression = CommonUtils.clamp01(this.skill + (Math.random() - 0.5));

    this.aiNavigation = new AINavigation(this, this.detectionSystem);
    this.aiDroneBrain = new AIDroneBrain(this, this.aiNavigation);
  }

  public override update(deltaTime: number): void {
    super.update(deltaTime);

    // Apply any pending input commands that are due
    this.applyPendingCommands();

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
      this.enqueueAim(trackedAimDirection);
      this.gameState.requestDroneShoot(this.id); // Request shooting when aiming at a target
    }

    // Select upgrades if available
    this.selectUpgradeTick(deltaTime);
  }

  public setAimTarget(target: TransformState | null) {
    this.aimTarget = target;
  }

  public calculateFleeHealthThreshold(): number {
    return CommonUtils.lerp(0, 0.25, this.skill) + CommonUtils.lerp(0.25, 0, this.aggression);
  }

  public calculateFleeLevelDifferenceThreshold(): number {
    return CommonUtils.lerp(0.05, 0.25, this.aggression);
  }

  public calculateGiveUpDistanceMultiplier(): number {
    return CommonUtils.lerp(1, 2, this.aggression);
  }

  public calculateAvoidanceWeight(): number {
    return CommonUtils.lerp(0.7, 1.5, this.skill);
  }

  //#region Latency Simulation
  public enqueueMovement(direction: Vector2): void {
    const applyAt = this.getNextInputApplyTime();
    this.pendingCommands.push({ applyAtTime: applyAt, movement: direction });
    this.pendingCommands.sort((a, b) => a.applyAtTime - b.applyAtTime); // Keep commands sorted by time just in case
  }

  public enqueueAim(direction: Vector2): void {
    const applyAt = this.getNextInputApplyTime();
    this.pendingCommands.push({ applyAtTime: applyAt, aim: direction });
    this.pendingCommands.sort((a, b) => a.applyAtTime - b.applyAtTime); // Keep commands sorted by time just in case
  }

  private applyPendingCommands(): void {
    if (this.pendingCommands.length === 0) return; // No pending commands
    const now = this.gameState.gameTimeSeconds;

    // Extract all commands that are due to be applied
    const due: AIDroneInputCommand[] = [];
    for (let i = 0; i < this.pendingCommands.length; i++) {
      if (this.pendingCommands[i].applyAtTime > now) break; // Commands are sorted by applyAtTime
      due.push(this.pendingCommands[i]);
    }
    if (due.length === 0) return; // No commands due yet

    // Remove due commands from pending list
    this.pendingCommands.splice(0, due.length);

    // Merge due commands into a single command to apply where more recent commands override earlier ones
    const commandToApply: AIDroneInputCommand = due[0];
    for (let i = 1; i < due.length; i++) {
      const cmd = due[i];
      if (cmd.movement) commandToApply.movement = cmd.movement;
      if (cmd.aim) commandToApply.aim = cmd.aim;
    }

    // Apply movement and aim from the command
    this.setRequestedMovement(commandToApply.movement ?? { x: 0, y: 0 }); // Default to no movement if none specified
    if (commandToApply.aim) this.setRequestedAim(commandToApply.aim);
  }

  private updateDetection(): void {
    const position: Vector2 = { x: this.posX, y: this.posY };
    const detectionRadius = 15; // Configurable

    // Update AI brain blackboard
    const priorityTargets = this.detectionSystem.findPriorityTarget(this.id, position, detectionRadius, this.team, { skill: this.skill, aggression: this.aggression });
    this.aiDroneBrain.updateDetectionState(priorityTargets);
  }

  private getNextInputApplyTime(): number {
    const jitter = CommonUtils.randomRange(-AI_INPUT_LATENCY_JITTER, AI_INPUT_LATENCY_JITTER);
    return this.gameState.gameTimeSeconds + Math.max(0, AI_INPUT_LATENCY_SECONDS + jitter);
  }
  //#endregion

  //#region Other
  private calculateTrackedAimDirection(): Vector2 | null {
    if (!this.aimTarget) return null;
    // For now just aim directly at target position
    const direction: Vector2 = {
      x: this.aimTarget.posX - this.posX,
      y: this.aimTarget.posY - this.posY,
    };
    return VectorUtils.normalize(direction);
  }

  private selectUpgradeTick(deltaTime: number): void {
    if (this.exhaustedAllUpgrades) return; // No more upgrades to select

    this.nextUpgradeSelectTimer -= deltaTime;
    if (this.nextUpgradeSelectTimer > 0 || this.upgradePoints <= 0) return; // Not time to select upgrade yet or no upgrade points available

    // Select a random available upgrade tree
    const availableTrees: UpgradeType[] = this.droneUpgrader.getAvailableTrees();
    if (availableTrees.length === 0) {
      // No available upgrade trees means no more upgrades to select
      this.exhaustedAllUpgrades = true;
      return;
    }
    const selectedTree = CommonUtils.randomChoice(availableTrees);

    // Select a random available upgrade from the selected tree
    const availableUpgrades = this.droneUpgrader.getAvailableUpgradesInTree(selectedTree);
    const selectedUpgrade = CommonUtils.randomChoice(availableUpgrades); // Assured to be non-empty since tree is available

    // Apply the selected upgrade
    this.droneUpgrader.tryApplyUpgrade(selectedUpgrade);

    // If there are still upgrade points, lean towards selecting the next upgrade sooner to simulate a player spending all their upgrades after they notice they have them
    this.nextUpgradeSelectTimer = CommonUtils.randomRange(MIN_UPGRADE_SELECTION_DELAY, this.upgradePoints > 1 ? MAX_UPGRADE_SELECTION_DELAY_REPEATED : MAX_UPGRADE_SELECTION_DELAY_NEW);
  }
}
