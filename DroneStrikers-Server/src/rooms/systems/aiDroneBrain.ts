import { AIDroneState } from "@rooms/schema/AIDroneState";
import { DroneState } from "@rooms/schema/DroneState";
import { TransformState } from "@rooms/schema/TransformState";
import { AINavigation } from "@rooms/systems/aiNavigation";
import { createBehaviourTreeInstance } from "src/behaviour_tree/behaviourTreeFactory";
import { BehaviourNode } from "src/types/behaviour_tree/behaviourTree";
import { BehaviourContext, IBlackboardSchema } from "src/types/behaviour_tree/behaviourTreeBlackboard";
import { BehaviourTreeDefinition } from "src/types/behaviour_tree/behaviourTreeDefinition";
import { PriorityTargets } from "src/types/detection";
import { VectorUtils } from "src/utils";

const DESIRED_CREATE_DISTANCE = 20.0;
const GIVE_UP_PATIENCE_TIME = 15.0; // seconds

export class AIDroneBrain {
  private parentDrone: AIDroneState;
  private aiNavigation: AINavigation;

  private behaviourTree: BehaviourNode<AIDroneBlackboard>;
  private context: BehaviourContext<AIDroneBlackboard>;

  constructor(parentDrone: AIDroneState, aiNavigation: AINavigation) {
    this.parentDrone = parentDrone;
    this.aiNavigation = aiNavigation;

    // Initialize the behaviour tree and context using the factory function
    const { tree, context } = createBehaviourTreeInstance(droneBehaviourTree);
    this.behaviourTree = tree;
    this.context = context;

    // Set initial blackboard values
    this.context.blackboard.set("aiDroneState", this.parentDrone);
    this.context.blackboard.set("aiNavigation", this.aiNavigation);
    this.context.blackboard.set("fleeHealthThreshold", this.parentDrone.getTraits().fleeHealthThreshold);
  }

  public update(deltaTime: number): void {
    this.context.deltaTime = deltaTime;
    this.behaviourTree.tick(this.context);
    this.aiNavigation.update(deltaTime);
  }

  public reset(): void {
    this.behaviourTree.reset();
  }

  public updateDetectionState(priorityTargets: PriorityTargets): void {
    // If current target is no longer valid, clear it
    const currentTarget = this.context.blackboard.get("currentTarget");
    if (currentTarget && currentTarget.toDespawn) {
      this.context.blackboard.set("currentTarget", null);
    }

    // Update blackboard with detection results
    this.context.blackboard.set("bestDrone", priorityTargets.bestDrone);
    this.context.blackboard.set("bestArenaObject", priorityTargets.bestArenaObject);
    this.context.blackboard.set("highestLevelDrone", priorityTargets.highestLevelDrone);

    // Update health percentage for flee conditions
    this.context.blackboard.set("healthPercent", this.parentDrone.health / this.parentDrone.maxHealth);
  }
}

//#region Behaviour Tree Definition

interface AIDroneBlackboard extends IBlackboardSchema {
  aiDroneState: AIDroneState | null;
  aiNavigation: AINavigation | null;
  healthPercent: number;
  fleeHealthThreshold: number;
  currentTarget: TransformState | null;
  giveUpTimer: number;
  highestLevelDrone: DroneState | null;
  bestDrone: DroneState | null;
  bestArenaObject: TransformState | null;
}

const droneBehaviourTree: BehaviourTreeDefinition<AIDroneBlackboard> = {
  name: "AIDroneBrain",
  root: {
    type: "prioritySelector",
    name: "Root Selector",
    children: [
      // [20] Flee Sequence
      {
        type: "sequence",
        name: "Flee Sequence",
        config: {
          processMultiple: true,
          priority: 20,
        },
        children: [
          {
            type: "condition",
            name: "Should Flee?",
            config: { condition: "shouldFlee" },
          },
          {
            type: "action",
            name: "Update Drone Target",
            config: { action: "updateFleeTarget" },
          },
          {
            type: "parallel",
            name: "Flee Actions",
            config: { policy: "successOnAll" },
            children: [
              {
                type: "action",
                name: "Set Attack Target",
                config: { action: "setAttackTarget" },
              },
              {
                type: "action",
                name: "Flee",
                config: { action: "flee" },
              },
            ],
          },
        ],
      },

      // [10] Target Selector
      {
        type: "prioritySelector",
        name: "Target Selector",
        config: { priority: 10 },
        children: [
          // [10-10] Give Up Sequence
          {
            type: "sequence",
            name: "Give Up Sequence",
            config: { processMultiple: true, priority: 10 },
            children: [
              {
                type: "action",
                name: "Tick Give Up Timer",
                config: { action: "tickGiveUpTimer" },
              },
              {
                type: "condition",
                name: "Should Give Up?",
                config: { condition: "shouldGiveUp" },
              },
              {
                type: "action",
                name: "Create Distance",
                config: { action: "createDistance" },
              },
              {
                type: "action",
                name: "Clear Target",
                config: { action: "clearTarget" },
              },
            ],
          },

          // [10-0] Pursue Selector
          {
            type: "prioritySelector",
            name: "Pursue Selector",
            children: [
              // [10-0-10] Pursue Drone
              {
                type: "sequence",
                name: "Pursue Drone Sequence",
                config: { processMultiple: true, priority: 10 },
                children: [
                  {
                    type: "condition",
                    name: "Has Drone in Range?",
                    config: { condition: "hasDroneInRange" },
                  },
                  {
                    type: "action",
                    name: "Update Pursue Target to Drone",
                    config: { action: "updateDroneTarget" },
                  },
                  {
                    type: "parallel",
                    name: "Pursue Drone Actions",
                    config: { policy: "successOnAll" },
                    children: [
                      {
                        type: "action",
                        name: "Set Attack Target Drone",
                        config: { action: "setAttackTarget" },
                      },
                      {
                        type: "action",
                        name: "Pursue Drone",
                        config: { action: "pursue" },
                      },
                    ],
                  },
                ],
              },

              // [10-0-0] Pursue Object
              {
                type: "sequence",
                name: "Pursue Object Sequence",
                config: { processMultiple: true },
                children: [
                  {
                    type: "condition",
                    name: "Has Object in Range?",
                    config: { condition: "hasObjectInRange" },
                  },
                  {
                    type: "action",
                    name: "Update Pursue Target to Object",
                    config: { action: "updateObjectTarget" },
                  },
                  {
                    type: "parallel",
                    name: "Pursue Object Actions",
                    config: { policy: "successOnAll" },
                    children: [
                      {
                        type: "action",
                        name: "Set Attack Target Object",
                        config: { action: "setAttackTarget" },
                      },
                      {
                        type: "action",
                        name: "Pursue Object",
                        config: { action: "pursue" },
                      },
                    ],
                  },
                ],
              },
            ],
          },
        ],
      },

      // [0] Wander (default)
      {
        type: "action",
        name: "Wander",
        config: { action: "wander", priority: 0 },
      },
    ],
  },
  actions: {
    updateFleeTarget: (context) => {
      const highestLevelDrone = context.blackboard.get("highestLevelDrone");
      if (!highestLevelDrone) {
        // No "highest level drone" in detection range (should not be in this node)
        console.error("AI Drone Brain: No highest level drone found in updateFleeTarget action. Should not happen if condition checks are correct.");
        return "FAILURE";
      }

      context.blackboard.set("currentTarget", highestLevelDrone);
      context.blackboard.set("giveUpTimer", 0); // Reset give up timer as new target acquired
      return "SUCCESS";
    },

    updateDroneTarget: (context) => {
      const bestDrone = context.blackboard.get("bestDrone");
      if (!bestDrone) {
        // No "best drone" in detection range (should not be in this node)
        console.error("AI Drone Brain: No best drone found in updateDroneTarget action. Should not happen if condition checks are correct.");
        return "FAILURE";
      }

      context.blackboard.set("currentTarget", bestDrone);
      context.blackboard.set("giveUpTimer", 0); // Reset give up timer as new target acquired
      return "SUCCESS";
    },

    updateObjectTarget: (context) => {
      const bestObject = context.blackboard.get("bestArenaObject");
      if (!bestObject) {
        // No "best object" in detection range (should not be in this node)
        console.error("AI Drone Brain: No best object found in updateObjectTarget action. Should not happen if condition checks are correct.");
        return "FAILURE";
      }

      context.blackboard.set("currentTarget", bestObject);
      context.blackboard.set("giveUpTimer", 0); // Reset give up timer as new target acquired
      return "SUCCESS";
    },

    setAttackTarget: (context) => {
      const target = context.blackboard.get("currentTarget");
      if (!target) return "FAILURE";
      const aiDroneState = context.blackboard.get("aiDroneState");
      if (!aiDroneState) {
        console.error("AI Drone Brain: No AI Drone State found in setAttackTarget action.");
        return "FAILURE";
      }
      aiDroneState.setAimTarget(target);
      return "SUCCESS";
    },

    tickGiveUpTimer: (context) => {
      const giveUpTimer = context.blackboard.get("giveUpTimer");
      const deltaTime = context.deltaTime;
      // TODO: More sophisticated patience based on damage dealt within time span
      context.blackboard.set("giveUpTimer", giveUpTimer + deltaTime);
      return "SUCCESS";
    },

    clearTarget: (context) => {
      context.blackboard.set("currentTarget", null);
      return "SUCCESS";
    },

    flee: (context) => {
      const highestLevelDrone = context.blackboard.get("highestLevelDrone");
      if (!highestLevelDrone) return "SUCCESS"; // No target to flee from, count as success from fleeing

      const aiNavigation = context.blackboard.get("aiNavigation");
      if (!aiNavigation) {
        console.error("AI Drone Brain: No AI Navigation system found in flee action.");
        return "FAILURE";
      }
      aiNavigation.fleeTarget(highestLevelDrone);
      return "RUNNING";
    },

    pursue: (context) => {
      const currentTarget = context.blackboard.get("currentTarget");
      if (!currentTarget) return "FAILURE"; // No target to pursue

      const aiNavigation = context.blackboard.get("aiNavigation");
      if (!aiNavigation) {
        console.error("AI Drone Brain: No AI Navigation system found in pursue action.");
        return "FAILURE";
      }

      aiNavigation.followTarget(currentTarget);
      return "RUNNING";
    },

    createDistance: (context) => {
      const aiDroneState = context.blackboard.get("aiDroneState");
      const targetState = context.blackboard.get("currentTarget");
      if (!aiDroneState || !targetState) return "FAILURE"; // Cannot create distance without states

      const distanceToTarget = VectorUtils.magnitude({
        x: targetState.posX - aiDroneState.posX,
        y: targetState.posY - aiDroneState.posY,
      });
      if (distanceToTarget >= DESIRED_CREATE_DISTANCE) return "SUCCESS"; // Already at desired distance

      const aiNavigation = context.blackboard.get("aiNavigation");
      if (!aiNavigation) {
        console.error("AI Drone Brain: No AI Navigation system found in createDistance action.");
        return "FAILURE";
      }
      aiNavigation.fleeTarget(targetState);

      return "RUNNING";
    },

    wander: (context) => {
      const aiNavigation = context.blackboard.get("aiNavigation");
      if (!aiNavigation) {
        console.error("AI Drone Brain: No AI Navigation system found in wander action.");
        return "FAILURE";
      }

      aiNavigation.wander();
      return "RUNNING";
    },
  },
  conditions: {
    shouldFlee: (context) => {
      const health = context.blackboard.get("healthPercent");
      const fleeThreshold = context.blackboard.get("fleeHealthThreshold");
      return health < fleeThreshold;
    },

    shouldGiveUp: (context) => {
      const giveUpTimer = context.blackboard.get("giveUpTimer");
      return giveUpTimer >= GIVE_UP_PATIENCE_TIME;
    },

    hasDroneInRange: (context) => {
      return context.blackboard.get("bestDrone") !== null;
    },

    hasObjectInRange: (context) => {
      return context.blackboard.get("bestArenaObject") !== null;
    },
  },
  initialBlackboard: {
    aiDroneState: null,
    aiNavigation: null,
    healthPercent: 1.0,
    fleeHealthThreshold: 0.3,
    currentTarget: null,
    giveUpTimer: 0,
    highestLevelDrone: null,
    bestDrone: null,
    bestArenaObject: null,
  },
};

//#endregion
