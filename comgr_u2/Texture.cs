using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace comgr_u2
{
    class Texture
    {
        private readonly Bitmap tex;
        private readonly Vector3[,] data;
        private readonly int width, height;

        public Texture(string path)
        {
            tex = new Bitmap(path);
            width = tex.Width;
            height = tex.Height;
            data = new Vector3[tex.Width, tex.Height];
            for (int y = 0; y < tex.Height; y++)
                for (int x = 0; x < tex.Width; x++)
                    data[x, y] = tex.GetPixel(x, y).AsVector();
        }

        public Vector3 Interpolate(Vector2 uv)
        {
            //return tex.GetPixel((int) (uv.X * tex.Width), (int) (uv.Y * tex.Height)).AsVector();

            /* Bilinear */
            float uw = uv.X * (width - 1);
            float vh = uv.Y * (height - 1);
            int posU = (int) uw;
            int posV = (int) vh;
            int posU1 = (posU + 1) % width;
            int posV1 = (posV + 1) % height;
            Vector3 uv00 = data[posU, posV];
            Vector3 uv10 = data[posU1, posV];
            Vector3 uv01 = data[posU, posV1];
            Vector3 uv11 = data[posU1, posV1];
            //return (uv10 * uv.X + uv00 * (1 - uv.X)) * (1 - uv.Y) + (uv11 * uv.X + uv01 * (1 - uv.X)) * uv.Y;
            return Vector3.Lerp(Vector3.Lerp(uv00, uv10, uw-posU), Vector3.Lerp(uv11, uv01, uw-posU), vh-posV);
        }
    }
}
