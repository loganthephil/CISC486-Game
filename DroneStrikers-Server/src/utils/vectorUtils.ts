import { Vector2 } from "src/types/commonTypes";
import { CommonUtils } from "src/utils";

export function magnitude(vector: Vector2): number {
  return Math.sqrt(vector.x * vector.x + vector.y * vector.y);
}

export function sqrMagnitude(vector: Vector2): number {
  return vector.x * vector.x + vector.y * vector.y;
}

export function normalize(vector: Vector2): Vector2 {
  const mag = magnitude(vector);
  if (mag === 0) {
    return { x: 0, y: 0 };
  }
  return { x: vector.x / mag, y: vector.y / mag };
}

export function add(v1: Vector2, v2: Vector2): Vector2 {
  return { x: v1.x + v2.x, y: v1.y + v2.y };
}

export function subtract(v1: Vector2, v2: Vector2): Vector2 {
  return { x: v1.x - v2.x, y: v1.y - v2.y };
}

/**
 * Multiplies the vector by the given scalar.
 * @param vector The vector to scale.
 * @param scalar The scalar to multiply by.
 * @returns The scaled vector.
 */
export function scale(vector: Vector2, scalar: number): Vector2 {
  return { x: vector.x * scalar, y: vector.y * scalar };
}

export function divide(vector: Vector2, scalar: number): Vector2 {
  if (scalar === 0) {
    throw new Error("Division by zero");
  }
  return { x: vector.x / scalar, y: vector.y / scalar };
}

export function dot(v1: Vector2, v2: Vector2): number {
  return v1.x * v2.x + v1.y * v2.y;
}

export function equals(v1: Vector2, v2: Vector2): boolean {
  return v1.x === v2.x && v1.y === v2.y;
}

/**
 * Returns true if the vector's magnitude is less than the specified threshold.
 * @param vector The vector to check.
 * @param threshold The threshold below which the vector is considered negligible. Defaults to 0.0001.
 * @returns True if the vector is negligible, false otherwise.
 */
export function isNegligible(vector: Vector2, threshold: number = 0.0001): boolean {
  return Math.abs(vector.x) < threshold && Math.abs(vector.y) < threshold;
}

/**
 * Moves the vector `current` towards `target` by the maximum distance `maxDistanceDelta`.
 * @param current The current vector.
 * @param target The target vector.
 * @param maxDistanceDelta Distance to move per call.
 * @returns The new position vector.
 */
export function moveTowards(current: Vector2, target: Vector2, maxDistanceDelta: number): Vector2 {
  const toVector_x = target.x - current.x;
  const toVector_y = target.y - current.y;
  const sqDist = toVector_x * toVector_x + toVector_y * toVector_y;

  if (sqDist === 0 || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta)) {
    return target;
  }

  const dist = Math.sqrt(sqDist);
  return {
    x: current.x + (toVector_x / dist) * maxDistanceDelta,
    y: current.y + (toVector_y / dist) * maxDistanceDelta,
  };
}

export function lerp(v1: Vector2, v2: Vector2, t: number): Vector2 {
  // Clamp t to the range [0, 1]
  t = Math.max(0, Math.min(1, t));
  return {
    x: v1.x + (v2.x - v1.x) * t,
    y: v1.y + (v2.y - v1.y) * t,
  };
}

/**
 * Spherically interpolates between two vectors.
 * @param v1 The starting vector.
 * @param v2 The ending vector.
 * @param t The interpolation factor between 0 and 1.
 * @returns The interpolated vector.
 */
export function slerp(v1: Vector2, v2: Vector2, t: number): Vector2 {
  // Clamp t to the range [0, 1]
  t = Math.max(0, Math.min(1, t));
  const dotProduct = CommonUtils.clamp(dot(v1, v2), -1, 1);
  const theta = Math.acos(dotProduct) * t;
  const relativeVec = normalize(subtract(v2, scale(v1, dotProduct)));
  return add(scale(v1, Math.cos(theta)), scale(relativeVec, Math.sin(theta)));
}
