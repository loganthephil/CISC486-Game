import { TransformState } from "@rooms/schema/TransformState";
import { DroneState } from "@rooms/schema/DroneState";
import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { ObjectType, Vector2 } from "src/types/commonTypes";
import { Team } from "src/types/team";
import { GameState } from "@rooms/schema/GameState";
import { ProjectileState } from "@rooms/schema/ProjectileState";
import { DetectionResult, DroneDetectionResult, ArenaObjectDetectionResult, ProjectileDetectionResult, PriorityTargets } from "src/types/detection";

export class DetectionSystem {
  private readonly cellSize: number;
  private grid = new Map<string, Set<string>>(); // cellKey -> Set of entity IDs

  private lastGridUpdateTime: number = 0;

  constructor(private gameState: GameState, cellSize: number = 4) {
    this.cellSize = cellSize;
  }

  /**
   * Scan for objects within radius of a position, filtered by team and type.
   * All options default to true, specify false to exclude.
   */
  public scanArea(
    scannerId: string,
    center: Vector2,
    radius: number,
    scannerTeam: Team,
    options: {
      includeDrones?: boolean;
      includeObjects?: boolean;
      includeProjectiles?: boolean;
      includeAllies?: boolean;
      includeEnemies?: boolean;
      includeNeutral?: boolean;
    } = {}
  ): DetectionResult[] {
    const { includeDrones = true, includeObjects = true, includeProjectiles = true, includeAllies = true, includeEnemies = true, includeNeutral = true } = options;

    this.updateGrid();

    const results: DetectionResult[] = [];
    const checkedIds = new Set<string>();
    const cells = this.getCellsInRadius(center, radius);

    for (const cellKey of cells) {
      const cellEntities = this.grid.get(cellKey);
      if (!cellEntities) continue;

      for (const entityId of cellEntities) {
        if (checkedIds.has(entityId)) continue;
        checkedIds.add(entityId);

        const entity = this.getEntityById(entityId);
        if (!entity) continue;

        // Distance check (narrow phase)
        const distance = this.calculateDistance(center, entity);
        if (distance > radius) continue;

        // Team filtering
        const entityTeam = this.getEntityTeam(entity);
        if (!this.shouldIncludeTeam(entityTeam, scannerTeam, { includeAllies, includeEnemies, includeNeutral })) {
          continue;
        }

        if (DetectionSystem.isDrone(entity)) {
          if (!includeDrones) continue;
          if (entityId === scannerId) continue; // Ignore self

          const healthPercent = DetectionSystem.getHealthPercent(entity);
          const level = DetectionSystem.getLevel(entity);

          const detectionResult: DroneDetectionResult = {
            object: entity,
            distance,
            scanRadius: radius,
            team: entityTeam,
            objectType: "Drone",
            healthPercent,
            level,
            value: DetectionSystem.calculateObjectValue(entity),
          };
          results.push(detectionResult);
          continue;
        } else if (DetectionSystem.isArenaObject(entity)) {
          if (!includeObjects) continue;

          const healthPercent = DetectionSystem.getHealthPercent(entity);

          const detectionResult: ArenaObjectDetectionResult = {
            object: entity,
            distance,
            scanRadius: radius,
            team: entityTeam,
            objectType: "ArenaObject",
            healthPercent,
            value: DetectionSystem.calculateObjectValue(entity),
          };
          results.push(detectionResult);
          continue;
        } else if (DetectionSystem.isProjectile(entity)) {
          if (!includeProjectiles) continue;

          const detectionResult: ProjectileDetectionResult = {
            object: entity,
            distance,
            scanRadius: radius,
            team: entityTeam,
            objectType: "Projectile",
          };
          results.push(detectionResult);
          continue;
        }
      }
    }

    return results.sort((a, b) => a.distance - b.distance);
  }

  /**
   * Find the most important target based on AI priorities
   */
  public findPriorityTarget(scannerId: string, center: Vector2, radius: number, scannerTeam: Team, traits: { aggression: number; skill: number }): PriorityTargets {
    // Scan area for all relevant objects
    const detected = this.scanArea(scannerId, center, radius, scannerTeam, {
      includeAllies: false,
    });

    let bestDrone: DroneState | null = null;
    let bestDroneScore = -Infinity;

    let bestObject: ArenaObjectState | null = null;
    let bestObjectScore = -Infinity;

    for (const result of detected) {
      if (result.objectType === "Drone") {
        const score = this.scoreDroneTarget(result, traits);
        if (score > bestDroneScore) {
          bestDroneScore = score;
          bestDrone = result.object;
        }
      } else if (result.objectType === "ArenaObject") {
        const score = this.scoreArenaObjectTarget(result, traits);
        if (score > bestObjectScore) {
          bestObjectScore = score;
          bestObject = result.object;
        }
      }
    }

    return {
      bestDrone,
      bestArenaObject: bestObject,
      highestLevelDrone: this.findHighestLevelDrone(detected),
    };
  }

  private findHighestLevelDrone(detections: DetectionResult[]): DroneState | null {
    let highestLevel = -1;
    let highestDrone: DroneState | null = null;

    for (const detection of detections) {
      if (detection.objectType !== "Drone" || detection.level <= highestLevel) continue;
      highestLevel = detection.level;
      highestDrone = detection.object;
    }

    return highestDrone;
  }

  private scoreDroneTarget(detection: DroneDetectionResult, traits: { aggression: number; skill: number }): number {
    const healthFactor = 1 - detection.healthPercent; // Prefer lower health
    const distanceFactor = 1 - detection.distance / detection.scanRadius; // Prefer closer
    const levelFactor = detection.level;

    // More aggressive drones care less about danger, more about value
    const dangerWeight = 1 - traits.aggression;
    const valueWeight = 0.5 + traits.aggression;

    // More value for lesser health, closer distance, higher value, and lower level
    return healthFactor * 1.25 + distanceFactor * 2.0 + (detection.value || 0) * valueWeight - levelFactor * dangerWeight;
  }

  private scoreArenaObjectTarget(detection: ArenaObjectDetectionResult, traits: { aggression: number; skill: number }): number {
    const healthFactor = 1 - (detection.healthPercent || 1);
    const distanceFactor = 1 - detection.distance / detection.scanRadius;

    // Less aggressive drones prefer objects more
    const aggressionPenalty = traits.aggression * 0.5;

    // More value for lesser health, closer distance, and higher value
    return healthFactor * 1.25 + distanceFactor * 2.0 + (detection.value || 0) - aggressionPenalty;
  }

  // Lazy grid update (only updates when needed, but only once per tick)
  private updateGrid(): void {
    if (this.gameState.gameTimeSeconds === this.lastGridUpdateTime) return; // Already updated this tick
    this.lastGridUpdateTime = this.gameState.gameTimeSeconds;

    this.grid.clear();

    // Add drones to grid
    this.gameState.drones.forEach((drone, id) => {
      this.addToGrid(id, { x: drone.posX, y: drone.posY });
    });

    // Add arena objects to grid
    this.gameState.arenaObjects.forEach((obj, id) => {
      this.addToGrid(id, { x: obj.posX, y: obj.posY });
    });

    // Add projectiles to grid
    this.gameState.projectiles.forEach((proj, id) => {
      this.addToGrid(id, { x: proj.posX, y: proj.posY });
    });
  }

  private addToGrid(id: string, position: Vector2): void {
    const cellKey = this.positionToCellKey(position);
    let cell = this.grid.get(cellKey);
    if (!cell) {
      cell = new Set();
      this.grid.set(cellKey, cell);
    }
    cell.add(id);
  }

  private getCellsInRadius(center: Vector2, radius: number): string[] {
    const cells: string[] = [];
    const minX = Math.floor((center.x - radius) / this.cellSize);
    const maxX = Math.floor((center.x + radius) / this.cellSize);
    const minY = Math.floor((center.y - radius) / this.cellSize);
    const maxY = Math.floor((center.y + radius) / this.cellSize);

    for (let x = minX; x <= maxX; x++) {
      for (let y = minY; y <= maxY; y++) {
        cells.push(`${x},${y}`);
      }
    }
    return cells;
  }

  private positionToCellKey(position: Vector2): string {
    const cellX = Math.floor(position.x / this.cellSize);
    const cellY = Math.floor(position.y / this.cellSize);
    return `${cellX},${cellY}`;
  }

  private getEntityById(id: string): TransformState | null {
    return this.gameState.drones.get(id) || this.gameState.arenaObjects.get(id) || this.gameState.projectiles.get(id) || null;
  }

  private calculateDistance(a: Vector2, b: TransformState): number {
    const dx = a.x - b.posX;
    const dy = a.y - b.posY;
    return Math.sqrt(dx * dx + dy * dy);
  }

  private getEntityTeam(entity: TransformState): Team {
    if ("team" in entity) return (entity as any).team;
    return 0; // Neutral
  }

  private shouldIncludeTeam(entityTeam: Team, scannerTeam: Team, options: { includeAllies: boolean; includeEnemies: boolean; includeNeutral: boolean }): boolean {
    if (entityTeam === 0) return options.includeNeutral; // Neutral
    if (entityTeam === scannerTeam) return options.includeAllies;
    return options.includeEnemies;
  }

  private static isDrone(entity: TransformState): entity is DroneState {
    return entity.objectType === "Drone";
  }

  private static isArenaObject(entity: TransformState): entity is ArenaObjectState {
    return entity.objectType === "ArenaObject";
  }

  private static isProjectile(entity: TransformState): entity is ProjectileState {
    return entity.objectType === "Projectile";
  }

  private static getHealthPercent(entity: DroneState | ArenaObjectState): number {
    return entity.health / entity.maxHealth;
  }

  private static getLevel(drone: DroneState): number {
    return drone.level;
  }

  private static calculateObjectValue(entity: TransformState): number {
    if (DetectionSystem.isArenaObject(entity) || DetectionSystem.isDrone(entity)) {
      // Base value on object type and remaining health
      const baseValue = entity.getExperienceDrop();
      const healthMultiplier = DetectionSystem.getHealthPercent(entity);
      return baseValue * healthMultiplier;
    }

    return 1;
  }
}
