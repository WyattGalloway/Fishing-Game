using System.Collections.Generic;
using Unity.VisualScripting;
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

    public FishGroup group;

    List<Transform> neighbors = new(); //list of neighbors fish will separate from
    Vector3 moveDirection;

    public bool IsRoaming { get; set; } = true;
    public bool IsLingering { get; set; } = false;

    void FixedUpdate()
    {
        if (!IsRoaming)
        {
            IsLingering = true;
            moveDirection = Vector3.zero;
            transform.position += moveDirection;
            return;
        }
        else if (IsRoaming && !IsLingering)
        {
            Boid();
        }
    }

    void Boid()
    {
        FindNeighbors();

        Vector3 separation = CalculateSeparation();
        Vector3 cohesion = CalculateCohesion();
        Vector3 alignment = CalculateAlignment();

        Vector3 localMove = separation + cohesion + alignment;

        if (group != null)
        {
            group.UpdateGroupAvoidance(lakeColllider);

            localMove = Vector3.Lerp(localMove, group.groupAvoidance * moveSpeed, 0.5f);
        }

        if (localMove.magnitude < 0.01f)
            localMove = transform.forward;

        moveDirection = localMove.normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * 5f);

        transform.position = ClampToBounds(transform.position);
    }

    public void Initialize(Collider lake, FishDataSO data, FishGroup fishGroup)
    {
        lakeColllider = lake;
        this.data = data;
        group = fishGroup;
        moveDirection = transform.forward;

        transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );
    }

    void OnDisable()
    {
        if (group != null)
            group.members.Remove(this);
    }

    #region Boid Behaviours
    void FindNeighbors()
    {
        neighbors.Clear();

        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (Collider collider in nearby)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Fish"))
                neighbors.Add(collider.transform);
        }
    }

    Vector3 CalculateSeparation()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 separationDirection = Vector3.zero;
        foreach (Transform neighbor in neighbors)
        {
            Vector3 diff = transform.position - neighbor.position;
            float distance = diff.magnitude;
            if (distance > 0 && distance < separationDistance)
                separationDirection += diff.normalized / distance;
        }

        return separationDirection.normalized * separationForce;
    }

    Vector3 CalculateAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 averageForward = Vector3.zero;
        foreach (Transform neighbor in neighbors)
            averageForward += neighbor.forward;

        averageForward /= neighbors.Count;
        return averageForward.normalized * alignmentForce;
    }

    Vector3 CalculateCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (Transform neighbor in neighbors)
            center += neighbor.position;

        center /= neighbors.Count;
        Vector3 cohesionDirection = (center - transform.position).normalized;
        return cohesionDirection * cohesionForce;
    }
    #endregion

    #region Boundary Helpers
    public bool IsNearBounds()
    {
        if (lakeColllider == null) return false;

        Vector3 pos = transform.position;
        Bounds bounds = lakeColllider.bounds;

        return pos.x < bounds.min.x + avoidanceBuffer || pos.x > bounds.max.x + avoidanceBuffer ||
               pos.y < bounds.min.y + avoidanceBuffer || pos.y > bounds.max.y + avoidanceBuffer ||
               pos.z < bounds.min.z + avoidanceBuffer || pos.z > bounds.max.z + avoidanceBuffer;
    }

    public Vector3 GetBoundaryNormal()
    {
        Vector3 normal = Vector3.zero;
        Vector3 position = transform.position;
        Bounds bounds = lakeColllider.bounds;

        if (position.x < bounds.min.x) normal += Vector3.right;
        else if (position.x > bounds.max.x) normal += Vector3.left;

        if (position.y < bounds.min.y) normal += Vector3.right;
        else if (position.y > bounds.max.y) normal += Vector3.left;

        if (position.z < bounds.min.z) normal += Vector3.right;
        else if (position.z > bounds.max.z) normal += Vector3.left;

        return normal.normalized;
    }

    Vector3 ClampToBounds(Vector3 pos)
    {
        if (lakeColllider == null) return pos;

        Bounds bounds = lakeColllider.bounds;
        pos.x = Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, bounds.min.y, bounds.max.y);
        pos.z = Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z);

        return pos;
    }
    #endregion

    [System.Serializable]
    public class FishGroup
    {
        public List<FishBoidBehaviour> members = new();
        public Vector3 groupAvoidance = Vector3.zero;

        public void UpdateGroupAvoidance(Collider lake)
        {
            Vector3 avoidanceSum = Vector3.zero;
            int count = 0;

            foreach (var fish in members)
            {
                if (fish.IsNearBounds())
                {
                    avoidanceSum += -fish.transform.forward;
                    count++;
                }
            }

            groupAvoidance = count > 0 ? (avoidanceSum / count).normalized : Vector3.zero;
        }
    }

}
