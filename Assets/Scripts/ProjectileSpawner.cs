using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileSpawner : MonoBehaviour
{
    public int width = 20;
    public int height = 10;
    public float spawnIntervalMs = 500f;
    public int bulletsPerSpawn = 3;
    public GameObject projectilePrefab;

    private float timer;
    private static bool hasCloned = false;

    void Start()
    {
        // Clone this spawner with offset, only once
        if (!hasCloned)
        {
            hasCloned = true;
            Vector3 offsetPos = transform.position + new Vector3(30f, 0f, 0f);
            GameObject clone = Instantiate(gameObject, offsetPos, transform.rotation);
        }
    }

    void Update()
    {
        timer += Time.deltaTime * 1000f; // Convert to milliseconds
        if (timer >= spawnIntervalMs)
        {
            timer = 0f;
            SpawnBullets();
        }
    }

    void SpawnBullets()
    {
        List<Vector2> edgePoints = GetEdgePoints();
        for (int i = 0; i < bulletsPerSpawn; i++)
        {
            Vector2 spawnPos = edgePoints[Random.Range(0, edgePoints.Count)];
            Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, transform.position.z);
            GameObject bullet = Instantiate(projectilePrefab, worldPos, Quaternion.identity);

            Vector2 dir = Random.insideUnitCircle.normalized;
            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.direction = dir;
            }
        }
    }

    List<Vector2> GetEdgePoints()
    {
        List<Vector2> points = new List<Vector2>();
        float left = transform.position.x - width / 2f;
        float right = transform.position.x + width / 2f;
        float top = transform.position.y + height / 2f;
        float bottom = transform.position.y - height / 2f;

        int segments = Mathf.CeilToInt((width + height) * 2);
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            float perim = (width + height) * 2f;
            float dist = t * perim;

            Vector2 pos;
            if (dist < width)
                pos = new Vector2(left + dist, top);
            else if (dist < width + height)
                pos = new Vector2(right, top - (dist - width));
            else if (dist < width * 2 + height)
                pos = new Vector2(right - (dist - (width + height)), bottom);
            else
                pos = new Vector2(left, bottom + (dist - (width * 2 + height)));

            points.Add(pos);
        }

        return points;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 topLeft = transform.position + new Vector3(-width / 2f, height / 2f, 0f);
        Vector3 topRight = transform.position + new Vector3(width / 2f, height / 2f, 0f);
        Vector3 bottomRight = transform.position + new Vector3(width / 2f, -height / 2f, 0f);
        Vector3 bottomLeft = transform.position + new Vector3(-width / 2f, -height / 2f, 0f);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
