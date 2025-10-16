# Drone Strikers
## Assignment 2
Gameplay Video: https://youtu.be/5UUWPTiwFzw?si=hHfU_ZJizZt1yoef \
Game executable, if needed, is in under `/Drone Strikers/Build`

![State Diagram for AI Drones](https://github.com/user-attachments/assets/fe13f113-b042-439d-9f80-c1bcf7536565)

### States
**Wander**: The drone will move in a random direction for a short period of time, before choosing a new direction.\
**Pursue**: The drone will move towards the highest priority targetable object (1. Highest level Drone in range; 2. Closest other targetable object).\
**Flee**: The drone will move away from the highest priority threat (the highest level Drone in detection range).

### Transitions
**Wander -> Pursue**: If any targetable object is detected within detection range and no higher level Drones are detected.\
**Pursue -> Wander**: If no targetable objects are detected within detection range.\
**Wander/Pursue -> Flee**: If a higher level Drone is detected within detection range.\
**Flee -> Wander**: If no higher level Drones are detected within detection range.

### Terms
**Targetable Object**: Any object that can be destroyed for experience. This includes other drones (AI or Player) and arena objects like Cylinder, Cubes, etc.\
**Detected Object**: Detected objects are object within a certain range of the AI drone (colliding with a special trigger) and are **not** on the same team as the AI drone.

## Proposal (Assignment 1)
### Title (Working)
Drone Strikers

### Core Gameplay
Players are in control of a “drone” and will be placed in one of two teams. Players must gather “experience” by destroying objects, AI entities, or other players (not on their team). Upon leveling up players will be presented with the ability to enhance their drone in various ways to prolong their survival. The goal is to be the strongest drone in the lobby and achieve “victory” for their team.

### Game Type
Arena Battle - Third-person, top down

### Player Setup
I will be aiming to support at least 10 players, and depending on how it goes, even more. It will be online multiplayer.

### AI Enemies
The most prevalent AI enemies will be AI-controlled drones that fill the role of additional players. These AI-controlled “player” drones (or AI players) will follow the same tasks as normal players would. There may also be some simpler AI-controlled drones that take the role of fodder for players or AI players to gain experience. Both AI types will use a finite state machine for their behaviour. AI players will need to make decisions that emulate how a player might play the game. The other basic AI drones will just attempt to damage the closest player or AI player. When no players are nearby, these basic AI drones will stay still.

### Scripted Events
At the very least, arena objects that are destroyed by players will need to respawn. This will likely happen after a certain amount of time and based on object saturation in a given area. Perhaps this will be implemented by running an automatic periodic check on a given location of the arena and if the object saturation is not too high, spawn another object. 

### Environment
The environment will be a very simple arena that is mostly devoid of colour (white, maybe with a grid). There may be changes in elevation if it proves to be a good addition. The drones and other objects will contrast with the sterile environment for ease of viewing. The environment simply facilitates the gameplay.

### Assets
The assets will be quite simple as well. I will search online for models that may work but I mostly anticipate that I will need to create them from scratch in Blender (or similar). Audio (if necessary) will likely come from Freesound.org.

### Team
Group 26\
Logan Philip 20294350\
As I am the only member I will handle everything.

# AI Citations
I used ChatGPT (GPT-5) with the prompt "What is a good approach for adding dynamic stat upgrades where values are enumerated in the following order: 1. Base 2. Add flat increases 3. Multiply by sum of additive multiplication increases. 4. Multiply by remaining multiplicative multiply stat increases." I asked this to get a better feel for the best approach to this feature's implementation.

I used ChatGPT (GPT-5) with the prompt "Is there a way to create a Unity component that acts as a hub for local events relevant to that game object? For example, within a single game object, other components may want to know when the object is damaged, destroyed, etc.". I asked this because I was curious if there was an in-code way to invoke/subscribe to events around a single game object's components. Doing so led me to writing the LocalEvents component. I may replace with simply using UnityEvents later if I don't get any unique value from this system.

I used ChatGPT (GPT-5) with the prompt "I need to modify the AI drone movement so it navigates around obstacles.  The idea I'm having is that it would do something like see movement vectors relative to its own movement vector of the surrounding objects (or maybe just the closest 1 or few). If the relative movement vector has a trajectory that would make it hit the drone, then move out of the way. This would, theoretically work for when the drone is moving or stationary, for moving out of the way of other stationary objects or moving objects." I asked this since I was quite clueless on where to even begin with implementing this feature. Since then, I have implemented the AINavigation component and have come to understand it very well.

All final code implementation, logic, and documentation for my project were written entirely by me.