import { ArenaObjectState, ArenaObjectType } from "@rooms/schema/ArenaObjectState";
import { GameState } from "@rooms/schema/GameState";
import { Vector2 } from "src/types/commonTypes";
import { CommonUtils, Constants } from "src/utils";

interface ObjectSpawnZone {
  position: Vector2;
  halfSize: number;
  spawnableTypes: ArenaObjectType[];
  spawnRateSeconds: number; // How often to spawn an object in this zone
  maxObjects: number; // Maximum number of objects allowed in this zone
  checkExclusionZones: boolean; // Whether to check exclusion zones when spawning
}

const OBJECT_SPAWN_ZONES_EXCLUSION_ZONES: { position: Vector2; halfSize: number; buffer: number }[] = [
  { position: { x: -65, y: 65 }, halfSize: 15, buffer: 5 }, // Red team spawn
  { position: { x: 65, y: -65 }, halfSize: 15, buffer: 5 }, // Blue team spawn
];

export class ArenaObjectSpawner {
  private state: GameState;

  private objectSpawnZones: ObjectSpawnZone[] = [];
  private nextSpawnTime: number[] = []; // Indexed corresponding to objectSpawnZones
  private objectCounts: number[] = []; // Indexed corresponding to objectSpawnZones

  constructor(state: GameState) {
    this.state = state;
    this.objectSpawnZones = generateSpawnZones(15); // 15x15 grid
    this.nextSpawnTime = new Array(this.objectSpawnZones.length).fill(-Infinity); // Immediately eligible to spawn
    this.objectCounts = new Array(this.objectSpawnZones.length).fill(0);
  }

  /**
   * Spawns arena objects in zones that are due for spawning.
   * @returns An array of spawned arena objects.
   */
  public doSpawnObjectsTick(): ArenaObjectState[] {
    const spawnedObjects: ArenaObjectState[] = [];

    this.objectSpawnZones.forEach((zone, index) => {
      // TODO: Create a method that checks if the zone is ready to spawn. Add condition that doesn't spawn if players are too close.
      if (this.nextSpawnTime[index] > this.state.gameTimeSeconds) return; // Only spawn if enough time has passed for this zone
      if (this.objectCounts[index] >= zone.maxObjects) return; // Already at max objects for this zone

      // Time to spawn a new object
      const spawnPosition: Vector2 = getRandomSpawnPosition(zone);

      // If zone requires exclusion checks, ensure spawn position is valid
      if (zone.checkExclusionZones && isPointInsideExclusion(spawnPosition)) return; // Skip this spawn

      const objectType = zone.spawnableTypes[Math.floor(Math.random() * zone.spawnableTypes.length)];
      const newObject = new ArenaObjectState(objectType, spawnPosition, () => {
        this.objectCounts[index] = Math.max(0, this.objectCounts[index] - 1); // Decrement count on destroy
      });
      spawnedObjects.push(newObject);

      this.objectCounts[index]++; // Increment count for this zone

      // Schedule next spawn time for this zone, with some random variance
      this.nextSpawnTime[index] = this.state.gameTimeSeconds + getVariableNextSpawnTime(zone.spawnRateSeconds);
    });

    return spawnedObjects;
  }
}

function getRandomSpawnPosition(zone: ObjectSpawnZone): Vector2 {
  const offsetX = (Math.random() * 2 - 1) * zone.halfSize;
  const offsetY = (Math.random() * 2 - 1) * zone.halfSize;
  return {
    x: zone.position.x + offsetX,
    y: zone.position.y + offsetY,
  };
}

function isPointInsideExclusion(check: Vector2): boolean {
  return OBJECT_SPAWN_ZONES_EXCLUSION_ZONES.some((ex) => {
    const half = ex.halfSize + ex.buffer;
    return Math.abs(check.x - ex.position.x) <= half && Math.abs(check.y - ex.position.y) <= half;
  });
}

/**
 * Generates spawn zones for arena objects.
 * @param xCount The number of zones along the x-axis.
 * @param yCount The number of zones along the y-axis.
 * @returns An array of object spawn zones.
 */
function generateSpawnZones(axisCount: number): ObjectSpawnZone[] {
  if (axisCount <= 0) return [];

  const max = Constants.MAP_MAX_COORDINATE;
  const halfSize = max / axisCount;
  const cellSize = halfSize * 2;

  const zones: ObjectSpawnZone[] = [];

  for (let ix = 0; ix < axisCount; ix++) {
    for (let iy = 0; iy < axisCount; iy++) {
      const centerX = -max + (ix + 0.5) * cellSize;
      const centerY = -max + (iy + 0.5) * cellSize;
      const zoneCenter: Vector2 = { x: centerX, y: centerY };

      // If zone is fully within any exclusion zone, skip it
      if (isZoneFullyInsideAnyExclusion(zoneCenter, halfSize)) {
        continue;
      }

      const checkExclusion = isZonePartiallyInsideAnyExclusion(zoneCenter, halfSize);

      // Radial distance for interpolation
      const r = Math.hypot(centerX, centerY);
      const rNorm = Math.min(1, r / max); // Normalize to map edge

      // Choose spawnable types based on radius (outer 50% => small+medium, inner 50% => medium+large)
      const spawnableTypes: ArenaObjectType[] = rNorm < 0.5 ? ["medium", "large"] : ["small", "medium"];

      zones.push({
        position: zoneCenter,
        halfSize,
        spawnableTypes,
        spawnRateSeconds: CommonUtils.lerp(10, 15, rNorm), // Faster on edges, slower in center
        maxObjects: Math.round(CommonUtils.lerp(2, 1, rNorm)), // More objects on edges, fewer in center
        checkExclusionZones: checkExclusion,
      });
    }
  }

  return zones;
}

function isZoneFullyInsideAnyExclusion(center: Vector2, halfSize: number): boolean {
  return OBJECT_SPAWN_ZONES_EXCLUSION_ZONES.some((ex) => {
    const exHalf = ex.halfSize + ex.buffer;
    // Zone fully inside if its furthest extents are within exclusion extents
    const insideX = Math.abs(center.x - ex.position.x) + halfSize <= exHalf;
    const insideY = Math.abs(center.y - ex.position.y) + halfSize <= exHalf;
    return insideX && insideY;
  });
}

function isZonePartiallyInsideAnyExclusion(center: Vector2, halfSize: number): boolean {
  return OBJECT_SPAWN_ZONES_EXCLUSION_ZONES.some((ex) => {
    const exHalf = ex.halfSize + ex.buffer;
    // Zone partially inside if its nearest extents are within exclusion extents
    const insideX = Math.abs(center.x - ex.position.x) - halfSize < exHalf;
    const insideY = Math.abs(center.y - ex.position.y) - halfSize < exHalf;
    return insideX && insideY;
  });
}

const SPAWN_ZONE_VARIANCE = 0.2; // 20% variance in spawn rates
function getVariableNextSpawnTime(baseAmount: number): number {
  const variance = baseAmount * SPAWN_ZONE_VARIANCE;
  const randomOffset = (Math.random() * 2 - 1) * variance;
  return baseAmount + randomOffset;
}
