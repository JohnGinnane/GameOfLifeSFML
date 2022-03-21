using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;

namespace GameOfLifeSFML {
    public static class intersection {
        public static bool pointInsidePoint(Vector2f a, Vector2f b) {
            return (a.X == b.X && a.Y == b.Y);
        }

        public static bool pointInsideCircle(Vector2f point, Vector2f circlePos, float circleRadius) {
            
            float distX = point.X - circlePos.X;
            float distY = point.Y - circlePos.Y;
            
            // We don't need to use square root here, we can just 
            // compare the squared values
            float distSq = distX * distX + distY * distY;
            float radSq = circleRadius * circleRadius;

            return distSq < radSq;
        }

        public static bool circleInsideCircle(Vector2f aPos, float aRadius, Vector2f bPos, float bRadius) {
            float distX = aPos.X - bPos.X;
            float distY = aPos.Y - bPos.Y;
            
            float distSq = distX * distX + distY * distY;
            float radSq = (aRadius + bRadius) * (aRadius + bRadius);

            return distSq < radSq;
        }

        public static bool pointInsideRectangle(Vector2f p, FloatRect r) {
            return (p.X >= r.Left && p.X <= r.Left + r.Width && p.Y >= r.Top && p.Y <= r.Top + r.Height);
        }

        public static bool rectangleInsideRectangle(FloatRect a, FloatRect b) {
            if (a.Left + a.Width >= b.Left &&
                a.Left           <= b.Left + b.Width && 
                a.Top + a.Height >= b.Top &&
                a.Top            <= b.Top + b.Height) {
                return true;
            }

            return false;
        }

        public static bool circleInsideRectangle(Vector2f circlePos, float circleRadius, FloatRect rect) {
            float testX = circlePos.X;
            float testY = circlePos.Y;

            if (circlePos.X < rect.Left) {
                // left edge
                testX = rect.Left;
            } else if (circlePos.X > rect.Left + rect.Width) {
                // right edge
                testX = rect.Left + rect.Width;
            }

            if (circlePos.Y < rect.Top) {
                // top edge
                testY = rect.Top;
            } else if (circlePos.Y > rect.Top + rect.Height) {
                // bottom edge
                testY = rect.Top + rect.Height;
            }

            return pointInsideCircle(new Vector2f(testX, testY), circlePos, circleRadius);
        }

        public static bool pointInsideLine(Vector2f point, Vector2f lineStart, Vector2f lineEnd) {
            float length = util.distance(lineStart, lineEnd);
            float d1 = util.distance(point, lineStart);
            float d2 = util.distance(point, lineEnd);
            float buffer = 0.1f;

            if (d1 + d2 >= length - buffer && d1 + d2 <= length + buffer) {
                return true;
            }

            return false;
        }

        public static bool lineInsideCircle(Vector2f lineStart, Vector2f lineEnd, Vector2f circlePos, float circleRadius) {            
            if (pointInsideCircle(lineStart, circlePos, circleRadius)) { return true; }
            if (pointInsideCircle(lineEnd,   circlePos, circleRadius)) { return true; }

            float distX = lineStart.X - lineEnd.X;
            float distY = lineStart.Y - lineEnd.Y;

            float lengthSq = (distX * distX) + (distY * distY);
            float dot = util.dot(circlePos - lineStart, lineEnd - lineStart) / lengthSq;
            Vector2f closestPoint = lineStart + (dot * (lineEnd - lineStart));

            if (!pointInsideLine(closestPoint, lineStart, lineEnd)) { return false; }

            return pointInsideCircle(closestPoint, circlePos, circleRadius);
        }

        public static bool lineInsideLine(Vector2f lineAStart, Vector2f lineAEnd, Vector2f lineBStart, Vector2f lineBEnd) {
            float x1 = lineAStart.X;
            float y1 = lineAStart.Y;
            float x2 = lineAEnd.X;
            float y2 = lineAEnd.Y;
            float x3 = lineBStart.X;
            float y3 = lineBStart.Y;
            float x4 = lineBEnd.X;
            float y4 = lineBEnd.Y;

            float uA = ((x4-x3)*(y1-y3) - (y4-y3)*(x1-x3)) / ((y4-y3)*(x2-x1) - (x4-x3)*(y2-y1));
            float uB = ((x2-x1)*(y1-y3) - (y2-y1)*(x1-x3)) / ((y4-y3)*(x2-x1) - (x4-x3)*(y2-y1));

            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1) { return true; }

            return false;
        }

        public static bool lineInsideRectangle(Vector2f lineStart, Vector2f lineEnd, FloatRect rect) {
            // check if either end is inside the rectangle to save time
            if (pointInsideRectangle(lineStart, rect)) { return true; }
            if (pointInsideRectangle(lineEnd, rect)) { return true; }

            bool left = lineInsideLine(lineStart, lineEnd, new Vector2f(rect.Left, rect.Top), new Vector2f(rect.Left, rect.Top + rect.Height));
            bool right = lineInsideLine(lineStart, lineEnd, new Vector2f(rect.Left+rect.Width, rect.Top), new Vector2f(rect.Left+rect.Width, rect.Top+rect.Height));
            bool top = lineInsideLine(lineStart, lineEnd, new Vector2f(rect.Left, rect.Top), new Vector2f(rect.Left+rect.Width, rect.Top));
            bool bottom = lineInsideLine(lineStart, lineEnd, new Vector2f(rect.Left, rect.Top+rect.Height), new Vector2f(rect.Left+rect.Width, rect.Top+rect.Height));

            if (left || right || top || bottom) { return true; }

            return false;
        }

        public static bool pointInsidePolygon(Vector2f point, List<Vector2f> polygon) {
            bool intersects = false;
            
            for (int i = 0; i < polygon.Count; i++) {
                int j = (i+1) % polygon.Count;
                
                Vector2f vc = polygon[i];
                Vector2f vn = polygon[j];
                float px = point.X;
                float py = point.Y;

                // www.jeffreythompson.org/collision-detection/poly-point.php
                if (((vc.Y >= py && vn.Y <  py) ||
                     (vc.Y <  py && vn.Y >= py)) &&
                     (px < (vn.X - vc.X) * (py - vc.Y) / (vn.Y - vc.Y) + vc.X)) {
                    intersects = !intersects;
                }
            }

            return intersects;
        }
    
        public static bool circleInsidePolygon(Vector2f circlePos, float circleRadius, List<Vector2f> polygon) {
            if (pointInsidePolygon(circlePos, polygon)) { return true; }

            for (int i = 0; i < polygon.Count; i++) {
                Vector2f vc = polygon[i];
                Vector2f vn = polygon[(i+1)%polygon.Count];

                if (lineInsideCircle(vc, vn, circlePos, circleRadius)) {
                    return true;
                }
            }

            return false;
        }

        public static bool rectangleInsidePolygon(FloatRect rect, List<Vector2f> polygon) {
            for (int i = 0; i < polygon.Count; i++) {
                Vector2f vc = polygon[i];
                Vector2f vn = polygon[(i+1)%polygon.Count];

                if (lineInsideRectangle(vc, vn, rect)) {
                    return true;
                }
            }
            
            if (pointInsidePolygon(new Vector2f(rect.Left, rect.Top), polygon)) { return true; }

            return false;
        }

        public static bool lineInsidePolygon(Vector2f lineStart, Vector2f lineEnd, List<Vector2f> polygon) {
            if (pointInsidePolygon(lineStart, polygon)) { return true; }
            if (pointInsidePolygon(lineEnd,   polygon)) { return true; }

            for (int i = 0; i < polygon.Count; i++) {
                Vector2f vc = polygon[i];
                Vector2f vn = polygon[(i+1)%polygon.Count];

                if (lineInsideLine(vc, vn, lineStart, lineEnd)) {
                    return true;
                }
            }

            return false;
        }

        public static bool polygonInsidePolygon(List<Vector2f> polygon1, List<Vector2f> polygon2) {
            for (int i = 0; i < polygon1.Count; i++) {
                Vector2f vc = polygon1[i];
                Vector2f vn = polygon1[(i+1)%polygon1.Count];

                if (lineInsidePolygon(vc, vn, polygon2)) {
                    return true;
                }
                
                if (pointInsidePolygon(vc, polygon2)) {
                    return true;
                }
            }

            return false;
        }
    }
}