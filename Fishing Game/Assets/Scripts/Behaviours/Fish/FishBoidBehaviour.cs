using System.Collections.Generic;
using UnityEngine;

public class FishBoidBehaviour : MonoBehaviour
{
    [Header("Boid Forces")]
    [SerializeField] float separationForce;
    [SerializeField] float separationDistance;
    [SerializeField] float alignmentForce;
    [SerializeField] float cohesionForce;

    [Header("Movement References")]
    [SerializeField] float avoidanceStrength;
    [SerializeField] float avoidanceBuffer;
    [SerializeField] float moveSpeed;
    [SerializeField] float separationRadius;

    [Header("References")]
    [SerializeField] Collider lakeColllider;
    public FishDataSO data;
    public float currentWeight { get; private set; }

    List<Transform> neighbors = new();

    void Update()
    {
        FindNeighbors();

        Vector3 move = Vector3.zero;

        Vector3 wander = new Vector3(
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.x) - 0.5f,
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.y) - 0.5f,
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.z) - 0.5f
        );

        wander = wander.normalized * 0.5f;

        move += wander;

        if (neighbors.Count > 0)
        {
            Vector3 separation = CalculateSeparation();
            Vector3 alignment = CalculateAlignment();
            Vector3 cohesion = CalculateCohesion();

            move = separation + alignment + cohesion;
        }

        move += CalculateBoundsAvoidance();

        if (move.magnitude < 0.01f)
            move = transform.forward;

        move = move.normalized * moveSpeed;
        
        transform.position += move * Time.deltaTime;
        transform.position = ClampToBounds(transform.position);
        

        if (move != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, move.normalized, Time.deltaTime * 5f);
        }
    }

    public void Initialize(Collider lake, FishDataSO fishData)
    {
        lakeColllider = lake;
        data = fishData;
        currentWeight = Random.Range(data.weightRange.x, data.weightRange.y);
    }

    void FindNeighbors()
    {
        neighbors.Clear();

        Collider[] nearbyNeighbors = Physics.OverlapSphere(transform.position, separationRadius);

        foreach (Collider col in nearbyNeighbors)
        {
            if (col.gameObject != gameObject && col.CompareTag("Fish"))
            {
                neighbors.Add(col.transform);
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 separationDirection = Vector3.zero;

        foreach (Transform neighbor in neighbors)
        {
            Vector3 separation = transform.position - neighbor.transform.position;
            float distance = separation.magnitude;

            if (distance > 0 && distance < separationDistance)
            {
                separationDirection += separation.normalized / distance;
            }
        }

        return separationDirection.normalized * separationForce;
    }

    Vector3 CalculateAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 averageDirection = Vector3.zero;

        foreach (Transform neighbor in neighbors)
        {
            averageDirection += neighbor.forward;
        }

        averageDirection /= neighbors.Count;

        return averageDirection.normalized * alignmentForce;
    }

    Vector3 CalculateCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 centerOfMass = Vector3.zero;

        foreach (Transform neighbor in neighbors)
        {
            centerOfMass += neighbor.position;
        }

        centerOfMass /= neighbors.Count;

        Vector3 cohesionDirection = (centerOfMass - transform.position).normalized;

        return cohesionDirection * cohesionForce;
    }

    Vector3 CalculateBoundsAvoidance()
    {
        if (lakeColllider == null) return Vector3.zero;

        Bounds bounds = lakeColllider.bounds;
        Vector3 avoidance = Vector3.zero;

        float buffer = avoidanceBuffer;

        Vector3 pos = transform.position;

        // X-axis
        if (pos.x < bounds.min.x + buffer)
            avoidance += Vector3.right * (1f - (pos.x - bounds.min.x) / buffer);
        else if (pos.x > bounds.max.x - buffer)
            avoidance += Vector3.left * (1f - (bounds.max.x - pos.x) / buffer);

        // Y-axis
        if (pos.y < bounds.min.y + buffer)
            avoidance += Vector3.up * (1f - (pos.y - bounds.min.y) / buffer);
        else if (pos.y > bounds.max.y - buffer)
            avoidance += Vector3.down * (1f - (bounds.max.y - pos.y) / buffer);

        // Z-axis
        if (pos.z < bounds.min.z + buffer)
            avoidance += Vector3.forward * (1f - (pos.z - bounds.min.z) / buffer);
        else if (pos.z > bounds.max.z - buffer)
            avoidance += Vector3.back * (1f - (bounds.max.z - pos.z) / buffer);

        return avoidance.normalized * avoidanceStrength;
    }


    Vector3 ClampToBounds(Vector3 position)
    {
        if (lakeColllider == null) return position;

        Bounds bounds = lakeColllider.bounds;

        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);

        return position;
    }
}
