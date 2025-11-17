import { Vector2 } from "src/types/commonTypes";

/**
 * Clamps a number between a minimum and maximum value
 * @param value The number to clamp
 * @param min The minimum value
 * @param max The maximum value
 * @returns The clamped value
 * @see clamp01 for clamping between 0 and 1
 */
export function clamp(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, value));
}

/**
 * Clamps a number between 0 and 1
 * @param value The number to clamp
 * @returns The clamped value between 0 and 1
 * @see clamp for clamping between arbitrary min and max values
 */
export function clamp01(value: number): number {
  return clamp(value, 0, 1);
}

export function randomRange(min: number, max: number): number {
  return Math.random() * (max - min) + min;
}

export function randomChoice<T>(array: T[]): T {
  const index = Math.floor(Math.random() * array.length);
  return array[index];
}

/**
 * Converts a given degree value to radians
 * @param degrees The degree value to convert
 * @returns The equivalent value in radians
 */
export function degToRad(degrees: number): number {
  return (degrees * Math.PI) / 180;
}

/**
 * Converts a given radian value to degrees
 * @param radians The radian value to convert
 * @returns The equivalent value in degrees
 */
export function radToDeg(radians: number): number {
  return (radians * 180) / Math.PI;
}

/**
 * Converts radians to a unit vector where 0 radians points upwards (0, 1)
 * @param radians The radian value to convert
 * @returns The equivalent unit vector
 */
export function radToVec2(radians: number): Vector2 {
  return {
    x: Math.sin(radians),
    y: Math.cos(radians),
  };
}

/**
 * Converts degrees to a unit vector where 0 degrees points upwards (0, 1)
 * @param degrees The degree value to convert
 * @returns The equivalent unit vector
 */
export function degToVec2(degrees: number): Vector2 {
  const rad = degToRad(degrees);
  return {
    x: Math.sin(rad),
    y: Math.cos(rad),
  };
}

/**
 * Linearly interpolates between two numbers.
 * @param start The starting value
 * @param end The ending value
 * @param t The interpolation factor between 0 and 1
 * @returns The interpolated value
 */
export function lerp(start: number, end: number, t: number): number {
  return start + (end - start) * t;
}

/**
 * Spherically interpolates between two radian angles.
 * @param startRad The starting angle in radians
 * @param endRad The ending angle in radians
 * @param t Interpolation factor between 0 and 1
 * @returns The interpolated angle in radians
 */
export function radianSlerp(startRad: number, endRad: number, t: number): number {
  const raw = endRad - startRad;
  const difference = Math.atan2(Math.sin(raw), Math.cos(raw));
  return startRad + difference * t;
}

/**
 * Smoothly steps the current radian angle towards the target radian angle by a maximum delta.
 * @param currentRad The current angle in radians
 * @param targetRad The target angle in radians
 * @param maxDeltaRad The maximum change in radians
 * @returns The new angle in radians after applying the smooth step
 */
export function radianSmoothStep(currentRad: number, targetRad: number, maxDeltaRad: number): number {
  const raw = targetRad - currentRad;
  const difference = Math.atan2(Math.sin(raw), Math.cos(raw));
  const clampedDelta = Math.max(-maxDeltaRad, Math.min(maxDeltaRad, difference));
  return currentRad + clampedDelta;
}

/**
 * Returns the closest point on or inside a circle to a given position.
 * @param center The center of the circle
 * @param radius The radius of the circle
 * @param position The position to find the closest point to
 * @returns The closest point on or inside the circle to the given position
 */
export function closestPointOnCircle(center: Vector2, radius: number, position: Vector2): Vector2 {
  const r = Math.max(0, radius);
  const dx = position.x - center.x;
  const dy = position.y - center.y;
  const distSq = dx * dx + dy * dy;

  if (r === 0) return { x: center.x, y: center.y };

  if (distSq === 0) return position; // At center, inside circle

  const dist = Math.sqrt(distSq);
  if (dist <= r) return position; // Inside or on the circle

  const scale = r / dist;
  return {
    x: center.x + dx * scale,
    y: center.y + dy * scale,
  };
}
