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
        public Vector3 light = new Vector3(4, -3, -5);


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
        Vector3 ambientLight = new Vector3(0.0f, 0.0f, 0.0f);
        float screendistance = 3.8f;
        float intensityD = 0.6f;
        float intensityS = 0.6f;
        int k = 10;
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
            Matrix4x4 rotMat = Matrix4x4.CreateFromYawPitchRoll(alpha, alpha, 0);
            Matrix4x4 proj = Matrix4x4.Transpose(new Matrix4x4(
                w, 0, w / 2, 0,
                0, w, h / 2, 0,
                0, 0, 0, 0,
                0, 0, 1, 0));
            Matrix4x4 mv = rotMat * transMat;
            alpha = alpha + 0.05f;

            Vector2[] pointsProj = new Vector2[points.Length];

            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < triangleIdx.Length / 3; i++)
            {
                Vertex a = new Vertex(points[triangleIdx[i, 0]], Colors.Red.AsVector());
                Vertex b = new Vertex(points[triangleIdx[i, 1]], Colors.Green.AsVector());
                Vertex c = new Vertex(points[triangleIdx[i, 2]], Colors.Blue.AsVector());
                Vector3 n = Vector3.Normalize(Vector3.Cross(c.Pos - a.Pos, b.Pos - a.Pos));
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
                t.TransformNormal(mv);
                t.TransformPos(mv * proj);


                if (a.TransformedN.Z < 0.0001f)
                {
                    triangles.Add(t);
                }
            }

            float[] zbuffer = new float[w * h * 3];
            Vector3[] colorbuffer = new Vector3[w * h];
            Vector2[] textureUVbuffer = new Vector2[w * h];
            Vector3[] normalbuffer = new Vector3[w * h];
            Vector3[] posbuffer = new Vector3[w * h];
            Texture[] texturesbuffer = new Texture[w * h];

            for (int i = 0; i < zbuffer.Length; i++)
                zbuffer[i] = float.PositiveInfinity;


            foreach (Triangle t in triangles)
            {
                int minX = t.MinX();
                int minY = t.MinY();
                int maxX = t.MaxX();
                int maxY = t.MaxY();
                int nextMinX = w - (maxX - minX);
                int index = minY * w + minX;
                var det = 1f / (t.AB.X * t.AC.Y - t.AC.X * t.AB.Y);
                var ap = new Vector2(minX - t.A.TransformedPos.X, minY - t.A.TransformedPos.Y);
                var uy = det * (t.AC.Y * ap.X - t.AC.X * ap.Y);
                var uxstep = det * t.AC.Y;
                var uystep = det * -t.AC.X;

                var vy = det * (-t.AB.Y * ap.X + t.AB.X * ap.Y);
                var vxstep = det * -t.AB.Y;
                var vystep = det * t.AB.X;
                for (int y = t.MinY(); y < t.MaxY(); y++)
                {
                    var u = uy;
                    var v = vy;
                    for (int x = t.MinX(); x < t.MaxX(); x++, index++)
                    {
                        //var u = (t.AC.Y * ap.X - t.AC.X * ap.Y) * det;
                        //var v = (-t.AB.Y * ap.X + t.AB.X * ap.Y) * det;
                        if (u >= 0 && v >= 0 && (u + v) < 1)
                        {
                            float depth = t.A.TransformedPos.W + u * (t.B.TransformedPos.W - t.A.TransformedPos.W) + v * (t.C.TransformedPos.W - t.A.TransformedPos.W);

                            if (zbuffer[index] > depth && depth > screendistance)
                            {
                                zbuffer[index] = depth;
                                Vector3 pos = t.GetPosition(u, v);
                                pos.X /= w;
                                pos.Y /= w;
                                posbuffer[index] = pos;

                                colorbuffer[index] = t.GetColor(u, v);
                                textureUVbuffer[index] = t.GetTexture(u, v);
                                texturesbuffer[index] = t.Texture;

                                normalbuffer[index] = t.A.TransformedN;

                            }
                        }
                        u += uxstep;
                        v += vxstep;
                    }
                    uy += uystep;
                    vy += vystep;
                    //ap.Y++;
                    index += nextMinX;
                }
            }

            byte[] pixels = new byte[w * h * 3];

            Parallel.For(0, w * h, i =>
            {
                if (zbuffer[i] == float.PositiveInfinity) return;
                int j = i * 3;
                // Diffuse
                Vector3 triangleToLight = Vector3.Normalize(light - posbuffer[i]);
                float diffuse = Math.Max((Vector3.Dot(normalbuffer[i], triangleToLight)) * intensityD, 0);

                // Specular
                Vector3 toEye = Vector3.Normalize(-posbuffer[i]);
                Vector3 r = Vector3.Normalize(normalbuffer[i] * Vector3.Dot(triangleToLight, normalbuffer[i]) * 2 - triangleToLight);

                float specular = ((float)Math.Pow(Math.Max(Vector3.Dot(r, toEye), 0), k)) * intensityS;
                Vector3 white = new Vector3(1, 1, 1);
                Texture t = texturesbuffer[i];
                Vector3 ct = white;
                if (t != null)
                    ct = t.Interpolate(textureUVbuffer[i]);
                Color c = (ambientLight + colorbuffer[i] * ct * diffuse + white * specular).AsColor();
                pixels[j] = c.R;
                pixels[j + 1] = c.G;
                pixels[j + 2] = c.B;
            });


            Bitmap.Lock();
            Bitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, w * 3, 0);
            Bitmap.Unlock();
        }
    }
}
