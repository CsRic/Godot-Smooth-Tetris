using Godot;
using System;

public partial class Tetromino : Node2D
{
    public int _NodeRow = -1;//smaller than 0 means it in waiting queue, else means in GrandMap
    public int _NodeColumn = -1;//smaller than 0 means it in waiting queue, else means in GrandMap
    public int[,] _blocks;
    public BlockSprite[,] _Sprites;
    public int _columns;
    public int _rows;
    public int _direction;
    public Vector2 _rotateCenterOffset;
    public char _type;
    public override void _Ready()
    {
        _direction = 3;
    }
    public override void _Process(float delta)
    {
        if (_useSmoothMove) SmoothMove(delta);
    }
    public int TrueRow(int row,int column)//Show the true row of a block in node's parent
    {
        switch(_direction)
        {
            case 0:
                return _NodeRow - 1 - column;
            case 1:
                return _NodeRow - 1 - row;
            case 2:
                return _NodeRow + column;
            case 3:
            default:
                return _NodeRow + row;
        }
    }
    public int TrueColumn(int row,int column)//Show the true column of a block in node's parent
    {
        switch(_direction)
        {
            case 0:
                return _NodeColumn + row;
            case 1:
                return _NodeColumn - column - 1;
            case 2:
                return _NodeColumn - row - 1;
            case 3:
            default:
                return _NodeColumn + column;
        }
    }
    public void BuildTetromino(char type)
    {
        _type = type;
        switch (type)
        {
            case 'I':
                _columns = 1;
                _rows = 4;
                _blocks = new int[_rows, _columns];
                _blocks[0, 0] = _blocks[1, 0] = _blocks[2, 0] = _blocks[3, 0] = 1;
                _rotateCenterOffset = new Vector2(0.5f, 1.5f);
                break;
            case 'O':
                _columns = 2;
                _rows = 2;
                _blocks = new int[_rows, _columns];
                _blocks[0, 0] = _blocks[0, 1] = _blocks[1, 0] = _blocks[1, 1] = 1;
                _rotateCenterOffset = new Vector2(1f, 1f);
                break;
            case 'T':
                _columns = 3;
                _rows = 2;
                _blocks = new int[_rows, _columns];
                _blocks[0, 0] = _blocks[0, 1] = _blocks[0, 2] = _blocks[1, 1] = 1;
                _rotateCenterOffset = new Vector2(1.5f, 0.5f);
                break;
            case 'L':
                _columns = 2;
                _rows = 3;
                _blocks = new int[_rows, _columns];
                _blocks[0, 0] = _blocks[1, 0] = _blocks[2, 0] = _blocks[2, 1] = 1;
                _rotateCenterOffset = new Vector2(0.5f, 2.5f);
                break;
            case 'J':
                _columns = 2;
                _rows = 3;
                _blocks = new int[_rows, _columns];
                _blocks[0, 1] = _blocks[1, 1] = _blocks[2, 0] = _blocks[2, 1] = 1;
                _rotateCenterOffset = new Vector2(1.5f, 2.5f);
                break;
            case 'S':
                _columns = 3;
                _rows = 2;
                _blocks = new int[_rows, _columns];
                _blocks[0, 1] = _blocks[0, 2] = _blocks[1, 0] = _blocks[1, 1] = 1;
                _rotateCenterOffset = new Vector2(1.5f, 0.5f);
                break;
            case 'Z':
            default:
                _columns = 3;
                _rows = 2;
                _blocks = new int[_rows, _columns];
                _blocks[0, 0] = _blocks[0, 1] = _blocks[1, 1] = _blocks[1, 2] = 1;
                _rotateCenterOffset = new Vector2(1.5f, 0.5f);
                break;
        }
    }
    public void ShowBlocks()//Must be called after BuildTetromino, Set node position first is highly recommanded.
    {
        Color baseColor;
        switch (_type)
        {
            case 'I':
                baseColor = new Color(51f / 256f, 185f / 256f, 170f / 256f);//cyan
                break;
            case 'O':
                baseColor = new Color(185f / 256f, 161f / 256f, 51f / 256f);//yellow
                break;
            case 'T':
                baseColor = new Color(185f / 256f, 61f / 256f, 51f / 256f);//orange
                break;
            case 'L':
                baseColor = new Color(51f / 256f, 185f / 256f, 110f / 256f);//green
                break;
            case 'J':
                baseColor = new Color(87f / 256f, 51f / 256f, 185f / 256f);//indigo
                break;
            case 'S':
                baseColor = new Color(51f / 256f, 110f / 256f, 185f / 256f);//blue
                break;
            case 'Z':
            default:
                baseColor = new Color(185f / 256f, 51f / 256f, 171f / 256f);//purple
                break;
        }
        var blockTexture = ResourceLoader.Load<Texture>("res://Art/White_Block.png", null, false);
        int i, j;
        _Sprites = new BlockSprite[_rows, _columns];
        for (i = 0; i < _rows; i++)
        {
            for (j = 0; j < _columns; j++)
            {
                if (_blocks[i, j] == 1)
                {
                    _Sprites[i, j] = new BlockSprite();
                    _Sprites[i, j].Texture = blockTexture;
                    _Sprites[i, j].Centered = false;
                    _Sprites[i, j].Scale = new Vector2(0.25f, 0.25f);
                    _Sprites[i, j].Position = new Vector2(j * 35 + 1.5f, i * 35 + 1.5f);
                    _Sprites[i, j].Modulate = baseColor;
                    AddChild(_Sprites[i, j]);
                    _Sprites[i, j].Name = "[" + i.ToString() + "," + j.ToString() + "]";
                }
            }
        }
    }
}

public partial class Tetromino : Node2D//This part handles the Moving Animation
{
    public float _SmoothMoveSpeed = 10;
    private bool _useSmoothMove = false;
    private float _BackupX, _BackupY;
    private void SmoothMove(float delta)
    {
        float speedX = (_NodeColumn * 35 - _BackupX) * _SmoothMoveSpeed;
        float speedY = (_NodeRow * 35 - _BackupY) * _SmoothMoveSpeed;
        if (Mathf.Abs(Position.x - _NodeColumn * 35) > Mathf.Abs(speedX * delta * 2) ||
        Mathf.Abs(Position.y - _NodeRow * 35) > Mathf.Abs(speedY * delta * 2))
        {
            float tempX = Position.x + speedX * delta;
            float tempY = Position.y + speedY * delta;
            Position = new Vector2(tempX, tempY);
        }
        else
        {
            Position = new Vector2(_NodeColumn * 35, _NodeRow * 35);
            _useSmoothMove = false;
        }
    }
    public void MoveByGridAnimated(int row, int column)
    {
        _useSmoothMove = true;
        _BackupX = Position.x;
        _BackupY = Position.y;
        _NodeColumn = column;
        _NodeRow = row;
    }
    public void MoveByGridImidiate(int row,int column)
    {
        _useSmoothMove = false;
        _NodeColumn = column;
        _NodeRow = row;
        Position = new Vector2(_NodeColumn * 35, _NodeRow * 35);
    }
}