using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ECSBalls : MonoBehaviour
{
    // Create a list of entities
    private List<Entity> _entities = new List<Entity>();
    // Create the component managers
    private ComponentManager<PositionComponent> _positionManager  = new ComponentManager<PositionComponent>();
    private ComponentManager<MovementComponent> _movementManager  = new ComponentManager<MovementComponent>();
    private ComponentManager<GizmoRenderComponent> _renderManager = new ComponentManager<GizmoRenderComponent>();
    private ComponentManager<PhysicsBallComponent> _physManager   = new ComponentManager<PhysicsBallComponent>();

    [Header("Balls")] 
    [SerializeField][Range(1,  10)] private int numEntities = 2;
    [SerializeField][Range(1, 200)] private int numBalls    = 100;
    [SerializeField] private Vector2 radRange = new Vector2(0.1f, 1);
    [Header("Position")]
    [SerializeField][Tooltip("minX-maxX")] private Vector2 spawnBoundsX;
    [SerializeField][Tooltip("minY-maxY")] private Vector2 spawnBoundsY;
    [SerializeField][Tooltip("minY-maxY")] private Vector2 spawnBoundsZ;
    [Header("Physics")] 
    [SerializeField][Range(1,20)]private float gravity = 10;
    [SerializeField][Range(1,10)]private int simulationSteps = 5; 
    [SerializeField] private TerrainCol terrainCol;
    private int _curID = 0;
    private MovementSystem    _movementSystem;
    private GizmoRenderSystem _renderSystem;
    private PhysicsSystem     _physicsSystem;
    private void Start()
    {
        CreateBalls();
        // Create a movement system
        //_movementSystem = new MovementSystem(_positionManager, _movementManager); // add timer // add respawn func
        _physicsSystem = new PhysicsSystem(_positionManager, _movementManager, _physManager, terrainCol);
        //renderSystem   = new GizmoRenderSystem(); // doesn't do anything
    }

    private void CreateBalls()
    {
        for (var e = 0; e < numEntities; e++)
        {
            var entity = new Entity { Id = _curID };
            _curID++;
            _entities.Add(entity);
            // Add components to the entity
            var xPositions = new float  [numBalls];
            var yPositions = new float  [numBalls];
            var zPositions = new float  [numBalls];
            var xVelocity  = new float  [numBalls];
            var yVelocity  = new float  [numBalls];
            var zVelocity  = new float  [numBalls];
            var radius     = new float  [numBalls];
            var simSteps   = new int    [numBalls];
            var toRender   = new bool   [numBalls];
            var respawns   = new bool   [numBalls];
            var dirSize    = new Vector3[numBalls];
            var shape = new GizmoRenderComponent.Shape[numBalls];
            for (var nr = 0; nr < numBalls; nr++)
            {
                xPositions[nr] = Random.Range(spawnBoundsX.x, spawnBoundsX.y);
                yPositions[nr] = Random.Range(spawnBoundsY.x, spawnBoundsY.y);
                zPositions[nr] = Random.Range(spawnBoundsZ.x, spawnBoundsZ.y);
                radius[nr]     = Random.Range(radRange.x, radRange.y);
                shape[nr]      = GizmoRenderComponent.Shape.Sphere;
                xVelocity[nr]  = Mathf.Sin(nr) / 2;
                yVelocity[nr]  = -gravity;
                zVelocity[nr]  = Mathf.Cos(nr) / 2;
                dirSize[nr]    = new Vector3(radius[e], 0, 0);
                toRender[nr]   = true;
                respawns[nr]   = true;
                simSteps[nr]   = simulationSteps;
            }

            // Add components to the entity
            _positionManager.AddComponent(_entities[e].Id,
                new PositionComponent { X = xPositions,        Y = yPositions,        Z = zPositions,        Size = xPositions.Length });
            _movementManager.AddComponent(_entities[e].Id,
                new MovementComponent { XVelocity = xVelocity, YVelocity = yVelocity, ZVelocity = zVelocity, Size = xVelocity.Length });
            _renderManager.AddComponent(_entities[e].Id,
                new GizmoRenderComponent { GizmoShape = shape, IsRendered = toRender, DirSize = dirSize,     Size = toRender.Length });
            _physManager.AddComponent(_entities[e].Id,
                new PhysicsBallComponent{ Radius = radius, SimSteps = simSteps, Gravity = gravity,           Size = radius.Length });
        }
    }
    private void Update()
    {
        //_movementSystem.Update(_entities);
        _physicsSystem.Update(_entities);
    }
    private void OnDrawGizmos() // a way to more easily use DOD rendering, but gives up on z order for other objects. aka it will be drawn in front of all other objects
    {
        if (_entities.Count==0) return;
        for (var e = 0; e < _entities.Count; e++)
        {
            if (!_positionManager.HasComponent(_entities[e].Id)) continue;
            if (!_renderManager.HasComponent(_entities[e].Id))   continue;
            var position = _positionManager.GetComponent(_entities[e].Id);
            var render   = _renderManager  .GetComponent(_entities[e].Id);
            for (var nr = 0; nr < render.Size; nr++)
            {
                if (!render.IsRendered[nr]) continue;
                var posVec = new Vector3(position.X[nr], position.Y[nr], position.Z[nr]);
                
                if (render.GizmoShape[nr] == GizmoRenderComponent.Shape.Sphere)
                {
                    Gizmos.DrawSphere(posVec, render.DirSize[nr].x);
                }
                else
                {
                    throw new NotImplementedException(); // not needed for the current folder, would be implemented in a fully fledged system.
                }
            }
        }
    }
}

