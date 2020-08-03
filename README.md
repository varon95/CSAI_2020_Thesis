# Simulation to Real World Knowledge Transfer
In this project, I investigated simulation to real world knowledge transfer. I trained an agent to drive a car around a track in a simulated environment using reinforcement learning. Then, to improve the agent, I tested how different inputs, rewards, and outputs influence training. Finally, the agent was used to control an RC car in the real world (this car was connected to my computer with an Arduino board running C++ code).

## Steps
- A car simulation was created in Unity.
- An agent was trained on the simulation with reinforcement learning using the ML-Agents toolkit.
- To be able to "see" the real world car, a camera was placed above the track.
- The car was tracked using a modified version of the script from my previous project.
- The remote was connected to the computer through an Arduino microcontroller.

## Demo (Video)
[![Sim to real](http://img.youtube.com/vi/6BAuK-sfows/0.jpg)](http://www.youtube.com/watch?v=6BAuK-sfows "")

## Results
The agent was able to control the real-world car, after training only on a simulation.

**You can read more about the project in my [master's thesis](https://github.com/varon95/CSAI_2020_Thesis/blob/master/AZVaradi_Thesis_CSAI_2020.pdf)**
