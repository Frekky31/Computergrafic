using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing.Core
{
    public class RenderTarget(int w, int h)
    {
    public readonly Vector3[] ColourBuffer = new Vector3[w * h];
	public readonly object[] locks = new object[w * h];

	public readonly int Width = w;
	public readonly int Height = h;
	public readonly Vector2 Size = new(w, h);

	public void Clear(Vector3 bgCol)
	{
		for (int i = 0; i < ColourBuffer.Length; i++)
		{
			ColourBuffer[i] = bgCol;
		}

		if (locks[0] == null)
		{
			for (int i = 0; i < locks.Length; i++)
			{
				locks[i] = new object();
			}
		}
	}
}
}
