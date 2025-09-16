using System.Collections.Generic;
using UnityEngine;

public class FishBoidBehaviour : MonoBehaviour
{
    [Header("Boid Forces")]
    [SerializeField] float separationForce; //how quickly fish are separated
    [SerializeField] float separationDistance; //at what distance the separation is determined
    [SerializeField] float alignmentForce; //how quickly fish are aligned with each other
    [SerializeField] float cohesionForce; //how quickly fish will move together

    [Header("Movement References")]
    [SerializeField] float avoidanceStrength; //how strongly the avoidance to the bounds is
    [SerializeField] float avoidanceBuffer; //how far the fish will begin to avoid the bounds
    [SerializeField] float moveSpeed; //how fast the fish move
    [SerializeField] float separationRadius; //the radius of the separation sphere

    [Header("References")]
    [SerializeField] Collider lakeColllider; //bounds of the lake
    public FishDataSO data; //data the fish need to be set. Name, weight, etc.
    public float currentWeight { get; private set; } //can set the weight of each fish

    List<Transform> neighbors = new(); //list of neighbors fish will separate from

    public bool IsRoaming { get; set; } = true;

    void Update()
    {
        FindNeighbors();

        Vector3 move = Vector3.zero;

        //ensures fish dont stay static if no neighbors are found
        Vector3 wander = new Vector3(
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.x) - 0.5f,
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.y) - 0.5f,
            Mathf.PerlinNoise(Time.time * 0.5f, transform.position.z) - 0.5f
        );

        wander = wander.normalized * 0.5f;

        if (IsRoaming)
            move += wander;

        //starts boid when neighbors are present
        if (neighbors.Count > 0)
        {
            Vector3 separation = CalculateSeparation();
            Vector3 alignment = CalculateAlignment();
            Vector3 cohesion = CalculateCohesion();

            move = separation + alignment + cohesion;
            IsRoaming = false;
        }

        //avoid the bounds to prevent clumping on the edges
        move += CalculateBoundsAvoidance();

        //moves fish forward if they havent moved yet
        if (move.magnitude < 0.01f)
            move = transform.forward;

        move = move.normalized * moveSpeed;
        
        //Adds all previous instructions of move to the position to move the fish
        transform.position += move * Time.deltaTime;
        transform.position = ClampToBounds(transform.position); //ensures fish dont leave bounds
        
        //ensures the forward is inline with all other fish
        if (move != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, move.normalized, Time.deltaTime * 5f);
        }
    }

    public void Initialize(Collider lake, FishDataSO fishData)
    {
        //initializes the data the fish need, since theyre prefabs and cant be assigned in inspector
        lakeColllider = lake;
        data = fishData;
        currentWeight = Random.Range(data.weightRange.x, data.weightRange.y);
    }

    void FindNeighbors()
    {
        neighbors.Clear(); //clear list to ensure no overlap

        Collider[] nearbyNeighbors = Physics.OverlapSphere(transform.position, separationRadius);

        //takes the collider array and adds fish in the collider to the neighbor list
        foreach (Collider col in nearbyNeighbors)
        {
            if (col.gameObject != gameObject && col.CompareTag("Fish")) //TODO: change fish to fish of same type
            {
                neighbors.Add(col.transform);
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 separationDirection = Vector3.zero;

        //gets the distance from the neighbor and ensures the fish are an appropriate distance
        foreach (Transform neighbor in neighbors)
        {
            Vector3 separation = transform.position - neighbor.transform.position;
            float distance = separation.magnitude;

            if (distance > 0 && distance < separationDistance)
            {
                separationDirection += separation.normalized / distance;
            }
        }

        //return the separation direction and ensures that the fish are separated by force
        return separationDirection.normalized * separationForce;
    }

    Vector3 CalculateAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 averageDirection = Vector3.zero;

        //ensures each fish goes in the same direction
        foreach (Transform neighbor in neighbors)
        {
            averageDirection += neighbor.forward;
        }

        averageDirection /= neighbors.Count; //averages out the direction by the count of neightbors

        return averageDirection.normalized * alignmentForce;
    }

    Vector3 CalculateCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 centerOfMass = Vector3.zero;

        //makes each fish clump have its own center of mass
        foreach (Transform neighbor in neighbors)
        {
            centerOfMass += neighbor.position;
        }

        centerOfMass /= neighbors.Count; //average out the center of mass between all neighbors

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
        // gets the x,y,z of each fish and calculates its avoidance based off its proximity to the bounds of the swimming area
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

        //ensure the fish dont leave the bounds of the lake by clamping x,y,z
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);

        return position;
    }
}
