import { Prettify, Vector2 } from "src/types/commonTypes";
import { DroneTeam } from "src/types/team";

export enum GameMessage {
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
export type PlayerSelectUpgradePayload = { upgradeId: string };

export type MessagePayloads = {
  [GameMessage.PLAYER_SELECT_TEAM]: Prettify<PlayerSelectTeamPayload>;
  [GameMessage.PLAYER_MOVE]: Prettify<PlayerMovePayload>;
  [GameMessage.PLAYER_AIM]: Prettify<PlayerAimPayload>;
  [GameMessage.PLAYER_SHOOT]: Prettify<PlayerShootPayload>;
  [GameMessage.PLAYER_SELECT_UPGRADE]: Prettify<PlayerSelectUpgradePayload>;
};

export type ClientMessage = {
  [K in GameMessage]: { type: K; payload: MessagePayloads[K] };
}[GameMessage];
