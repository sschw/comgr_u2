using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace comgr_u2
{
    class Triangle
    {
        public Vertex A { get; set; }
        public Vertex B { get; set; }
        public Vertex C { get; set; }

        public Texture Texture { get; set; }

        // 2d
        public Vector2 AB { get; set; }
        public Vector2 AC { get; set; }

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Vector3 GetPosition(float u, float v)
        {
            Vector4 pos = A.HomogenePosition + u * (B.HomogenePosition - A.HomogenePosition) + v * (C.HomogenePosition - A.HomogenePosition);

            Vector3 retPos = new Vector3(pos.X, pos.Y, pos.Z) / pos.W;
            return retPos;
        }

        public Vector3 GetColor(float u, float v)
        {
            Vector4 color = A.HomogeneColor + u * (B.HomogeneColor - A.HomogeneColor) + v * (C.HomogeneColor - A.HomogeneColor);

            Vector3 returnColor = new Vector3(color.X, color.Y, color.Z) / color.W;
            return returnColor;
        }

        public Vector3 GetTexture(float u, float v)
        {
            if (Texture == null) return new Vector3(1, 1, 1);
            Vector3 textureCoordinates = A.HomogeneUV + u * (B.HomogeneUV - A.HomogeneUV) + v * (C.HomogeneUV - A.HomogeneUV);
            Vector2 uv = new Vector2(textureCoordinates.X, textureCoordinates.Y) / textureCoordinates.Z;
            return Texture.Interpolate(uv);
        }

        public void TransformNormal(Matrix4x4 matrix)
        {
            A.TransformNormal(matrix);
            B.TransformNormal(matrix);
            C.TransformNormal(matrix);
        }

        public void TransformPos(Matrix4x4 matrix)
        {
            A.TransformPos(matrix);
            B.TransformPos(matrix);
            C.TransformPos(matrix);

            AB = (B.TransformedPos - A.TransformedPos).ToVec2();
            AC = (C.TransformedPos - A.TransformedPos).ToVec2();
        }

        public int MinX()
        {
            return (int)Math.Min(A.TransformedPos.X, Math.Min(B.TransformedPos.X, C.TransformedPos.X));
        }

        public int MinY()
        {
            return (int)Math.Min(A.TransformedPos.Y, Math.Min(B.TransformedPos.Y, C.TransformedPos.Y));
        }

        public int MaxX()
        {
            return (int)Math.Max(A.TransformedPos.X, Math.Max(B.TransformedPos.X, C.TransformedPos.X));
        }

        public int MaxY()
        {
            return (int)Math.Max(A.TransformedPos.Y, Math.Max(B.TransformedPos.Y, C.TransformedPos.Y));
        }
    }
}
