export type StatType =
  | "aimSpeed"
  | "attackDamage"
  | "attackPierce"
  | "attackSpeed"
  | "bodyDamage"
  | "healthRegen"
  | "maxHealth"
  | "moveAcceleration"
  | "moveDeceleration"
  | "moveSpeed"
  | "projectileSpeed"
  | "recoilForce";

export type DroneStats = { [K in StatType]: number };

const DEFAULT_DRONE_STATS: DroneStats = {
  aimSpeed: 5,
  attackDamage: 5,
  attackPierce: 1,
  attackSpeed: 2,
  bodyDamage: 5,
  healthRegen: 1,
  maxHealth: 50,
  moveAcceleration: 8,
  moveDeceleration: 10,
  moveSpeed: 5,
  projectileSpeed: 10,
  recoilForce: 2,
};

export function createDroneStats(overrides?: Partial<DroneStats>): DroneStats {
  return { ...DEFAULT_DRONE_STATS, ...overrides };
}
