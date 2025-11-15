import { TransformState } from "@rooms/schema/TransformState";
import { DroneState } from "@rooms/schema/DroneState";
import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { Vector2 } from "src/types/commonTypes";
import { Team } from "src/types/team";
import { GameState } from "@rooms/schema/GameState";

export interface DetectionResult {
  object: TransformState;
  distance: number;
  radius: number;
  isDrone: boolean;
  team: Team;
  healthPercent?: number;
  level?: number;
  value?: number; // For prioritization
}

export class DetectionSystem {
  private readonly cellSize: number;
  private grid = new Map<string, Set<string>>(); // cellKey -> Set of entity IDs

  constructor(private gameState: GameState, cellSize: number = 4) {
    this.cellSize = cellSize;
  }

  /**
   * Scan for objects within radius of a position, filtered by team and type
   */
  public scanArea(
    scannerId: string,
    center: Vector2,
    radius: number,
    scannerTeam: Team,
    options: {
      includeDrones?: boolean;
      includeObjects?: boolean;
      includeAllies?: boolean;
      includeEnemies?: boolean;
      includeNeutral?: boolean;
    } = {}
  ): DetectionResult[] {
    const { includeDrones = true, includeObjects = true, includeAllies = false, includeEnemies = true, includeNeutral = true } = options;

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

        let isDrone = false;
        let healthPercent: number | undefined = undefined;
        let level: number | undefined = undefined;

        if (DetectionSystem.isDrone(entity)) {
          if (!includeDrones) continue;
          if (entityId === scannerId) continue; // Ignore self
          isDrone = true;
          healthPercent = this.getHealthPercent(entity);
          level = this.getLevel(entity);
        }

        if (DetectionSystem.isArenaObject(entity)) {
          if (!includeObjects) continue;
          healthPercent = this.getHealthPercent(entity);
        }

        results.push({
          object: entity,
          distance,
          radius,
          isDrone: isDrone,
          team: entityTeam,
          healthPercent: healthPercent,
          level: level,
          value: this.calculateObjectValue(entity),
        });
      }
    }

    return results.sort((a, b) => a.distance - b.distance);
  }

  /**
   * Find the most important target based on AI priorities
   */
  public findPriorityTarget(scannerId: string, center: Vector2, radius: number, scannerTeam: Team, traits: { aggression: number; skill: number }): { drone?: DroneState; object?: ArenaObjectState } {
    const detected = this.scanArea(scannerId, center, radius, scannerTeam, {
      includeDrones: true,
      includeObjects: true,
      includeEnemies: true,
      includeNeutral: true,
    });

    let bestDrone: DroneState | undefined;
    let bestDroneScore = -Infinity;

    let bestObject: ArenaObjectState | undefined;
    let bestObjectScore = -Infinity;

    for (const result of detected) {
      if (result.isDrone) {
        const drone = result.object as DroneState;
        const score = this.scoreDroneTarget(result, traits);
        if (score > bestDroneScore) {
          bestDroneScore = score;
          bestDrone = drone;
        }
      } else {
        const object = result.object as ArenaObjectState;
        const score = this.scoreObjectTarget(result, traits);
        if (score > bestObjectScore) {
          bestObjectScore = score;
          bestObject = object;
        }
      }
    }

    // Prefer drones over objects based on aggression
    if (bestDrone && bestDroneScore > bestObjectScore * (1 + traits.aggression)) {
      return { drone: bestDrone };
    }

    return { object: bestObject };
  }

  private scoreDroneTarget(detection: DetectionResult, traits: { aggression: number; skill: number }): number {
    const healthFactor = 1 - (detection.healthPercent || 1); // Prefer lower health
    const distanceFactor = 1 - detection.distance / detection.radius; // Prefer closer
    const levelFactor = detection.level || 1;

    // More aggressive drones care less about danger, more about value
    const dangerWeight = 1 - traits.aggression;
    const valueWeight = 0.5 + traits.aggression;

    return healthFactor * 1.25 + distanceFactor * 2.0 + (detection.value || 0) * valueWeight - levelFactor * dangerWeight;
  }

  private scoreObjectTarget(detection: DetectionResult, traits: { aggression: number; skill: number }): number {
    const healthFactor = 1 - (detection.healthPercent || 1);
    const distanceFactor = 1 - detection.distance / detection.radius;

    // Less aggressive drones prefer objects more
    const aggressionPenalty = traits.aggression * 0.5;

    return healthFactor * 1.25 + distanceFactor * 2.0 + (detection.value || 0) - aggressionPenalty;
  }

  private updateGrid(): void {
    this.grid.clear();

    // Add drones to grid
    this.gameState.drones.forEach((drone, id) => {
      this.addToGrid(id, { x: drone.posX, y: drone.posY });
    });

    // Add arena objects to grid
    this.gameState.arenaObjects.forEach((obj, id) => {
      this.addToGrid(id, { x: obj.posX, y: obj.posY });
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
    return this.gameState.drones.get(id) || this.gameState.arenaObjects.get(id) || null;
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
    return "experience" in entity; // DroneState has experience field
  }

  private static isArenaObject(entity: TransformState): entity is ArenaObjectState {
    return "objectType" in entity; // ArenaObjectState has objectType field
  }

  private getHealthPercent(entity: DroneState | ArenaObjectState): number {
    return entity.health / entity.maxHealth;
  }

  private getLevel(drone: DroneState): number {
    return drone.level;
  }

  private calculateObjectValue(entity: TransformState): number {
    if (DetectionSystem.isArenaObject(entity) || DetectionSystem.isDrone(entity)) {
      // Base value on object type and remaining health
      const baseValue = entity.getExperienceDrop();
      const healthMultiplier = this.getHealthPercent(entity);
      return baseValue * healthMultiplier;
    }

    return 1;
  }
}
