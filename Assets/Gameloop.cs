using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Printer : MonoBehaviour
{
    void Print<T>(T data)
    {
        print(data); // Prints to the unity console output. Not visible here in terminal
    }
}
public class Gameloop : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Create a list of entities
    List<Entity> entities = new List<Entity>();
    // Create the component managers
    ComponentManager<PositionComponent> positionManager = new ComponentManager<PositionComponent>();
    ComponentManager<MovementComponent> movementManager = new ComponentManager<MovementComponent>();
    ComponentManager<RenderComponent>   renderManager   = new ComponentManager<RenderComponent>();
    ComponentManager<TimerComponent>    timerManager    = new ComponentManager<TimerComponent>();
    [Header("Particles")]
    [SerializeField][Range(1,10)]    private int   numberOfEntites;
    [SerializeField][Range(0,1000)]  private int   numberOfParticlesPerEntity;
    [SerializeField][Range(0,100)]   private float gravity;
    [Header("Timer")]
    [SerializeField][Tooltip("minTime-maxTime")] private Vector2 timeToLive;
    [SerializeField] private bool isRespawning = true;
    [Header("Position")]
    [SerializeField][Tooltip("minX-maxX")] private Vector2 spawnBoundsX;
    [SerializeField][Tooltip("minY-maxY")] private Vector2 spawnBoundsY;
    [SerializeField][Tooltip("minZ-maxZ")] private Vector2 spawnBoundsZ;
    [Header("Renderable")]
    [SerializeField] private Material material;
    [SerializeField] private Mesh     mesh;
    [SerializeField] private Camera   camera;

    private int curID = 0;
    private MovementSystem movementSystem;
    private RenderSystem   renderSystem;
    void Start()
    {
        CreateParticles();
        // Create a movement system
        movementSystem = new MovementSystem(positionManager, movementManager); // add timer // add respawn func
        renderSystem   = new RenderSystem(positionManager, renderManager, camera.transform); // add timer func
    }
    private void CreateParticles()
    {
        // Create and add entities
        for (var i = 0; i < numberOfEntites; i++)
        {
            var entity = new Entity { Id = curID };
            curID++;
            entities.Add(entity);
            GameObject entityGameObject = new GameObject(name: "entity" + i); // made to section of the 
            // Add components to the entity
            var XPositions = new float[numberOfParticlesPerEntity];
            var YPositions = new float[numberOfParticlesPerEntity];
            var ZPositions = new float[numberOfParticlesPerEntity];
            var XVelocity = new float[numberOfParticlesPerEntity];
            var YVelocity = new float[numberOfParticlesPerEntity];
            var ZVelocity = new float[numberOfParticlesPerEntity];
            var timer = new float[numberOfParticlesPerEntity];
            var isLive = new bool [numberOfParticlesPerEntity];
            var respawns = new bool [numberOfParticlesPerEntity];

            for (int j = 0; j < numberOfParticlesPerEntity; j++)
            {
                XPositions[j] = Random.Range(spawnBoundsX.x, spawnBoundsX.y);
                YPositions[j] = Random.Range(spawnBoundsY.x, spawnBoundsY.y);
                ZPositions[j] = Random.Range(spawnBoundsZ.x, spawnBoundsZ.y);
                XVelocity[j] = Mathf.Sin(j) / 2;
                YVelocity[j] = -gravity;
                ZVelocity[j] = Mathf.Cos(j) / 2;
                timer[j] = Random.Range(timeToLive.x, timeToLive.y);
                isLive[j] = true;
                respawns[j] = true;
            }
            // Add components to the entity
            positionManager.AddComponent(entity.Id, new PositionComponent { X = XPositions, Y = YPositions, Z = ZPositions, size_ = XPositions.Length });
            movementManager.AddComponent(entity.Id, new MovementComponent { XVelocity = XVelocity, YVelocity = YVelocity, ZVelocity = ZVelocity, size_ = XVelocity.Length });
            timerManager.AddComponent(entity.Id, new TimerComponent { Timer = timer, isLive = isLive,respawnable = respawns, size_ = timer.Length });

            var gameObjects = new GameObject[numberOfParticlesPerEntity];
            //Material[]   materials   = new Material  [numberOfParticlesPerEntity];
            //Mesh[]       meshes      = new Mesh      [numberOfParticlesPerEntity];
            for (var j = 0; j < numberOfParticlesPerEntity; j++)
            {
                var tempMesh = mesh;
                var tempMaterial = material;
                var tempGameObject = new GameObject
                {
                    name = "entity" + i.ToString() + "renderComponent" + j.ToString(),
                    transform = { parent = entityGameObject.transform }
                };
                tempGameObject.AddComponent<MeshFilter>();
                var tempMeshFilter = tempGameObject.GetComponent<MeshFilter>();

                tempMeshFilter.mesh = tempMesh;
                tempGameObject.AddComponent<MeshRenderer>();

                var tempMeshRenderer = tempGameObject.GetComponent<MeshRenderer>();
                tempMeshRenderer.materials[0] = tempMaterial;

                //meshes[j]      = tempMesh;
                //materials[j]   = tempMaterial;
                gameObjects[j] = tempGameObject;
            }
            renderManager.AddComponent(entity.Id, new RenderComponent { gameObjects = gameObjects, size_ = gameObjects.Length });
            //int[] health = new[] { 5, 2, 10, 3, 5, 3, 5, 6, 8, 7, 3 };
            //healthManager.AddComponent(entity.Id, new HealthComponent { Health = health, size_ = health.Length});
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Simulate system update for all entities
        movementSystem.Update(entities);
    }
    private void LateUpdate()
    {
        renderSystem.Update(entities);
    }
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
    public int size_;
}
public struct MovementComponent
{
    public float[] XVelocity;
    public float[] YVelocity;
    public float[] ZVelocity;
    public int size_;
}
public struct TimerComponent
{
    public float[] Timer;
    public bool[]  respawnable;
    public bool[]  isLive;
    public int     size_;
}
public struct RenderComponent
{
    // might have to construct gameObjects and update positions in accordance too the position attribute.
    // This won't really be super ecs friendly, but it's mostly the only way to access the built in renderer.
    // in effect this will make it so all entities will have a gameObject, all gameObjects have a transform by default, and isn't necessarily ordered in a DoD format.
    // Unity *DOES* have a ECS plugin, Unity Dots, which does solve said issue, but is in many ways an antithesis of why we are having this folder in the first place- to learn.
    
    // I was thinking of ways around doing this. One option would be to construct meshes, and iterate over the vertices moving them to their position. + their local vertex positions.
    // This would be very expensive, as it would have to be done every frame, or at the very least every frame it moves.
    // Secondly- It might not even work, without combining all of the meshes into 1 so it could be added to this objects mesh-filter, and rendered out here. But this would limit it to only using one material.
    // hence, making gameObjects with the necessary components attached. with the caveat that it *is* inefficient, but less so than what the alternative seems to be.
    public GameObject[] gameObjects;
    //public Material[]   materials;
    //public Mesh[]       meshes;
    //public MeshFilter[] meshFilters;
    //public MeshRenderer[] meshRenderers;
    public int size_;
}
public class ComponentManager<T> where T : struct
{
    private Dictionary<int, T> components = new Dictionary<int, T>();
    public void AddComponent(int entityId, T component)
    {
        components[entityId] = component;
    }
    public T GetComponent(int entityId)
    {
        return components[entityId];
    }
    public bool HasComponent(int entityId)
    {
        return components.ContainsKey(entityId);
    }
    public void RemoveComponent(int entityId)
    {
        components.Remove(entityId);
    }
    public Dictionary<int, T> GetAllComponents()
    {
        return components;
    }
}
public abstract class System_
{
    public abstract void Update(List<Entity> entities);
}
public class RenderSystem : System_
{
    private ComponentManager<PositionComponent> positionManager;
    private ComponentManager<RenderComponent> renderManager;
    private Transform cameraTrans;
    public RenderSystem(ComponentManager<PositionComponent> positionManager, ComponentManager<RenderComponent> renderManager, Transform cameraTrans)
    {
        this.positionManager = positionManager;
        this.renderManager = renderManager;
        this.cameraTrans = this.cameraTrans;
    }
    public override void Update(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (!positionManager.HasComponent(entity.Id))
                continue;
            if (!renderManager.HasComponent(entity.Id))
                continue;
            var position = positionManager.GetComponent(entity.Id);
            var render = renderManager.GetComponent(entity.Id);
            if (position.size_ != render.size_)
                continue;
            for (int i = 0; i < position.size_; i++)
            {
                render.gameObjects[i].transform.position = new Vector3(position.X[i], position.Y[i], position.Z[i]);
                render.gameObjects[i].transform.LookAt(cameraTrans);
            }
        }
    }
}
public class MovementSystem : System_
{
    private ComponentManager<PositionComponent> positionManager;
    private ComponentManager<MovementComponent> movementManager;
    public MovementSystem(ComponentManager<PositionComponent> positionManager, ComponentManager<MovementComponent> movementManager)
    {
        this.positionManager = positionManager;
        this.movementManager = movementManager;
    }
    public override void Update(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (!positionManager.HasComponent(entity.Id))
                continue;
            if (!movementManager.HasComponent(entity.Id))
                continue;
            var position = positionManager.GetComponent(entity.Id);
            var movement = movementManager.GetComponent(entity.Id);
            if (position.size_ != movement.size_)
                continue;
            for (int i = 0; i < position.size_; i++)
            {
                position.X[i] += movement.XVelocity[i]*Time.deltaTime;
                position.Y[i] += movement.YVelocity[i]*Time.deltaTime;
                position.Z[i] += movement.ZVelocity[i]*Time.deltaTime;
                //Printer.print($"Entity {entity.Id} moved to({position.X[i]}, {position.Y[i]}, {position.Z[i]})");
            }
            positionManager.AddComponent(entity.Id, position); // Update
            //position
        }
    }
}