using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualBasic;
using MoonTools.ECS;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Spawn;
using MyGame.Utility;

namespace MyGame.Systems;

public class MotionWithFlags : MoonTools.ECS.System
{
    Filter VelocityFilter;
    Filter InteractFilter;
    Filter SolidFilter;
    Filter CollidesWithSolidsFilter;
    MoonTools.ECS.System OffsetSystem;

    // SpatialHash<EntityCollisionHash> InteractSpatialHash = new SpatialHash<EntityCollisionHash>(0, 0, 1000, 1000, 32);
    SpatialHashWithFlags InteractSpatialHash = new SpatialHashWithFlags(0, 0, 1000, 1000, 32);
    SpatialHashWithFlags SolidSpatialHash = new SpatialHashWithFlags(0, 0, 1000, 1000, 32);
    SpatialHashWithFlags CollidesWithSolidSpatialHash = new SpatialHashWithFlags(0, 0, 1000, 1000, 32);
    List<Entity> MovingSolids = new List<Entity>();
    List<Entity> MovingPushables = new List<Entity>();
    List<Entity> MovingOthers = new List<Entity>();
    List<Entity> PushableEntities = new List<Entity>();
    List<Entity> RidingEntities = new List<Entity>();
    // SpatialHash<Entity> InteractSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
    // SpatialHash<Entity> SolidSpatialHash = new SpatialHash<Entity>(0, 0, 1000, 1000, 32);
    public MotionWithFlags(World world) : base(world)
    {
        GlobalCollision.Init();
        OffsetSystem = new OffsetSystem(world);
        VelocityFilter = FilterBuilder.Include<Position>().Include<Velocity>().Build();
        InteractFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CanInteract>().Build();
        SolidFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<Solid>().Build();
        CollidesWithSolidsFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<CollidesWithSolids>().Build();
    }

    void ClearInteractSpatialHash()
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
    FloatRectangle GetWorldFloatRect(Position p, Rectangle r) => new FloatRectangle(p.RawPosition.X + r.X, p.RawPosition.Y + r.Y, r.Width, r.Height);

    enum SolidCheck
    {
        Miss, HitEntity, HitTilemap
    }

    (Entity other, SolidCheck hit) CheckSolidCollision(Entity e, Rectangle rect, EffectorFlags flags, Vector2 moveDir)
    {
        foreach ((Entity other, Rectangle otherRect, EffectorFlags otherEffectorFlags, EffectedFlags otherEffectedFlags) in SolidSpatialHash.Retrieve(e, rect))
        {
            if(((long)flags & (long)otherEffectedFlags) != 0) {
                if((otherEffectedFlags & EffectedFlags.IsDownPlatform) != 0) {
                    if(moveDir.Y > 0 && 
                        rect.Bottom - 1 == otherRect.Top &&
                        Rectangle.HorizontalOverlap(rect, otherRect)
                    )
                    {
                        return (other, SolidCheck.HitEntity);
                    }
                }
                else if (Rectangle.TestOverlap(rect, otherRect))
                {
                    return (other, SolidCheck.HitEntity);
                }
            }
            
        }

        return (default, SolidCheck.Miss);
    }

    Position CollidesWithSolidSweepTest(Entity e, float dt) {
        var velocity = Get<Velocity>(e).Value;
        var position = Get<Position>(e);
        var rect = Get<Rectangle>(e);
        return CollidesWithSolidSweepTest(e, position, velocity * dt, rect);
    }

    Position CollidesWithSolidSweepTest(Entity e, Position position, Vector2 movement, Rectangle r)
    {
        
        // var r = Get<Rectangle>(e);
        var flags = Has<EffectorFlags>(e) ? Get<EffectorFlags>(e) : EffectorFlags.None;

        // var movement = new Vector2(velocity.X, velocity.Y) * dt;
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

            (var other, var hit) = CheckSolidCollision(e, rect, flags, new Vector2(MathF.Sign(movement.X), 0));

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

            (var other, var hit) = CheckSolidCollision(e, rect, flags, new Vector2(0, movement.Y));
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
    // just moves from start to finish. use for solids that dont collide w/ other solids
    Position SolidMovement(Entity e, float dt) {
        var velocity = Get<Velocity>(e);
        var position = Get<Position>(e);
        var r = Get<Rectangle>(e);
        var rect = GetWorldRect(position, r);
        var flags = Has<EffectedFlags>(e) ? Get<EffectedFlags>(e) : EffectedFlags.None;

        var movement = new Vector2(velocity.Value.X, velocity.Value.Y) * dt;
        var targetPosition = position + movement;

        bool isDownPlatform = (flags & EffectedFlags.IsDownPlatform) != 0; 

        if(movement.X != 0) {
            PushableEntities.Clear();
            RidingEntities.Clear();

            var endRect = GetWorldRect(new Position(targetPosition.X, position.Y), r);
            var floatRect = GetWorldFloatRect(new Position(targetPosition.X, position.Y), r);

            foreach(var pushable in CollidesWithSolidsFilter.Entities) {
                var pushFlags = Get<EffectorFlags>(pushable);
                if(((long) flags & (long) pushFlags) != 0) {
                    var pushPos = Get<Position>(pushable);
                    var pushBaseRect = Get<Rectangle>(pushable);
                    var pushRect = GetWorldRect(pushPos, pushBaseRect);
                    // 
                    if(Rectangle.TestOverlap(endRect, pushRect) && !isDownPlatform) {
                        // var pushRect = Get<Rectangle>(pushable);
                        // var pushWorldRect = GetWorldRect(pushPos, pushRect);
                        var pushDir = movement.X < 0 ?
                            new Vector2(endRect.Left - pushRect.Right, 0)
                            : new Vector2(endRect.Right - pushRect.Left, 0);
                        var result = CollidesWithSolidSweepTest(pushable, pushPos, pushDir, pushBaseRect);
                        Set(pushable, result);
                    }
                    else if(
                        // Rectangle.BottomEdgeOverlap(pushRect, rect)
                        Rectangle.TestOverlap(
                            new Rectangle(rect.X, rect.Top, rect.Width, 1),
                            new Rectangle(pushRect.X, pushRect.Bottom, pushRect.Width, 1))
                    ) {
                        // var ridingPos = Get<Position>(pushable);
                        // var ridingRect = Get<Rectangle>(pushable);
                        var result = CollidesWithSolidSweepTest(pushable, pushPos, new Vector2(targetPosition.X - position.X, 0), pushBaseRect);
                        Set(pushable, result);
                    }
                }
            }
        }

        if(movement.Y != 0) {
            PushableEntities.Clear();
            RidingEntities.Clear();

            var endRect = GetWorldRect(targetPosition, r);
            var floatRect = GetWorldFloatRect(targetPosition, r);
            var startFloatRect = GetWorldFloatRect(new Position(targetPosition.X, position.Y), r);
            var startRect = GetWorldRect(new Position(targetPosition.X, position.Y), r);

            foreach(var pushable in CollidesWithSolidsFilter.Entities) {
                var pushFlags = Get<EffectorFlags>(pushable);
                if(((long) flags & (long) pushFlags) != 0) {
                    var pushPos = Get<Position>(pushable);
                    var pushBaseRect = Get<Rectangle>(pushable);
                    var pushRect = GetWorldRect(pushPos, pushBaseRect);
                    var pushFloatRect = GetWorldFloatRect(pushPos, pushBaseRect);
                    // if(Rectangle.TestOverlap(rect, new Rectangle(pushRect.X, )))
                    if(
                        Rectangle.TestOverlap(endRect, pushRect) &&
                        (!isDownPlatform || (movement.Y < 0 && !Rectangle.TestOverlap(startRect, pushRect)))
                        // FloatRectangle.TestOverlap(floatRect, pushFloatRect)
                    ) {
                        // var pushDir = movement.Y < 0 ?
                        //     new Vector2(0, floatRect.Top - pushFloatRect.Bottom)
                        //     : new Vector2(0, floatRect.Bottom - pushFloatRect.Top);
                        var pushDir = movement.Y < 0 ?
                            new Vector2(0, endRect.Top - pushRect.Bottom)
                            : new Vector2(0, endRect.Bottom - pushRect.Top);
                        var result = CollidesWithSolidSweepTest(pushable, pushPos, pushDir, pushBaseRect);
                        Set(pushable, result);
                    }
                    else if(
                        // Rectangle.BottomEdgeOverlap(pushRect, startRect)
                        // Rectangle.TestOverlap(startRect, new Rectangle(pushRect.X, pushRect.Bottom + 1, pushRect.Width, pushRect.Height + 1))
                        Rectangle.TestOverlap(
                            new Rectangle(rect.X, rect.Top, rect.Width, 1),
                            new Rectangle(pushRect.X, pushRect.Bottom, pushRect.Width, 1))
                        // FloatRectangle.TestOverlap(startFloatRect, new FloatRectangle(pushFloatRect.X, pushFloatRect.Y, pushFloatRect.Width, pushFloatRect.Height + 1))
                    ) {
                        // var ridingPos = Get<Position>(pushable);
                        // var ridingRect = Get<Rectangle>(pushable);
                        var result = CollidesWithSolidSweepTest(pushable, pushPos, new Vector2(0, targetPosition.Y - position.Y), pushBaseRect);
                        Set(pushable, result);
                    }
                }
            }
        }

        
        // move this entity on this axis
        // create rectangle: start left to end right
        // loop over collidesw/solids entities
        // for pushables: push them until flsuh w/ edge
        // for riding: push them full amount


        return position + movement;
    }
    // checks each pixel and moves accordingly. solids that collide w/ other solids
    Position SolidSweepTest(Entity e, float dt)
    {
        var velocity = Get<Velocity>(e);
        var position = Get<Position>(e);
        var r = Get<Rectangle>(e);
        var rect = GetWorldRect(position, r);
        var flags = Has<EffectorFlags>(e) ? Get<EffectorFlags>(e) : EffectorFlags.None;

        var movement = new Vector2(velocity.Value.X, velocity.Value.Y) * dt;
        var targetPosition = position + movement;

        var xEnum = new IntegerEnumerator(position.X, targetPosition.X);
        var yEnum = new IntegerEnumerator(position.Y, targetPosition.Y);

        int mostRecentValidXPosition = position.X;
        int mostRecentValidYPosition = position.Y;

        SolidCheck xHit = SolidCheck.Miss;
        SolidCheck yHit = SolidCheck.Miss;

        PushableEntities.Clear();
        RidingEntities.Clear();

        foreach(var pushable in CollidesWithSolidsFilter.Entities) {
            var pushFlags = Get<EffectedFlags>(pushable);
            if(((long) flags & (long) pushFlags) != 0) {
                var pushRect = GetWorldRect(Get<Position>(pushable), Get<Rectangle>(pushable));
                if(Rectangle.TestOverlap(rect, pushRect)) {
                    PushableEntities.Add(pushable);
                }
                else if(Rectangle.TestOverlap(rect, new Rectangle(pushRect.X, pushRect.Y, pushRect.Width, pushRect.Height + 1))) {
                    RidingEntities.Add(pushable);
                }
            }
        }

        // Vector2 pushDir = new Vector2(Math.Sign(movement.X), 0);

        foreach (var x in xEnum)
        {
            var newPos = new Position(x, position.Y);
            rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect, flags, new Vector2(movement.X, 0));

            xHit = hit;

            if (xHit != SolidCheck.Miss && Has<CollidesWithSolids>(e))
            {
                movement.X = mostRecentValidXPosition - position.X;
                position = position.SetX(position.X); // truncates x coord
                break;
            }

            foreach(var pushable in PushableEntities) {
                var pushPos = Get<Position>(pushable);
                var pushRect = Get<Rectangle>(pushable);
                var pushWorldRect = GetWorldRect(pushPos, pushRect);
                if(Rectangle.TestOverlap(rect, pushWorldRect)) {
                    var pushDir = movement.X < 0 ?
                        new Vector2(rect.Left - pushWorldRect.Right, 0)
                        : new Vector2(rect.Right - pushWorldRect.Left, 0);
                    var result = CollidesWithSolidSweepTest(pushable, pushPos, pushDir, pushRect);
                    Set(pushable, result);
                }
            }

            mostRecentValidXPosition = x;
        }

        Vector2 rideDir = new Vector2(movement.X, 0);

        foreach(var riding in RidingEntities) {
            var ridingPos = Get<Position>(riding);
            var ridingRect = Get<Rectangle>(riding);
            var result = CollidesWithSolidSweepTest(riding, ridingPos, rideDir, ridingRect);
            Set(riding, result);
        }

        // pushDir = new Vector2(0, Math.Sign(movement.Y));

        foreach (var y in yEnum)
        {
            var newPos = new Position(mostRecentValidXPosition, y);
            rect = GetWorldRect(newPos, r);

            (var other, var hit) = CheckSolidCollision(e, rect, flags, new Vector2(0, movement.Y));
            yHit = hit;

            if (yHit != SolidCheck.Miss && Has<CollidesWithSolids>(e)) // && Has<Solid>(other)
            {
                movement.Y = mostRecentValidYPosition - position.Y;
                position = position.SetY(position.Y); // truncates y coord
                break;
            }

            foreach(var pushable in PushableEntities) {
                var pushPos = Get<Position>(pushable);
                var pushRect = Get<Rectangle>(pushable);
                var pushWorldRect = GetWorldRect(pushPos, pushRect);
                if(Rectangle.TestOverlap(rect, pushWorldRect)) {
                    var pushDir = movement.Y < 0 ?
                        new Vector2(0, rect.Top - pushWorldRect.Bottom)
                        : new Vector2(0, rect.Bottom - pushWorldRect.Top);
                    var result = CollidesWithSolidSweepTest(pushable, pushPos, pushDir, pushRect);
                    Set(pushable, result);
                }
            }
            mostRecentValidYPosition = y;
        }

        rideDir = new Vector2(0, movement.Y);

        foreach(var riding in RidingEntities) {
            var ridingPos = Get<Position>(riding);
            var ridingRect = Get<Rectangle>(riding);
            var result = CollidesWithSolidSweepTest(riding, ridingPos, rideDir, ridingRect);
            Set(riding, result);
        }

        return position + movement;
    }
    float GetMoveSpeed(Entity entity)
    {
        if (Has<MoveSpeed>(entity))
        {
            return Get<MoveSpeed>(entity).Value;
        }
        else if (Has<GroundAirMoveSpeed>(entity))
        {
            return Has<Grounded>(entity) ? Get<GroundAirMoveSpeed>(entity).Ground : Get<GroundAirMoveSpeed>(entity).Air;
        }
        return MoveConsts.MOVE_SPEED;
    }
    public bool GetIntendedMove(Entity entity, Position p, out Vector2 intendedMove) {
        if(Has<CantMoveTimer>(entity)) {
            intendedMove = Vector2.Zero;
            return true;
        }
        if(Has<IntendedMoveOneFrame>(entity)){
            intendedMove = Get<IntendedMoveOneFrame>(entity).Value;
            Remove<IntendedMoveOneFrame>(entity);
            return true;
        }
        if(Has<IntendedMove>(entity)){
            intendedMove = Get<IntendedMove>(entity).Value;
            Remove<IntendedMove>(entity);
            return true;
        }
        if(Has<MoveTowardPlayer>(entity)) {
            intendedMove = MathUtils.SafeNormalize(new Vector2(Globals.PlayerX - p.X, Globals.PlayerY - p.Y));
            return true;
        }
        if(Has<MoveToPosition>(entity)) {
            intendedMove = MathUtils.SafeNormalize(Get<MoveToPosition>(entity).Position - p);
            return true;
        }
        intendedMove = default;
        return false;
    }
    public Vector2 CalcIntendedVelocity(Entity entity, Vector2 vel, Vector2 intended, float dt)
    {
        float maxMoveSpeed = GetMoveSpeed(entity);
        // Vector2 intended = Get<IntendedMove>(entity).Value;
        Vector2 targetVelocity = intended * maxMoveSpeed;
        if (TryGet(entity, out AccelParams accel))
        {
            // float accel = Get<AccelParams>(entity).GetAccel(Has<Grounded>(entity), targetVelocity > 0 && vel.X < 0 || targetVelocity < 0 && vel.X > 0);
            // if(targetVelocity != 0 && MathF.Sign(vel.X) == MathF.Sign(intended) && )
            return MathUtils.MoveTowards(vel, targetVelocity, accel.Value * dt);
            // vel.X = MathUtils.LerpDecay(vel.X, targetVelocity, accel, dt);
        }
        else
        {
            return targetVelocity;
        }
    }
    public (Position, Vector2) CalcVelocity(Entity entity, float dt) {
        var pos = Get<Position>(entity);
        var vel = Get<Velocity>(entity).Value;
        
        bool ignoreIntendedY = false;
        if(Has<AttemptJumpThisFrame>(entity) && Has<Grounded>(entity)) {
            float jumpSpeed = Has<CanJump>(entity) ? Get<CanJump>(entity).Value : MoveConsts.JUMP_STRENGTH_PLAYER;
            vel.Y = -jumpSpeed;
        }
        if(Has<Gravity>(entity)) {
            vel.Y += Get<Gravity>(entity).Value * dt;
            ignoreIntendedY = true;
        }
        
        if (GetIntendedMove(entity, pos, out Vector2 intendedMove))
        {
            var intendedVelocity = CalcIntendedVelocity(entity, vel, intendedMove, dt);
            if(ignoreIntendedY) {
                vel.X = intendedVelocity.X;
            }
            else {
                vel = intendedVelocity;
            }
            Remove<IntendedMove>(entity);
        }
        if(Has<AccelerateDir>(entity)) {
            Vector2 accel = Get<AccelerateDir>(entity).Value;
            vel += accel * dt;
        }
        
        Set(entity, new Velocity(vel));
        return (pos, vel);
    }

    bool ShouldSkipMovement(Entity entity) {
        return HasInRelation<Offset>(entity);
    }

    // public void PerformMovement(Entity entity, float dt) {
    //     if (HasInRelation<Offset>(entity))
    //     {
    //         return;
    //     }
    //     (var pos, var vel) = CalcVelocity(entity, dt);
        

    //     if (Has<Rectangle>(entity) && Has<CollidesWithSolids>(entity) && !Has<IgnoreCollision>(entity))
    //     {
    //         var result = SweepTest(entity, dt);
    //         Set(entity, result);
    //     }
    //     else
    //     {
    //         var scaledVelocity = vel * dt;
    //         Set(entity, pos + scaledVelocity);
    //     }

        
    // }

    public override void Update(TimeSpan delta)
    {
        float dt = (float)delta.TotalSeconds;
        ClearInteractSpatialHash();
        ClearSolidSpatialHash();
        MovingSolids.Clear();
        MovingPushables.Clear();
        MovingOthers.Clear();

        // insert entities into the spatial hash

        // foreach (var entity in SolidFilter.Entities)
        // {
        //     var position = Get<Position>(entity);
        //     var rect = Get<Rectangle>(entity);
        //     var flags = Get<EffectedFlags>(entity);
        //     SolidSpatialHash.InsertNoCollide(entity, GetWorldRect(position, rect), EffectorFlags.None, flags);
        // }

        // get entities in each category
        foreach(var entity in VelocityFilter.Entities) {
            if(Has<Solid>(entity)) MovingSolids.Add(entity);
            else if(Has<CollidesWithSolids>(entity)) MovingPushables.Add(entity);
            else MovingOthers.Add(entity);
        }

        // add unmoving solids to hash
        foreach (var entity in SolidFilter.Entities)
        {
            // if(Has<Velocity>(entity)) continue;
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);
            var flags = Get<EffectedFlags>(entity);
            SolidSpatialHash.InsertNoCollide(entity, GetWorldRect(position, rect), EffectorFlags.None, flags);
        }

        // SolidSpatialHash.PrintCellsAndId();

        // move solids before filling interact hash
        // FUTURE: how to handle collisions btwn moving solids?
        foreach(var entity in MovingSolids) {
            if(ShouldSkipMovement(entity)) continue;
            var pos = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);
            SolidSpatialHash.RemoveEntry(entity, GetWorldRect(pos, rect));
            // SolidSpatialHash.PrintCellsAndId();
            (pos, var vel) = CalcVelocity(entity, dt);

            // need to perform push checks on colliding entities
            var result = SolidMovement(entity, (float)delta.TotalSeconds);
            Set(entity, result);
            SolidSpatialHash.InsertNoCollide(entity, GetWorldRect(result, rect), EffectorFlags.None, Get<EffectedFlags>(entity));
            // var scaledVelocity = vel * (float)delta.TotalSeconds;
            // Set(entity, pos + scaledVelocity);
        }        
        // move pushables after solids are in place
        foreach(var entity in MovingPushables) {
            if(ShouldSkipMovement(entity)) continue;
            (var pos, var vel) = CalcVelocity(entity, dt);

            var result = CollidesWithSolidSweepTest(entity, (float)delta.TotalSeconds);
            Set(entity, result);
        }

        // these dont care about solids
        foreach(var entity in MovingOthers) {
            if(ShouldSkipMovement(entity)) continue;
            (var pos, var vel) = CalcVelocity(entity, dt);

            var scaledVelocity = vel * (float)delta.TotalSeconds;
            Set(entity, pos + scaledVelocity);
        }

        foreach (var entity in SolidFilter.Entities)
        {
            UnrelateAll<TouchingSolid>(entity);
        }
        EntityUtils.RemoveAll<TouchingWall>(World);
        // MARK: Touch Solid Check
        RemoveAll<Grounded>();
        foreach (var entity in CollidesWithSolidsFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rectangle = Get<Rectangle>(entity);
            var flags = Get<EffectorFlags>(entity);

            var leftPos = new Position(position.X - 1, position.Y);
            var rightPos = new Position(position.X + 1, position.Y);
            var upPos = new Position(position.X, position.Y - 1);
            var downPos = new Position(position.X, position.Y + 1);

            var leftRectangle = GetWorldRect(leftPos, rectangle);
            var rightRectangle = GetWorldRect(rightPos, rectangle);
            var upRectangle = GetWorldRect(upPos, rectangle);
            var downRectangle = GetWorldRect(downPos, rectangle);

            var (leftOther, leftCollided) = CheckSolidCollision(entity, leftRectangle, flags, new Vector2(-1, 0));
            var (rightOther, rightCollided) = CheckSolidCollision(entity, rightRectangle, flags, new Vector2(1, 0));
            var (upOther, upCollided) = CheckSolidCollision(entity, upRectangle, flags, new Vector2(0, -1));
            var (downOther, downCollided) = CheckSolidCollision(entity, downRectangle, flags, new Vector2(0, 1));

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
            if(Has<Velocity>(entity)) {
                var velocity = Get<Velocity>(entity).Value;
                bool changeVelocity = false;
                
                if(downCollided != SolidCheck.Miss && velocity.Y > 0) {
                    changeVelocity = true;
                    velocity.Y = 0;
                    Set(entity, new Grounded());
                }
                else if(upCollided != SolidCheck.Miss && velocity.Y < 0) {
                    changeVelocity = true;
                    velocity.Y = 0;
                }
                if(leftCollided != SolidCheck.Miss && velocity.X < 0) {
                    changeVelocity = true;
                    velocity.X = 0;
                }
                else if(rightCollided != SolidCheck.Miss && velocity.X > 0) {
                    changeVelocity = true;
                    velocity.X = 0;
                }
                if(changeVelocity) {
                    Set(entity, new Velocity(velocity));
                }

            }
        }
        // MARK: Offset
        OffsetSystem.Update(delta);

        // MARK: Interact
        foreach (var entity in InteractFilter.Entities)
        {
            var position = Get<Position>(entity);
            var rect = Get<Rectangle>(entity);
            var effectorFlags = Get<EffectorFlags>(entity);
            var effectedFlags = Get<EffectedFlags>(entity);

            InteractSpatialHash.InsertAndCollide(entity, GetWorldRect(position, rect), effectorFlags, effectedFlags);
        }
        // foreach((var entityA, var entityB) in Relations<Colliding>()){
        //     UnrelateAll<Colliding>(entityA);
        //     UnrelateAll<Colliding>(entityB);
        // }
        int flagID = 0;
        foreach(var stuff in InteractSpatialHash.Collisions) {
            flagID++;
        }
        foreach((var source, var target) in Collisions(EffectorFlags.CanDamage)) {
            Console.WriteLine($"damage interact btwn {source} and {target}");
            if((Has<OwnedByEnemy>(source) && Has<OwnedByPlayer>(target)) ||
                Has<OwnedByPlayer>(source) && Has<OwnedByEnemy>(target)) {
                // if(!Has< Has<Health>(target)) {

                // }
                Console.WriteLine("dealing damage!");
            }
        }
        // foreach (var entity in InteractFilter.Entities)
        // {
        //     var position = Get<Position>(entity);
        //     var rect = GetWorldRect(position, Get<Rectangle>(entity));

        //     foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
        //     {
        //         if (entity != other && rect.Intersects(otherRect))
        //         {
        //             Relate(entity, other, new Colliding());
        //         }
        //     }
        // }

        // start of attempt
        // foreach (var entity in InteractFilter.Entities)
        // {
        //     var position = Get<Position>(entity);
        //     var rect = Get<Rectangle>(entity);
        //     (EffectorFlags effectorFlags, EffectedFlags effectedFlags) = Get<CollisionFlags>(entity);

        //     InteractSpatialHash.Insert(entity, GetWorldRect(position, rect), effectorFlags, effectedFlags);
        // }

        // foreach(var collision in InteractSpatialHash.Collisions[EffectorFlags])
        // end of attempt


        // foreach (var entity in InteractFilter.Entities)
        // {
        //     foreach (var other in OutRelations<Colliding>(entity))
        //     {
        //         Unrelate<Colliding>(entity, other);
        //     }
        // }

        // foreach (var entity in InteractFilter.Entities)
        // {
        //     var position = Get<Position>(entity);
        //     var rect = GetWorldRect(position, Get<Rectangle>(entity));

        //     foreach (var (other, otherRect) in InteractSpatialHash.Retrieve(rect))
        //     {
        //         if (entity != other && rect.Intersects(otherRect))
        //         {
        //             Relate(entity, other, new Colliding());
        //         }
        //     }
        // }
    }
    List<Collision> Collisions(EffectorFlags flag) => InteractSpatialHash.Collisions[GlobalCollision.flagToID[flag]];
}