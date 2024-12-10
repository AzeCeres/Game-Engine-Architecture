using System;
using UnityEngine;
using Random = UnityEngine.Random;


[RequireComponent(typeof(TerrainGenerator))]
public class TerrainCol : MonoBehaviour // short for TerrainCollider, which couldn't be used because of a built-in class
{
    private TerrainGenerator _terrain;
    [Header("Gizmos")] [SerializeField] private bool displayGizmos;
    private void Awake()
    {
        _terrain = GetComponent<TerrainGenerator>();
    }
    public float CheckHeightAtPosition(Vector3 position)
    {
        var centeredPos = CenterPosition(position);
        var tempPos       = position - transform.position;
        
        return InterpolateHeightAtPoint(tempPos);
        //return CheckHeightAtVertex(centeredPos);
    }

    public bool CheckBounds(Vector3 position)
    {
        var tempPos = position - transform.position;
        var size = _terrain.GetSize();
        var TOLERANCE = 0.05;
        var result = (Math.Abs(tempPos.x - Math.Clamp(tempPos.x, 0, size.x - 1)) < TOLERANCE || Math.Abs(tempPos.z - Math.Clamp(tempPos.z, 0, size.y - 1)) < TOLERANCE); // tolerance to account for floating point precision errors. 
        return result;
    }
    public Vector3 CheckNormalsAtPosition(Vector3 position)
    {
        return CheckNormalsAtVertex(CenterPosition(position));
    }
    private Vector3Int CenterPosition(Vector3 position)
    {
        var tempPos = position - transform.position; // centres the positions to 0,0,0
        var size = _terrain.GetSize();
        tempPos.x = Math.Clamp(tempPos.x, 0, size.x - 1);
        tempPos.z = Math.Clamp(tempPos.z, 0, size.y - 1);
        var centeredPos = new Vector3Int((int)tempPos.x, (int)tempPos.y, (int)tempPos.z);
        return centeredPos;
    }

    public Vector3 CheckNormalsAtVertex(Vector3Int position)
    {
        var size = _terrain.GetSize();
        var normal = _terrain.getNormals()[(int)position.z * size.x + position.x + position.z]; // truncates to nearest vertex normal
        return normal;
    }
    private float CheckHeightAtVertex(Vector3Int position)
    {
        var size = _terrain.GetSize();
        var vertex = _terrain.getVerticies()[(int)position.z * size.x + position.x + position.z]; // truncates to nearest vertex
        return vertex.y;
    }
    // Function to interpolate the height at a specified point using barycentric coordinates
    private float InterpolateHeightAtPoint(Vector3 position)
    {
        // Convert texture coordinates to pixel coordinates
        var vertexX = (int)position.x;
        var vertexZ = (int)position.z;
        // Determine the three vertexes that form a triangle around the given coordinates
        var vertex1 = new Vector2Int(vertexX + 0, vertexZ + 0);
        var vertex2 = new Vector2Int(vertexX + 1, vertexZ + 0);
        var vertex3 = new Vector2Int(vertexX + 0, vertexZ + 1);
        var vertex4 = new Vector2Int(vertexX + 1, vertexZ + 1);
    
        // Calculate the barycentric coordinates of the point within the triangle
        var point = new Vector2(position.x, position.z);
        var barycentric1 = CalculateBarycentricCoordinates(point, vertex1, vertex3, vertex2);
        var barycentric2 = CalculateBarycentricCoordinates(point, vertex3, vertex2, vertex4);
        print(barycentric1);
        print(barycentric2);
    
        // Retrieve the heights of the three pixels
        var height1 = CheckHeightAtVertex(new Vector3Int(vertex1.x,0, vertex1.y));
        var height2 = CheckHeightAtVertex(new Vector3Int(vertex2.x,0, vertex2.y));
        var height3 = CheckHeightAtVertex(new Vector3Int(vertex3.x,0, vertex3.y));
        var height4 = CheckHeightAtVertex(new Vector3Int(vertex4.x,0, vertex4.y));
    
        // Interpolate the height using barycentric coordinates
        float interpolatedHeight;
        if (barycentric1 is { x: > 0, y: > 0, z: > 0 })
        { 
            interpolatedHeight = height1 * barycentric1.x + height3 * barycentric1.y + height2 * barycentric1.z;
        }
        else
        {
            interpolatedHeight = height3 * barycentric2.x + height2 * barycentric2.y + height4 * barycentric2.z;
        }
        return interpolatedHeight;
    }
    
    // Function to calculate barycentric coordinates of a point within a triangle
    private static Vector3 CalculateBarycentricCoordinates(Vector2 point, Vector2Int p1, Vector2Int p2, Vector2Int p3)
    {
        // Precompute vectors
        Vector2 v0 = p3 - p1;
        Vector2 v1 = p2 - p1;
        Vector2 v2 = new Vector2(point.x - p1.x, point.y - p1.y);
    
        // Compute dot products
        
        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);
    
        // Compute determinant
        var denom = dot00 * dot11 - dot01 * dot01;
    
        // Check if the triangle is degenerate
        if (denom == 0)
            return new Vector3(0.0f, 0.0f, 0.0f); // Return default barycentric coordinates
    
        // Compute barycentric coordinates
        var invDenom = 1.0f / denom;
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;
        var w = 1.0f - u - v;
    
        return new Vector3(u, v, w);
    }
    
    private Vector3[] gizmosPos = new Vector3[100];
    private Vector3 prevPos = new Vector3(0, 0, 0);
    private void OnDrawGizmos()
    {
        if (!displayGizmos) return;
        if (_terrain == null)
        {
            return;
        }
        if (_terrain.getVerticies() == null)
        {
            return;
        }
        var size = _terrain.GetSize();
        for (int i = 0; i < gizmosPos.Length; i++)
        {
            var position = transform.position;
            if (gizmosPos[i].Equals(new Vector3()) || !position.Equals(prevPos))
            {
                
                gizmosPos[i] = new Vector3(Random.Range(position.x, size.x + position.x), 0, Random.Range(position.z, size.y + position.x));
                gizmosPos[i].y = CheckHeightAtPosition(gizmosPos[i]);
            }
            Gizmos.DrawSphere(gizmosPos[i], .1f);
        }
        prevPos = transform.position;
    }
}
