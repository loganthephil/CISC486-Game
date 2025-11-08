# Drone Strikers
## Assignment 3
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

Not shown in video (due to video length constraints):
- AI Drones will wander when no targetable objects are detected. Due to the high density of objects in the arena, this wandering behaviour is not visible very often.
- If an AI Drone does not make significant progress in destroying its target within a certain time limit, it will "give up", flee for a short period of time, then return to normal behaviour.

### Decision Tree Diagram for Reference
![Decision Tree for AI Drones](https://github.com/user-attachments/assets/c34527fb-6003-4495-8598-387857a0998e)

### Instructions to Run (if needed)
1. Download the repository as a ZIP and extract it
2. Navigate to the extracted directory: `/Drone Strikers/Builds/Dev`
3. Run `Drone Strikers.exe`

# Team
Group 26\
Logan Philip - 20294350

# AI Citations
I used ChatGPT (GPT-5) with the prompt "What is a good approach for adding dynamic stat upgrades where values are enumerated in the following order: 1. Base 2. Add flat increases 3. Multiply by sum of additive multiplication increases. 4. Multiply by remaining multiplicative multiply stat increases." I asked this to get a better feel for the best approach to this feature's implementation.

I used ChatGPT (GPT-5) with the prompt "Is there a way to create a Unity component that acts as a hub for local events relevant to that game object? For example, within a single game object, other components may want to know when the object is damaged, destroyed, etc.". I asked this because I was curious if there was an in-code way to invoke/subscribe to events around a single game object's components. Doing so led me to writing the LocalEvents component. I may replace with simply using UnityEvents later if I don't get any unique value from this system.

I used ChatGPT (GPT-5) with the prompt "I need to modify the AI drone movement so it navigates around obstacles.  The idea I'm having is that it would do something like see movement vectors relative to its own movement vector of the surrounding objects (or maybe just the closest 1 or few). If the relative movement vector has a trajectory that would make it hit the drone, then move out of the way. This would, theoretically work for when the drone is moving or stationary, for moving out of the way of other stationary objects or moving objects." I asked this since I was quite clueless on where to even begin with implementing this feature. Since then, I have implemented the AINavigation component and have come to understand it very well.

All final code implementation, logic, and documentation for my project were written entirely by me.