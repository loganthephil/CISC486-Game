import { GameState } from "@rooms/schema/GameState";
import { CommonUtils, Constants } from "src/utils";

// Time between which an AI player will join if there are not enough human players
const MIN_AI_PLAYER_JOIN_SECONDS = 5;
const MAX_AI_PLAYER_JOIN_SECONDS = 15;

export class AIPlayerManager {
  private gameState: GameState;

  private nextAIPlayerJoinTime: number = 0;

  constructor(gameState: GameState) {
    this.gameState = gameState;
  }

  public update(deltaTime: number): void {
    // Add an AI player to the game if needed
    this.tryAddAIPlayer();

    // Spawn AI players if AI player is new or their drone was destroyed
    this.spawnAIPlayerDrones();
  }

  private tryAddAIPlayer(): void {
    const humanPlayerCount = this.gameState.humanPlayerCount;
    const aiPlayerCount = this.gameState.aiPlayerCount;

    if (humanPlayerCount >= Constants.MAX_HUMAN_PLAYERS || aiPlayerCount >= Constants.MAX_AI_PLAYERS) return; // No need for AI players
    if (this.nextAIPlayerJoinTime > this.gameState.gameTimeSeconds) return; // Not time yet

    // Time to add an AI player
    const aiPlayerId = `AI_${Date.now()}`;
    const aiPlayerName = `AI Drone ${aiPlayerCount + 1}`; // TODO: Replace with realistic usernames

    this.gameState.AIPlayerJoin(aiPlayerId, { name: aiPlayerName });
    console.log(`AI Player ${aiPlayerName} joined the game.`);

    // Schedule next AI player join time
    const joinDelay = CommonUtils.randomRange(MIN_AI_PLAYER_JOIN_SECONDS, MAX_AI_PLAYER_JOIN_SECONDS);
    this.nextAIPlayerJoinTime = this.gameState.gameTimeSeconds + joinDelay;
  }

  private spawnAIPlayerDrones(): void {
    const humanPlayerCount = this.gameState.humanPlayerCount;
    const teamsNotFull = this.gameState.getTeamsNotFull();

    for (const [aiPlayerId, aiPlayer] of this.gameState.aiPlayers) {
      if (this.gameState.droneExists(aiPlayerId)) continue; // Drone is still alive, do nothing

      // Check if we should remove the AI player instead of respawning
      if (humanPlayerCount >= Constants.MAX_HUMAN_PLAYERS || teamsNotFull.length === 0) {
        // Remove AI player if there are enough human players
        this.gameState.AIPlayerLeave(aiPlayerId);
        console.log(`AI Player ${aiPlayer.name} left the game.`);
      }

      // Spawn or respawn the AI player's drone
      else {
        // Get a random team that is not full
        const team = CommonUtils.randomChoice(teamsNotFull); // TODO: AI Drone should probably prefer the same team that it was on before
        this.gameState.attemptSpawnDroneForAIPlayer(aiPlayerId, team);
      }
    }
  }
}
