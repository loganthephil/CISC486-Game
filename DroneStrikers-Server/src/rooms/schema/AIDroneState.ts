import { DroneState } from "@rooms/schema/DroneState";
import { AIDroneBrain } from "@rooms/systems/aiDroneBrain";
import { DetectionSystem } from "@rooms/systems/detectionSystem";
import { Vector2 } from "src/types/commonTypes";
import { DroneTeam } from "src/types/team";
import { CommonUtils } from "src/utils";

export class AIDroneState extends DroneState {
  private aiDroneBrain: AIDroneBrain;
  private detectionSystem: DetectionSystem;

  // AI Drone Traits
  private skill: number;
  private aggression: number;

  constructor(name: string, team: DroneTeam, position: Vector2, detectionSystem: DetectionSystem) {
    super(name, team, position);
    this.detectionSystem = detectionSystem;

    // Initialize random traits
    this.skill = Math.random();
    this.aggression = CommonUtils.clamp01(this.skill + (Math.random() - 0.5));

    this.aiDroneBrain = new AIDroneBrain();
  }

  public override update(deltaTime: number): void {
    super.update(deltaTime);
    this.aiDroneBrain.update(deltaTime);
  }
}
