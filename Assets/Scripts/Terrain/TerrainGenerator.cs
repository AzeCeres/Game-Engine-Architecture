using System;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))] // won't render without it. Suppose it isn't needed to require it for that reason, but might make it easier for people unfamiliar with unity to use the code with it.
public class TerrainGenerator : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh       _terrainMesh;
	private Vector3[] _vertices;
    public Vector3[] getVerticies()
    {
        return _vertices;
    }
    private Vector2[] _uv;
	private int[]     _triangles;
    [SerializeField][Tooltip("Amount of Quads in the x-z field. size.y corresponds to the z axis.")] 
    private Vector2Int size = new Vector2Int(20, 20);
    public Vector2Int GetSize() { return size;}
	//[SerializeField]private int xSize = 20;
	//[SerializeField]private int zSize = 20;
	[SerializeField]private float strength = 0.3f;
    [SerializeField]private float strength2 = 1f;
    [SerializeField]private float strength3 = 3f;
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _terrainMesh = new Mesh();
    }
    void Start()
    {
        _meshFilter.mesh = _terrainMesh;
        CreateTerrain();
        UpdateMesh();
    }
    void Update()
    {
        
    }
    void CreateTerrain()
    {
        _vertices = new Vector3[(size.x + 1) * (size.y + 1)];
        _uv = new Vector2[(size.x + 1) * (size.y + 1)];
        for (int i = 0, z = 0; z <= size.y; z++)
        {
            for (var x = 0; x <= size.x; x++)
            {
                float y = Mathf.PerlinNoise(x * strength, z * strength) * 2f;
                y -= Mathf.PerlinNoise(x * strength2, z * strength2) * 4f;
                y += Mathf.PerlinNoise(x * strength3, z * strength3) * 3f;
                _vertices[i] = new Vector3(x, y, z);
                _uv[i] = new Vector2(z / (float)size.y, x / (float)size.x);
                i++;
            }   
        }
        _triangles = new int[size.y * size.x * 6];
        int tris = 0, vert = 0;
        for (var z = 0; z < size.y; z++)
        {
            for (var x = 0; x < size.x; x++)
            {
                _triangles[tris + 0] = vert + 0; 
                _triangles[tris + 1] = vert + size.x + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1; 
                _triangles[tris + 4] = vert + size.x + 1;
                _triangles[tris + 5] = vert + size.x + 2;
                tris += 6;
                vert++;
            }
            vert++;
        }
    }
    void UpdateMesh()
    {
        _terrainMesh.vertices = _vertices;
        _terrainMesh.triangles = _triangles;
        _terrainMesh.uv = _uv;
        _terrainMesh.RecalculateNormals();
    }
    private void OnDrawGizmos() // too many verts
    {
        if (_vertices == null) return;
        if (_vertices.Length == 0 ) return;
        int x = 0, z = 0;
        for (int i = 0; i <= size.x; i++)
        {
            Gizmos.DrawSphere(_vertices[x + z*size.x + z], .1f); // from bottom left to top right | x&z = 0 to x&z = size.x
            Gizmos.DrawSphere(_vertices[x + z*size.x], .1f); // from bottom left to top left. Doesn't skip the ghost vertex per row | x&z = 0 to x = 0, z = size.x
            Gizmos.DrawSphere(_vertices[x], .1f); // from bottom left to bottom right | x&z = 0 to x = size.x, z = 0
            x++;
            z++;
        }
    }
}
