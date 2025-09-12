

using System;
using MoonTools.ECS;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
namespace MyGame.Systems;

public sealed class AdvanceCharCountSystem : TimedCallbackSystem<AdvanceCharCount>
{
    public AdvanceCharCountSystem(World world) : base(world) { }
    protected override void OnFinish(Entity entity, AdvanceCharCount component)
    {
        // Console.WriteLine("advance char count ended!");
        if (Has<Text>(entity) && Has<DisplayCharCount>(entity))
        {
            int nextCharCount = Get<DisplayCharCount>(entity).Value + 1;
            Set(entity, new DisplayCharCount(nextCharCount));
            // Console.WriteLine($"new char count: {nextCharCount}");
            if (nextCharCount < TextStorage.GetString(Get<Text>(entity).TextID).Length)
            {
                Set(entity, new AdvanceCharCount(Has<AdvanceCharSpeed>(entity) ? Get<AdvanceCharSpeed>(entity).TimePerCharacter : TextConsts.TEXT_ADVANCE_CHAR_TIME));
            }
        }
        else if (Has<TextSpriteParent>(entity))
        {
            Console.WriteLine("running text sprite char count callback!");
            if (HasOutRelation<DontDraw>(entity))
            {
                var child = OutRelationSingleton<DontDraw>(entity); //NthOutRelation<Child>(entity, nextCharCount);
                Unrelate<DontDraw>(entity, child);
                Set(entity, new AdvanceCharCount(Has<AdvanceCharSpeed>(entity) ? Get<AdvanceCharSpeed>(entity).TimePerCharacter : TextConsts.TEXT_ADVANCE_CHAR_TIME));
            }
        }
    }
}