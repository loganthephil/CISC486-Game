import { StatModifier } from "src/types/stats";

export type UpgradeType = "turret" | "body" | "movement";

export interface DroneUpgrade {
  name: string;
  description: string;
  type: UpgradeType;
  modifiers: Omit<StatModifier, "sourceID">[];
}

// TODO: Move descriptions to client side only (don't need to be sent from the server)

const TURRET_UPGRADES = {
  machineGun: {
    name: "Machine Gun",
    description: "Fires rapidly but with lower damage per shot",
    type: "turret",
    modifiers: [
      { stat: "attackDamage", type: "percentAdd", value: -0.2 },
      { stat: "attackSpeed", type: "percentAdd", value: 0.5 },
      { stat: "aimSpeed", type: "percentAdd", value: -0.1 },
    ],
  },
  sniper: {
    name: "Sniper",
    description: "Slower firing but high damage and pierce",
    type: "turret",
    modifiers: [
      { stat: "attackDamage", type: "percentAdd", value: 0.6 },
      { stat: "projectileSpeed", type: "flat", value: 2 },
      { stat: "attackSpeed", type: "percentAdd", value: -0.25 },
      { stat: "recoilForce", type: "flat", value: 1 },
      { stat: "attackPierce", type: "flat", value: 1 },
      { stat: "aimSpeed", type: "percentAdd", value: 0.1 },
    ],
  },
} as const satisfies Record<string, DroneUpgrade>;

const BODY_UPGRADES = {
  tank: {
    name: "Tank",
    description: "Increased health but slower movement",
    type: "body",
    modifiers: [
      { stat: "maxHealth", type: "percentAdd", value: 0.25 },
      { stat: "moveSpeed", type: "percentAdd", value: -0.1 },
      { stat: "recoilForce", type: "percentAdd", value: -0.15 }, // Less knockback. Replace with "stability" later
    ],
  },
  light: {
    name: "Light",
    description: "Less health but increased healing and movement speed",
    type: "body",
    modifiers: [
      { stat: "maxHealth", type: "percentAdd", value: -0.25 },
      { stat: "moveSpeed", type: "percentAdd", value: 0.1 },
      { stat: "healthRegen", type: "flat", value: 2 },
    ],
  },
} as const satisfies Record<string, DroneUpgrade>;

const MOVEMENT_UPGRADES = {
  quadWheels: {
    name: "Quad Wheels",
    description: "Faster movement speed but reduced control",
    type: "movement",
    modifiers: [
      { stat: "moveSpeed", type: "flat", value: 2 },
      { stat: "moveAcceleration", type: "percentAdd", value: 0.75 },
      { stat: "moveDeceleration", type: "percentAdd", value: 0.75 },
    ],
  },
  quadLegs: {
    name: "Quad Legs",
    description: "Improved control and stability",
    type: "movement",
    modifiers: [
      { stat: "moveAcceleration", type: "percentAdd", value: 2 },
      { stat: "moveDeceleration", type: "percentAdd", value: 2 },
      { stat: "recoilForce", type: "percentAdd", value: -0.25 }, // Less knockback. Replace with "stability" later
    ],
  },
} as const satisfies Record<string, DroneUpgrade>;

export const DRONE_UPGRADES = {
  ...TURRET_UPGRADES,
  ...BODY_UPGRADES,
  ...MOVEMENT_UPGRADES,
} as const satisfies Record<string, DroneUpgrade>;

export type DroneUpgradeID = keyof typeof DRONE_UPGRADES;
