# DalekSteps

This is an application of Unity ml-agents Reinforcement Learning to help a Dalek navigate as an alternative to the use of Stairs.

The Dalek has to find a way to reach the Tardis, which is on another ground level. Daleks cannot climb steps ! 

Note there is no pursuit of the Tardis pre programmed into the Dalek agent. It will only discover that this is a Positive reward objective upon a successful encounter with the Tardis object. So this is a very sparse and challenging Reward Environment. The Agent undegoes a lot of  a lot of failures (Falling off the platform, losing the ramp object etc), random movements before it discovers the positive objective to reach the Tardis, and hence on how best to achieve it, across different Dalek and Ramp random start positions and orientations.

After around 10 Million Training Steps the Dalek learns to move the ramp, and to get onto the higher ground and approach the Tardis reasonably well. There are however still some  stuck/failed scenarios the agnets policy cannot cope with. 

![ScreenShot](Main.PNG)

## Overview    ##
This is a Unity ML-Agents project, based upon Ml-Agents Release 18  (Python Package 0.27.0 and Unity Package 2.1.0). The Unity package is included here. The Trained Brain is configured into the project, so the Unity Scene can be already be played through with the Trained Brain, without the need to perform python Training. 

This is an introductory ml-agents projects, loosely based around experience of the Unity WallJump examples. There is a single Agent script, to understand and modify the Agent Behaviours. Hopefully you can find some improvements to the training time or the eventual perfomance. 

Please see the /Assets/Prefab Folder, for the DalekArena. This Prefab has the design for a Dalek Arena, including the Dalek, Agent, with the Agent Scripts etc. The Ramp Rigid body moveable GameObject, and the fixed Wall, Ground and Tardis GameObjects. Note that this DalekArena Prefab is replicated 24 Times, in the Scene to speed up Tra8ining performance.  

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

All changes and modifications to the Dalek Agent should be made within the context of the Dalek within the DalekArena Prefab. The Dalek Arena is replicated 24 times to improve training speed. 
![ScreenShot](MultipleEnvs.PNG)
 
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

This will take a long time to learn a reasonable policy. Experience suggests around 12 Million Epochs. 
The initial 4 Million or so steps are mostly negative episodes, with few if any discoveries of the Postive Reward. The Environment is very Sparse. However the cumulative grows to postive around 4 Million epochs, and then from 6 Million epochs the rewards gently grows. With a Curiosity ICM component the Agent is also rewarding itslef in exploring the state space, and so the agent needs to reduce its curiosity losses (Foward and Inverse networks). These show a steady decline over 12 Million epochs.

![ScreenShot](Learning.PNG)

## Observations and Performance ##
- The Training Time feels a little excessive. Despite Increasing teh Learning rates from the default 0.0003 to 0.00075, the Reward signals still grow very slowly in the final Training Phase.
- There are still some skip across the gaps from Ramp to Higher Ground. The Dalek Collision Box has been reduced, and could perhaps be reduced further. But that makes the environment even more challenging
- There are some stuck policies, The Dalek still sometiems moves he ramp to the Left, where it has no reasonbale access, or ability to move the ramp closer to the Wall. This is a failed scenario, and the Agent typically goes into a sulk, waits out the episiode by the repeated manoeuvres around the steps. 

So any suggestions or recommendations are welcome. 

Cheers

