import { createBehaviourTreeInstance } from "src/behaviour_tree/behaviourTreeFactory";
import { BehaviourNode } from "src/types/behaviour_tree/behaviourTree";
import { BehaviourContext, IBlackboardSchema } from "src/types/behaviour_tree/behaviourTreeBlackboard";
import { BehaviourTreeDefinition } from "src/types/behaviour_tree/behaviourTreeDefinition";

export class AIDroneBrain {
  private behaviourTree: BehaviourNode<AIDroneBlackboard>;
  private context: BehaviourContext<AIDroneBlackboard>;

  constructor() {
    // Initialize the behaviour tree and context using the factory function
    const { tree, context } = createBehaviourTreeInstance(droneBehaviourTree);
    this.behaviourTree = tree;
    this.context = context;
  }

  public update(deltaTime: number): void {
    this.context.deltaTime = deltaTime;
    this.behaviourTree.tick(this.context);
  }

  public reset(): void {
    this.behaviourTree.reset();
  }
}

//#region Behaviour Tree Definition

interface AIDroneBlackboard extends IBlackboardSchema {
  healthPercent: number;
  fleeHealthThreshold: number;
  currentTarget: any;
  giveUpTimer: number;
  hasDroneInRange: boolean;
  hasObjectInRange: boolean;
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
            config: { action: "updateDroneTarget" },
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
    updateDroneTarget: (context) => {
      // Type-safe access to blackboard
      const currentTarget = context.blackboard.get("currentTarget");
      // Implementation would update target based on detected drones
      return "SUCCESS";
    },

    updateObjectTarget: (context) => {
      // Implementation would update target based on detected objects
      return "SUCCESS";
    },

    setAttackTarget: (context) => {
      const target = context.blackboard.get("currentTarget");
      if (!target) return "FAILURE";
      // Implementation would set target in target provider
      return "SUCCESS";
    },

    flee: (context) => {
      // Implementation for fleeing behavior
      return "RUNNING";
    },

    tickGiveUpTimer: (context) => {
      const giveUpTimer = context.blackboard.get("giveUpTimer") || 0;
      const deltaTime = context.deltaTime;
      // Implementation would update give up timer based on conditions
      context.blackboard.set("giveUpTimer", giveUpTimer + deltaTime);
      return "SUCCESS";
    },

    createDistance: (context) => {
      // Implementation for creating distance from target
      return "RUNNING";
    },

    clearTarget: (context) => {
      context.blackboard.set("currentTarget", null);
      return "SUCCESS";
    },

    pursue: (context) => {
      // Implementation for pursuing target
      return "RUNNING";
    },

    wander: (context) => {
      // Implementation for wandering behavior
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
      const GIVE_UP_PATIENCE_TIME = 15; // seconds
      return giveUpTimer >= GIVE_UP_PATIENCE_TIME;
    },

    hasDroneInRange: (context) => {
      return context.blackboard.get("hasDroneInRange") || false;
    },

    hasObjectInRange: (context) => {
      return context.blackboard.get("hasObjectInRange") || false;
    },
  },
  initialBlackboard: {
    healthPercent: 1.0,
    fleeHealthThreshold: 0.3,
    currentTarget: null,
    giveUpTimer: 0,
    hasDroneInRange: false,
    hasObjectInRange: false,
  },
};

//#endregion Behaviour Tree Definition
