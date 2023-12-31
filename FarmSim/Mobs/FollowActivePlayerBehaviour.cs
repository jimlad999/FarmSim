using FarmSim.Rendering;
using Microsoft.Xna.Framework;

namespace FarmSim.Mobs;

class FollowActivePlayerBehaviour : Behaviour
{
    private const int FollowDistanceFromPlayer = 2 * Renderer.TileSize;
    private int _playerX;
    private int _playerY;
    private int _targetX;
    private int _targetY;
    private Vector2 _normalizedDirection;
    private bool _needNewTarget = true;
    private bool _waiting;

    public override void Reset()
    {
        _needNewTarget = true;
        _waiting = false;
    }

    public override bool TryExecute(Mob mob, GameTime gameTime)
    {
        var player = GlobalState.PlayerManager.ActivePlayer;
        if (_needNewTarget || player.XInt != _playerX || player.YInt != _playerY)
        {
            _playerX = player.XInt;
            _playerY = player.YInt;
            _normalizedDirection = new Vector2(x: _playerX - mob.XInt, y: _playerY - mob.YInt);
            _normalizedDirection.Normalize();
            // TODO: consider pathing and valid terrain
            _targetX = _playerX - (int)(FollowDistanceFromPlayer * _normalizedDirection.X);
            _targetY = _playerY - (int)(FollowDistanceFromPlayer * _normalizedDirection.Y);
            _needNewTarget = false;
            _waiting = false;
        }
        if (_waiting)
        {
            return true;
        }
        else if (!mob.TryMove(gameTime, _normalizedDirection, _targetX, _targetY))
        {
            _waiting = true;
        }
        // should always be successful
        return true;
    }
}
