import { Schema, type } from "@colyseus/schema";
import { DroneTeam } from "src/types/team";

export class LeaderboardEntry extends Schema {
  @type("string") name: string;
  @type("number") experience: number;
  @type("uint8") team: DroneTeam;

  constructor(name: string, experience: number, team: DroneTeam) {
    super();
    this.name = name;
    this.experience = experience;
    this.team = team;
  }
}
