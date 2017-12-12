using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace comgr_u2
{
    class Vertex
    {
        public Vector3 Pos { get; set; }

        public Vector3 Color { get; set; }

        public Vector4 HomogeneColor
        {
            get
            {
                return new Vector4(Color / TransformedPos.W, 1 / TransformedPos.W);
            }
        }

        public Vector3 N { get; set; }

        public Vector3 TransformedN { get; set; }

        public Vector2 UV { get; set; }

        public Vector3 HomogeneUV { get
            {
                return new Vector3(UV / TransformedPos.W, 1 / TransformedPos.W);
            }
        }

        public Vector4 TransformedPos;

        public Vector4 HomogenePosition
        {
            get
            {
                return new Vector4(TransformedPos.X / TransformedPos.W, TransformedPos.Y / TransformedPos.W, 1, 1 / TransformedPos.W);
            }
        }

        public Vertex(Vector3 pos, Vector3 color)
        {
            Pos = pos;
            Color = color;
        }

        public void TransformPos(Matrix4x4 matrix)
        {
            TransformedPos = Vector4.Transform(Pos, matrix);
            TransformedPos.X /= TransformedPos.W;
            TransformedPos.Y /= TransformedPos.W;
        }

        public void TransformNormal(Matrix4x4 matrix)
        {
            Matrix4x4 normalMatrix;
            Matrix4x4.Invert(matrix, out normalMatrix);
            normalMatrix = Matrix4x4.Transpose(normalMatrix);

            TransformedN = Vector3.TransformNormal(N, normalMatrix);
        }
    }
}
