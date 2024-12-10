

using System.Collections.Generic;
using UnityEngine;

public class ComponentManager<T> where T : struct
{
    private readonly Dictionary<int, T> _components = new Dictionary<int, T>();
    public void AddComponent(int entityId, T component)
    {
        _components[entityId] = component;
    }
    public T GetComponent(int entityId)
    {
        return _components[entityId];
    }
    public bool HasComponent(int entityId)
    {
        return _components.ContainsKey(entityId);
    }
    public void RemoveComponent(int entityId)
    {
        _components.Remove(entityId);
    }
    public Dictionary<int, T> GetAllComponents()
    {
        return _components;
    }
}
public struct PhysicsBallComponent
{
    public float[] Mass; // unimplemented
    public float[] Drag; // unimplemented
    public float[] Friction; // unimplemented
    public float[] Radius;
    public int[] SimSteps;
    public float Gravity;
    public int Size;
}
public struct Entity
{
    public int Id;
}
public struct PositionComponent
{
    public float[] X;
    public float[] Y;
    public float[] Z;
    public int Size;
}
public struct MovementComponent
{
    public float[] XVelocity;
    public float[] YVelocity;
    public float[] ZVelocity;
    public int Size;
}
public struct TimerComponent
{
    public float[] Timer;
    public bool[]  Respawnable;
    public bool[]  IsLive;
    public int     Size;
}

public struct GizmoRenderComponent
{
    public bool[] IsRendered;
    public enum Shape
    {
        Sphere,
        Cube,
        Mesh,
        Line
    }
    public Shape[] GizmoShape;
    public bool[] IsWire;
    public Vector3[] DirSize;//Direction for Line, size for cube, .x radius for sphere. 
    public Mesh GizmoMesh;
    public int Size;
}
public struct OldRenderComponent
{
    // might have to construct gameObjects and update positions in accordance too the position attribute.
    // This won't really be super ecs friendly, but it's mostly the only way to access the built in renderer.
    // in effect this will make it so all entities will have a gameObject, all gameObjects have a transform by default, and isn't necessarily ordered in a DoD format.
    // Unity *DOES* have a ECS plugin, Unity Dots, which does solve said issue, but is in many ways an antithesis of why we are having this folder in the first place- to learn.
    
    // I was thinking of ways around doing this. One option would be to construct meshes, and iterate over the vertices moving them to their position. + their local vertex positions.
    // This would be very expensive, as it would have to be done every frame, or at the very least every frame it moves.
    // Secondly- It might not even work, without combining all of the meshes into 1 so it could be added to this objects mesh-filter, and rendered out here. But this would limit it to only using one material.
    // hence, making gameObjects with the necessary components attached. with the caveat that it *is* inefficient, but less so than what the alternative seems to be.
    public GameObject[] GameObjects;
    //public Material[]   materials;
    //public Mesh[]       meshes;
    //public MeshFilter[] meshFilters;
    //public MeshRenderer[] meshRenderers;
    public int Size;
}