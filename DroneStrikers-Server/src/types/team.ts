export enum Team {
  Neutral = 0,
  Red = 1,
  Blue = 2,
}

export type ObjectTeam = Team.Neutral;
export type DroneTeam = Team.Red | Team.Blue;
