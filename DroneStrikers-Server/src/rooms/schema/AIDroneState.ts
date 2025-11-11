import { DroneState } from "@rooms/schema/DroneState";
import { Vector2 } from "src/types/commonTypes";
import { DroneTeam } from "src/types/team";

export class AIDroneState extends DroneState {
  constructor(name: string, team: DroneTeam, position: Vector2) {
    super(name, team, position);
  }
}
