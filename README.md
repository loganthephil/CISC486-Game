# Drone Strikers

## Assignment 4

Gameplay Video: https://youtu.be/5pVAIBB5B04

To play the game, head over to https://knonix.itch.io/drone-strikers and use the password `CISC486`.\
The game's server backend uses a free Render.com server which goes to "sleep" after inactivity, so it may take a minute for the game to connect to a server.

Unfortunately I wasn't able to get around to implementing client-side prediction for the player drone like I was hoping, which means that when playing the game on the internet-hosted server, the gameplay can be a bit "slow". However, the game performs well when running on a local network, which is what is being used in the demonstration video.

### Controls:

Move - WASD\
Shoot - Left Mouse\
Auto Shoot - E

Destroy objects to get XP. Select and upgrade on the left when you get enough XP. Try to be #1 in the lobby!\
This game is modelled after .io games like agar.io that don't have a clear win state but instead have a leaderboard where players try to be the best in the lobby.

# Team

Group 26\
Logan Philip - 20294350

# AI Citations

I used ChatGPT (GPT-5) with the prompt "What is a good approach for adding dynamic stat upgrades where values are enumerated in the following order: 1. Base 2. Add flat increases 3. Multiply by sum of additive multiplication increases. 4. Multiply by remaining multiplicative multiply stat increases." I asked this to get a better feel for the best approach to this feature's implementation.

I used ChatGPT (GPT-5) with the prompt "Is there a way to create a Unity component that acts as a hub for local events relevant to that game object? For example, within a single game object, other components may want to know when the object is damaged, destroyed, etc.". I asked this because I was curious if there was an in-code way to invoke/subscribe to events around a single game object's components. Doing so led me to writing the LocalEvents component. I may replace with simply using UnityEvents later if I don't get any unique value from this system.

I used ChatGPT (GPT-5) with the prompt "I need to modify the AI drone movement so it navigates around obstacles. The idea I'm having is that it would do something like see movement vectors relative to its own movement vector of the surrounding objects (or maybe just the closest 1 or few). If the relative movement vector has a trajectory that would make it hit the drone, then move out of the way. This would, theoretically work for when the drone is moving or stationary, for moving out of the way of other stationary objects or moving objects." I asked this since I was quite clueless on where to even begin with implementing this feature. Since then, I have implemented the AINavigation component and have come to understand it very well.

All final code implementation, logic, and documentation for my project were written entirely by me.
