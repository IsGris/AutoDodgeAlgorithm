/*
    AutoDodgeAlgorithm - Dynamic dodge system for avoiding projectiles in 2D space.

    How it works:
    - The algorithm divides the playfield into a grid of small rectangular cells.
    - It calculates a "weight" for each cell which represents how dangerous it is for the player to be there.
    - The weight is calculated based on:
        1. Distance to the desired player position.
        2. Distance to the current player position.
        3. Presence and predicted path of projectiles.
    - Projectiles add danger weights to their current positions and all future positions based on their speed, direction, and size.
    - Every frame, the algorithm finds the best (safest) group of cells where the player could fit.
    - The player is then smoothly moved to the center of that safest area.

    Key features:
    - Handles varying player sizes.
    - Predicts and accounts for future projectile positions.
    - Can visualize weights in real-time using Gizmos.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

// Enum to classify objects detected in the grid
public enum objectType
{
    player,
    projectile
}

public class AutoDodgeAlgorithm : MonoBehaviour
{
    public ProjectileSpawner spawner;
    public PlayerController player;
    public Transform desiredPlayerPoint;

    public LayerMask objectsLayer = ~0;

    [NonSerialized] public float oneRectUnits; // Size of a single grid cell in world units
    [NonSerialized] public float[,] weightMatrix; // Matrix representing weight (danger) per grid cell
    [NonSerialized] public List<Vector2Int> playerCoords; // Grid positions occupied by the player
    [NonSerialized] public Dictionary<GameObject, List<Vector2Int>> projectiles; // Grid positions occupied by projectiles

    [NonSerialized] public Vector2Int PlayerLocalRectsSize; // Size of the player in grid cells
    [NonSerialized] public Vector3 LeftBottom; // Bottom-left world position of the grid

    [Header("Algorithm variables")]
    public int rectsPerUnit = 4; // Number of grid cells per world unit
    public float distanceWeightMultiplier = 1.0f; // Weight multiplier for distance to desired point
    public float distancePlayerWeightMultiplier = 1.0f; // Weight multiplier for distance from current player position
    public float projectileWeight = 10.0f; // Base weight applied by projectiles
    public float projectileWeightDecrease = 20.0f; // Rate at which projectile weight decreases over time

    [Header("Cosmetic variables")]
    public bool DisplayWeightColor = true;
    public bool DisplayWeightText = false;

    private void Start()
    {
        // Initialize grid and sizes
        oneRectUnits = 1.0f / rectsPerUnit;
        weightMatrix = new float[spawner.width * rectsPerUnit, spawner.height * rectsPerUnit];
        playerCoords = new();
        projectiles = new Dictionary<GameObject, List<Vector2Int>>();
        LeftBottom = transform.position - new Vector3(spawner.width / 2, spawner.height / 2, 0);

        PlayerLocalRectsSize = new(
            Mathf.CeilToInt(player.gameObject.transform.lossyScale.x / oneRectUnits),
            Mathf.CeilToInt(player.gameObject.transform.lossyScale.y / oneRectUnits));
    }

    private void Update()
    {
        // Recalculate weights and choose best player position every frame
        InitializeObjectAndPlayerCoordMatrix();
        InitializePlayerDistanceWeight();
        InitializeProjectileWeight();

        // Make player go to best position
        player.targetPosition = FindBestPositionForPlayer();
    }

    public Vector2 FindBestPositionForPlayer()
    {
        float minTotalWeight = float.MaxValue;
        Vector2Int bestStartCoord = Vector2Int.zero;

        int matrixWidth = weightMatrix.GetLength(0);
        int matrixHeight = weightMatrix.GetLength(1);

        // Iterate over all valid player-sized subgrids to find lowest total weight
        for (int x = 0; x <= matrixWidth - PlayerLocalRectsSize.x; x++)
        {
            for (int y = 0; y <= matrixHeight - PlayerLocalRectsSize.y; y++)
            {
                float totalWeight = 0f;
                bool isValid = true;

                for (int dx = 0; dx < PlayerLocalRectsSize.x && isValid; dx++)
                {
                    for (int dy = 0; dy < PlayerLocalRectsSize.y; dy++)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;

                        if (!IsValidCoord(new Vector2Int(checkX, checkY)))
                        {
                            isValid = false;
                            break;
                        }

                        totalWeight += weightMatrix[checkX, checkY];
                    }
                }

                if (isValid && totalWeight < minTotalWeight)
                {
                    minTotalWeight = totalWeight;
                    bestStartCoord = new Vector2Int(x, y);
                }
            }
        }

        // Return center of the safest subgrid
        Vector2 centerCoord = new Vector2(
            bestStartCoord.x + PlayerLocalRectsSize.x / 2f,
            bestStartCoord.y + PlayerLocalRectsSize.y / 2f
        );

        return GridToWorld(Mathf.FloorToInt(centerCoord.x), Mathf.FloorToInt(centerCoord.y));
    }

    private void InitializeProjectileWeight()
    {
        foreach (var projectilePair in projectiles)
        {
            GameObject projectileObj = projectilePair.Key;
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile == null) continue;

            Collider2D collider = projectileObj.GetComponent<Collider2D>();
            if (collider == null) continue;

            // Apply initial weight at current positions
            foreach (var coord in projectilePair.Value)
            {
                if (IsValidCoord(coord))
                {
                    weightMatrix[coord.x, coord.y] += projectileWeight;
                }
            }

            // Predict future path and apply decreasing weight
            Vector2 currentPos = projectileObj.transform.position;
            Vector2 direction = projectile.direction.normalized;
            float speed = projectile.speed;
            Bounds bounds = collider.bounds;

            // Estimate how long the projectile will stay within bounds
            float maxTimeX = direction.x > 0
                ? (spawner.width / 2 - currentPos.x + bounds.extents.x) / (speed * direction.x)
                : (-spawner.width / 2 - currentPos.x - bounds.extents.x) / (speed * direction.x);
            float maxTimeY = direction.y > 0
                ? (spawner.height / 2 - currentPos.y + bounds.extents.y) / (speed * direction.y)
                : (-spawner.height / 2 - currentPos.y - bounds.extents.y) / (speed * direction.y);

            float maxTime = Mathf.Min(
                maxTimeX > 0 ? maxTimeX : float.MaxValue,
                maxTimeY > 0 ? maxTimeY : float.MaxValue
            );

            if (maxTime <= 0) continue;

            float timeStep = oneRectUnits / speed;
            for (float t = 0; t < maxTime; t += timeStep)
            {
                Vector2 futurePos = currentPos + direction * speed * t;
                Vector2 minCorner = futurePos - new Vector2(bounds.extents.x, bounds.extents.y);
                Vector2 maxCorner = futurePos + new Vector2(bounds.extents.x, bounds.extents.y);

                Vector2Int minCoord = WorldToGrid(minCorner);
                Vector2Int maxCoord = WorldToGrid(maxCorner);

                for (int x = minCoord.x; x <= maxCoord.x; x++)
                {
                    for (int y = minCoord.y; y <= maxCoord.y; y++)
                    {
                        if (!IsValidCoord(new Vector2Int(x, y))) continue;

                        Vector2 cellCenter = GridToWorld(x, y);
                        float timeToCell = Vector2.Distance(currentPos, cellCenter) / speed;

                        float weight = projectileWeight - (timeToCell * projectileWeightDecrease);
                        if (weight > 0)
                        {
                            weightMatrix[x, y] += weight;
                        }
                    }
                }
            }
        }
    }

    // Check if a grid coordinate is inside bounds
    private bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < weightMatrix.GetLength(0) &&
               coord.y >= 0 && coord.y < weightMatrix.GetLength(1);
    }

    // Convert world position to grid coordinate
    private Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - LeftBottom.x) / oneRectUnits),
            Mathf.FloorToInt((worldPos.y - LeftBottom.y) / oneRectUnits)
        );
    }

    // Convert grid coordinate to world center position
    private Vector2 GridToWorld(int x, int y)
    {
        return new Vector2(
            LeftBottom.x + x * oneRectUnits + oneRectUnits / 2,
            LeftBottom.y + y * oneRectUnits + oneRectUnits / 2
        );
    }

    // Assign initial weights based on distance to player and desired point
    private void InitializePlayerDistanceWeight()
    {
        for (int currX = 0; currX < weightMatrix.GetLength(0); currX++)
        {
            for (int currY = 0; currY < weightMatrix.GetLength(1); currY++)
            {
                Vector2Int currentCoord = new Vector2Int(currX, currY);
                Vector2 worldCoord = GridToWorld(currX, currY);

                float playerDistance = GetNearestPlayerDistance(currentCoord);
                float playerWeight = playerDistance * distancePlayerWeightMultiplier;

                float desiredDistance = Vector2.Distance(worldCoord, desiredPlayerPoint.position);
                float desiredWeight = desiredDistance * distanceWeightMultiplier;

                weightMatrix[currX, currY] = playerWeight + desiredWeight;
            }
        }
    }

    // Find shortest distance from current cell to any part of the player
    private float GetNearestPlayerDistance(Vector2Int currentCoord)
    {
        float minDistance = float.MaxValue;

        foreach (var coord in playerCoords)
        {
            float distance = Vector2Int.Distance(currentCoord, coord);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    // Populate playerCoords and projectile map based on objects in grid
    private void InitializeObjectAndPlayerCoordMatrix()
    {
        playerCoords.Clear();
        projectiles.Clear();
        for (int currX = 0; currX < weightMatrix.GetLength(0); currX++)
        {
            for (int currY = 0; currY < weightMatrix.GetLength(1); currY++)
            {
                var currObjects = GetRectObjects(currX, currY);
                foreach (var obj in currObjects)
                {
                    if (obj.Item1 == objectType.player)
                        playerCoords.Add(new(currX, currY));
                    else if (obj.Item1 == objectType.projectile)
                    {
                        if (projectiles.ContainsKey(obj.Item2))
                            projectiles[obj.Item2].Add(new(currX, currY));
                        else
                            projectiles[obj.Item2] = new List<Vector2Int> { new Vector2Int(currX, currY) };
                    }
                }
            }
        }
    }

    // Detect all tagged objects (Player or Projectile) in the given grid cell
    private List<(objectType, GameObject)> GetRectObjects(int x, int y)
    {
        List<(objectType, GameObject)> result = new();

        Vector3 boxCenter = new(LeftBottom.x + x * oneRectUnits + oneRectUnits / 2, LeftBottom.y + y * oneRectUnits + oneRectUnits / 2);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, new Vector2(oneRectUnits, oneRectUnits), 0);

        foreach (var col in colliders)
        {
            switch (col.tag)
            {
                case "Projectile":
                    result.Add((objectType.projectile, col.gameObject));
                    break;
                case "Player":
                    result.Add((objectType.player, col.gameObject));
                    break;
            }
        }

        return result;
    }
    private void OnDrawGizmos()
    {
        if (weightMatrix == null || (!DisplayWeightColor && !DisplayWeightText)) return;

        int width = weightMatrix.GetLength(0);
        int height = weightMatrix.GetLength(1);

        float minWeight = float.MaxValue;
        float maxWeight = float.MinValue;

        if (DisplayWeightColor)
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    float w = weightMatrix[x, y];
                    if (w < minWeight) minWeight = w;
                    if (w > maxWeight) maxWeight = w;
                }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float w = weightMatrix[x, y];

                if (DisplayWeightColor)
                {
                    float normalized = (w - minWeight) / (maxWeight - minWeight + 0.0001f);
                    Color color = Color.Lerp(Color.green, Color.red, normalized);
                    color.a = 0.9f;
                    Gizmos.color = color;
                    Vector3 pos = new Vector3(LeftBottom.x + x * oneRectUnits + oneRectUnits / 2, LeftBottom.y + y * oneRectUnits + oneRectUnits / 2, 0);
                    Vector3 size = new Vector3(oneRectUnits, oneRectUnits, 0.01f);
                    Gizmos.DrawCube(pos, size);
                }
                if (DisplayWeightText)
                {
                    Vector3 pos = new Vector3(LeftBottom.x + x * oneRectUnits + oneRectUnits / 2, LeftBottom.y + y * oneRectUnits + oneRectUnits / 2, 0);
                    UnityEditor.Handles.color = Color.black;
                    UnityEditor.Handles.Label(pos, w.ToString("F2"));
                }
            }
        }
    }
}
