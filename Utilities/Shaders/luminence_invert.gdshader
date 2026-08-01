shader_type canvas_item;

uniform float luminence_cutoff: hint_range(0.0, 1.0) = .5;
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture;

void fragment() {
	vec4 screen_c = texture(SCREEN_TEXTURE, SCREEN_UV);
	float luminence = dot(screen_c.rgb, vec3(0.2126, 0.7152, 0.0722));
	
	if (luminence < luminence_cutoff)
	{
		COLOR = vec4(1.0, 1.0, 1.0, 1.0);
	}
	else
	{
		COLOR = vec4(0, 0, 0, 1);
	}
}
