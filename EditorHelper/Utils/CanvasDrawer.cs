using UnityEngine;

namespace EditorHelper.Utils {
    /// <summary>
    /// .
    /// </summary>
    public static class CanvasDrawer {
        /// <summary>
        /// Make Color to Texture2D.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Texture2D MakeTexture(int width, int height, Color col) {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Make Color to Round Rectangle Texture2D.
        /// </summary>
        /// <param name="resolutionmultiplier"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="borderRadius"></param>
        /// <param name="borderColor"></param>
        /// <returns></returns>
        public static Texture2D MakeRoundRectangle(int resolutionmultiplier, int width, int height, int borderThickness,
            int borderRadius, Color borderColor) {

            width = width * resolutionmultiplier;
            height = height * resolutionmultiplier;

            var texture = new Texture2D(width, height);
            var color = new Color[width * height];

            for (int x = 0; x < texture.width; x++) {
                for (int y = 0; y < texture.height; y++) {
                    color[x + width * y] = ColorBorder(x, y, width, height, borderThickness, borderRadius, borderColor);
                }
            }

            texture.SetPixels(color);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Find out what color is used at this position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="borderThickness"></param>
        /// <param name="borderRadius"></param>
        /// <param name="borderColor"></param>
        /// <returns></returns>
        private static Color ColorBorder(int x, int y, int width, int height, int borderThickness, int borderRadius,
            Color borderColor) {
            var internalRectangle = new Rect((borderThickness + borderRadius), (borderThickness + borderRadius),
                width - 2 * (borderThickness + borderRadius), height - 2 * (borderThickness + borderRadius));

            var point = new Vector2(x, y);
            if (internalRectangle.Contains(point)) return borderColor;

            var origin = Vector2.zero;

            if (x < borderThickness + borderRadius) {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(borderRadius + borderThickness, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(borderRadius + borderThickness, height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(borderRadius + borderThickness, y);
            } else if (x > width - (borderRadius + borderThickness)) {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(width - (borderRadius + borderThickness), borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(width - (borderRadius + borderThickness),
                        height - (borderRadius + borderThickness));
                else
                    origin = new Vector2(width - (borderRadius + borderThickness), y);
            } else {
                if (y < borderRadius + borderThickness)
                    origin = new Vector2(x, borderRadius + borderThickness);
                else if (y > height - (borderRadius + borderThickness))
                    origin = new Vector2(x, height - (borderRadius + borderThickness));
            }

            if (!origin.Equals(Vector2.zero)) {
                float distance = Vector2.Distance(point, origin);

                if (distance > borderRadius + borderThickness + 1) {
                    return Color.clear;
                } else if (distance > borderRadius + 1) {
                    return borderColor;
                }
            }

            return borderColor;
        }
    }
}