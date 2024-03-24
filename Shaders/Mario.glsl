VERTEX:
#version 330
#include Partials/Methods.gl

uniform mat4 u_mvp;
uniform mat4 u_model;

layout(location=0) in vec3 a_position;
layout(location=1) in vec3 a_normal;
layout(location=2) in vec3 a_color;
layout(location=3) in vec2 a_tex;

out vec3 v_normal;
out vec3 v_color;
out vec2 v_tex;
out vec3 v_world;

void main(void)
{
	gl_Position = u_mvp * vec4(a_position, 1.0);

	v_normal = TransformNormal(a_normal, u_model);
    v_color = a_color;
	v_tex = a_tex;
	v_world = vec3(u_model * vec4(a_position, 1.0));
}

FRAGMENT:
#version 330
#include Partials/Methods.gl

uniform sampler2D u_texture;
uniform vec4      u_color;
uniform float     u_near;
uniform float     u_far;
uniform vec3      u_sun;
uniform float     u_effects;
uniform float     u_silhouette;
uniform vec4      u_silhouette_color;
uniform float     u_time;
uniform vec4      u_vertical_fog_color;
uniform float     u_cutout;

in vec2 v_tex;
in vec3 v_normal;
in vec3 v_color;
in vec3 v_world;

layout(location = 0) out vec4 o_color;

void main(void)
{
	// get texture color
	// Offset UVs very slightly, so that 1|1 coords (meaning transparent) don't accidentally 
	// wrap back around to the metal cap texture.
	vec4 src = texture(u_texture, v_tex - vec2(0.00001));

	// only enable if you want ModelFlags.Cutout types to work, didn't end up using
	//if (src.a < u_cutout)
	//	discard;

	float depth = LinearizeDepth(gl_FragCoord.z, u_near, u_far);
	float fall = Map(v_world.z, 50, 0, 0, 1);
	float fade = Map(depth, 0.9, 1, 1, 0);
	vec3 col = mix(v_color.rbg, src.rgb, src.a); // SM64 uses the RBGA32 format

	// apply depth values
	gl_FragDepth = depth;

	// lighten texture color based on normal
	float lighten = max(0.0, -dot(v_normal, u_sun));
	col = mix(col, vec3(1,1,1), lighten * 0.10 * u_effects);

	// shadow
	float darken = max(0.0, dot(v_normal, u_sun));
	col = mix(col, vec3(4/255.0, 27/255.0, 44/255.0), darken * 0.80 * u_effects);

	// passthrough mode
	col = mix(col, u_silhouette_color.rgb, u_silhouette);

	// fade bottom to white
	col = mix(col, u_vertical_fog_color.rgb, fall);

	o_color = vec4(col, 1);
}