import { ArraySchema, MapSchema, Schema, type } from "@colyseus/schema";
import { AIDroneState } from "@rooms/schema/AIDroneState";
import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { DroneState } from "@rooms/schema/DroneState";
import { ProjectileState } from "@rooms/schema/ProjectileState";
import { DroneType, Vector2 } from "src/types/commonTypes";
import { ClientMessage, GameMessage } from "src/types/clientMessage";
import { DroneTeam, Team } from "src/types/team";
import { Player } from "src/types/player";
import { Constants } from "src/utils";
import { DroneStats } from "src/types/stats";
import { CollisionSystem } from "@rooms/controllers/collisionSystem";
import { Collider, ColliderID, CollisionLayer } from "@rooms/controllers/collider";

export class GameState extends Schema {
  @type("number") gameTimeSeconds: number = 0;

  @type({ map: DroneState }) drones = new MapSchema<DroneState>();
  @type({ map: ArenaObjectState }) arenaObjects = new MapSchema<ArenaObjectState>();
  @type({ map: ProjectileState }) projectiles = new MapSchema<ProjectileState>();

  // Keep track of players that have joined the session (independent of drones)
  private players: Map<string, Player> = new Map<string, Player>();

  // Collision system
  private collisionSystem: CollisionSystem;
  private collidersById: Map<ColliderID, Collider> = new Map<ColliderID, Collider>();

  constructor() {
    super();
    this.collisionSystem = new CollisionSystem({ cellSize: 3 });
  }

  /**
   * Update the game state by the specified delta time.
   * @param deltaTime Time in seconds since the last update.
   */
  public update(deltaTime: number) {
    this.gameTimeSeconds += deltaTime;

    // Update all drones
    const dronesToRemove: string[] = [];
    this.drones.forEach((drone, id) => {
      drone.update(deltaTime);
      if (drone.toDespawn) dronesToRemove.push(id);
    });

    // Update all projectiles
    const projectilesToRemove: string[] = [];
    this.projectiles.forEach((projectile, id) => {
      projectile.update(deltaTime);
      if (projectile.toDespawn) projectilesToRemove.push(id);
    });

    // Update all projectiles
    const toRemoveObjects: string[] = [];
    this.arenaObjects.forEach((obj, id) => {
      // obj.update(deltaTime); // Arena objects might not need per-frame updates
      if (obj.toDespawn) toRemoveObjects.push(id);
    });

    // Handle collisions after updating all entities
    this.collisionSystem.update();

    // Remove objects marked for despawn
    dronesToRemove.forEach((id) => {
      this.removeDrone(id);
    });
    projectilesToRemove.forEach((id) => {
      this.projectiles.delete(id);
      this.unregisterCollider(id);
    });
    toRemoveObjects.forEach((id) => {
      this.arenaObjects.delete(id);
      this.unregisterCollider(id);
    });
  }

  public onPlayerJoin(id: string, player: Player) {
    this.players.set(id, player);
  }

  public onPlayerLeave(id: string) {
    this.players.delete(id);
  }

  public processMessage(clientId: string, message: ClientMessage): boolean {
    // Non-drone player messages
    switch (message.type) {
      case GameMessage.PLAYER_SELECT_TEAM:
        const { team } = message.payload;
        // Ensure valid team
        if (team < Team.Red || team > Team.Blue) {
          return false; // Invalid team
        }

        return this.attemptSpawnDroneForPlayer(clientId, team);

      default:
        break; // Continue to drone message handling
    }

    // Handle player drone messages
    const playerDrone: DroneState = this.drones.get(clientId) as DroneState;
    if (!playerDrone) return false; // No drone found for this client

    switch (message.type) {
      case GameMessage.PLAYER_MOVE:
        const { movement } = message.payload;
        if (!movement) {
          console.log("Invalid movement payload:", message.payload);
          return false;
        }
        playerDrone.setRequestedMovement(movement);
        return true;

      case GameMessage.PLAYER_AIM:
        const { direction } = message.payload;
        playerDrone.setRequestedAim(direction);
        return true;

      case GameMessage.PLAYER_SHOOT:
        // Trigger shooting
        const projectile = this.droneShoot(clientId);
        if (!projectile) return false; // Shooting failed

        // Add projectile to game state
        const projectileId = `${clientId}_proj_${Date.now()}`;
        this.projectiles.set(projectileId, projectile);
        this.registerProjectileCollider(projectileId, projectile);
        return true;

      case GameMessage.PLAYER_SELECT_UPGRADE:
        const { upgradeId } = message.payload;
        return true;

      default:
        return false; // Unhandled message type
    }
  }

  private attemptSpawnDroneForPlayer(playerId: string, team: DroneTeam): boolean {
    // If the player has a drone, reject team change (teams are locked once a drone is spawned)
    if (this.drones.has(playerId)) {
      return false; // Cannot change team once drone is spawned
    }

    const player = this.players.get(playerId);
    if (!player) {
      return false; // No player found
    }

    // Add drone for player on selected team
    this.addDrone(playerId, player.name, team, "Player");
    return true;
  }

  public addDrone(id: string, name: string, team: DroneTeam, droneType: DroneType): DroneState | null {
    // Spawn position
    const spawnPosition: Vector2 = { x: 0, y: 0 }; // TODO: Replace with actual spawn logic

    const drone = droneType === "Player" ? new DroneState(name, team, spawnPosition) : new AIDroneState(name, team, spawnPosition);
    this.drones.set(id, drone);
    this.registerDroneCollider(id, drone); // Register collider for the drone
    return drone;
  }

  public removeDrone(id: string) {
    this.drones.delete(id);
    this.unregisterCollider(id);
  }

  /**
   * Create and add a projectile fired by the specified drone.
   * @param droneId The ID of the drone that is firing the projectile.
   * @returns The created ProjectileState, or null if no projectile was created.
   */
  public droneShoot(droneId: string): ProjectileState | null {
    // Spawn position (get from drone)
    const drone: DroneState = this.drones.get(droneId);
    if (!drone) return null; // Invalid drone

    const droneStats: DroneStats = drone.getStats();

    // Check if drone can fire
    if (this.gameTimeSeconds - drone.nextShotAvailableTime < 0) {
      return null; // Cannot fire yet
    }
    const attackCooldown = 1 / droneStats.attackSpeed;
    drone.nextShotAvailableTime = this.gameTimeSeconds + attackCooldown; // Update next shot available time

    // Create projectile
    const currentAim = drone.getAimVector();
    const velocity: Vector2 = {
      x: currentAim.x * droneStats.projectileSpeed,
      y: currentAim.y * droneStats.projectileSpeed,
    };

    // Calculate spawn position with offset
    const spawnPosition: Vector2 = {
      x: drone.posX + currentAim.x * Constants.DRONE_WEAPON_PROJECTILE_OFFSET,
      y: drone.posY + currentAim.y * Constants.DRONE_WEAPON_PROJECTILE_OFFSET,
    };
    return new ProjectileState(droneStats.attackDamage, droneStats.attackPierce, drone.team, spawnPosition, velocity);
  }

  //#region Collision System
  // TODO: Consider moving how colliders are added into each specific class (if possible)
  private registerDroneCollider(id: string, drone: DroneState) {
    const collider = new Collider({
      id: id,
      transform: drone,
      layer: CollisionLayer.Drone,
      mask: CollisionLayer.ArenaObject | CollisionLayer.Projectile | CollisionLayer.Drone,
      isTrigger: false,
      team: drone.team,
      handler: drone,
      rigidbody: drone.getRigidbody(),
    });
    this.collisionSystem.register(collider);
    this.collidersById.set(id, collider);
  }

  private registerProjectileCollider(id: string, projectile: ProjectileState) {
    const collider = new Collider({
      id,
      transform: projectile,
      layer: CollisionLayer.Projectile,
      mask: CollisionLayer.Drone | CollisionLayer.ArenaObject | CollisionLayer.Projectile,
      isTrigger: true,
      team: projectile.team,
      handler: projectile,
    });
    this.collisionSystem.register(collider);
    this.collidersById.set(id, collider);
  }

  public registerArenaObjectCollider(id: string, obj: ArenaObjectState) {
    const collider = new Collider({
      id,
      transform: obj,
      layer: CollisionLayer.ArenaObject,
      mask: CollisionLayer.Drone | CollisionLayer.Projectile | CollisionLayer.ArenaObject,
      isTrigger: false,
      handler: obj,
    });
    this.collisionSystem.register(collider);
    this.collidersById.set(id, collider);
  }

  private unregisterCollider(id: ColliderID) {
    this.collisionSystem.unregister(id);
    this.collidersById.delete(id);
  }
  //#endregion
}
