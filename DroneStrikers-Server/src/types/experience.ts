/**
 * Strategies for determining how much experience to drop when an entity is destroyed.
 */
export type ExperienceDropStrategy = { type: "fixed"; amount: number } | { type: "percentage"; percent: number };

/**
 * Creates a experience drop strategy that drops a fixed amount of experience.
 * @param amount The fixed amount of experience to drop.
 * @returns A fixed drop strategy.
 */
export function createFixedDrop(amount: number): ExperienceDropStrategy {
  return { type: "fixed", amount };
}

/**
 * Creates a experience drop strategy that drops a percentage of a given experience.
 * @param pct The percentage (0 to 1) of the experience to drop.
 * @returns A percentage drop strategy.
 */
export function createPercentageDrop(percent: number): ExperienceDropStrategy {
  return { type: "percentage", percent };
}

/**
 * Computes the amount of experience to drop based on the given strategy.
 * @param strategy The experience drop strategy.
 * @param ctx Additional context for computing the drop. If using a percentage strategy, provide `selfExp`.
 * @returns The amount of experience to drop.
 */
export function computeExperienceDrop(strategy: ExperienceDropStrategy, ctx: { selfExp?: number }): number {
  switch (strategy.type) {
    case "fixed":
      return Math.max(0, Math.floor(strategy.amount));
    case "percentage": {
      return Math.max(0, Math.floor((ctx.selfExp ?? 0) * strategy.percent));
    }
  }
}
