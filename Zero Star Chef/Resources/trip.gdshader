/*
	Shader is based on https://www.shadertoy.com/view/Xd2Bzw by Klems
*/

shader_type canvas_item;

uniform float time;
uniform vec2 resolution;

void fragment(){
	vec2 p = floor(FRAGCOORD.xy);
	
	// set position
	vec2 v = resolution;
	p = (p - v * 0.5) * 0.4 / v.y;
	
	// breathing effect
	p += p * sin(dot(p, p) * 20.0 - (time / 10.0)) * 0.04;
	
	vec4 c = vec4(0.0);
	
	for(float i = 0.5; i < 8.0; i++){
		// fractal formular
		float a = 0.01 * (time * 0.1) * i * i + 0.78;
		mat2 rot = mat2(
			vec2(cos(a), sin(a)),
			vec2(-sin(a), cos(a))
		);
		p = abs(2.0 * fract(p - 0.5) - 1.0) * rot;
		
		// Colors
		c += exp(-abs(p.y) * 5.0) * (cos(vec4(2.0, 3.0, 1.0, 0.0) * i) * 0.5 + 0.5);
	}
	
	// Palette
	c.gb *= 0.5;
	c.a = 0.2;
	COLOR = c;
}