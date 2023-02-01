using Godot;
using System;

public class ControlMainView : Node2D
{
    [Export]
    public PackedScene MainView;
    [Export]
    public PackedScene Title;
    private MainView _mainView;
    private UI_Select _title;
    public long  score;
    public override void _Ready()
    {
        _mainView = (MainView)MainView.Instance();
        _title = (UI_Select)Title.Instance();
        ShowTitle();
        score = 0;
    }
    public override void _Process(float delta)
    {
        UpdateScore();
    }
    public void FreshGame(int bpm = 120)
    {
        score = 0;
        GrandMap.Map = new int[GrandMap.RowSize, GrandMap.ColumnSize];

        _title.QueueFree();
        
        _mainView = (MainView)MainView.Instance();
        AddChild(_mainView);
        _mainView.GetNode<Timer>("TurnTimer").WaitTime = 60f / bpm;
        _mainView.Connect("GameOver", this, "ShowTitle");
        MoveChild(_mainView, 0);
    }
    public void ShowTitle()
    {
        _mainView.QueueFree();
        _title = (UI_Select)Title.Instance();
        AddChild(_title);
    }
    public void UpdateScore()
    {
        GetNode<Label>("Number").Text = score.ToString();
    }
    public void QuitGame()
    {
        QueueFree();
        GetTree().Quit();
    }
}
