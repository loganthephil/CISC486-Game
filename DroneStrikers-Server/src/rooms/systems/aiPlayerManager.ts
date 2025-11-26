import { GameState } from "@rooms/schema/GameState";
import { generateAIUsername } from "@rooms/systems/usernameGenerator";
import { createAIPlayer } from "src/types/player";
import { DroneTeam } from "src/types/team";
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
    const aiPlayerName = generateAIUsername();

    const aiPlayer = createAIPlayer(aiPlayerName);
    this.gameState.AIPlayerJoin(aiPlayerId, aiPlayer);
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
        let team = aiPlayer.preferredTeam;
        
        // If preferred team is full or not set, pick a random available team
        if (!team || !teamsNotFull.includes(team)) team = CommonUtils.randomChoice(teamsNotFull);

        this.gameState.attemptSpawnDroneForAIPlayer(aiPlayerId, team);
        aiPlayer.preferredTeam = team; // Update preferred team
      }
    }
  }
}
