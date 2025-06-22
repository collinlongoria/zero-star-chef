using Godot;
using System;

public partial class CookingParticles : GpuParticles2D
{
	public enum ParticleType
	{
		OFF,
		SPARKLE,
		SMOKE
	}

	private ParticleType _currentParticleType = ParticleType.OFF;
	
	private Texture2D _smokeTexture;
	private Texture2D _sparkleTexture;
	private ParticleProcessMaterial _smokeMaterial;
	private ParticleProcessMaterial _sparkleMaterial;

	public override void _Ready()
	{
		_smokeTexture = GD.Load<Texture2D>("res://Assets/smoke.png");
		_sparkleTexture = GD.Load<Texture2D>("res://Assets/sparkle.png");
		
		_smokeMaterial = GD.Load<ParticleProcessMaterial>("res://Resources/smoke.tres");
		_sparkleMaterial = GD.Load<ParticleProcessMaterial>("res://Resources/sparkle.tres");
	}

	public void SetParticleType(ParticleType particleType)
	{
		if (particleType == _currentParticleType) return;
		
		switch (particleType)
		{
			case ParticleType.OFF:
				Emitting = false;
				break;
			case ParticleType.SMOKE:
				Emitting = true;
				Texture = _smokeTexture;
				ProcessMaterial = _smokeMaterial;
				Amount = 6;
				Lifetime = 4.0;
				break;
			case ParticleType.SPARKLE:
				Emitting = true;
				Texture = _sparkleTexture;
				ProcessMaterial = _sparkleMaterial;
				Amount = 8;
				Lifetime = 2.0;
				break;
		}
		
		_currentParticleType = particleType;
	}
}
