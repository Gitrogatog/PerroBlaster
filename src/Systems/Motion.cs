// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using Microsoft.VisualBasic;
// using MoonTools.ECS;
// using MyGame;
// using MyGame.Components;
// using MyGame.Content;
// using MyGame.Relations;
// using MyGame.Spawn;
// using MyGame.Utility;

// namespace MyGame.Systems;

// public class Motion : MoonTools.ECS.System
// {
//     Filter VelocityFilter;
//     Filter InteractFilter;
//     Filter SolidFilter;
//     Filter CollidesWithSolidsFilter;
//     MoonTools.ECS.System OffsetSystem;

//     // SpatialHash<EntityCollisionHash> InteractSpatialHash = new SpatialHash<EntityCollisionHash>(0, 0, 1000, 1000, 32);
//     SpatialHashWithFlags InteractSpatialHash2 = new SpatialHashWithFlags(0, 0, 1000, 1000, 32);
//     SpatialHash<Entity> InteractSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
//     SpatialHash<Entity> SolidSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
//     public Motion(World world) : base(world)
//     {
//         GlobalCollision.Init();
//         OffsetSystem = new OffsetSystem(world);
//         VelocityFilter = FilterBuilder.Include<Position>().Include<Velocity>().Build();
//         InteractFilter = FilterBuilder.Include<Position>().Include<RectangleWithFlags>().Include<CanInteract>().Exclude<IgnoreCollision>().Build();
//         SolidFilter = FilterBuilder.Include<Position>().Include<RectangleWithFlags>().Include<Solid>().Exclude<IgnoreCollision>().Build();
//         CollidesWithSolidsFilter = FilterBuilder.Include<Position>().Include<RectangleWithFlags>().Include<CollidesWithSolids>().Exclude<IgnoreCollision>().Build();
//     }

//     void ClearInteractSpatialHash()
//     {
//         InteractSpatialHash.Clear();
//     }

//     void ClearSolidSpatialHash()
//     {
//         SolidSpatialHash.Clear();
//     }

//     RectangleWithFlags GetWorldRect(Position p, Rectangle r)
//     {
//         return new RectangleWithFlags(p.X + r.X, p.Y + r.Y, r.Width, r.Height, r.EffectorFlags, r.EffectedFlags);
//     }

//     enum SolidCheck
//     {
//         Miss, HitEntity, HitTilemap
//     }

//     (Entity other, SolidCheck hit) CheckSolidCollision(Entity e, Rectangle rect)
//     {
//         foreach ((var other, var otherRect) in SolidSpatialHash.Retrieve(e, rect))
//         {
//             if (rect.Intersects(otherRect))
//             {
//                 return (other, SolidCheck.HitEntity);
//             }
//         }

//         return (default, SolidCheck.Miss);
//     }

//     Position SweepTest(Entity e, float dt)
//     {
//         var velocity = Get<Velocity>(e);
//         var position = Get<Position>(e);
//         var r = Get<Rectangle>(e);

//         var movement = new Vector2(velocity.Value.X, velocity.Value.Y) * dt;
//         var targetPosition = position + movement;

//         var xEnum = new IntegerEnumerator(position.X, targetPosition.X);
//         var yEnum = new IntegerEnumerator(position.Y, targetPosition.Y);

//         int mostRecentValidXPosition = position.X;
//         int mostRecentValidYPosition = position.Y;

//         SolidCheck xHit = SolidCheck.Miss;
//         SolidCheck yHit = SolidCheck.Miss;

//         foreach (var x in xEnum)
//         {
//             var newPos = new Position(x, position.Y);
//             var rect = GetWorldRect(newPos, r);

//             (var other, var hit) = CheckSolidCollision(e, rect);

//             xHit = hit;

//             if (xHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) //Has<Solid>(other) &&
//             {
//                 movement.X = mostRecentValidXPosition - position.X;
//                 position = position.SetX(position.X); // truncates x coord
//                 break;
//             }

//             mostRecentValidXPosition = x;
//         }

//         foreach (var y in yEnum)
//         {
//             var newPos = new Position(mostRecentValidXPosition, y);
//             var rect = GetWorldRect(newPos, r);

//             (var other, var hit) = CheckSolidCollision(e, rect);
//             yHit = hit;

//             if (yHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) // && Has<Solid>(other)
//             {
//                 movement.Y = mostRecentValidYPosition - position.Y;
//                 position = position.SetY(position.Y); // truncates y coord
//                 break;
//             }

//             mostRecentValidYPosition = y;
//         }

//         return position + movement;
//     }
//     float GetMoveSpeed(Entity entity)
//     {
//         if (Has<MoveSpeed>(entity))
//         {
//             return Get<MoveSpeed>(entity).Value;
//         }
//         else if (Has<GroundAirMoveSpeed>(entity))
//         {
//             return Has<Grounded>(entity) ? Get<GroundAirMoveSpeed>(entity).Ground : Get<GroundAirMoveSpeed>(entity).Air;
//         }
//         return MoveConsts.MOVE_SPEED;
//     }
//     public bool GetIntendedMove(Entity entity, Position p, out Vector2 intendedMove) {
//         if(Has<CantMoveTimer>(entity)) {
//             intendedMove = Vector2.Zero;
//             return true;
//         }
//         if(Has<IntendedMoveOneFrame>(entity)){
//             intendedMove = Get<IntendedMoveOneFrame>(entity).Value;
//             Remove<IntendedMoveOneFrame>(entity);
//             return true;
//         }
//         if(Has<IntendedMove>(entity)){
//             intendedMove = Get<IntendedMove>(entity).Value;
//             return true;
//         }
//         if(Has<MoveTowardPlayer>(entity)) {
//             intendedMove = MathUtils.SafeNormalize(new Vector2(Globals.PlayerX - p.X, Globals.PlayerY - p.Y));
//             return true;
//         }
//         if(Has<MoveToPosition>(entity)) {
//             intendedMove = MathUtils.SafeNormalize(Get<MoveToPosition>(entity).Position - p);
//             return true;
//         }
//         intendedMove = default;
//         return false;
//     }
//     public Vector2 CalcIntendedVelocity(Entity entity, Vector2 vel, Vector2 intended, float dt)
//     {
//         float maxMoveSpeed = GetMoveSpeed(entity);
//         // Vector2 intended = Get<IntendedMove>(entity).Value;
//         Vector2 targetVelocity = intended * maxMoveSpeed;
//         if (TryGet(entity, out AccelParams accel))
//         {
//             // float accel = Get<AccelParams>(entity).GetAccel(Has<Grounded>(entity), targetVelocity > 0 && vel.X < 0 || targetVelocity < 0 && vel.X > 0);
//             // if(targetVelocity != 0 && MathF.Sign(vel.X) == MathF.Sign(intended) && )
//             return MathUtils.MoveTowards(vel, targetVelocity, accel.Value * dt);
//             // vel.X = MathUtils.LerpDecay(vel.X, targetVelocity, accel, dt);
//         }
//         else
//         {
//             return targetVelocity;
//         }
//     }

//     public override void Update(TimeSpan delta)
//     {
//         float dt = (float)delta.TotalSeconds;
//         ClearInteractSpatialHash();
//         ClearSolidSpatialHash();

//         // insert entities into the spatial hash

//         foreach (var entity in SolidFilter.Entities)
//         {
//             var position = Get<Position>(entity);
//             var rect = Get<Rectangle>(entity);
//             SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
//         }

//         foreach (var entity in VelocityFilter.Entities)
//         {
//             if (HasInRelation<Offset>(entity))
//             {
//                 continue;
//             }
//             var pos = Get<Position>(entity);
//             var vel = Get<Velocity>(entity).Value;
            
//             if (GetIntendedMove(entity, pos, out Vector2 intendedMove))
//             {
//                 vel = CalcIntendedVelocity(entity, vel, intendedMove, dt);
//                 Remove<IntendedMove>(entity);
//             }
//             if(Has<AccelerateDir>(entity)) {
//                 Vector2 accel = Get<AccelerateDir>(entity).Value;
//                 vel += accel * dt;
//             }

//             if (Has<Rectangle>(entity) && Has<CollidesWithSolids>(entity) && !Has<IgnoreCollision>(entity))
//             {
//                 var result = SweepTest(entity, (float)delta.TotalSeconds);
//                 Set(entity, result);
//             }
//             else
//             {
//                 var scaledVelocity = vel * (float)delta.TotalSeconds;
//                 Set(entity, pos + scaledVelocity);
//             }

//             Set(entity, new Velocity(vel));


//             // update spatial hashes

//             if (Has<CanInteract>(entity))
//             {
//                 var position = Get<Position>(entity);
//                 var rect = Get<Rectangle>(entity);

//                 InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
//             }

//             if (Has<Solid>(entity))
//             {
//                 var position = Get<Position>(entity);
//                 var rect = Get<Rectangle>(entity);
//                 SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
//             }
//         }

//         foreach (var entity in SolidFilter.Entities)
//         {
//             UnrelateAll<TouchingSolid>(entity);
//         }
//         EntityUtils.RemoveAll<TouchingWall>(World);
//         // MARK: Touch Solid Check
//         foreach (var entity in CollidesWithSolidsFilter.Entities)
//         {
//             var position = Get<Position>(entity);
//             var rectangle = Get<Rectangle>(entity);

//             var leftPos = new Position(position.X - 1, position.Y);
//             var rightPos = new Position(position.X + 1, position.Y);
//             var upPos = new Position(position.X, position.Y - 1);
//             var downPos = new Position(position.X, position.Y + 1);

//             var leftRectangle = GetWorldRect(leftPos, rectangle);
//             var rightRectangle = GetWorldRect(rightPos, rectangle);
//             var upRectangle = GetWorldRect(upPos, rectangle);
//             var downRectangle = GetWorldRect(downPos, rectangle);

//             var (leftOther, leftCollided) = CheckSolidCollision(entity, leftRectangle);
//             var (rightOther, rightCollided) = CheckSolidCollision(entity, rightRectangle);
//             var (upOther, upCollided) = CheckSolidCollision(entity, upRectangle);
//             var (downOther, downCollided) = CheckSolidCollision(entity, downRectangle);

//             if (leftCollided == SolidCheck.HitEntity)
//             {
//                 Relate(entity, leftOther, new TouchingSolid());
//             }

//             if (rightCollided == SolidCheck.HitEntity)
//             {
//                 Relate(entity, rightOther, new TouchingSolid());
//             }

//             if (upCollided == SolidCheck.HitEntity)
//             {
//                 Relate(entity, upOther, new TouchingSolid());
//             }
//             if (downCollided == SolidCheck.HitEntity)
//             {
//                 Relate(entity, downOther, new TouchingSolid());
//             }
//         }
//         // MARK: Offset
//         OffsetSystem.Update(delta);

//         // MARK: Interact
//         foreach (var entity in InteractFilter.Entities)
//         {
//             var position = Get<Position>(entity);
//             var rect = Get<Rectangle>(entity);

//             InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
//         }
//         foreach((var entityA, var entityB) in Relations<Colliding>()){
//             UnrelateAll<Colliding>(entityA);
//             UnrelateAll<Colliding>(entityB);
//         }
//         foreach (var entity in InteractFilter.Entities)
//         {
//             var position = Get<Position>(entity);
//             var rect = GetWorldRect(position, Get<Rectangle>(entity));

//             foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
//             {
//                 if (entity != other && rect.Intersects(otherRect))
//                 {
//                     Relate(entity, other, new Colliding());
//                 }
//             }
//         }

//         // start of attempt
//         // foreach (var entity in InteractFilter.Entities)
//         // {
//         //     var position = Get<Position>(entity);
//         //     var rect = Get<Rectangle>(entity);
//         //     (EffectorFlags effectorFlags, EffectedFlags effectedFlags) = Get<CollisionFlags>(entity);

//         //     InteractSpatialHash.Insert(entity, GetWorldRect(position, rect), effectorFlags, effectedFlags);
//         // }

//         // foreach(var collision in InteractSpatialHash.Collisions[EffectorFlags])
//         // end of attempt


//         // foreach (var entity in InteractFilter.Entities)
//         // {
//         //     foreach (var other in OutRelations<Colliding>(entity))
//         //     {
//         //         Unrelate<Colliding>(entity, other);
//         //     }
//         // }

//         // foreach (var entity in InteractFilter.Entities)
//         // {
//         //     var position = Get<Position>(entity);
//         //     var rect = GetWorldRect(position, Get<Rectangle>(entity));

//         //     foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
//         //     {
//         //         if (entity != other && rect.Intersects(otherRect))
//         //         {
//         //             Relate(entity, other, new Colliding());
//         //         }
//         //     }
//         // }
//     }
// }