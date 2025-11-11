import { Collider, ColliderID, CollisionLayer } from "@rooms/controllers/collider";
import { Vector2 } from "src/types/commonTypes";
import { VectorUtils } from "src/utils";

export interface CollisionSystemProperties {
  cellSize?: number; // Size of each grid cell for spatial partitioning
}

/**
 * Simple collision system using spatial hashing for broad phase detection
 * and circle-circle collision for narrow phase.
 * Spacial hashing divides the world into a grid of cells, each storing colliders within that cell.
 * This reduces the number of collision checks by only testing colliders in neighboring cells.
 * @method register Register a collider with the system
 * @method unregister Unregister a collider from the system
 * @method update Perform collision detection and invoke callbacks
 */
export class CollisionSystem {
  private readonly colliders: Map<string, Collider> = new Map();
  private readonly cellSize: number;

  // Spatial hash grid
  private grid = new Map<string, Set<ColliderID>>(); // cellKey -> Set of Collider IDs

  // Track pair for enter/stay/exit
  private prevPairs: Set<string> = new Set();
  private currentPairs: Set<string> = new Set();

  constructor(properties?: CollisionSystemProperties) {
    this.cellSize = properties?.cellSize ?? 4; // Cell size should be about the size of largest object
  }

  public register(collider: Collider) {
    this.colliders.set(collider.id, collider);
  }

  public unregister(id: ColliderID) {
    this.colliders.delete(id);
    // The grid and pair sets will be cleaned up in the update step
  }

  public update() {
    // Rebuild the grid
    this.grid.clear();
    for (const collider of this.colliders.values()) {
      this.insertIntoGrid(collider);
    }

    this.currentPairs.clear();

    // Detect potential collisions via neighboring cells
    for (const collider of this.colliders.values()) {
      const cells = this.coveredCells(collider);
      const checkedColliders = new Set<ColliderID>();

      for (const cellKey of cells) {
        const cellPos: Vector2 = decodeCellKey(cellKey);

        // Check neighboring cells
        for (let offsetX = -1; offsetX <= 1; offsetX++) {
          for (let offsetY = -1; offsetY <= 1; offsetY++) {
            const neighborKey = encodeCellKey(cellPos.x + offsetX, cellPos.y + offsetY);
            const cellColliders = this.grid.get(neighborKey);
            if (!cellColliders) continue; // No colliders in this cell

            // Broad phase: check against colliders in this cell
            for (const otherId of cellColliders) {
              // Don't check self or already checked
              if (otherId === collider.id || checkedColliders.has(otherId)) continue;
              checkedColliders.add(otherId);

              const other = this.colliders.get(otherId);
              if (!other) continue;

              // Layer mask filter
              if (!canLayersInteract(collider, other)) continue;

              // Narrow phase: circle-circle collision
              const sumRadius = collider.radius + other.radius;
              const delta: Vector2 = {
                x: other.position.x - collider.position.x,
                y: other.position.y - collider.position.y,
              };
              const distSq = delta.x * delta.x + delta.y * delta.y;
              if (distSq > sumRadius * sumRadius) continue; // No collision

              // Narrow phase collision detected
              const key = pairKey(collider.id, other.id);
              this.currentPairs.add(key);

              const isTriggerPair = collider.isTrigger || other.isTrigger;

              if (!this.prevPairs.has(key)) {
                // Enter
                if (isTriggerPair) {
                  collider.handler?.onTriggerEnter?.(collider, other);
                  other.handler?.onTriggerEnter?.(other, collider);
                } else {
                  collider.handler?.onCollisionEnter?.(collider, other);
                  other.handler?.onCollisionEnter?.(other, collider);
                }
              } else {
                // Stay
                if (isTriggerPair) {
                  collider.handler?.onTriggerStay?.(collider, other);
                  other.handler?.onTriggerStay?.(other, collider);
                } else {
                  collider.handler?.onCollisionStay?.(collider, other);
                  other.handler?.onCollisionStay?.(other, collider);
                }
              }

              // Physical resolution for solid pairs
              if (!isTriggerPair) {
                this.resolveSolidOverlap(collider, other, delta, sumRadius);
              }
            }
          }
        }
      }
    }

    // Handle exit events
    for (const key of this.prevPairs) {
      if (!this.currentPairs.has(key)) {
        const [idA, idB] = key.split("|");
        const colliderA = this.colliders.get(idA);
        const colliderB = this.colliders.get(idB);
        if (!colliderA || !colliderB) continue;

        if (colliderA.isTrigger || colliderB.isTrigger) {
          colliderA.handler?.onTriggerExit?.(colliderA, colliderB);
          colliderB.handler?.onTriggerExit?.(colliderB, colliderA);
        } else {
          colliderA.handler?.onCollisionExit?.(colliderA, colliderB);
          colliderB.handler?.onCollisionExit?.(colliderB, colliderA);
        }
      }
    }

    // Set prevPairs for next frame
    this.prevPairs = new Set(this.currentPairs);
  }

  private resolveSolidOverlap(colliderA: Collider, colliderB: Collider, delta: Vector2, sumRadius: number) {
    const rigidbodyA = colliderA.rigidbody;
    const rigidbodyB = colliderB.rigidbody;

    const invA = rigidbodyA?.getInverseMass() ?? 0;
    const invB = rigidbodyB?.getInverseMass() ?? 0;
    const totalInv = invA + invB;

    if (totalInv === 0) return; // Both kinematic/infinite mass

    // Compute normal and penetration
    let dist = VectorUtils.magnitude(delta);
    if (dist === 0) {
      // To avoid division by zero, nudge in arbitrary direction
      dist = sumRadius;
      delta = { x: sumRadius, y: 0 };
    }

    const penetration = sumRadius - dist;
    if (penetration <= 0) return; // No penetration

    const normal: Vector2 = { x: delta.x / dist, y: delta.y / dist };

    // Positional correction (split by inverse mass)
    const moveA = penetration * (invA / totalInv);
    const moveB = penetration * (invB / totalInv);

    if (rigidbodyA && invA > 0) rigidbodyA.translate(-normal.x * moveA, -normal.y * moveA);
    if (rigidbodyB && invB > 0) rigidbodyB.translate(normal.x * moveB, normal.y * moveB);

    // Velocity resolution (both inelastic and elastic)
    const velA: Vector2 = colliderA.position;
    const velB: Vector2 = colliderB.position;

    const relativeVel: Vector2 = {
      x: velB.x - velA.x,
      y: velB.y - velA.y,
    };
    const velAlongNormal = relativeVel.x * normal.x + relativeVel.y * normal.y;
    if (velAlongNormal >= 0) return; // Currently separating or stationary

    const elasticity = Math.max(rigidbodyA.elasticity, rigidbodyB.elasticity);
    const impulseScalar = (-(1 + elasticity) * velAlongNormal) / totalInv;

    const impulseA: Vector2 = {
      x: -impulseScalar * normal.x * invA,
      y: -impulseScalar * normal.y * invA,
    };
    const impulseB: Vector2 = {
      x: impulseScalar * normal.x * invB,
      y: impulseScalar * normal.y * invB,
    };

    if (rigidbodyA && invA > 0) {
      colliderA.transform.velX += impulseA.x;
      colliderA.transform.velY += impulseA.y;
    }
    if (rigidbodyB && invB > 0) {
      colliderB.transform.velX += impulseB.x;
      colliderB.transform.velY += impulseB.y;
    }
  }

  // Insert collider into spatial hash grid
  private insertIntoGrid(collider: Collider) {
    const cells = this.coveredCells(collider);
    for (const cellKey of cells) {
      let set = this.grid.get(cellKey);
      if (!set) {
        set = new Set<ColliderID>();
        this.grid.set(cellKey, set);
      }
      set.add(collider.id);
    }
  }

  // Get all grid cell keys covered by the collider
  private coveredCells(collider: Collider): string[] {
    const radius = collider.radius;
    const pos = collider.position;
    const minX = Math.floor((pos.x - radius) / this.cellSize);
    const maxX = Math.floor((pos.x + radius) / this.cellSize);
    const minY = Math.floor((pos.y - radius) / this.cellSize);
    const maxY = Math.floor((pos.y + radius) / this.cellSize);

    const keys: string[] = [];
    for (let cellX = minX; cellX <= maxX; cellX++) {
      for (let cellY = minY; cellY <= maxY; cellY++) {
        keys.push(encodeCellKey(cellX, cellY));
      }
    }
    return keys;
  }
}

function pairKey(a: string, b: string): string {
  return a < b ? `${a}|${b}` : `${b}|${a}`;
}

function encodeCellKey(cellX: number, cellY: number): string {
  return `${cellX},${cellY}`;
}

function decodeCellKey(key: string): Vector2 {
  const [sx, sy] = key.split(",");
  return { x: parseInt(sx, 10), y: parseInt(sy, 10) };
}

function canLayersInteract(a: Collider, b: Collider): boolean {
  // If either collider's mask doesn't include the other's layer, they can't interact
  if ((a.mask & b.layer) === 0 || (b.mask & a.layer) === 0) return false;

  // Team filtering for projectiles vs drones
  const aIsProj = a.layer === CollisionLayer.Projectile;
  const bIsProj = b.layer === CollisionLayer.Projectile;
  const aIsDrone = a.layer === CollisionLayer.Drone;
  const bIsDrone = b.layer === CollisionLayer.Drone;

  if ((aIsProj && bIsDrone) || (bIsProj && aIsDrone)) {
    if (a.team !== undefined && b.team !== undefined && a.team === b.team) {
      return false;
    }
  }

  return true;
}
