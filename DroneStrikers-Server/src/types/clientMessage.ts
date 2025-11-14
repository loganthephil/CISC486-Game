import { Prettify, Vector2 } from "src/types/commonTypes";
import { DroneUpgradeID } from "src/types/droneUpgrade";
import { DroneTeam } from "src/types/team";

export enum ClientMessageType {
  PLAYER_SELECT_TEAM,
  PLAYER_MOVE,
  PLAYER_AIM,
  PLAYER_SHOOT,
  PLAYER_SELECT_UPGRADE,
}

export type PlayerSelectTeamPayload = { team: DroneTeam };
export type PlayerMovePayload = { movement: Vector2 };
export type PlayerAimPayload = { direction: Vector2 };
export type PlayerShootPayload = {};
export type PlayerSelectUpgradePayload = { upgradeId: DroneUpgradeID };

export type ClientMessagePayloads = {
  [ClientMessageType.PLAYER_SELECT_TEAM]: Prettify<PlayerSelectTeamPayload>;
  [ClientMessageType.PLAYER_MOVE]: Prettify<PlayerMovePayload>;
  [ClientMessageType.PLAYER_AIM]: Prettify<PlayerAimPayload>;
  [ClientMessageType.PLAYER_SHOOT]: Prettify<PlayerShootPayload>;
  [ClientMessageType.PLAYER_SELECT_UPGRADE]: Prettify<PlayerSelectUpgradePayload>;
};

export type ClientMessage = {
  [K in ClientMessageType]: { type: K; payload: ClientMessagePayloads[K] };
}[ClientMessageType];
