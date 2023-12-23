using FarmSim.Rendering;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmSim.Mobs;

class RandomWonderBehaviour : Behaviour
{
    private readonly int _wonderDistance;
    private readonly int _wonderDistance2Plus1;
    private readonly string[] _validTerrain;
    private readonly int _maxWaitTime;
    private double _waitTimeMilliseconds;
    private bool _needsNewTarget;
    private int _targetX;
    private int _targetTileX;
    private int _targetY;
    private int _targetTileY;
    private Vector2 _normalizedDirection;

    public RandomWonderBehaviour(
        int wonderDistance,
        string[] validTerrain,
        int maxWaitTimeMilliseconds)
    {
        _wonderDistance = wonderDistance;
        _wonderDistance2Plus1 = wonderDistance * 2 + 1;
        _validTerrain = validTerrain;
        _maxWaitTime = maxWaitTimeMilliseconds;

        // don't set _waitTimeMilliseconds on first creation so that mobs start moving immediately
        _needsNewTarget = true;
    }

    public override void Reset()
    {
        _needsNewTarget = true;
        _waitTimeMilliseconds = RandomUtil.Rand.NextDouble() * _maxWaitTime;
    }

    public override bool TryExecute(Mob mob, GameTime gameTime)
    {
        if (_waitTimeMilliseconds > 0)
        {
            _waitTimeMilliseconds -= gameTime.ElapsedGameTime.TotalMilliseconds;
        }
        else if (_needsNewTarget)
        {
            do
            {
                _targetX = mob.XInt + RandomUtil.Rand.Next(_wonderDistance2Plus1) - _wonderDistance;
                _targetTileX = _targetX / Renderer.TileSize;
                if (_targetX < 0) --_targetTileX;
                _targetY = mob.YInt + RandomUtil.Rand.Next(_wonderDistance2Plus1) - _wonderDistance;
                _targetTileY = _targetY / Renderer.TileSize;
                if (_targetY < 0) --_targetTileY;
            }
            while (!_validTerrain.Contains(GlobalState.TerrainManager.GetTile(tileX: _targetTileX, tileY: _targetTileY).Terrain));
            _needsNewTarget = false;
            _normalizedDirection = new Vector2(x: _targetX - mob.XInt, y: _targetY - mob.YInt);
            _normalizedDirection.Normalize();
        }
        else if (!mob.TryMove(gameTime, _normalizedDirection, _targetX, _targetY))
        {
            Reset();
        }

        return true;
    }
}
