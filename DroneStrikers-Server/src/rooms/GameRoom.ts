import { Room, Client } from "@colyseus/core";
import { Encoder } from "@colyseus/schema";
import { GameState } from "./schema/GameState";
import { PlayerOptions } from "src/types/playerOptions";
import { Constants } from "src/utils";
import { ClientMessage } from "src/types/clientMessage";
import { createPlayer } from "src/types/player";

Encoder.BUFFER_SIZE = 32 * 1024;

export class GameRoom extends Room<GameState> {
  maxClients = 10;
  state: GameState = new GameState(this);

  onCreate(options: any) {
    this.patchRate = Constants.PATCH_RATE_MS;
    this.setSimulationInterval((dt) => this.handleTick(dt / 1000), Constants.FIXED_TIME_STEP_MS);

    this.registerMessageHandlers(); // Setup message handlers
  }

  onJoin(client: Client, options: PlayerOptions) {
    console.log(client.sessionId, "joined!");

    // TODO: Enforce name validity
    // if (droneName.length < 1) {
    //   // Invalid name, disconnect the client
    //   client.leave();
    //   return;
    // }

    this.state.onPlayerJoin(client.sessionId, createPlayer(options.name));
  }

  onLeave(client: Client, consented: boolean) {
    console.log(client.sessionId, "left!");

    // Remove Drone associated with this client
    this.state.removeDrone(client.sessionId);

    this.state.onPlayerLeave(client.sessionId);

    client.leave(); // Ensure the client is fully disconnected
  }

  onDispose() {
    console.log("Room", this.roomId, "disposing...");
  }

  handleTick = (deltaTime: number) => this.state.update(deltaTime); // Update the game state each tick

  private registerMessageHandlers() {
    this.onMessage("*", (client, type, payload) => {
      if (typeof type !== "number") return; // ignore string-typed messages

      // Cast to ClientMessage type
      const msg = { type, payload } as ClientMessage;
      this.state.processMessage(client.sessionId, msg);
    });
  }
}
