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

        public Texture(string path)
        {
            tex = new Bitmap(path);
        }

        public Vector3 Interpolate(Vector2 uv)
        {
            //return tex.GetPixel((int) (uv.X * tex.Width), (int) (uv.Y * tex.Height)).AsVector();

            /* Bilinear */
            int posU = (int)(uv.X * tex.Width-1);
            int posV = (int)(uv.Y * tex.Height-1);
            Vector3 uv00 = tex.GetPixel(posU, posV).AsVector();
            Vector3 uv10 = tex.GetPixel((posU + 1) % tex.Width, posV).AsVector();
            Vector3 uv01 = tex.GetPixel(posU, (posV + 1) % tex.Height).AsVector();
            Vector3 uv11 = tex.GetPixel((posU + 1) % tex.Width, (posV + 1) % tex.Height).AsVector();
            return (uv10 * uv.X + uv00 * (1 - uv.X)) * (1 - uv.Y) + (uv11 * uv.X + uv01 * (1 - uv.X)) * uv.Y;
        }
    }
}
