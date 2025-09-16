using System;
using System.Numerics;
using Microsoft.VisualBasic;
using MoonTools.ECS;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Utility;

namespace MyGame.Systems;

public class Motion : MoonTools.ECS.System
{
    Filter VelocityFilter;
    Filter InteractFilter;
    Filter SolidFilter;
    Filter CollidesWithSolidsFilter;

    SpatialHash<Entity> InteractSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
    SpatialHash<Entity> SolidSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);

    public Motion(World world) : base(world)
    {
        VelocityFilter = FilterBuilder.Include<Position>().Include<Velocity>().Build();
        InteractFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CanInteract>().Exclude<IgnoreCollision>().Build();
        SolidFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<Solid>().Exclude<IgnoreCollision>().Build();
        CollidesWithSolidsFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CollidesWithSolids>().Exclude<IgnoreCollision>().Build();
    }

    void ClearCanBeHeldSpatialHash()
    {
        InteractSpatialHash.Clear();
    }

    void ClearSolidSpatialHash()
    {
        SolidSpatialHash.Clear();
    }

    Rectangle GetWorldRect(Position p, Rectangle r)
    {
        return new Rectangle(p.X + r.X, p.Y + r.Y, r.Width, r.Height);
    }

    enum SolidCheck
    {
        Miss, HitEntity, HitTilemap
    }

    (Entity other, SolidCheck hit) CheckSolidCollision(Entity e, Rectangle rect)
    {
        foreach (var (other, otherRect) in SolidSpatialHash.Retrieve(e, rect))
        {
            if (rect.Intersects(otherRect))
            {
                return (other, SolidCheck.HitEntity);
            }
        }

        return (default, SolidCheck.Miss);
    }

    Position SweepTest(Entity e, float dt)
    {
        var velocity = Get<Velocity>(e);
        var position = Get<Position>(e);
        var r = Get<Rectangle>(e);

        var movement = new Vector2(velocity.Value.X, velocity.Value.Y) * dt;
        var targetPosition = position + movement;

        var xEnum = new IntegerEnumerator(position.X, targetPosition.X);
        var yEnum = new IntegerEnumerator(position.Y, targetPosition.Y);

        int mostRecentValidXPosition = position.X;
        int mostRecentValidYPosition = position.Y;

        SolidCheck xHit = SolidCheck.Miss;
        SolidCheck yHit = SolidCheck.Miss;

        foreach (var x in xEnum)
        {
            var newPos = new Position(x, position.Y);
            var rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect);

            xHit = hit;

            if (xHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) //Has<Solid>(other) &&
            {
                movement.X = mostRecentValidXPosition - position.X;
                position = position.SetX(position.X); // truncates x coord
                break;
            }

            mostRecentValidXPosition = x;
        }

        foreach (var y in yEnum)
        {
            var newPos = new Position(mostRecentValidXPosition, y);
            var rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect);
            yHit = hit;

            if (yHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) // && Has<Solid>(other)
            {
                movement.Y = mostRecentValidYPosition - position.Y;
                position = position.SetY(position.Y); // truncates y coord
                break;
            }

            mostRecentValidYPosition = y;
        }

        return position + movement;
    }

    public Vector2 CalcIntendedVelocity(Entity entity, Vector2 vel, float dt)
    {
        float maxMoveSpeed = (Has<MoveSpeed>(entity) ? Get<MoveSpeed>(entity).Value : MoveConsts.MOVE_SPEED);
        float intended = Get<IntendedMove>(entity).Value;
        float targetVelocity = intended * maxMoveSpeed;
        if (Has<CanPivot>(entity))
        {
            (float decel, float pivotSpeed) = Get<CanPivot>(entity);
            // start pivot vs continue pivot
            bool didStartPivot = false;
            bool intendedOppositeToVelocity = MathF.Sign(vel.X) != MathF.Sign(intended);

            if (Math.Abs(vel.X) >= pivotSpeed && intendedOppositeToVelocity && Has<Grounded>(entity))
            {
                Set<IsPivoting>(entity);
                didStartPivot = true;
            }
            if (didStartPivot || Has<IsPivoting>(entity))
            {
                if (!intendedOppositeToVelocity)
                {
                    Remove<IsPivoting>(entity);
                }
                else
                {
                    vel.X = MathUtils.MoveTowards(vel.X, 0, decel * dt);
                    if (vel.X == 0)
                    {
                        Remove<IsPivoting>(entity);
                        vel.X = targetVelocity;
                    }
                    return vel;
                }
            }
        }
        if (Has<AccelParams>(entity))
        {
            float accel = Get<AccelParams>(entity).GetAccel(Has<Grounded>(entity), targetVelocity > 0 && vel.X < 0 || targetVelocity < 0 && vel.X > 0);
            vel.X = MathUtils.MoveTowards(vel.X, targetVelocity, accel * dt);
            // vel.X = MathUtils.LerpDecay(vel.X, targetVelocity, accel, dt);
        }
        else
        {
            vel.X = targetVelocity;
        }
        return vel;
    }

    public override void Update(TimeSpan delta)
    {
        float dt = (float)delta.TotalSeconds;
        ClearCanBeHeldSpatialHash();
        ClearSolidSpatialHash();

        foreach (var entity in InteractFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);

            InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
        }

        foreach (var entity in InteractFilter.Entities)
        {
            foreach (var other in OutRelations<Colliding>(entity))
            {
                Unrelate<Colliding>(entity, other);
            }
        }

        foreach (var entity in InteractFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = GetWorldRect(position, Get<Rectangle>(entity));

            foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
            {
                if (entity != other && rect.Intersects(otherRect))
                {
                    Relate(entity, other, new Colliding());
                }
            }
        }
        // insert entities into the spatial hash
        foreach (var entity in SolidFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);
            SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
        }

        foreach (var entity in VelocityFilter.Entities)
        {
            if (HasInRelation<Offset>(entity))
            {
                continue;
            }
            var pos = Get<Position>(entity);
            var vel = Get<Velocity>(entity).Value;

            // if (!Has<PreventInput>(entity))
            {
                if (Has<AttemptJumpThisFrame>(entity))
                {
                    if (Has<Grounded>(entity) || Has<IsPivoting>(entity))
                    {
                        if (Has<CanJump>(entity))
                        {
                            float jumpSpeed = Get<CanJump>(entity).Value;
                            if (Has<MaxSpeedJump>(entity) && Has<MoveSpeed>(entity))
                            {
                                float maxJumpSpeed = Get<MaxSpeedJump>(entity).Value;
                                if (Has<IsPivoting>(entity))
                                {
                                    vel.Y = -maxJumpSpeed;
                                }
                                else
                                {
                                    vel.Y = -MathUtils.Lerp(jumpSpeed, maxJumpSpeed, MathF.Abs(vel.X / Get<MoveSpeed>(entity).Value));
                                }
                            }
                            else
                            {
                                vel.Y = -jumpSpeed;
                            }
                            Remove<Grounded>(entity);
                            Set(entity, new IsJumping());
                            Set(CreateEntity(), new PlayStaticSFX(StaticAudio.PickUp));
                            Remove<IsPivoting>(entity);
                        }
                    }
                    // vel.Y = -Get<CanJump>(entity).Value;
                    else if (Has<CanWallJump>(entity) && Has<TouchingWall>(entity))
                    {
                        bool touchingRight = Get<TouchingWall>(entity).Right;
                        vel.Y = -MoveConsts.WALLJUMP_SPEED_Y;
                        vel.X = touchingRight ? -MoveConsts.WALLJUMP_SPEED_X : MoveConsts.WALLJUMP_SPEED_X;
                        Set(entity, new Facing(!touchingRight));
                        Set(entity, new IsJumping());
                        Set(CreateEntity(), new PlayStaticSFX(StaticAudio.PickUp));
                        Remove<CoyoteGrounded>(entity);
                    }

                    Remove<AttemptJumpThisFrame>(entity);
                }
                if (Has<IntendedMove>(entity))
                {
                    vel = CalcIntendedVelocity(entity, vel, dt);
                    Remove<IntendedMove>(entity);
                }
            }

            if (Has<Gravity>(entity))
            {
                if (Has<Grounded>(entity) || Has<IsPivoting>(entity))
                {
                    vel.Y = MathF.Min(vel.Y, 0);
                    Remove<Grounded>(entity); // removes grounded on all entities before checking again to see if they are grounded this frame
                }
                else
                {
                    bool isWallSliding = Has<CanWallJump>(entity) && Has<TouchingWall>(entity) && vel.Y >= 0;
                    float grav = isWallSliding ? MoveConsts.WALL_GRAVITY : MoveConsts.GRAVITY; //Get<Gravity>(entity).Value;
                    vel.Y = MathF.Min(vel.Y + grav * (float)delta.TotalSeconds, isWallSliding ? MoveConsts.WALL_MAX_FALL_SPEED : MoveConsts.MAX_FALL_SPEED);
                }
            }


            if (Has<Rectangle>(entity) && Has<CollidesWithSolids>(entity) && !Has<IgnoreCollision>(entity))
            {
                var result = SweepTest(entity, (float)delta.TotalSeconds);
                Set(entity, result);
            }
            else
            {
                var scaledVelocity = vel * (float)delta.TotalSeconds;
                Set(entity, pos + scaledVelocity);
            }

            Set(entity, new Velocity(vel));


            // update spatial hashes

            if (Has<CanInteract>(entity))
            {
                var position = Get<Position>(entity);
                var rect = Get<Rectangle>(entity);

                InteractSpatialHash.Insert(entity, GetWorldRect(position, rect));
            }

            if (Has<Solid>(entity))
            {
                var position = Get<Position>(entity);
                var rect = Get<Rectangle>(entity);
                SolidSpatialHash.Insert(entity, GetWorldRect(position, rect));
            }
        }

        foreach (var entity in SolidFilter.Entities)
        {
            UnrelateAll<TouchingSolid>(entity);
        }
        EntityUtils.RemoveAll<TouchingWall>(World);

        foreach (var entity in CollidesWithSolidsFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rectangle = Get<Rectangle>(entity);

            var leftPos = new Position(position.X - 1, position.Y);
            var rightPos = new Position(position.X + 1, position.Y);
            var upPos = new Position(position.X, position.Y - 1);
            var downPos = new Position(position.X, position.Y + 1);

            var leftRectangle = GetWorldRect(leftPos, rectangle);
            var rightRectangle = GetWorldRect(rightPos, rectangle);
            var upRectangle = GetWorldRect(upPos, rectangle);
            var downRectangle = GetWorldRect(downPos, rectangle);

            var (leftOther, leftCollided) = CheckSolidCollision(entity, leftRectangle);
            var (rightOther, rightCollided) = CheckSolidCollision(entity, rightRectangle);
            var (upOther, upCollided) = CheckSolidCollision(entity, upRectangle);
            var (downOther, downCollided) = CheckSolidCollision(entity, downRectangle);

            if (leftCollided == SolidCheck.HitEntity)
            {
                Relate(entity, leftOther, new TouchingSolid());
            }

            if (rightCollided == SolidCheck.HitEntity)
            {
                Relate(entity, rightOther, new TouchingSolid());
            }

            if (upCollided == SolidCheck.HitEntity)
            {
                Relate(entity, upOther, new TouchingSolid());
            }
            if (downCollided == SolidCheck.HitEntity)
            {
                Relate(entity, downOther, new TouchingSolid());
            }
            if (Has<Velocity>(entity))
            {
                var velocity = Get<Velocity>(entity).Value;
                if (leftCollided != SolidCheck.Miss)
                {
                    if (velocity.X < 0)
                    {
                        if (Has<BouncesOffWalls>(entity) && -Get<BouncesOffWalls>(entity).MinSpeed >= velocity.X)
                        {
                            Set(entity, new Velocity(-velocity.X, velocity.Y));
                        }
                        else
                        {
                            Set(entity, new Velocity(0, velocity.Y));
                        }
                    }
                    if (Has<Facing>(entity) && !Get<Facing>(entity).Right)
                    {
                        Set(entity, new TouchingWall(false));
                    }
                }
                if (rightCollided != SolidCheck.Miss)
                {
                    if (velocity.X > 0)
                    {
                        if (Has<BouncesOffWalls>(entity) && Get<BouncesOffWalls>(entity).MinSpeed <= velocity.X)
                        {
                            Set(entity, new Velocity(-velocity.X, velocity.Y));
                        }
                        else
                        {
                            Set(entity, new Velocity(0, velocity.Y));
                        }

                    }
                    if (Has<Facing>(entity) && Get<Facing>(entity).Right)
                    {
                        Set(entity, new TouchingWall(true));
                    }
                }
                if (upCollided != SolidCheck.Miss)
                {
                    if (velocity.Y < 0)
                    {
                        Set(entity, new Velocity(velocity.X, 0));
                    }
                }
                if (downCollided != SolidCheck.Miss && Has<Gravity>(entity) && velocity.Y >= -1)
                {
                    Set(entity, new Grounded());
                    Remove<IsJumping>(entity);
                    if (Has<CanCoyoteJump>(entity))
                    {
                        Set(entity, new CoyoteGrounded());
                    }
                }
            }
        }
    }
}