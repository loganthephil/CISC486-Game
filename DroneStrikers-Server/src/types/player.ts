import { DroneTeam } from "src/types/team";

export interface Player {
  name: string;
}

export interface AIPlayer extends Player {
  preferredTeam?: DroneTeam;
}

export function createPlayer(name: string): Player {
  return { name };
}

export function createAIPlayer(name: string, preferredTeam?: DroneTeam): AIPlayer {
  return { name, preferredTeam };
}
