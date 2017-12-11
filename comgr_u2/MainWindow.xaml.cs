using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace comgr_u2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WriteableBitmap Bitmap { get; set; }
        public Vector3 light = new Vector3(0, 5, 0);


        Vector3[] points = new Vector3[]
        {
            new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1),
            new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1)
        };
        int[,] triangleIdx = new int[,]
        {
            { 0, 1, 2 },
            { 0, 2, 3 },
            { 7, 6, 5 },
            { 7, 5, 4 },
            { 0, 3, 7 },
            { 0, 7, 4 },
            { 2, 1, 5 },
            { 2, 5, 6 },
            { 3, 2, 6 },
            { 3, 6, 7 },
            { 1, 0, 4 },
            { 1, 4, 5 }
        };
        float screendistance = 3.8f;
        float intensityD = 0.6f;
        float intensityS = 0.6f;
        int k = 30;
        Texture tex = new Texture("./texture.bmp");

        public MainWindow()
        {
            Bitmap = new WriteableBitmap(400, 400, 8, 8, PixelFormats.Bgr24, null);
            InitializeComponent();

            CompositionTarget.Rendering += OnRender;
        }
        private float alpha = 0;

        private void OnRender(object sender, EventArgs args)
        {
            Vector3 translate = new Vector3(0, 0, 5);
            int w = Bitmap.PixelWidth;
            int h = Bitmap.PixelHeight;

            Matrix4x4 transMat = Matrix4x4.CreateTranslation(translate);
            Matrix4x4 rotMat = Matrix4x4.CreateRotationX(alpha);
            Matrix4x4 proj = Matrix4x4.Transpose(new Matrix4x4(
                w, 0, w / 2, 0,
                0, w, h / 2, 0,
                0, 0, 0, 0,
                0, 0, 1, 0));
            Matrix4x4 mvp = rotMat * transMat * proj;
            alpha = alpha + 0.05f;

            Vector2[] pointsProj = new Vector2[points.Length];

            //for (int i = 0; i < points.Length; i++)
            //{
            //    Vector3 p = points[i];
            //    Vector4 l = Vector4.Transform(p, mvp);
            //    pointsProj[i] = new Vector2(w * l.X / l.Z + w / 2f, w * l.Y / l.Z + h / 2f);//new Point(l.X/l.W, l.Y/l.W);
            //}
            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < triangleIdx.Length / 3; i++)
            {
                Vertex a = new Vertex(points[triangleIdx[i, 0]], Colors.Red.AsVector());
                Vertex b = new Vertex(points[triangleIdx[i, 1]], Colors.Green.AsVector());
                Vertex c = new Vertex(points[triangleIdx[i, 2]], Colors.Blue.AsVector());
                Vector3 n = Vector3.Normalize(Vector3.Cross(b.Pos - a.Pos, c.Pos - a.Pos));
                // Normal facing camera
                a.N = n;
                b.N = n;
                c.N = n;
                // This will look strange but is easly done.
                a.UV = new Vector2(1, 1);
                b.UV = new Vector2(0, 1);
                c.UV = new Vector2(1, 0);
                Triangle t = new Triangle(a, b, c);
                t.Texture = tex;
                t.Transform(mvp);


                // Inverse of mvp doesn't work
                Vector4 tmpN = Vector4.Transform(n, rotMat);
                Vector3 transfN = new Vector3(tmpN.X, tmpN.Y, tmpN.Z);
                a.TransformedN = transfN;
                b.TransformedN = transfN;
                c.TransformedN = transfN;
                
                    
                if (a.TransformedN.Z > -0.0001f)
                {
                    triangles.Add(t);
                }
            }

            byte[] pixels = new byte[w*h*3];
            float[] zbuffer = new float[w*h*3];
            for (int i = 0; i < zbuffer.Length; i++)
                zbuffer[i] = float.PositiveInfinity;


            /*foreach (Triangle t in triangles)
            {
                int zbufferPos = t.MinY();
                var det = 1f / (t.AB.X * t.AC.Y - t.AC.X * t.AB.Y);
                for (int y = t.MinY(); y < t.MaxY(); y++)
                {
                    var ap = new Vector2(0, y) - new Vector2(t.A.TransformedPos.X, t.A.TransformedPos.Y);
                    var u = (t.AC.Y * ap.X + -t.AC.X * ap.Y) * det;
                    var v = (-t.AB.Y * ap.X + t.AB.X * ap.Y) * det;

                    var uAdd = t.AC.Y * det;
                    var vAdd = -t.AB.Y * det;
                    for (int x = t.MinX(); x < t.MaxX(); x++, zbufferPos++)
                    {
                        if (u >= 0 && v >= 0 && (u + v) < 1)
                        {
                            float d = 0.0f; //TODO
                            if (zbuffer[zbufferPos] > d)
                                zbuffer[zbufferPos] = d;
                        }
                    }
                    u += uAdd;
                    v += vAdd;
                    ap.X++;
                }
            }*/

            foreach (Triangle t in triangles)
            {
                int minX = t.MinX();
                int minY = t.MinY();
                int maxX = t.MaxX();
                int maxY = t.MaxY();
                int nextMinX = Bitmap.PixelWidth - (maxX - minX);
                int colorPos = (minY*Bitmap.PixelWidth + minX) * 3;
                int zbufferPos = minY*Bitmap.PixelWidth + minX;
                var det = 1f / (t.AB.X * t.AC.Y - t.AC.X * t.AB.Y);
                for (int y = t.MinY(); y < t.MaxY(); y++)
                {
                    for (int x = t.MinX(); x < t.MaxX(); x++, colorPos += 3, zbufferPos++)
                    {
                        var ap = new Vector2(x - t.A.TransformedPos.X, y-t.A.TransformedPos.Y);
                        var u = (t.AC.Y * ap.X - t.AC.X * ap.Y) * det;
                        var v = (-t.AB.Y * ap.X + t.AB.X * ap.Y) * det;
                        if (u >= 0 && v >= 0 && (u + v) < 1)
                        {
                            float depth = t.A.TransformedPos.W + u * (t.B.TransformedPos.W - t.A.TransformedPos.W) + v * (t.C.TransformedPos.W - t.A.TransformedPos.W);
                            
                            if (zbuffer[zbufferPos] > depth && depth > screendistance) {
                                zbuffer[zbufferPos] = depth;
                                Vector3 cc = t.GetColor(u, v);
                                Vector3 ct = t.GetTexture(u, v);

                                // Diffuse
                                Vector3 triangleToLight = Vector3.Normalize(light - t.A.Pos);
                                float diffuse = Math.Max((Vector3.Dot(t.A.TransformedN, triangleToLight)) * intensityD, 0);

                                // Specular
                                Vector3 eye = -t.A.Pos;
                                float specular = Math.Max(((float) Math.Pow(Vector3.Dot(Vector3.Normalize((-t.A.TransformedN * Vector3.Dot(light, -t.A.TransformedN) - light) * 2), eye), k)) * intensityS, 0);

                                Color c = (cc * ct * diffuse + cc* ct * specular).AsColor();
                                pixels[colorPos] = c.R;
                                pixels[colorPos + 1] = c.G;
                                pixels[colorPos + 2] = c.B;
                            }
                        }
                    }
                    colorPos += nextMinX * 3;
                    zbufferPos += nextMinX;
                }
            }

            Bitmap.Lock();
            Bitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, w * 3, 0);
            Bitmap.Unlock();
        }
    }
}
