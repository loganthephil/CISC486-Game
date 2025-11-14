import { Prettify } from "src/types/commonTypes";

export enum ServerMessageType {
  UPGRADE_APPLIED = "upgrade_applied",
}

export type UpgradeAppliedPayload = { droneId: string; upgradeId: string };

export type ServerMessagePayloads = {
  [ServerMessageType.UPGRADE_APPLIED]: Prettify<UpgradeAppliedPayload>;
};

export type ServerMessage = {
  [K in ServerMessageType]: { type: K; payload: ServerMessagePayloads[K] };
}[ServerMessageType];
