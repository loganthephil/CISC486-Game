## Proposal
**Title (Working)**\
Drone Strikers

**Core Gameplay**\
Players are in control of a “drone” and will be placed in one of two teams. Players must gather “experience” by destroying objects, AI entities, or other players (not on their team). Upon leveling up players will be presented with the ability to enhance their drone in various ways to prolong their survival. The goal is to be the strongest drone in the lobby and achieve “victory” for their team.

**Game Type**\
Arena Battle - Third-person, top down

**Player Setup**\
I will be aiming to support at least 10 players, and depending on how it goes, even more. It will be online multiplayer.

**AI Enemies**\
The most prevalent AI enemies will be AI-controlled drones that fill the role of additional players. These AI-controlled “player” drones (or AI players) will follow the same tasks as normal players would. There may also be some simpler AI-controlled drones that take the role of fodder for players or AI players to gain experience. Both AI types will use a finite state machine for their behaviour. AI players will need to make decisions that emulate how a player might play the game. The other basic AI drones will just attempt to damage the closest player or AI player. When no players are nearby, these basic AI drones will stay still.

**Scripted Events**\
At the very least, arena objects that are destroyed by players will need to respawn. This will likely happen after a certain amount of time and based on object saturation in a given area. Perhaps this will be implemented by running an automatic periodic check on a given location of the arena and if the object saturation is not too high, spawn another object. 

**Environment**\
The environment will be a very simple arena that is mostly devoid of colour (white, maybe with a grid). There may be changes in elevation if it proves to be a good addition. The drones and other objects will contrast with the sterile environment for ease of viewing. The environment simply facilitates the gameplay.

**Assets**\
The assets will be quite simple as well. I will search online for models that may work but I mostly anticipate that I will need to create them from scratch in Blender (or similar). Audio (if necessary) will likely come from Freesound.org.

**Team**\
Group 26\
Logan Philip 20294350\
As I am the only member I will handle everything.

**AI Citations**/
I used ChatGPT (GPT-5) with the prompt "What is a good approach for adding dynamic stat upgrades where values are enumerated in the following order: 1. Base 2. Add flat increases 3. Multiply by sum of additive multiplication increases. 4. Multiply by remaining multiplicative multiply stat increases." I asked this to get a better feel for the best approach to this feature's implementation.

All final code implementation, logic, and documentation for my project were written entirely by me.