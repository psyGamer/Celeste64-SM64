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
    // In SM64 the coords are +Y up instead of +Z up
    vec3 pos = a_position.xzy;
    vec3 norm = a_normal.xzy;
    
	gl_Position = u_mvp * vec4(pos, 1.0);

	v_normal = TransformNormal(norm, u_model);
    v_color = a_color;
    v_tex = a_tex;
	v_world = vec3(u_model * vec4(pos, 1.0));
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
uniform float     u_model_state;

in vec2 v_tex;
in vec3 v_normal;
in vec3 v_color;
in vec3 v_world;

layout(location = 0) out vec4 o_color;

const int  ModelStateMetal      = 1;
const int  ModelStateNoiseAlpha = 2;
const vec2 MetalTextureWrap = vec2(64.0 / 704.0, 32.0 / 64.0);

float random(in vec3 value) {
    float random = dot(sin(value), vec3(12.9898, 78.233, 37.719));
    return fract(sin(random) * 143758.5453);
}

void main(void)
{
    // Offset UVs very slightly, so that 1|1 coords (meaning transparent) don't accidentally 
    // wrap back around to the metal cap texture.
    vec4 src = texture(u_texture, v_tex - vec2(0.00001));
    
    float alpha = src.a;
    // Set alpha to 1.0 when UV coords are 1|1 -> flat color through vertex color
    // Or when we're using the metal cap
    if (v_tex == vec2(1.0) || (int(u_model_state) & ModelStateMetal) != 0)
        alpha = 1.0;
    
    // Ugly special caes for the wing cap:
    // Discard instead of using vertex color
	if (alpha < 0.1 && v_color == vec3(1.0))
		discard;

	float depth = LinearizeDepth(gl_FragCoord.z, u_near, u_far);
	float fall = Map(v_world.z, 50, 0, 0, 1);
	float fade = Map(depth, 0.9, 1, 1, 0);
	vec3 col = mix(v_color.rgb, src.rgb, src.a);

    if ((int(u_model_state) & ModelStateMetal) != 0) {
        vec3 metalXY = texture(u_texture, v_normal.xy * MetalTextureWrap / 2.0f + MetalTextureWrap / 2.0f).rgb;
        vec3 metalYZ = texture(u_texture, v_normal.yz * MetalTextureWrap / 2.0f + MetalTextureWrap / 2.0f).rgb;
        vec3 metalXZ = texture(u_texture, v_normal.xz * MetalTextureWrap / 2.0f + MetalTextureWrap / 2.0f).rgb;

        col = metalXY * 0.33 + metalYZ * 0.33 + metalXZ * 0.33;
    }

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

    float noiseAlpha = 1.0;
    // Apply noise alpha after discard, since we still want to draw the outline properly in that case
    if ((int(u_model_state) & ModelStateNoiseAlpha) != 0) {
        noiseAlpha *= floor(random(floor(vec3(gl_FragCoord.xy, u_time * 30.0))) + 0.5);
    }
	o_color = vec4(col, noiseAlpha);
}