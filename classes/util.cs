using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace GameOfLifeSFML {
    public static class util {
        public static Vector2f randvec2(float minx, float maxx, float miny, float maxy) {
            Vector2f v = new Vector2f();
            v.X = randfloat(minx, maxx);
            v.Y = randfloat(miny, maxy);
            return v;
        }

        public static Vector2f randvec2(float min, float max) {
            return randvec2(min, max, min, max);
        }

        public static int randint(int min, int max) {
            Random r = new Random((int)(DateTime.Now.Ticks%Int32.MaxValue));
            return min + (int)(Math.Round(r.NextDouble() * (max - min)));
        }

        public static float randfloat(float min, float max) {
            Random r = new Random((int)(DateTime.Now.Ticks&Int32.MaxValue));
            return min + (float)r.NextDouble() * (max - min);
        }

        public static byte randbyte()  {
            Random r = new Random((int)(DateTime.Now.Ticks%Int32.MaxValue));
            return (byte)(r.NextDouble() * 256);
        }

        public static VertexArray rotate(VertexArray va, float angle) {
            VertexArray vaout = new VertexArray(va);
            
            for (uint i = 0; i < vaout.VertexCount; i++) {
                vaout[i] = new Vertex(rotate(vaout[i].Position, angle), vaout[i].Color);
            }
            
            return vaout;
        }

        public static Color hsvtocol(float hue, float sat, float val)
        {
            hue %= 360f;

            while(hue<0) hue += 360;

            if(sat<0f) sat = 0f;
            if(sat>1f) sat = 1f;

            if(val<0f) val = 0f;
            if(val>1f) val = 1f;

            int h = (int)(hue/60f);
            float f = hue/60-h;
            byte p = (byte)(val*(1f-sat) * 255);
            byte q = (byte)(val*(1f-sat*f) * 255);
            byte t = (byte)(val*(1f-sat*(1-f)) * 255);
            
            byte bVal = (byte)(val * 255);
            switch(h) {
                default:
                case 0:
                case 6: return new Color(bVal, t, p);
                case 1: return new Color(q, bVal, p);
                case 2: return new Color(p, bVal, t);
                case 3: return new Color(p, q, bVal);
                case 4: return new Color(t, p, bVal);
                case 5: return new Color(bVal, p, q);
            }
        }

        public static float distance(Vector2f a, Vector2f b) {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static float magnitude(Vector2f vec) {
            return (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
        }

        public static Vector2f normalise(Vector2f vec) {
            return vec / magnitude(vec);
        }

        public static float dot(Vector2f a, Vector2f b) {
            return (a.X * b.X) + (a.Y * b.Y);
        }

        public static Vector2f reflect(Vector2f dir, Vector2f normal) {
            return -2f * dot(dir, normal) * normal + dir;
        }

        public static Vector2f vector2f(double angle) {
            return new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static Vector2f rotate(Vector2f vector, double angle) {
            if (angle == 0) { return vector; }
            if (angle == (float)Math.PI /  2f) { return new Vector2f(-vector.Y,  vector.X); }
            if (angle == (float)Math.PI / -2f) { return new Vector2f( vector.Y, -vector.X); }
            if (angle == (float)Math.PI)       { return new Vector2f(-vector.X, -vector.Y); }

            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Vector2f(vector.X * c - vector.Y * s,
                                vector.X * s + vector.Y * c);
        }

        public static VertexArray VectorsToVertexArray(List<Vector2f> vectors, Color outlineColour, Color fillColour) {
            VertexArray va = new VertexArray(PrimitiveType.LineStrip, (uint)vectors.Count);

            for (uint i = 0; i < vectors.Count; i++) {
                Vertex v = new Vertex(vectors[(int)i], outlineColour);
                va[i] = v;
            }

            return va;
        }

        public static VertexArray VectorsToVertexArray(List<Vector2f> vectors, Color outlineColour) {
            return VectorsToVertexArray(vectors, outlineColour, Color.White);
        }

        public static VertexArray VectorsToVertexArray(List<Vector2f> vectors) {
            return VectorsToVertexArray(vectors, Color.White);
        }

        public static FloatRect RectShapeToFloatRect(RectangleShape rs) {
            return new FloatRect(rs.Position, rs.Size);
        }

        public static VertexArray translate(VertexArray va, Vector2f offset) {
            if (offset == new Vector2f()) { return va; }
            VertexArray newVa = new VertexArray(va.PrimitiveType, va.VertexCount);

            for (uint i = 0; i < va.VertexCount; i++) {
                newVa[i] = new Vertex(va[i].Position + offset, va[i].Color);
            }

            return newVa;
        }
        
        public static Vector2f multi(Vector2f a, Vector2f b) {
            return new Vector2f(a.X * b.X, a.Y * b.Y);
        }

        public static VertexArray scale(VertexArray va, float scale) {
            if (scale == 1f) { return va; }

            VertexArray newVa = new VertexArray(va);

            for (uint i = 0; i < newVa.VertexCount; i++) {
                newVa[i] = new Vertex(va[i].Position * scale, va[i].Color, va[i].TexCoords);
            }

            return newVa;
        }

        public static Vector2f randomScreenPos() {
            return randvec2(0, Global.ScreenSize.X, 0, Global.ScreenSize.Y);
        }
    } // end class
} // end namespace