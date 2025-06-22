using Godot;
using System;

public partial class TripEffect : ColorRect
{
	[Export] public float Alpha = 1.0f;
	
	private ShaderMaterial _shader;

	public override void _Ready()
	{
		_shader = this.Material as ShaderMaterial;
	}

	public override void _Process(double delta)
	{
		if (_shader == null) return;
		
		_shader.SetShaderParameter("time", (float)Time.GetTicksMsec() / 100f);
		
		var res = GetViewport().GetVisibleRect().Size;
		_shader.SetShaderParameter("resolution", res);
		
		_shader.SetShaderParameter("alpha", Alpha);
	}
}
