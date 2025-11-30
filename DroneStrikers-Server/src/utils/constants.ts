// -- DEVELOPMENT CONSTANTS --
/**
 * Enable debug drone spawns for development purposes.
 */
export const ENABLE_DEBUG_DRONE_SPAWNS = false;

export const DEBUG_DISABLE_ARENA_OBJECTS = false;

/**
 * Enable debug latency simulation for development purposes. MAKE SURE TO DISABLE FOR PRODUCTION.
 */
export const ENABLE_DEBUG_LATENCY_SIMULATION = false;

export const DEBUG_LATENCY_MS = 50;

// -- TIMING CONSTANTS --
/**
 * Fixed time step for the game simulation in milliseconds.
 */
export const FIXED_TIME_STEP_MS = 20;

/**
 * Fixed time step for the game simulation in seconds.
 */
export const FIXED_TIME_STEP_S = 0.02;

/**
 * Patch rate for sending state updates to clients in milliseconds.
 */
export const PATCH_RATE_MS = 50;

// -- GAME CONSTANTS --
export const MAX_HUMAN_PLAYERS = 10;

export const MAX_AI_PLAYERS = 0; // Normally should be 9, but change for testing

export const MAX_DRONE_PER_TEAM = 5;

export const MAX_USERNAME_LENGTH = 20;

/**
 * Maximum coordinate value for the game map (both X and Y axes).
 */
export const MAP_MAX_COORDINATE = 75;

/**
 * Offset from drone center to projectile spawn point
 */
export const DRONE_WEAPON_PROJECTILE_OFFSET = 0.8;

/**
 * Base knockback force applied when an object collides with another.
 */
export const BASE_KNOCKBACK_FORCE = 4;
