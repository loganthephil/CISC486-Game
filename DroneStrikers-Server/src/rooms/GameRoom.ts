import { Room, Client } from "@colyseus/core";
import { Encoder } from "@colyseus/schema";
import { GameState } from "./schema/GameState";
import { PlayerOptions } from "src/types/playerOptions";
import { Constants } from "src/utils";
import { ClientMessage } from "src/types/clientMessage";
import { createPlayer } from "src/types/player";

Encoder.BUFFER_SIZE = 32 * 1024;

export class GameRoom extends Room<GameState> {
  maxClients = Constants.MAX_HUMAN_PLAYERS;
  state: GameState = new GameState(this);

  onCreate(options: any) {
    console.log("GameRoom created!", this.roomId, " | Options: ", options);

    this.patchRate = Constants.PATCH_RATE_MS;
    this.setSimulationInterval((dt) => this.handleTick(dt / 1000), Constants.FIXED_TIME_STEP_MS);

    this.registerMessageHandlers(); // Setup message handlers
  }

  onJoin(client: Client, options: PlayerOptions) {
    console.log(client.sessionId, "joined!", "options:", options);

    if (!options.username || typeof options.username !== "string") {
      // Invalid name, disconnect the client
      client.leave(4001, "Invalid player name: missing or not a string");
      return;
    }

    if (options.username.length < 1) {
      // Invalid name, disconnect the client
      client.leave(4002, "Invalid player name: name too short");
      return;
    }

    // Cap the username length
    if (options.username.length > Constants.MAX_USERNAME_LENGTH) {
      options.username = options.username.substring(0, Constants.MAX_USERNAME_LENGTH);
    }

    this.state.onPlayerJoin(client.sessionId, createPlayer(options.username));
  }

  onLeave(client: Client, consented: boolean) {
    console.log(client.sessionId, "left!");

    // Remove Drone associated with this client
    this.state.removeDrone(client.sessionId);

    this.state.onPlayerLeave(client.sessionId);
  }

  onDispose() {
    console.log("Room", this.roomId, "disposing...");
  }

  handleTick = (deltaTime: number) => {
    // console.time("GameRoom Tick");
    this.state.update(deltaTime); // Update the game state each tick
    // console.timeEnd("GameRoom Tick");
  };
  private registerMessageHandlers() {
    this.onMessage("*", (client, type, payload) => {
      if (typeof type !== "number") return; // ignore string-typed messages

      // Cast to ClientMessage type
      const msg = { type, payload } as ClientMessage;
      this.state.processMessage(client.sessionId, msg);
    });
  }
}
