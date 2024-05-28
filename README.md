# Intuitive Physics in VR Environment
This repository contains the code and resources for the Data Science Capstone project supervised by Dr. Xiao, which focuses on exploring intuitive physics in a virtual reality (VR) environment. It builds upon the previous project [SummerResearch2021](https://github.com/JesseSchwartz25/SummerResearch2021) and introduces a new variable, RotationEvent.

# Overview
The project aims to investigate how humans perceive and reason about the stability of towers in a simulated VR setting. Participants are placed in a VR/Haptic environment (Pimax 5k HMD and 3DS TouchX Haptic Device) and asked to view and interact with towers of varying stability. By leveraging immersive VR technology and haptic feedback, we can gather data on human intuitions and decision-making processes related to assessing the stability of physical structures.

# Experiment Protocol
1. Participants are presented with a tower in the VR environment and can freely rotate and touch the tower blocks using the TouchX haptic device.
2. After observing the tower, participants are asked to:
   - Judge the stability of the tower on a scale of 1-7, with 1 being the least stable and 7 being the most stable.
   - Assess which zone(s) the tower will fall into. There are five zones, one in each cardinal direction and a stable zone. Participants can pick two zones but are required to pick at least one.
   - Ensure they have the best viewing angle before submitting the answer.
4. After submitting the answer,  the tower will reset to the initial view, and participants watch the outcome of whether the tower falls or remains standing.
Participants assess 50 towers, each repeated 2 times.

# Image-based Prediction Modeling
In addition to studying human intuitions, this project also aims to develop image-based models to predict whether a tower will fall or remain standing, based solely on image data of the constructed towers. The dataset consists of images of different viewing angles of 50 towers, totaling 164 unstable and 144 stable tower images. Pre-trained models such as ResNet-50 and InceptionV3 are employed for this task.

# Features

* VR Environment: A fully interactive and immersive VR environment built using Unity or other game engines, allowing participants to experience and interact with virtual towers and scenarios.
* Physics Simulations: Realistic physics simulations implemented within the VR environment, enabling the study of tower stability, motion, forces, and collisions.
* Haptic Feedback: Integration with the 3DS TouchX Haptic Device, allowing participants to touch and feel the tower blocks, enhancing the immersive experience.
* Data Collection: Mechanisms to capture user interactions, decisions, and responses within the VR environment, facilitating the analysis of intuitive physics understanding.
* Image-based Modeling: Implementation of pre-trained deep learning models like ResNet-50 and InceptionV3 for predicting tower stability based on image data.
* Model Explainability: Techniques like Grad-CAM (Gradient-weighted Class Activation Mapping) to generate heatmaps highlighting the regions in the input image that contribute most to the model's prediction.

# Getting Started

To get started with this project, follow these steps:

## Prerequisites

- Unity 2020.3.8f1 (Newer versions may work but are not guaranteed)
- Pimax 5k HMD
- 3DS TouchX Haptic Device

## Installation

1. Clone the repository.
2. Open the project in Unity 2020.3.8f1.
3. Set up the Pimax HMD.
4. Set up the 3DS TouchX Haptic Device.

## Running the Experiment

1. Start the Unity project and navigate to the `TestEnvironment` scene.
2. Ensure that the Pimax HMD and TouchX device are properly connected and calibrated.
3. Press the Play button in Unity to start the experiment.
4. Follow the on-screen instructions to complete the experiment trials.

## Data Collection

After each participant completes the experiment, the data will be saved in a text file named `[participant_name]_Data.txt` within the `IntPhysTowersExperimentData` folder on the desktop.

In the experiment, we saved:

- Zone1
  - First zone selected by the user, 0-5.
- Zone2
  - Second zone selected by the user, 0-5, may be -1 if not selected
- Stability
  - Stability score selected by the user, 1-7
- Time
  - How long since the tower was spawned until an answer was submitted, in seconds
- Avg Pos
  - Mean position of the center of all blocks after the tower has fallen, in vector3 [x,y,z]
- Furthest
  - Which block moved the furthest in the xz plane from the [0,0] in vector3 [x,y,z]
- CenterOfMass
  - Mean position of the center of all blocks after the tower has fallen, accounting for mass. In iteration 1 this is the same as Avg position because all blocks have the same mass, in vector3 [x,y,z]
- Y Rotation
  - Final Y rotation seen by the user before hitting submit, used Euler y.
- Majority
  - Which zones the middle/unity origin of the blocks were in, array form: [0,2,0,4,0], for example, where each index indicates how many blocks were in that zone.
  - This data is often later manipulated because a block can 'fall' but still be counted in zone 0.
- Start Pos
  - The unity origin for all blocks before falling, in vector3[] [x,y,z]
- Final Pos
  - The unity origin for all blocks after falling, in vector3[] [x,y,z]
- RotationEvent
  - Record rotation of the tower whenever participants rotate it, and take a screenshot of the last rotation before submission. 


# Acknowledgments
Thanks to Dr. Bei Xiao and her lab team members, Jesse Schwartz, Michael Reinisch, and Chenxi Liao, for their valuable contributions and insights throughout this project.
