/**
 * Defines an interface for damageable objects.
 * An object implementing this interface can take damage and report if it has been destroyed.
 * Additionally, it can provide the amount of experience to drop upon destruction.
 */
export interface IDamageable {
  /**
   * Applies damage to the object.
   * @param amount The amount of damage to apply.
   * @returns Whether the object has been destroyed.
   */
  takeDamage(amount: number): boolean;

  /**
   * Gets the amount of experience to drop when the object is destroyed.
   * @returns The amount of experience to drop.
   */
  getExperienceDrop(): number;
}

/**
 * Type guard to check if an object implements the IDamageable interface.
 * @param obj The object to check.
 * @returns True if the object is damageable, false otherwise.
 */
export function isDamageable(obj: any): obj is IDamageable {
  return typeof obj.takeDamage === "function" && typeof obj.getExperienceDrop === "function";
}
