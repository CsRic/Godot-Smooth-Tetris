using Godot;
using System;

public partial class BlockSprite : Sprite
{
    public int _SpriteColumn;
    public int _SpriteRow;
    public override void _Process(float delta)
    {
        if (_useSmoothMove) SmoothMove(delta);
    }
}
public partial class BlockSprite: Sprite//This part handles the Moving Animation
{
    private bool _useSmoothMove = false;
    private float _BackupX, _BackupY;
    private void SmoothMove(float delta)
    {
        float speedX = (_SpriteColumn * 35 - _BackupX) * 10;
        float speedY = (_SpriteRow * 35 - _BackupY) * 10;
        if (Mathf.Abs(Position.x - _SpriteColumn * 35) > Mathf.Abs(speedX * delta * 3) ||
        Mathf.Abs(Position.y - _SpriteRow * 35) > Mathf.Abs(speedY * delta * 3))
        {
            float tempX = Position.x + speedX * delta;
            float tempY = Position.y + speedY * delta;
            Position = new Vector2(tempX, tempY);
        }
        else
        {
            Position = new Vector2(_SpriteColumn * 35 + 1.5f, _SpriteRow * 35 + 1.5f);
            _useSmoothMove = false;
        }
    }
    public void MoveByGridAnimated(int row, int column)
    {
        _useSmoothMove = true;
        _BackupX = Position.x;
        _BackupY = Position.y;
        _SpriteColumn = column;
        _SpriteRow = row;
    }
    public void MoveByGridImidiate(int row, int column)
    {
        _useSmoothMove = false;
        _SpriteColumn = column;
        _SpriteRow = row;
        Position = new Vector2(_SpriteColumn * 35 + 1.5f, _SpriteRow * 35 + 1.5f);
    }
}