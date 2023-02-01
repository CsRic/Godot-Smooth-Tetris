using Godot;
using System;

public partial class MainView : Node2D
{
    [Signal]
    public delegate void GameOver();
    public Timer _turnTimer;
    public Timer _quarterTurnTimer;
    private TextureRect MainGrid, SubGrid;
    private int _waitQueueLength = 3;//it should be at least 1
    private Tetromino[] tetromino;//Tetromino nodes waiting for deploying
    public bool _isDroping = false;
    private void GenerateNextTetromino(int number)//Deploy in SubGrid
    {
        tetromino[number] = new Tetromino();
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        RNG.Randomize();
        int a = RNG.RandiRange(0, 6);

        switch (a)
        {
            case 0:
                tetromino[number].BuildTetromino('I');
                break;
            case 1:
                tetromino[number].BuildTetromino('O');
                break;
            case 2:
                tetromino[number].BuildTetromino('T');
                break;
            case 3:
                tetromino[number].BuildTetromino('L');
                break;
            case 4:
                tetromino[number].BuildTetromino('J');
                break;
            case 5:
                tetromino[number].BuildTetromino('S');
                break;
            case 6:
            default:
                tetromino[number].BuildTetromino('Z');
                break;
        }

        tetromino[number].MoveByGridImidiate(0, number * 4 + 1);
        SubGrid.AddChild(tetromino[number]);
        tetromino[number].ShowBlocks();
        tetromino[number].MoveByGridAnimated(0, (number - 1) * 4 + 1);
    }
    private void initialGenerate()
    {
        for (int i = 1; i <= _waitQueueLength; i++)
        {
            GenerateNextTetromino(i);
        }
    }
    private bool ReachEnd()//whether tetromino[0] has to be frozen
    {
        if (tetromino[0] == null) return false;
        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j);
                    int b = tetromino[0].TrueColumn(i, j);
                    if (a < 0) continue;//still over topline
                    if (a >= GrandMap.RowSize - 1)//reach buttom
                        return true;
                    if (GrandMap.Map[a + 1, b] == 1)//reach block below
                        return true;
                }
            }
        }
        return false;
    }
    private void ChangeMainGridColorToMovingTetromino()
    {
        var a = (BlockSprite)tetromino[0].GetChild(0);
        MainGrid.SelfModulate = a.Modulate;
    }
    private void ChangeSubGridColorToWaitingTetromino()
    {
        var a = (BlockSprite)tetromino[1].GetChild(0);
        SubGrid.SelfModulate = a.Modulate;
    }
    public void CalledEveryTurn()//Main Script of Auto-Next-Turn
    {
        if (!_isDroping)//should add a new tetromino
        {
            controlable = false;
            tetromino[0] = new Tetromino();
            tetromino[0].BuildTetromino(tetromino[1]._type);
            tetromino[1].Free();
            for (int i = 1; i < _waitQueueLength; i++)
            {
                tetromino[i] = new Tetromino();
                tetromino[i].BuildTetromino(tetromino[i + 1]._type);
                tetromino[i + 1].Free();
                tetromino[i].MoveByGridImidiate(0, i * 4 + 1);
                SubGrid.AddChild(tetromino[i]);
                tetromino[i].ShowBlocks();
                tetromino[i].MoveByGridAnimated(0, (i - 1) * 4 + 1);
            }
            GenerateNextTetromino(_waitQueueLength);
            _isDroping = true;
            MainGrid.AddChild(tetromino[0]);
            tetromino[0].ShowBlocks();
            tetromino[0].MoveByGridImidiate(0, (GrandMap.ColumnSize - 1) / 2);

            ChangeMainGridColorToMovingTetromino();
            ChangeSubGridColorToWaitingTetromino();

            GetNode<Tetromino>(ButtomShowTetromino.GetPath()).QueueFree();
            ButtomShowTetromino = new Tetromino();
            ButtomShowTetromino._SmoothMoveSpeed = 20;
            ButtomShowTetromino.BuildTetromino(tetromino[0]._type);
            MainGrid.AddChild(ButtomShowTetromino);
            ButtomShowTetromino.Rotation = tetromino[0].Rotation;
            ButtomShowTetromino._direction = tetromino[0]._direction;
            ButtomShowTetromino.ShowBlocks();
            ButtomShowTetromino.MoveByGridImidiate(-10, -10);//so it won't show immediately
            ButtomShow();
        }
        else//Drop and control tetromino[0]
        {
            controlable = true;
            _quarterTurnTimer.Start();//Sometimes I pressed a key before it can make effort, So I start the Timer Immediately after 'controllable'. Smoother.
            if (ReachEnd())
            {
                _isDroping = false;
                for (int i = 0; i < tetromino[0]._rows; i++)//Change blocks' parent from (Node)Tetromino to (TextureRect)MainGrid
                {
                    for (int j = 0; j < tetromino[0]._columns; j++)
                    {
                        if (tetromino[0]._blocks[i, j] == 1)
                        {
                            int a = (int)tetromino[0].TrueRow(i, j);
                            int b = (int)tetromino[0].TrueColumn(i, j);
                            if (a <= 0)
                            {
                                EmitSignal("GameOver");
                                this.QueueFree();
                                return;
                            }

                            var block = tetromino[0].GetNode<BlockSprite>("[" + i.ToString() + "," + j.ToString() + "]");
                            tetromino[0].RemoveChild(block);
                            block.MoveByGridImidiate(a, b);
                            MainGrid.AddChild(block, true);
                            block.Name = "Block[" + a.ToString() + "," + b.ToString() + "]";
                            GrandMap.Map[a, b] = 1;//register to Map[,]
                        }
                    }
                }
                if (HasNode(tetromino[0].Name))
                    RemoveChild(tetromino[0]);//tetromino[0] is no longer needed
                Eraselines();
                CalledEveryTurn();
            }
            else
            {
                tetromino[0]._SmoothMoveSpeed = 5f / _turnTimer.WaitTime;
                tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow + 1, tetromino[0]._NodeColumn);
            }
        }
    }
    public override void _Ready()
    {
        MainGrid = GetNode<TextureRect>("MainGrid");
        SubGrid = GetNode<TextureRect>("SubGrid");

        MainGrid.MarginRight = GrandMap.ColumnSize * 35 + 0 + MainGrid.MarginLeft;
        MainGrid.MarginBottom = GrandMap.RowSize * 35 + 1 + MainGrid.MarginTop;

        tetromino = new Tetromino[_waitQueueLength + 1];
        ButtomShowTetromino = new Tetromino();
        AddChild(ButtomShowTetromino);

        initialGenerate();
        _turnTimer = GetNode<Timer>("TurnTimer");
        _quarterTurnTimer = GetNode<Timer>("QuarterTurnTimer");
        _quarterTurnTimer.WaitTime = 0.06f;
    }
    public override void _Process(float delta)
    {
        if (controlable && ButtomShowTetromino.IsInsideTree())
        {
            if (Input.IsActionJustPressed("dash")) MoveButtom();
            else if (Input.IsActionJustPressed("clockwise")) RotationInGame(1);
            else if (Input.IsActionJustPressed("counter-clockwise") || Input.IsActionJustPressed("ui_up")) RotationInGame(-1);

            if(Input.IsActionJustPressed("ui_left")||Input.IsActionJustPressed("ui_right")||Input.IsActionJustPressed("ui_down"))//Besides moving constantly, I want to have one move when just pressed.
            {
                InputToMove();//manual trigger
                _quarterTurnTimer.Start();
            }
            if(Input.IsActionJustReleased("ui_left")||Input.IsActionJustReleased("ui_right")||Input.IsActionJustReleased("ui_down"))//So the Timer should restart and stop often to make sure when just pressed, the manual trigger and timer-based trigger won't start together, in case move twice by one key hit. 
            {
                if(!(Input.IsActionPressed("ui_left")||Input.IsActionPressed("ui_right")||Input.IsActionPressed("ui_down")))
                _quarterTurnTimer.Stop();
            }
        }
    }
    public void CalledEveryQuarterTurn()
    {
        if (controlable) { InputToMove(); ButtomShow(); }
    }
}
public partial class MainView : Node2D//Functions for tetromino[0] control
{
    public bool controlable = false;
    private float _moveTimerWaitingTime = 0.25f;
    private int minDepth;//Depth from buttom
    private Tetromino ButtomShowTetromino;
    private void MoveLeftOneGrid()
    {
        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j);
                    int b = tetromino[0].TrueColumn(i, j);
                    if (a >= 0)
                    {
                        if (b == 0) return;
                        if (GrandMap.Map[a, b - 1] == 1) return;
                    }
                }
            }
        }
        tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow, tetromino[0]._NodeColumn - 1);
    }
    private void MoveRightOneGrid()
    {
        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j);
                    int b = tetromino[0].TrueColumn(i, j);
                    if (a >= 0)
                    {
                        if (b == GrandMap.ColumnSize - 1) return;
                        if (GrandMap.Map[a, b + 1] == 1) return;
                    }
                }
            }
        }
        tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow, tetromino[0]._NodeColumn + 1);
    }
    private void MoveDownOneGrid()
    {
        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j);
                    int b = tetromino[0].TrueColumn(i, j);
                    if (a >= 0)
                    {
                        if (a == GrandMap.RowSize - 1 || GrandMap.Map[a + 1, b] == 1)
                        {
                            // CalledEveryTurn();//straight to next turn. No waste time
                            return;
                        }
                    }
                }
            }
        }
        tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow + 1, tetromino[0]._NodeColumn);
    }
    private void MoveButtom()
    {
        ButtomShow();
        tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow + minDepth, tetromino[0]._NodeColumn);
        CalledEveryTurn();
    }
    private void ButtomShow()//shows the will-be blocks in the buttom
    {
        int tempDepth = 0;
        minDepth = -1;
        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j);
                    int b = tetromino[0].TrueColumn(i, j);
                    if (a < 0) continue;
                    if (b >= 0 && b < GrandMap.ColumnSize && a < GrandMap.RowSize)
                    { for (tempDepth = 0; a + tempDepth < GrandMap.RowSize && GrandMap.Map[tempDepth + a, b] == 0; tempDepth++) ; }
                    tempDepth--;
                    if (minDepth == -1) minDepth = tempDepth;
                    if (minDepth > tempDepth) minDepth = tempDepth;
                }
            }
        }
        if (minDepth == -1) return;
        //now I get the minDepth from buttom
        ButtomShowTetromino.MoveByGridAnimated(tetromino[0]._NodeRow + minDepth, tetromino[0]._NodeColumn);
        ButtomShowTetromino.Modulate = new Color(0.99f, 0.99f, 0.99f, 0.45f);
    }
    private void InputToMove()//triggered by _quarterTurnTimer and Initial Key-hit
    {
        if (Input.IsActionPressed("ui_left"))
        {
            MoveLeftOneGrid();
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            MoveRightOneGrid();
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            MoveDownOneGrid();
        }
        else
        {

        }
        ButtomShow();
    }
    private void RotationInGame(int mode)//check and rotate. Pretend to be possible first. mode == 1: clockwise; mode == -1: counter-clockwise
    {
        Vector2 backupRelativeRotateCenterOffset = new Vector2(tetromino[0]._rotateCenterOffset.Rotated((3 - tetromino[0]._direction) * Mathf.Pi / 2));
        int backupDirection = tetromino[0]._direction;
        tetromino[0]._direction = (backupDirection - mode) % 4;//Try to see what will happen if rotated

        Vector2 relativeRotateCenterOffset = new Vector2(tetromino[0]._rotateCenterOffset.Rotated((3 - tetromino[0]._direction) * Mathf.Pi / 2));
        Vector2 moveToTrueCenter = new Vector2(backupRelativeRotateCenterOffset - relativeRotateCenterOffset);
        int rowOffset = (int)Math.Round(moveToTrueCenter.y);
        int columnOffset = (int)Math.Round(moveToTrueCenter.x);

        if (CheckCanRotate(rowOffset, columnOffset)) { StartRotate(rowOffset, columnOffset, mode); return; }
        if (CheckCanRotate(rowOffset, columnOffset - 1)) { StartRotate(rowOffset, columnOffset - 1, mode); return; }
        if (CheckCanRotate(rowOffset, columnOffset + 1)) { StartRotate(rowOffset, columnOffset + 1, mode); return; }
        if (CheckCanRotate(rowOffset, columnOffset - 2)) { StartRotate(rowOffset, columnOffset - 2, mode); return; }
        if (CheckCanRotate(rowOffset, columnOffset + 2)) { StartRotate(rowOffset, columnOffset + 2, mode); return; }

        //If goes to here, it's really unrotatable
        tetromino[0]._direction = backupDirection;
        ButtomShow();
    }
    private bool CheckCanRotate(int rowOffset, int columnOffset)
    {
        bool canRotate = true;

        for (int i = 0; i < tetromino[0]._rows; i++)
        {
            if (!canRotate) break;
            for (int j = 0; j < tetromino[0]._columns; j++)
            {
                if (tetromino[0]._blocks[i, j] == 1)
                {
                    int a = tetromino[0].TrueRow(i, j) + rowOffset;
                    int b = tetromino[0].TrueColumn(i, j) + columnOffset;
                    if (a > GrandMap.RowSize - 1 || b < 0 || b > GrandMap.ColumnSize - 1) { canRotate = false; break; }
                    if (a < 0) continue;
                    if (GrandMap.Map[a, b] == 1) { canRotate = false; break; }
                }
            }
        }
        return canRotate;
    }
    private void StartRotate(int rowOffset, int columnOffset, int mode)
    {

        tetromino[0].MoveByGridAnimated(tetromino[0]._NodeRow + rowOffset, tetromino[0]._NodeColumn + columnOffset);
        tetromino[0].Rotation += mode * Mathf.Pi / 2;
        tetromino[0].Rotation %= Mathf.Pi * 2;

        ButtomShowTetromino._direction = tetromino[0]._direction;
        ButtomShowTetromino.Rotation = tetromino[0].Rotation;
        ButtomShow();
    }
}
public partial class MainView : Node2D//erase line check 
{
    public int ErasedLinesNumber;
    public void Eraselines()
    {
        int i, j, k;
        ErasedLinesNumber = 0;
        for (i = 0; i < GrandMap.RowSize; i++)
        {
            bool shouldErase = true;
            for (j = 0; j < GrandMap.ColumnSize; j++)
            {
                if (GrandMap.Map[i, j] == 0) { shouldErase = false; break; }
            }
            if (shouldErase)
            {
                for (j = 0; j < GrandMap.ColumnSize; j++)
                {
                    MainGrid.GetNode<BlockSprite>("Block[" + i.ToString() + "," + j.ToString() + "]").Free();
                    GrandMap.Map[i, j] = 0;
                }
                for (k = i; k >= 0; k--)
                {
                    for (j = 0; j < GrandMap.ColumnSize; j++)
                    {
                        if (GrandMap.Map[k, j] == 1)
                        {
                            GrandMap.Map[k, j] = 0;
                            var temp = MainGrid.GetNode<BlockSprite>("Block[" + k.ToString() + "," + j.ToString() + "]");
                            temp.Name = "Block[" + (k + 1).ToString() + "," + j.ToString() + "]";
                            temp.MoveByGridAnimated(k + 1, j);
                            GrandMap.Map[k + 1, j] = 1;
                        }
                    }
                }
                ErasedLinesNumber++;
            }
            GetNode<ControlMainView>("/root/ControlMainView").score += ErasedLinesNumber * ErasedLinesNumber;
        }
    }
}
