using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace myScript {
    public class HeightMapManager : MonoBehaviour {

        public FrameController frameController;

        public Dictionary<string, TileReader> tileReaders = new Dictionary<string, TileReader>();

        public Texture2D debugTexture;
        public int updateFrame = 10;
        private int _counter = 0;

        private void DebugWithPixels(TileRegion region)
            {
                List<Color> pixels = new List<Color>();
                for (int y = debugTexture.height - 1; y >= 0 ; y--) {
                    for (int x = 0; x < debugTexture.width; x++) {
                        float depth = 0f;
                        if (x < region.width && y < region.height) {
                            int i = x * region.height + y;
                            depth = region.depths[i];
                        }
                        pixels.Add(Color.Lerp(Color.green, Color.blue, math.clamp(depth / 255f, 0, 1)));
                    }
                }
                debugTexture.SetPixels(pixels.ToArray());
                debugTexture.Apply();
            }

        public TileRegion CurrentRegion { get; private set; }
        public static HeightMapManager Instance { get; private set; }

        private void Awake()
            {
                Instance = this;
            }

        private PinHandler _visualizer;

        private void Update()
            {
                if (!frameController)
                    return;

                if (_counter < updateFrame ) {
                    if (CurrentRegion != null) {
                        CurrentRegion.worldFrame = frameController.GetWorldFrame();
                    }
                    _counter++;
                    return;
                }
                _counter = 0;

                CurrentRegion = UpdateTileRegion();
            }

        private TileRegion UpdateTileRegion()
            {
                var tileRects = frameController.GetTilesInFrame();

                //if there is no tiles 
                if (tileRects.Count == 0) {
                    return new TileRegion();
                }

                foreach (var tileRect in tileRects ) {
                    if (!tileReaders.ContainsKey(tileRect.id)) {
                        var reader = new TileReader();
                        Texture2D tileTexture = Resources.Load<Texture2D>("HeightMaps_500/terrain_" + tileRect.id);
                        if (tileTexture != null) {
                            reader.LoadTexture(tileTexture);
                            tileReaders.Add(tileRect.id, reader);
                        }
                    }
                }

                TileRegion resultRegion =  new TileRegion {worldFrame = frameController.GetWorldFrame() };

                var topLeft = tileRects.Find((t) => t.horizontal == Side.Left  && t.vertical == Side.Top);
                var topRight = tileRects.Find((t) => t.horizontal == Side.Right  && t.vertical == Side.Top);
                var bottomRight = tileRects.Find((t) => t.horizontal == Side.Right  && t.vertical == Side.Bottom);
                var bottomLeft = tileRects.Find((t) => t.horizontal == Side.Left  && t.vertical == Side.Bottom);

                if (topLeft == null || bottomRight == null) {
                    return new TileRegion();
                }
                
                if (topLeft.id == bottomRight.id) { //frame fully contained in single tile
                    var rect = new Rect(topLeft.rect.width, topLeft.rect.height, bottomRight.rect.width - topLeft.rect.width, bottomRight.rect.height - topLeft.rect.height);
                    if (rect.width != rect.height) {
                        rect.width = rect.height; //ensure perfect squares (can be off by 1 because of rounding)
                    }
                    TileRect tr = new TileRect {
                        id = topLeft.id,
                        rect = rect
                    };
                    resultRegion = GetTileDepths(tr);
                } else if (topLeft.HorizontalId == bottomRight.HorizontalId)  { //top bottom relationship 

                    TileRect top = new TileRect {
                        id = topLeft.id,
                        rect = new Rect(topLeft.rect.width, topLeft.rect.height, bottomRight.rect.width - topLeft.rect.width, 1f - topLeft.rect.height)
                    };
                    TileRect bottom = new TileRect {
                        id = bottomRight.id,
                        rect = new Rect(topLeft.rect.width, 0, bottomRight.rect.width - topLeft.rect.width, bottomRight.rect.height)
                    };

                    StitchDepthsTB(resultRegion, GetTileDepths(top), GetTileDepths(bottom));
                } else if (topLeft.VerticalId == bottomRight.VerticalId)  { //left right relationship

                    TileRect left = new TileRect {
                        id = topLeft.id,
                        rect = new Rect(topLeft.rect.width, topLeft.rect.height, 1f - topLeft.rect.width, bottomRight.rect.height - topLeft.rect.height)
                    };
                    var topRegion = GetTileDepths(left);
                    TileRect right = new TileRect {
                        id = bottomRight.id,
                        rect = new Rect(0f, topLeft.rect.height, bottomRight.rect.width, bottomRight.rect.height - topLeft.rect.height)
                    };
                    resultRegion.depths.AddRange(topRegion.depths);
                    var bottomRegion = GetTileDepths(right);
                    resultRegion.depths.AddRange(bottomRegion.depths);
                    resultRegion.width = topRegion.width + bottomRegion.width;
                    resultRegion.height = bottomRegion.height;
                } else { //4 quads

                    TileRect tl = new TileRect {
                        id = topLeft.id,
                        rect = new Rect(topLeft.rect.width, topLeft.rect.height, 1f - topLeft.rect.width, 1f - topLeft.rect.height)
                    };
                    TileRect tr = new TileRect {
                        id = topRight.id,
                        rect = new Rect(0f, topRight.rect.height, topRight.rect.width, 1f - topRight.rect.height)
                    };
                    TileRect bl = new TileRect {
                        id = bottomLeft.id,
                        rect = new Rect(bottomLeft.rect.width, 0f, 1f - bottomLeft.rect.width, bottomLeft.rect.height)
                    };
                    TileRect br = new TileRect {
                        id = bottomRight.id,
                        rect = new Rect(0f, 0f, bottomRight.rect.width, bottomRight.rect.height)
                    };

                    StitchDepthsTB(resultRegion, GetTileDepths(tl), GetTileDepths(bl));
                    StitchDepthsTB(resultRegion, GetTileDepths(tr), GetTileDepths(br));
                }
                return resultRegion;
            }

        private void StitchDepthsTB(TileRegion tileRegion, TileRegion topRegion, TileRegion bottomRegion)
            {
                if (topRegion.width != bottomRegion.width) {
                    throw new ArgumentException("widths not equal");
                }

                //increment the height to allow stacking
                tileRegion.width += topRegion.width;
                tileRegion.height = topRegion.height + bottomRegion.height;
                for (int c = 0; c < topRegion.width; c++) {
                    int r;
                    for (r = 0; r < topRegion.height; r++) {
                        tileRegion.depths.Add(topRegion.depths[c * topRegion.height + r]);
                    }
                    for (r = 0; r < bottomRegion.height; r++) {
                        tileRegion.depths.Add(bottomRegion.depths[c * bottomRegion.height + r]);
                    }
                }
            }

        private TileRegion GetTileDepths(TileRect tileRect)
            {
                var tileReader = tileReaders[tileRect.id];
                var imageHeight = (int) math.floor(tileRect.rect.height * tileReader.imageHeight);
                var imageWidth = (int) math.floor(tileRect.rect.width * tileReader.imageWidth);

                return new TileRegion {
                    height = imageHeight,
                    width = imageWidth,
                    depths = tileReader.GetHeights(
                        (int) (tileRect.rect.x * tileReader.imageWidth),
                        (int) (tileRect.rect.y * tileReader.imageHeight),
                        imageWidth,
                        imageHeight
                    )
                };
            }

    }

    public class TileRegion {

        public FrameWorld worldFrame;
        public int width;
        public int height;
        public List<float> depths = new List<float>();

    }
}