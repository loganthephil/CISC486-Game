import { DroneState } from "@rooms/schema/DroneState";
import { DroneStats } from "@rooms/systems/droneStats";
import { DRONE_UPGRADES, DroneUpgradeID, UpgradeType } from "src/types/droneUpgrade";
import { DRONE_UPGRADE_TREES, UpgradeTreeNode } from "src/types/upgradeTree";

const UPGRADE_POINT_LEVELS: Set<number> = new Set<number>([2, 5, 10, 15, 20, 25, 30, 40, 50, 75]);

export class DroneUpgrader {
  // TODO: Check if its okay to use static map here
  private static readonly experienceToNextLevelMap: Map<number, number> = new Map<number, number>();

  private readonly drone: DroneState;
  private readonly droneStats: DroneStats;

  private currentUpgradeNode: Map<UpgradeType, UpgradeTreeNode>; // Current position in each upgrade tree

  constructor(drone: DroneState, droneStats: DroneStats) {
    this.drone = drone;
    this.droneStats = droneStats;

    // Initialize current upgrade nodes to the root of each tree
    const upgradeTreesRecord = DRONE_UPGRADE_TREES;
    this.currentUpgradeNode = new Map<UpgradeType, UpgradeTreeNode>(Object.entries(upgradeTreesRecord) as [UpgradeType, UpgradeTreeNode][]);
  }

  /**
   * Awards experience to the drone and checks for level up.
   * @param amount The amount of experience to award.
   */
  public awardExperience(amount: number) {
    this.drone.experience += amount;

    // Continuously check for level ups while enough experience is available
    while (this.drone.experience >= this.requiredExperienceForNextLevel) {
      this.drone.level += 1;

      // Award upgrade point if the new level is a level that grants an upgrade point
      if (!UPGRADE_POINT_LEVELS.has(this.drone.level)) continue;
      this.drone.upgradePoints += 1;
    }

    // Update progress to next level
    this.drone.progressToNextLevel = this.getProgressToNextLevel();
  }

  /**
   * Returns the experience required for the next level.
   */
  public get requiredExperienceForNextLevel(): number {
    return DroneUpgrader.experienceToLevel(this.drone.level + 1);
  }

  /**
   * Calculates and returns the progress towards the next level as a value between 0 and 1.
   */
  public getProgressToNextLevel(): number {
    const currentLevel = this.drone.level;
    const experienceToCurrentLevel = DroneUpgrader.experienceToLevel(currentLevel);
    const denominator = DroneUpgrader.experienceToLevel(currentLevel + 1) - experienceToCurrentLevel;
    return (this.drone.experience - experienceToCurrentLevel) / denominator;
  }

  public tryApplyUpgrade(upgradeId: DroneUpgradeID): boolean {
    if (this.drone.upgradePoints <= 0) return false; // Not enough upgrade points

    const upgrade = DRONE_UPGRADES[upgradeId];
    if (!upgrade) return false; // Invalid upgrade ID

    // Check if the upgrade is available in the current upgrade tree for its upgrade type
    const upgradeType = upgrade.type;
    const currentNode = this.currentUpgradeNode.get(upgradeType);
    if (!currentNode) return false; // Invalid upgrade type

    const nextNode = currentNode.children.find((child) => child.upgradeId === upgradeId);
    if (!nextNode) return false; // Upgrade not available in current tree

    // Apply upgrade modifiers to drone stats
    for (const m of upgrade.modifiers) {
      this.droneStats.addModifier(m.stat, m.type, m.value, upgrade.name); // <-- Use upgrade name as sourceID
    }

    this.drone.upgradePoints -= 1; // Consume an upgrade point
    this.currentUpgradeNode.set(upgradeType, nextNode); // Move to the next node in the upgrade tree

    // Update last applied upgrade ID based on upgrade type
    switch (upgradeType) {
      case "turret":
        this.drone.lastTurretUpgradeId = upgradeId;
        break;
      case "body":
        this.drone.lastBodyUpgradeId = upgradeId;
        break;
      case "movement":
        this.drone.lastMovementUpgradeId = upgradeId;
        break;
    }

    return true; // Upgrade successfully applied
  }

  private static experienceToLevel(level: number): number {
    if (!this.experienceToNextLevelMap.has(level)) {
      // Calculate experience required for the given level if not already cached
      const experienceRequired = 10 * Math.pow(level - 1, 1.5);
      this.experienceToNextLevelMap.set(level, experienceRequired);
      return experienceRequired;
    }
    return this.experienceToNextLevelMap.get(level) ?? 0;
  }
}
