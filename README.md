# DalekSteps

This is an application of Unity ml-agents Reinforcement Learning to help a Dalek navigate an alternative to using Stairs.

The Dalek has to find a way to reach the Tardis, which is on another ground level. Daleks cannot climb steps ! 
Note there is no pursuit of the Tardis pre programmed into the Dalek agent. It will only discover that this is a positive reward objective upon a successful encounter with the Tardis object. So by a lot of random reinforcement learning, a lot of failures (Falling off the platform, losing the ramp object etc) will it discover its the objective to reach the Tardis, and how to achieve it.

After around 10 Million Training Steps the Dalek learns to move the ramp, to get to the higher ground and approach the Tardis. 

![ScreenShot](MAIN.PNG)

## Overview    ##
This is a Unity ML-Agents project, based upon Ml-Agents Release 18  (Python Package 0.27.0 and Unity Package 2.1.0). The Unity package is included here. The Trained Brain is configured into the project, so the Unity Scene can be already be played through with the Trained Brain, without the need to perform python Training. 

Please see the /Assets/Prefab Folder, for the DalekArena. This Prefab has the design for a Dalek Arena, including the Dalek, Agent, with the Agent Scripts etc. The Ramp Rigid body moveable GameObject, and the fixed Wall, Ground and Tardis GameObjects. Note that this DalekArena Prefab is replicated 24 Times, in the Scene to speed up Tra8ining perfomance.  

All chnages and modifications to the Dalek Agent should be made within the context of the Dalek within the DalekArena Prefab.
 
![ScreenShot](Design.PNG)

The Dalek Agent is configured with the following Obervations:
  - Dalek Forward RayCasts, which can Detect Walls, The Ramp, Steps, Tardis (if on same level)
  - Dalek Vector 3 Position, local relative to the Wall  
  - Ramp Vector 3 Position local relative to the Wall
  - Ramp float Orientation (around Y axis) 
  - Delta Vector3 distance to Tardis 

The Dalek Movements are controlled with two Action Branches:
  - Rotation around Y Axis Discrete Action Branch  (0 : None, 1: Turn Left, 2: Turn Right) 
  - Move Forward Discrete Action Branch   (0: None, 1:Move Forward (Dalek local Z Axis))

### Reward Signals ###
The Agent and the Game Environment are all ahndled within the single /Assets/Scripts/DalekController.cs script.  This script controls the Dalek Behvaiors, Episode Management, Overrides the ML Agents Initialise(), CollectObservations() and OnActionReceived() and Heuristic() etc as well as assign the Reward signals.
There are only four Distinct Rewards
   - +5.0f Episode Success (Dalek Reaches the Tardis)
   - -5.0f Episode Failure (Dalek or the Ramp Falls off the Arena Platform)
   - -2.0f Excessive Episode Length( Episode steps exceeding 6000)
   - An accumlatative -0.00025f  upon every ActionReceived 
 
The Dalek can be manually controlled, if the Dalek Agent Behaviour is set to Heuristic, in the Agnet Behavior script. Left/Right Arrows to Rotate, and Space Bar to Move Forward.  
## Training ##
The Dalek Agent has been trained using the Unity-ML PPO + Curiosity algorithm. Please see the /config/DalekStairs.yaml  

To perform Training against the proposed /config/DalekStairs.yaml, with an appropriate pyhton mlagenst environment:  

(mlagents) $:> mlagents-learn config/DalekStairs.yaml  --run-id=DalekRunXX

![ScreenShot](Terminal2.PNG)

This will take a long time to learn a reasonable policy. Experince suggests around 12 Million Epochs.  


