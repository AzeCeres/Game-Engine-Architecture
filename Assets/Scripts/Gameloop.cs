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
    // Create a list of entities
    List<Entity> entities = new List<Entity>();
    // Create the component managers
    ComponentManager<PositionComponent> positionManager = new ComponentManager<PositionComponent>();
    ComponentManager<MovementComponent> movementManager = new ComponentManager<MovementComponent>();
    ComponentManager<OldRenderComponent>   renderManager   = new ComponentManager<OldRenderComponent>();
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
    private OldRenderSystem   _oldRenderSystem;
    void Start()
    {
        CreateParticles();
        // Create a movement system
        movementSystem = new MovementSystem(positionManager, movementManager); // add timer // add respawn func
        _oldRenderSystem   = new OldRenderSystem(positionManager, renderManager, camera.transform); // add timer func
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
            positionManager.AddComponent(entity.Id, new PositionComponent { X = XPositions, Y = YPositions, Z = ZPositions, Size = XPositions.Length });
            movementManager.AddComponent(entity.Id, new MovementComponent { XVelocity = XVelocity, YVelocity = YVelocity, ZVelocity = ZVelocity, Size = XVelocity.Length });
            timerManager.AddComponent(entity.Id, new TimerComponent { Timer = timer, IsLive = isLive,Respawnable = respawns, Size = timer.Length });

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
            renderManager.AddComponent(entity.Id, new OldRenderComponent { GameObjects = gameObjects, Size = gameObjects.Length });
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
        _oldRenderSystem.Update(entities);
    }
}



