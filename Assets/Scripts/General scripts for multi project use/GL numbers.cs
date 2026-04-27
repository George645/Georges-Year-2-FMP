using System;
using UnityEngine;

namespace MiscFunctions {
    public class GLNumbers {
        static float classSize;
        static Vector3 forwardDirection;
        static Vector3 upwardsDirection;
        static Material LineMaterial;

        public static void DisplayNumber(int number, Vector3? position = null, Vector3? frontDirection = null, Vector3? upDirection = null, float size = 1) { // implement rotations
            LineMaterial = (Material)Resources.Load("BlackForLines");
            if (position == null) position = Vector3.zero;
            if (frontDirection == null) frontDirection = Vector3.forward;
            if (upDirection == null) upDirection = Vector3.up;

            forwardDirection = (Vector3)frontDirection;
            classSize = size;
            GL.Begin(GL.LINES);
            LineMaterial.SetPass(0);
            GL.Color(Color.black);
            Vector3 aPosition = (Vector3)position - Vector3.right * (number.ToString().Length / 2 * size);
            foreach (char character in number.ToString()) {
                switch (character) {
                    case '0':
                        Draw0((Vector3)aPosition);
                        break;
                    case '1':
                        Draw1((Vector3)aPosition);
                        break;
                    case '2':
                        Draw2((Vector3)aPosition);
                        break;
                    case '3':
                        Draw3((Vector3)aPosition);
                        break;
                    case '4':
                        Draw4((Vector3)aPosition);
                        break;
                    case '5':
                        Draw5((Vector3)aPosition);
                        break;
                    case '6':
                        Draw6((Vector3)aPosition);
                        break;
                    case '7':
                        Draw7((Vector3)aPosition);
                        break;
                    case '8':
                        Draw8((Vector3)aPosition);
                        break;
                    case '9':
                        Draw9((Vector3)aPosition);
                        break;
                    default:
                        throw new Exception("character not implemented" + character);
                }
                aPosition += Vector3.right * size;

            }
            GL.End();
        }
        #region drawNumbers
        private static void Draw0(Vector3 position) {

            for (int i = 0; i < 360; i++) { 
                GL.Vertex(position + classSize * new Vector3(0.5f * Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad)));
                GL.Vertex(position + classSize * new Vector3(0.5f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad)));

                GL.Vertex(position + classSize * new Vector3(0.5f * 0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad)));
                GL.Vertex(position + classSize * new Vector3(0.5f * 0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad)));
            }
        }
        private static void Draw1(Vector3 position) {
            GL.Vertex(position + classSize * new Vector3(.1f, 0, 1));
            GL.Vertex(position + classSize * new Vector3(.1f, 0, -1));
            GL.Vertex(position + classSize * new Vector3(-.1f, 0, 1));
            GL.Vertex(position + classSize * new Vector3(-.1f, 0, -1));

            GL.Vertex(position + classSize * new Vector3(.1f, 0, -1));
            GL.Vertex(position + classSize * new Vector3(-.1f, 0, -1));

            GL.Vertex(position + classSize * new Vector3(.1f, 0, 1));
            GL.Vertex(position + classSize * new Vector3(-.1f, 0, 1));
        }
        private static void Draw2(Vector3 position) {
            for (int i = 0; i < 180; i++) {
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position + classSize * (0.75f * new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (0.75f * new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }

            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.5f) - new Vector3(0.1f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.5f) + new Vector3(0, 0, 0.1f)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.5f) + new Vector3(0.1f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.5f) - new Vector3(0, 0, 0.1f)));

            GL.Vertex(position + classSize * (new Vector3(-.5f, 0, -1)));
            GL.Vertex(position + classSize * (new Vector3(.5f, 0, -1)));
            GL.Vertex(position + classSize * (0.75f * new Vector3(-.5f, 0, -1)));
            GL.Vertex(position + classSize * (0.75f * new Vector3(.5f, 0, -1)));
        }
        private static void Draw3(Vector3 position) {
            for (int i = -90; i < 180; i++) {
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
            for (int i = -180; i < 90; i++) {
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
        }
        private static void Draw4(Vector3 position) {
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 0.1f)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.1f)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.1f)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, -0.1f)));

            GL.Vertex(position + classSize * (new Vector3(-0.1f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(-0.1f, 0, -1)));
            GL.Vertex(position + classSize * (new Vector3(0.1f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(0.1f, 0, -1)));


            GL.Vertex(position + classSize * (new Vector3(0.1f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.1f)));
            GL.Vertex(position + classSize * (new Vector3(-0.1f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 0.1f)));
        }
        private static void Draw5(Vector3 position) {
            for (int i = -90; i < 180; i++) {
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f  * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f  * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f  * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f  * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }

            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(0.75f * -0.5f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(0.75f * -0.5f, 0, 1)));

            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 0.75f * 1)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.75f * 1)));
        }
        private static void Draw6(Vector3 position) {
            for (int i = 60; i < 200; i++) {
                GL.Vertex(position + classSize * new Vector3(0.5f * Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad)));
                GL.Vertex(position + classSize * new Vector3(0.5f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad)));

                GL.Vertex(position + classSize * new Vector3(0.5f * 0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad)));
                GL.Vertex(position + classSize * new Vector3(0.5f * 0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad)));
            }
            for (int i = 0; i < 360; i++) {
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
        }
        private static void Draw7(Vector3 position) {
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 1)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, 0.75f * 1)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.75f * 1)));

            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.5f) - new Vector3(0.1f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.5f) + new Vector3(0, 0, 0.1f)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0.5f) + new Vector3(0.1f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(-0.5f, 0, -0.5f) - new Vector3(0, 0, 0.1f)));
        }
        private static void Draw8(Vector3 position) {
            for (int i = 0; i < 360; i++) {
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f  * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f  * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f  * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f  * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
            for (int i = 0; i < 360; i++) {
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position - classSize * (new Vector3(0, 0, 0.5f) - 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
        }
        private static void Draw9(Vector3 position) {

            for (int i = 0; i < 360; i++) {
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, Mathf.Sin((1 + i) * Mathf.Deg2Rad))));

                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos(i * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin(i * Mathf.Deg2Rad))));
                GL.Vertex(position + classSize * (new Vector3(0, 0, 0.5f) + 0.5f * new Vector3(0.75f * Mathf.Cos((i + 1) * Mathf.Deg2Rad), 0, 0.75f * Mathf.Sin((1 + i) * Mathf.Deg2Rad))));
            }
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(0.5f, 0, -1)));
            GL.Vertex(position + classSize * (new Vector3(0.75f * 0.5f, 0, 0)));
            GL.Vertex(position + classSize * (new Vector3(0.75f * 0.5f, 0, -1)));
        }
        #endregion
    }
}
