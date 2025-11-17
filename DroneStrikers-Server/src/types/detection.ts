import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { DroneState } from "@rooms/schema/DroneState";
import { ProjectileState } from "@rooms/schema/ProjectileState";
import { ObjectType } from "src/types/commonTypes";
import { Team } from "src/types/team";

export interface BaseDetectionResult<T, K extends ObjectType = ObjectType> {
  object: T;
  distance: number;
  scanRadius: number;
  team: Team;
  objectType: K;
}

export interface DroneDetectionResult extends BaseDetectionResult<DroneState, "Drone"> {
  healthPercent: number;
  level: number; // Negative priority for higher level drones
  value: number; // Higher priority for more valuable drones
}

export interface ArenaObjectDetectionResult extends BaseDetectionResult<ArenaObjectState, "ArenaObject"> {
  healthPercent: number;
  value: number; // Higher priority for more valuable objects
}

export interface ProjectileDetectionResult extends BaseDetectionResult<ProjectileState, "Projectile"> {}

export type DetectionResult = DroneDetectionResult | ArenaObjectDetectionResult | ProjectileDetectionResult;

export interface PriorityTargets {
  bestDrone: DroneState | null;
  bestArenaObject: ArenaObjectState | null;
  highestLevelDrone: DroneState | null;
}
