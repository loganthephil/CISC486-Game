# Drone Strikers
### Note on Pathfinding Implementation
"Traditional" pathfinding like A* or NavMesh did not make sense for my game, so instead I elected to create a steering algorithm that "path finds" throughout the arena dynamically. 
There are no static objects in my game except for the walls that surround the entire map. The positions of objects, enemy Drones, and projectiles are always changing. 
Due to this, I made a steering algorithm where the AI Drone will scan for nearby objects and predict if it will collide with said object if the AI continues moving in the same direction (including when stationary). 
This creates a system where the AI will attempt to dodge objects and projectiles in its path. 
This can be seen most evidently (but not exclusively) when the AI is pursuing another Drone, where, depending on the simulated "skill-level" of the AI Drone (set on spawn), 
it will stay away from any dangerous objects in the path towards its target. 
The AI always uses the same steering algorithm since there is never a point where the AI would want to stop dodging objects that would damage it, 
however, depending on factors like the AI Drone's current level (not to be confused with skill-level), some threats may become less important to dodge than others, 
having less of an effect on the steering avoidance vector.

### Descision-Making and Steering Demonstration
Gameplay Video: https://www.youtube.com/watch?v=66NdgFn9gKA

Not shown in video:
- AI Drones will wander when no targetable objects are detected. Due to the high density of objects in the arena, this wandering behaviour is not visible very often.
- If an AI Drone does not make significant progress in destroying its target within a certain time limit, it will "give up", flee for a short period of time, then return to normal behaviour.

### Decision Tree Diagram for Reference
![Decision Tree for AI Drones](https://github.com/user-attachments/assets/c34527fb-6003-4495-8598-387857a0998e)

### Instructions to Run
Go to https://knonix.itch.io/drone-strikers

OR
1. Download the repository as a ZIP and extract it
2. Navigate to the extracted directory: `/Drone Strikers/Builds/Dev`
3. Run `Drone Strikers.exe`
