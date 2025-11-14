import { DroneUpgradeID, UpgradeType } from "src/types/droneUpgrade";

export interface UpgradeTreeNode {
  children: UpgradeTreeNode[]; // Next possible upgrades
  upgradeId?: DroneUpgradeID; // ID of the upgrade at this node (root does not have an upgrade)
}

export const DRONE_UPGRADE_TREES = {
  turret: {
    children: [
      {
        upgradeId: "machineGun",
        children: [],
      },
      {
        upgradeId: "sniper",
        children: [],
      },
    ],
  },
  body: {
    children: [
      {
        upgradeId: "tank",
        children: [],
      },
      {
        upgradeId: "light",
        children: [],
      },
    ],
  },
  movement: {
    children: [
      {
        upgradeId: "quadWheels",
        children: [],
      },
      {
        upgradeId: "quadLegs",
        children: [],
      },
    ],
  },
} as const satisfies Record<UpgradeType, UpgradeTreeNode>;
