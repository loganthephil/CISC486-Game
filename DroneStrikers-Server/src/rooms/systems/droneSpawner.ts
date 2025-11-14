import { AIDroneState } from "@rooms/schema/AIDroneState";
import { DroneState } from "@rooms/schema/DroneState";
import { DroneType, Vector2 } from "src/types/commonTypes";
import { DroneTeam, Team } from "src/types/team";
import { ENABLE_DEBUG_DRONE_SPAWNS } from "src/utils/constants";

interface TeamSpawnZone {
  team: DroneTeam;
  position: Vector2;
  halfSize: number;
}

const DEFAULT_SPAWN_ZONES: Record<DroneTeam, TeamSpawnZone> = {
  [Team.Red]: { team: Team.Red, position: { x: -65, y: 65 }, halfSize: 8 },
  [Team.Blue]: { team: Team.Blue, position: { x: 65, y: -65 }, halfSize: 8 },
};

const DEBUG_SPAWN_ZONES: Record<DroneTeam, TeamSpawnZone> = {
  [Team.Red]: { team: Team.Red, position: { x: 0, y: 0 }, halfSize: 8 },
  [Team.Blue]: { team: Team.Blue, position: { x: 0, y: 0 }, halfSize: 8 },
};

const SPAWN_ZONES: Record<DroneTeam, TeamSpawnZone> = ENABLE_DEBUG_DRONE_SPAWNS ? DEBUG_SPAWN_ZONES : DEFAULT_SPAWN_ZONES;

export class DroneSpawner {
  public createDrone(name: string, team: DroneTeam, droneType: DroneType): DroneState | null {
    const zone = SPAWN_ZONES[team];
    if (!zone) {
      console.warn(`No spawn zone found for team ${team}`);
      return null;
    }

    // Spawn position
    // TODO: Implement more advanced spawn logic (e.g., avoid collisions with other drones)
    const spawnPosition: Vector2 = getRandomSpawnPosition(zone);

    return droneType === "Player" ? new DroneState(name, team, spawnPosition) : new AIDroneState(name, team, spawnPosition);
  }
}

function getRandomSpawnPosition(zone: TeamSpawnZone): Vector2 {
  const offsetX = (Math.random() * 2 - 1) * zone.halfSize;
  const offsetY = (Math.random() * 2 - 1) * zone.halfSize;
  return {
    x: zone.position.x + offsetX,
    y: zone.position.y + offsetY,
  };
}
