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

export type Stats = { [K in StatType]: number };

export const DEFAULT_STATS: Stats = {
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

export type ModifierType = "flat" | "percentAdd" | "percentMult";

export interface StatModifier {
  stat: StatType;
  type: ModifierType;
  value: number;
  sourceID: string; // Unique ID to identify the source of the modifier
}

export class Stat {
  private baseValue: number;
  private finalValue: number;
  private modifiers: StatModifier[] = [];

  private isDirty: boolean = false;

  constructor(value: number) {
    this.baseValue = value;
    this.finalValue = this.baseValue;
  }

  public get value(): number {
    if (this.isDirty) {
      this.recalculate();
    }
    return this.finalValue;
  }

  public addModifier(modifier: StatModifier): void {
    this.modifiers.push(modifier);
    this.isDirty = true;
  }

  public removeModifier(sourceID: string): void {
    this.modifiers = this.modifiers.filter((mod) => mod.sourceID !== sourceID);
    this.isDirty = true;
  }

  private recalculate(): void {
    let flat = 0;
    let percentAdd = 0;
    let percentMult = 1;

    for (const mod of this.modifiers) {
      switch (mod.type) {
        case "flat":
          flat += mod.value;
          break;
        case "percentAdd":
          percentAdd += mod.value;
          break;
        case "percentMult":
          percentMult *= 1 + mod.value;
          break;
      }
    }

    this.finalValue = (this.baseValue + flat) * (1 + percentAdd) * percentMult;
    this.isDirty = false;
  }
}

/**
 * Creates a map of StatType to Stat instances initialized with default values.
 * @returns A Map where keys are StatType and values are Stat instances.
 */
export function createDefaultStatsMap(): Map<StatType, Stat> {
  const statsMap: Map<StatType, Stat> = new Map<StatType, Stat>();
  (Object.keys(DEFAULT_STATS) as StatType[]).forEach((statType) => {
    statsMap.set(statType, new Stat(DEFAULT_STATS[statType]));
  });
  return statsMap;
}
