export type Prettify<T> = { [K in keyof T]: T[K] } & {};

/**
 * A 2D vector interface.
 * Contains x and y components.
 */
export interface Vector2 {
  x: number;
  y: number;
}

export type DroneType = "Player" | "AI";
