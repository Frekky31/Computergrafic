using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Numerics;

namespace RayTracing.Texture
{
    public static class TilePattern
    {
        const float PI = 3.14159265358979323846f;

        // Public sampler: fragCoord in pixels, iResolution = (width,height), iTime unused here but kept for parity
        public static Vector3 Sample(Vector2 fragCoord, Vector2 iResolution, float iTime = 0f)
        {
            // st = gl_FragCoord.xy / u_resolution.xy;
            Vector2 st = new Vector2(fragCoord.X / iResolution.X, fragCoord.Y / iResolution.Y);

            st = Tile(st, 3.0f);
            st = RotateTilePattern(st);

            // step(st.x, st.y) in GLSL returns 1.0 if st.y >= st.x else 0.0
            float v = Step(st.X, st.Y);

            return new Vector3(v, v, v);
        }

        // --- helpers (GLSL -> C#) ---

        // rotate2D: translate to center (0.5), rotate, translate back
        static Vector2 Rotate2D(Vector2 _st, float _angle)
        {
            _st -= new Vector2(0.5f, 0.5f);
            float c = MathF.Cos(_angle);
            float s = MathF.Sin(_angle);
            // mat2(c,-s, s,c) * _st  => (c*x - s*y, s*x + c*y)
            Vector2 rotated = new Vector2(c * _st.X - s * _st.Y, s * _st.X + c * _st.Y);
            rotated += new Vector2(0.5f, 0.5f);
            return rotated;
        }

        // tile: multiply by zoom and fract
        static Vector2 Tile(Vector2 _st, float _zoom)
        {
            _st *= _zoom;
            return Fract(_st);
        }

        // rotateTilePattern: the tiled-rotation logic from the shader
        static Vector2 RotateTilePattern(Vector2 _st)
        {
            // scale coords by 2x2 grid
            _st *= 2.0f;

            // compute index according to position
            float index = 0.0f;
            index += Step(1.0f, Mod(_st.X, 2.0f));
            index += Step(1.0f, Mod(_st.Y, 2.0f)) * 2.0f;

            // make each cell 0..1
            _st = Fract(_st);

            // rotate cells based on index
            if (index == 1.0f)
            {
                // rotate cell 1 by 90 degrees
                _st = Rotate2D(_st, PI * 0.5f);
            }
            else if (index == 2.0f)
            {
                // rotate cell 2 by -90 degrees
                _st = Rotate2D(_st, PI * -0.5f);
            }
            else if (index == 3.0f)
            {
                // rotate cell 3 by 180 degrees
                _st = Rotate2D(_st, PI);
            }

            return _st;
        }

        // --- small GLSL-like utilities ---

        // fract: fractional part
        static Vector2 Fract(Vector2 v) => new Vector2(Fract(v.X), Fract(v.Y));
        static float Fract(float x) => x - MathF.Floor(x);

        // mod: GLSL mod(x,y) = x - y * floor(x/y)
        static float Mod(float x, float y) => x - y * MathF.Floor(x / y);

        // step(edge, x) : returns 0 if x < edge, else 1
        static float Step(float edge, float x) => x < edge ? 0.0f : 1.0f;
    }

}
