import { MapSchema, type } from "@colyseus/schema";
import { ArenaObjectState } from "@rooms/schema/ArenaObjectState";
import { DroneState } from "@rooms/schema/DroneState";
import { ProjectileState } from "@rooms/schema/ProjectileState";
import { DroneType, Vector2 } from "src/types/commonTypes";
import { ClientMessage, ClientMessageType } from "src/types/clientMessage";
import { DroneTeam, Team } from "src/types/team";
import { Player } from "src/types/player";
import { Constants } from "src/utils";
import { BehaviorState } from "@rooms/schema/BehaviourState";
import { Room } from "colyseus";
import { ArenaObjectSpawner } from "@rooms/systems/arenaObjectSpawner";
import { Collider, CollisionLayer, ColliderID } from "@rooms/systems/collider";
import { CollisionSystem } from "@rooms/systems/collisionSystem";
import { DroneSpawner } from "@rooms/systems/droneSpawner";

export class GameState extends BehaviorState {
  // -- BELOW ARE SYNCED TO ALL PLAYERS --
  @type("number") gameTimeSeconds: number = 0;

  @type({ map: DroneState }) drones = new MapSchema<DroneState>();
  @type({ map: ArenaObjectState }) arenaObjects = new MapSchema<ArenaObjectState>();
  @type({ map: ProjectileState }) projectiles = new MapSchema<ProjectileState>();
  // -- ABOVE ARE SYNCED TO ALL PLAYERS --

  private room: Room;

  // Keep track of players that have joined the session (independent of drones)
  private players: Map<string, Player> = new Map<string, Player>();

  // Collision system
  private collisionSystem: CollisionSystem;
  // private collidersById: Map<ColliderID, Collider> = new Map<ColliderID, Collider>();

  private droneSpawner: DroneSpawner = new DroneSpawner();
  private arenaObjectSpawner: ArenaObjectSpawner = new ArenaObjectSpawner(this);

  constructor(room: Room) {
    super();
    this.room = room;
    this.collisionSystem = new CollisionSystem({ cellSize: 3 });
  }

  public override update(deltaTime: number) {
    this.gameTimeSeconds += deltaTime;

    // Spawn arena objects
    const newArenaObjects = this.arenaObjectSpawner.doSpawnObjectsTick();
    newArenaObjects.forEach((obj) => {
      const objId = `arenaObj_${Date.now()}_${Math.floor(Math.random() * 1000)}`;
      this.arenaObjects.set(objId, obj);
      this.registerArenaObjectCollider(objId, obj);
    });

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

    // Update all arena objects
    const toRemoveObjects: string[] = [];
    this.arenaObjects.forEach((obj, id) => {
      obj.update(deltaTime);
      if (obj.toDespawn) toRemoveObjects.push(id);
    });

    // Handle collisions after updating all entities
    this.collisionSystem.processCollisions();

    // Remove objects marked for despawn
    dronesToRemove.forEach((id) => {
      this.removeDrone(id);
      this.unregisterCollider(id);
    });
    projectilesToRemove.forEach((id) => {
      this.projectiles.delete(id);
      this.unregisterCollider(id);
    });
    toRemoveObjects.forEach((id) => {
      this.arenaObjects.get(id)?.onDestroy(); // Call onDestroy callback
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
      case ClientMessageType.PLAYER_SELECT_TEAM:
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
      case ClientMessageType.PLAYER_MOVE:
        const { movement } = message.payload;
        if (!movement) {
          console.log("Invalid movement payload:", message.payload);
          return false;
        }
        playerDrone.setRequestedMovement(movement);
        return true;

      case ClientMessageType.PLAYER_AIM:
        const { direction } = message.payload;
        playerDrone.setRequestedAim(direction);
        return true;

      case ClientMessageType.PLAYER_SHOOT:
        // Trigger shooting
        const projectile = this.droneShoot(clientId);
        if (!projectile) return false; // Shooting failed

        // Add projectile to game state
        const projectileId = `${clientId}_proj_${Date.now()}`;
        this.projectiles.set(projectileId, projectile);
        this.registerProjectileCollider(projectileId, projectile);
        return true;

      case ClientMessageType.PLAYER_SELECT_UPGRADE:
        const { upgradeId } = message.payload;

        playerDrone.droneUpgrader.tryApplyUpgrade(upgradeId);
        return true;

      default:
        console.log("Unhandled client message type:", (message as any).type);
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
    const drone = this.droneSpawner.createDrone(name, team, droneType);
    if (!drone) {
      console.warn(`Failed to spawn drone for player ${name} on team ${team}`);
      return null;
    }

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

    // Check if drone can fire
    if (this.gameTimeSeconds - drone.nextShotAvailableTime < 0) {
      return null; // Cannot fire yet
    }

    const projectileSpeedStat = drone.getStatValue("projectileSpeed");
    const attackSpeedStat = drone.getStatValue("attackSpeed");
    const attackDamageStat = drone.getStatValue("attackDamage");
    const attackPierceStat = drone.getStatValue("attackPierce");

    const attackCooldown = 1 / attackSpeedStat;
    drone.nextShotAvailableTime = this.gameTimeSeconds + attackCooldown; // Update next shot available time

    // Create projectile
    const currentAim = drone.getAimVector();
    const velocity: Vector2 = {
      x: currentAim.x * projectileSpeedStat,
      y: currentAim.y * projectileSpeedStat,
    };

    // Calculate spawn position with offset
    const spawnPosition: Vector2 = {
      x: drone.posX + currentAim.x * Constants.DRONE_WEAPON_PROJECTILE_OFFSET,
      y: drone.posY + currentAim.y * Constants.DRONE_WEAPON_PROJECTILE_OFFSET,
    };
    return new ProjectileState(drone, attackDamageStat, attackPierceStat, drone.team, spawnPosition, velocity);
  }

  //#region Collision System
  // TODO: Consider moving how colliders are added into each specific class (if possible)
  private registerDroneCollider(id: string, drone: DroneState) {
    const collider = new Collider({
      id: id,
      transform: drone,
      layer: CollisionLayer.Drone,
      mask: CollisionLayer.Drone | CollisionLayer.Projectile | CollisionLayer.ArenaObject,
      isTrigger: false,
      team: drone.team,
      handler: drone,
      rigidbody: drone.getRigidbody(),
    });
    this.collisionSystem.register(collider);
    // this.collidersById.set(id, collider);
  }

  private registerProjectileCollider(id: string, projectile: ProjectileState) {
    const collider = new Collider({
      id,
      transform: projectile,
      layer: CollisionLayer.Projectile,
      mask: CollisionLayer.Drone | CollisionLayer.Projectile | CollisionLayer.ArenaObject,
      isTrigger: true,
      team: projectile.team,
      handler: projectile,
    });
    this.collisionSystem.register(collider);
    // this.collidersById.set(id, collider);
  }

  public registerArenaObjectCollider(id: string, obj: ArenaObjectState) {
    const collider = new Collider({
      id,
      transform: obj,
      layer: CollisionLayer.ArenaObject,
      mask: CollisionLayer.Drone | CollisionLayer.Projectile | CollisionLayer.ArenaObject,
      isTrigger: false,
      handler: obj,
      rigidbody: obj.getRigidbody(),
    });
    this.collisionSystem.register(collider);
    // this.collidersById.set(id, collider);
  }

  private unregisterCollider(id: ColliderID) {
    this.collisionSystem.unregister(id);
    // this.collidersById.delete(id);
  }
  //#endregion
}
