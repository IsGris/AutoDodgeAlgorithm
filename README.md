# AutoDodgeAlgorithm

**AutoDodgeAlgorithm** is a simple and dynamic algorithm designed to create a bot that automatically dodges bullets in 2D top-down perspective games.

---

## Description

This algorithm divides the playfield into a grid of small rectangular cells and assigns a "weight" to each cell representing how dangerous it is for the player to be in that cell.

The weight is calculated based on several factors:
- Distance to the desired player position.
- Distance to the current player position.
- Presence and predicted path of projectiles.

Projectiles add danger weights not only at their current positions but also at future positions based on their speed, direction, and size.

Each frame, the algorithm selects the safest area — a group of cells where the player can fit — and smoothly moves the player to the center of that area.

**All algorithm settings can be adjusted in the GameObject named `AutoDodgeAlgorithm`.**  
**This algorithm was developed using Unity version 6000.1.5f1.**

You can watch a demo of the algorithm in action here:  
🔗 https://youtu.be/BBk1qUSVQ5g

And here is a result of 15 minutes of continuous algorithm work (screenshot):  
🖼️ https://ibb.co/cVgtGtQ

---

## Key Features

- Supports varying player sizes.
- Predicts future projectile positions for timely dodging.
- Can visualize weight zones in real-time using Gizmos (Unity).

---

## Important Note

This algorithm is a simple starting point inspired by the A* approach. It is not claimed to be the best solution but serves as a solid base for improvements and customizations. If you have ideas on how to make it better, I’m open to suggestions!
