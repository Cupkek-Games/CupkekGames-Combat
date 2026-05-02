using UnityEngine;
using System.Collections.Generic;

namespace CupkekGames.Combat
{
  public static class TargetAreaColliderExtensions
  {
    /// <summary>
    /// Finds all colliders within the specified arc area.
    /// </summary>
    /// <param name="position">Center position of the arc.</param>
    /// <param name="radius">Radius of the arc.</param>
    /// <param name="rotation">The rotation representing the arc's forward direction.</param>
    /// <param name="arc">The angular width of the arc in degrees.</param>
    /// <returns>List of colliders within the arc area.</returns>
    public static List<Collider> FindCollidersInArc(Vector3 position, Quaternion rotation, float radius, float arc)
    {
      // Step 1: Get all colliders within the radius using Physics.OverlapSphere
      Collider[] hitColliders = Physics.OverlapSphere(position, radius);

      List<Collider> collidersInArc = new List<Collider>();
      float halfArc = arc / 2f;

      // Step 2: Determine the forward direction from the rotation
      Vector3 forwardDirection = rotation * Vector3.forward;

      // Step 3: Filter colliders by angle
      foreach (Collider collider in hitColliders)
      {
        // Vector from the position to the collider's position
        Vector3 directionToCollider = collider.transform.position - position;

        // Ignore the y component to work in the XZ plane
        directionToCollider.y = 0;

        // Normalize direction vectors
        directionToCollider.Normalize();
        forwardDirection.y = 0;  // Flatten to the Y plane if working in 2D space
        forwardDirection.Normalize();

        // Calculate the angle between the forward direction and the direction to the collider
        float angleToCollider = Vector3.Angle(forwardDirection, directionToCollider);

        // Check if the angle is within the arc range
        if (angleToCollider <= halfArc)
        {
          collidersInArc.Add(collider);
        }
      }

      return collidersInArc;
    }

    /// <summary>
    /// Checks if a given angle is within the arc.
    /// </summary>
    /// <param name="arcCenter">Center angle of the arc.</param>
    /// <param name="angleToCheck">Angle to check.</param>
    /// <param name="halfArc">Half of the arc width.</param>
    /// <returns>True if the angle is within the arc, false otherwise.</returns>
    private static bool IsAngleWithinArc(float arcCenter, float angleToCheck, float halfArc)
    {
      float minAngle = arcCenter - halfArc;
      float maxAngle = arcCenter + halfArc;

      return angleToCheck >= minAngle && angleToCheck <= maxAngle;
    }

    /// <summary>
    /// Finds all colliders within the specified spherical area.
    /// </summary>
    /// <param name="position">Center position of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <returns>List of colliders within the spherical area.</returns>
    public static List<Collider> FindCollidersInSphere(Vector3 position, float radius)
    {
      // Get all colliders within the radius using Physics.OverlapSphere
      Collider[] hitColliders = Physics.OverlapSphere(position, radius);

      // Convert the array to a list and return it
      return new List<Collider>(hitColliders);
    }

    public static List<Collider> FindCollidersInLine(Vector3 start, Vector3 end, float width, float height = 4f)
    {
      Vector3 direction = (end - start).normalized;
      float length = Vector3.Distance(start, end);
      Vector3 center = start + direction * (length / 2f);
      Quaternion rotation = Quaternion.LookRotation(direction);

      Vector3 halfExtents = new Vector3(width / 2f, height / 2f, length / 2f);

      DebugBox(start, end, width, height, Color.red, 1f);

      Collider[] colliders = Physics.OverlapBox(center, halfExtents, rotation);
      return new List<Collider>(colliders);
    }
    public static void DebugBox(Vector3 start, Vector3 end, float width, float height, Color color, float duration = 0f)
    {
      Vector3 direction = (end - start).normalized;
      float length = Vector3.Distance(start, end);
      Vector3 center = start + direction * (length / 2f);
      Quaternion rotation = Quaternion.LookRotation(direction);
      Vector3 halfExtents = new Vector3(width / 2f, height / 2f, length / 2f);

      // Get 8 corners of the box in local space
      Vector3[] localCorners = new Vector3[8]
      {
        new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
        new Vector3( halfExtents.x, -halfExtents.y, -halfExtents.z),
        new Vector3(-halfExtents.x,  halfExtents.y, -halfExtents.z),
        new Vector3( halfExtents.x,  halfExtents.y, -halfExtents.z),
        new Vector3(-halfExtents.x, -halfExtents.y,  halfExtents.z),
        new Vector3( halfExtents.x, -halfExtents.y,  halfExtents.z),
        new Vector3(-halfExtents.x,  halfExtents.y,  halfExtents.z),
        new Vector3( halfExtents.x,  halfExtents.y,  halfExtents.z),
      };

      // Transform to world space
      for (int i = 0; i < 8; i++)
        localCorners[i] = rotation * localCorners[i] + center;

      // Draw edges
      void DrawEdge(int a, int b) => Debug.DrawLine(localCorners[a], localCorners[b], color, duration);

      DrawEdge(0, 1); DrawEdge(1, 3); DrawEdge(3, 2); DrawEdge(2, 0); // Bottom
      DrawEdge(4, 5); DrawEdge(5, 7); DrawEdge(7, 6); DrawEdge(6, 4); // Top
      DrawEdge(0, 4); DrawEdge(1, 5); DrawEdge(2, 6); DrawEdge(3, 7); // Sides
    }

  }
}