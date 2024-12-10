using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class System_
{
    public abstract void Update(List<Entity> entities);
}


public class GizmoRenderSystem : System_
{
    public override void Update(List<Entity> entities)
    { 
        
    }
}


public class OldRenderSystem : System_
{
    private readonly ComponentManager<PositionComponent> _positionManager;
    private readonly ComponentManager<OldRenderComponent> _renderManager;
    private readonly Transform _cameraTrans;
    public OldRenderSystem(ComponentManager<PositionComponent> positionManager, ComponentManager<OldRenderComponent> renderManager, Transform cameraTrans)
    {
        this._positionManager = positionManager;
        this._renderManager = renderManager;
        this._cameraTrans = cameraTrans;
    }
    public override void Update(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (!_positionManager.HasComponent(entity.Id))
                continue;
            if (!_renderManager.HasComponent(entity.Id))
                continue;
            var position = _positionManager.GetComponent(entity.Id);
            var render = _renderManager.GetComponent(entity.Id);
            if (position.Size != render.Size)
                continue;
            for (int i = 0; i < position.Size; i++)
            {
                render.GameObjects[i].transform.position = new Vector3(position.X[i], position.Y[i], position.Z[i]);
                render.GameObjects[i].transform.LookAt(_cameraTrans);
            }
        }
    }
}
public class MovementSystem : System_
{
    private readonly ComponentManager<PositionComponent> _positionManager;
    private readonly ComponentManager<MovementComponent> _movementManager;
    public MovementSystem(ComponentManager<PositionComponent> positionManager, ComponentManager<MovementComponent> movementManager)
    {
        this._positionManager = positionManager;
        this._movementManager = movementManager;
    }
    public override void Update(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (!_positionManager.HasComponent(entity.Id))
                continue;
            if (!_movementManager.HasComponent(entity.Id))
                continue;
            var position = _positionManager.GetComponent(entity.Id);
            var movement = _movementManager.GetComponent(entity.Id);
            if (position.Size != movement.Size)
                continue;
            for (int i = 0; i < position.Size; i++)
            {
                position.X[i] += movement.XVelocity[i]*Time.deltaTime;
                position.Y[i] += movement.YVelocity[i]*Time.deltaTime;
                position.Z[i] += movement.ZVelocity[i]*Time.deltaTime;
                //Printer.print($"Entity {entity.Id} moved to({position.X[i]}, {position.Y[i]}, {position.Z[i]})");
            }
            _positionManager.AddComponent(entity.Id, position); // Update
            //position
        }
    }
}

public class PhysicsSystem : System_
{
    private readonly ComponentManager<PositionComponent> _positionManager;
    private readonly ComponentManager<MovementComponent> _movementManager;
    private readonly ComponentManager<PhysicsBallComponent> _physicsManager;
    private readonly TerrainCol _terrainCol;
        public PhysicsSystem(ComponentManager<PositionComponent> positionManager, ComponentManager<MovementComponent> movementManager, ComponentManager<PhysicsBallComponent> physicsManager,  TerrainCol terrainCol)
    {
        this._positionManager = positionManager;
        this._movementManager = movementManager;
        this._physicsManager  = physicsManager;
        this._terrainCol      = terrainCol;
    }
    public override void Update(List<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (!_positionManager.HasComponent(entity.Id))
                continue;
            if (!_movementManager.HasComponent(entity.Id))
                continue;
            if (!_physicsManager.HasComponent(entity.Id))
                continue;
            var pos    = _positionManager.GetComponent(entity.Id);
            var mov  = _movementManager.GetComponent(entity.Id);
            var phys = _physicsManager.GetComponent(entity.Id);

            for (var nr = 0; nr < pos.Size; nr++)
            {

                var simSteps = phys.SimSteps[nr];
                mov.YVelocity[nr] -= phys.Gravity * Time.deltaTime;
                for (int ss = 0; ss < simSteps; ss++)
                {
                    pos.X[nr] += (mov.XVelocity[nr] / simSteps)*Time.deltaTime;
                    pos.Y[nr] += (mov.YVelocity[nr] / simSteps)*Time.deltaTime;
                    pos.Z[nr] += (mov.ZVelocity[nr] / simSteps)*Time.deltaTime;
                    var posVec = new Vector3(pos.X[nr], pos.Y[nr]-phys.Radius[nr], pos.Z[nr]);
                    if(!_terrainCol.CheckBounds(posVec)) continue; //skip further checks if it's out of bounds for the terrain.

                    var heightAtPosition = _terrainCol.CheckHeightAtPosition(posVec);
                    if (heightAtPosition < posVec.y)
                    {
                        //Ball is still in the air.
                    }
                    else
                    {
                        // ball is under the terrain.
                        var diff = heightAtPosition - posVec.y;
                        pos.Y[nr] += diff;
                        var norms = _terrainCol.CheckNormalsAtPosition(posVec);
                        var movVec = new Vector3(mov.XVelocity[nr], mov.YVelocity[nr], mov.ZVelocity[nr]);
                        var newMovVec = movVec + norms;
                        newMovVec.Normalize();
                        var shortenedVec = (diff / Mathf.Sqrt(Vector3.Dot(movVec, movVec))) * movVec; // Ivo Terek (https://math.stackexchange.com/users/118056/ivo-terek), How to resize a vector to a specific length?, URL (version: 2014-08-14): https://math.stackexchange.com/q/897753
                        pos.X[nr] += (shortenedVec.x/ simSteps)*Time.deltaTime;
                        pos.Y[nr] += (shortenedVec.y/ simSteps)*Time.deltaTime;
                        pos.Z[nr] += (shortenedVec.z/ simSteps)*Time.deltaTime;
                        mov.YVelocity[nr] += shortenedVec.y + phys.Gravity * Time.deltaTime;
                        mov.XVelocity[nr] += shortenedVec.x;
                        mov.ZVelocity[nr] += shortenedVec.z;
                    }
                }
            }
        }
    }
}