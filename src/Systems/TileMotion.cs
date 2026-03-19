using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Utility;
namespace MyGame.Systems;

public class TileMotion : MoonTools.ECS.System
{
    private Filter MovementFilter;

    public TileMotion(World world) : base(world)
    {
        MovementFilter = FilterBuilder
            .Include<TilePosition>()
            .Include<MoveToTile>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        while(Some<AttemptMoveToTile>()) {
            var entity = GetSingletonEntity<AttemptMoveToTile>();
            (int x, int y) = Get<AttemptMoveToTile>(entity);
            Remove<AttemptMoveToTile>(entity);
            if(Has<MoveToTile>(entity)) {
                continue;
            }
            Set(entity, new FacingDirection(x, y));
            float moveSpeed = Has<MoveSpeed>(entity) ? Get<MoveSpeed>(entity).Value : MoveConsts.MOVE_SPEED;
            (int prevX, int prevY) = Get<TilePosition>(entity);
            x += prevX;
            y += prevY;
            
            // if(TryGet<FourDirectionAnim>(entity, out FourDirectionAnim anim)) {
            //     Set(entity)
            // }
            if(!GlobalTilemap.IsBlocked(x, y)) {
                float progress = Has<TempTileProgress>(entity) ? Get<TempTileProgress>(entity).Value : 0;
                Set(entity, new MoveToTile(x, y, prevX, prevY, moveSpeed, progress));
                GlobalTilemap.ChangePosition(entity, x, y);
            }
        }
        RemoveAll<TempTileProgress>();
        while(Some<UnprocessedTilePosition>()) {
            var entity = GetSingletonEntity<UnprocessedTilePosition>();
            Remove<UnprocessedTilePosition>(entity);
            (int x, int y) = Get<TilePosition>(entity);
            GlobalTilemap.ChangePosition(entity, x, y);
            Set(entity, EntityUtils.TileToWorld(x, y));
        }
        foreach (var entity in MovementFilter.Entities)
        {
            (int targetX, int targetY, int prevX, int prevY, float speed, float progress) = Get<MoveToTile>(entity);
            progress += speed * (float)delta.TotalSeconds * Dimensions.INV_TILE_SIZE;
            if(progress >= 1) {
                Remove<MoveToTile>(entity);
                Set(entity, new FinishStepThisFrame());
                if(progress > 1) {
                    Set(entity, new TempTileProgress(progress - 1));
                    progress = 1;
                }
            }
            else {
                Set(entity, new MoveToTile(targetX, targetY, prevX, prevY, speed, progress));
            }
            Set(entity, EntityUtils.TileToWorld(MathUtils.Lerp(prevX, targetX, progress), MathUtils.Lerp(prevY, targetY, progress)));
        }
    }
}
