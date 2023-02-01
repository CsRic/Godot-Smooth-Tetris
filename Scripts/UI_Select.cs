using Godot;
using System;

public class UI_Select : Control
{
    Label _mainTitle;
    Timer _hueTimer;
    Light2D _light;
    public override void _Ready()
    {
        _mainTitle = GetNode<Label>("MainTitle");
        _hueTimer = GetNode<Timer>("HueTimer");
        _light = GetNode<Light2D>("Light2D");
    }
    public override void _Process(float delta)
    {
        float ParamTime = _hueTimer.TimeLeft / _hueTimer.WaitTime * 2 * Mathf.Pi;
        float r = 1f / 2.1f * Mathf.Sin(ParamTime) + 0.5f;
        float g = 1f / 2.1f * Mathf.Sin(ParamTime + Mathf.Pi / 3f * 2) + 0.5f;
        float b = 1f / 2.1f * Mathf.Sin(ParamTime + Mathf.Pi / 3f * 4) + 0.5f;
        var color = new Color(r, g, b);
        _mainTitle.Modulate = color;
    }
    private void MainTitleHueChange()
    {
        float ParamTime = _hueTimer.TimeLeft / _hueTimer.WaitTime * 2 * Mathf.Pi;
        float r = 1f / 2.1f * Mathf.Sin(ParamTime) + 0.5f;
        float g = 1f / 2.1f * Mathf.Sin(ParamTime + Mathf.Pi / 3f * 2) + 0.5f;
        float b = 1f / 2.1f * Mathf.Sin(ParamTime + Mathf.Pi / 3f * 4) + 0.5f;
        var color = new Color(r, g, b);
        _mainTitle.Modulate = color;
    }
    private void LightEnergyChange()
    {
        float ParamTime = _hueTimer.TimeLeft / _hueTimer.WaitTime * 2 * Mathf.Pi;
        float energy = Mathf.Sin(ParamTime) / 3f + 0.5f;
        _light.Energy = energy;
    }
    private void GameStart(int bpm)
    {
        var a = GetNode<ControlMainView>("/root/ControlMainView");
        a.FreshGame(bpm);
    }
}
