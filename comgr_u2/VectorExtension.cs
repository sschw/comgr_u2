using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace comgr_u2
{
    static class VectorExtension
    {
        public static Vector3 AsVector(this Color c)
        {
            return new Vector3(c.ScR, c.ScG, c.ScB);
        }

        public static Vector3 AsVector(this System.Drawing.Color c)
        {
            return Color.FromRgb(c.R, c.G, c.B).AsVector();
        }

        public static Color AsColor(this Vector3 v)
        {
            return Color.FromScRgb(1, Math.Min(v.X, 1), Math.Min(v.Y, 1), Math.Min(v.Z, 1));
        }

        public static Vector2 ToVec2(this Vector4 v)
        {
            return new Vector2(v.X, v.Y);
        }
    }
}
