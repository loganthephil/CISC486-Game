import { createDefaultStatsMap, ModifierType, Stat, Stats, StatType } from "src/types/stats";

export class DroneStats {
  private _stats: Map<StatType, Stat>;

  constructor() {
    this._stats = createDefaultStatsMap();
  }

  /**
   * Retrieves the value of the specified stat.
   * @param stat The stat to retrieve.
   * @returns The value of the stat.
   */
  public getValue(stat: StatType): number {
    return this._stats.get(stat)?.value ?? 0;
  }

  /**
   * Adds a modifier to the specified stat.
   * @param stat The stat to add the modifier to.
   * @param type The type of modifier to add.
   * @param value The value of the modifier.
   * @param sourceID The unique ID of the source of the modifier.
   */
  public addModifier(stat: StatType, type: ModifierType, value: number, sourceID: string): void {
    const statObj = this._stats.get(stat);
    if (statObj) {
      statObj.addModifier({ stat, type, value, sourceID });
    }
  }

  /**
   * Removes a modifier from the specified stat.
   * @param stat The stat to remove the modifier from.
   * @param sourceID The unique ID of the source of the modifier.
   */
  public removeModifier(stat: StatType, sourceID: string): void {
    const statObj = this._stats.get(stat);
    if (statObj) {
      statObj.removeModifier(sourceID);
    }
  }
}
