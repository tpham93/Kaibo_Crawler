using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;

namespace Kaibo_Crawler
{
    class Map
    {
        enum TileType
        {
            Wall,
            Floor,
            Floor_With_Key,
            Door,
            Count
        }

        private struct LoadData
        {
            TileType[,] tiles;
            Vector2 startPosition;

            public TileType[,] Tiles
            {
                get { return tiles; }
            }
            public Vector2 StartPosition
            {
                get { return startPosition; }
            }

            public LoadData(TileType[,] tiles, Vector2 startPosition)
            {
                this.tiles = tiles;
                this.startPosition = startPosition;
            }
        }

        private static readonly Color STARTPOINT_COLOR = new Color(0, 255, 0);
        private static readonly Color FLOOR_COLOR = new Color(255, 255, 255);
        private static readonly Color FLOOR_WITH_KEY_COLOR = new Color(0, 0, 255);
        private static readonly Color DOOR_COLOR = new Color(255, 0, 0);

        private Model wallModel;
        private Model floorModel;

        private string filepath;
        private Size2 tileSize;
        private TileType[,] tiles;
        private Vector2 startPosition;

        public Vector3 StartPosition
        {
            get { return new Vector3((startPosition.X + 0.5f) * tileSize.Width, 0, (startPosition.Y + 0.5f) * tileSize.Height); }
        }

        public Map(string filepath, Size2 tileSize)
        {
            this.filepath = filepath;
            this.tileSize = tileSize;
        }

        public void LoadContent(GraphicsDevice device, ContentManager content)
        {
            LoadData loadData = LoadMapData(device, filepath);
            tiles = loadData.Tiles;
            startPosition = loadData.StartPosition;
            var importer = new Assimp.AssimpImporter();


            string fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/wall.3ds");
            Assimp.Scene scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            wallModel = new Model(scene, device, content);

            fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/floor.3ds");
            scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            floorModel = new Model(scene, device, content);

        }

        public bool intersects(Vector3 playerPosition, Vector2 size)
        {
            Point playerTilePosition = worldToTileCoordinates(playerPosition);


            if (playerTilePosition.X >= 0 || playerTilePosition.Y >= 0 || playerTilePosition.X <= tiles.GetUpperBound(0) || playerTilePosition.Y <= tiles.GetUpperBound(1))
            {

                Point[] corners = new Point[4];
                corners[0] = worldToTileCoordinates(playerPosition + new Vector3(-size.X, 0, -size.Y));
                corners[1] = worldToTileCoordinates(playerPosition + new Vector3(-size.X, 0, size.Y));
                corners[2] = worldToTileCoordinates(playerPosition + new Vector3(size.X, 0, -size.Y));
                corners[3] = worldToTileCoordinates(playerPosition + new Vector3(size.X, 0, size.Y));

                for (int i = 0; i < corners.Length; ++i)
                {
                    int x = corners[i].X;
                    int y = corners[i].Y;

                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        TileType type = tiles[x, y];
                        switch (type)
                        {
                            case TileType.Wall:
                            case TileType.Door:
                                return true;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public void trigger(Player player)
        {
            Vector2 playerTileLookAt = worldToTileCoordinates(player.Position + Vector3.Normalize(player.Direction) * Math.Min(tileSize.Width, tileSize.Height));
            TileType type = tiles[(int)playerTileLookAt.X, (int)playerTileLookAt.Y];
            switch (type)
            {
                case TileType.Floor_With_Key:
                    player.addKey();
                    tiles[(int)playerTileLookAt.X, (int)playerTileLookAt.Y] = TileType.Floor;
                    break;
                case TileType.Door:
                    if (player.hasKey())
                    {
                        tiles[(int)playerTileLookAt.X, (int)playerTileLookAt.Y] = TileType.Floor;
                    }
                    break;
                default:
                    break;
            }
        }

        private Point worldToTileCoordinates(Vector3 worldCoordinate)
        {
            return new Point((int)(worldCoordinate.X) / tileSize.Width, (int)(worldCoordinate.Z) / tileSize.Height);
        }

        private static LoadData LoadMapData(GraphicsDevice device, string filepath)
        {
            Texture t = Texture.Load(device, filepath);
            TileType[,] tiles = new TileType[t.Width, t.Height];
            List<Vector2> startPositions = new List<Vector2>();
            Color[] pixel = t.GetData<Color>();

            for (int y = 0; y < t.Height; ++y)
            {
                for (int x = 0; x < t.Width; ++x)
                {
                    Color currentPixel = pixel[x + y * t.Height];
                    if (currentPixel == FLOOR_COLOR)
                    {
                        tiles[x, y] = TileType.Floor;
                    }
                    else if (currentPixel == STARTPOINT_COLOR)
                    {
                        startPositions.Add(new Vector2(x, y));
                        tiles[x, y] = TileType.Floor;
                    }
                    else if (currentPixel == FLOOR_WITH_KEY_COLOR)
                    {
                        tiles[x, y] = TileType.Floor_With_Key;
                    }
                    else if (currentPixel == DOOR_COLOR)
                    {
                        tiles[x, y] = TileType.Door;
                    }
                    else
                    {
                        tiles[x, y] = TileType.Wall;
                    }
                }
            }

            if (startPositions.Count == 0)
            {
                throw new Exception("no startpoint found!");
            }

            Random r = new Random();
            return new LoadData(tiles, startPositions[r.Next() % startPositions.Count]);
        }

        struct PointLight
        {
            public Vector3 pos;
            float att1;				// -1 / (outer_range-inner_range)
            public Vector3 color;
            float att2;				// 1 + inner_range / (outer_range-inner_range)
            // The inner_range is the distance at which the light starts to fade out.
            // The outer_range is the distance where the light gets 0.

            public void Set(float innerRange, float outerRange)
            {
                att1 = -1 / (outerRange - innerRange);
                att2 = 1 - innerRange * att1;
            }
        }

        public void Draw(Player player, GraphicsDevice graphicsDevice, SharpDX.Toolkit.Graphics.Effect effect, GameTime gameTime)
        {
            Matrix transformation = Matrix.Identity;

            var transformCB = effect.ConstantBuffers["Transforms"];
            transformCB.Parameters["worldViewProj"].SetValue(player.Cam.viewProjection);
            transformCB.Parameters["world"].SetValue(player.Cam.view);
            Matrix worldInvTr = Helpers.CreateInverseTranspose(ref player.Cam.view);
            transformCB.Parameters["worldInvTranspose"].SetValue(worldInvTr);
            transformCB.Parameters["cameraPos"].SetValue(player.Position);

            var lightCB = effect.ConstantBuffers["Lights"];
            lightCB.Parameters["lightDir"].SetValue(new Vector3(0));
            lightCB.Parameters["specularPower"].SetValue(0);
            lightCB.Parameters["dirLightColor"].SetValue(new Vector3(0.0f));
            lightCB.Parameters["numPointLights"].SetValue(1);


            PointLight[] pointLights = new PointLight[1];

            pointLights[0].Set(10.0f, 60.0f + 7.5f * (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 150));  
            pointLights[0].pos = player.Position;
            pointLights[0].color = new Vector3(1.0f, 0.8f, 0.5f);


            lightCB.Parameters["lights"].SetValue(pointLights);

            for (int y = -1; y <= tiles.GetUpperBound(1) + 1; ++y)
            {
                for (int x = -1; x <= tiles.GetUpperBound(0) + 1; ++x)
                {

                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Wall:
                            case TileType.Door:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -10, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(wallModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                    else
                    {
                        transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -10, (y + 0.5f) * tileSize.Height);
                        Helpers.drawModel(wallModel, graphicsDevice, effect, transformation, player, gameTime);
                    }
                }
            }
            for (int y = 0; y <= tiles.GetUpperBound(1); ++y)
            {
                for (int x = 0; x <= tiles.GetUpperBound(0); ++x)
                {
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Floor:
                            case TileType.Floor_With_Key:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -10, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(floorModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }

        }
    }
}
