﻿using System;
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
            Goal,
            Count
        }

        private struct LoadData
        {
            TileType[,] tiles;
            Vector2 startPosition;
            Texture2D minimap;

            public TileType[,] Tiles
            {
                get { return tiles; }
            }
            public Vector2 StartPosition
            {
                get { return startPosition; }
            }
            public Texture2D Minimap
            {
                get { return minimap; }
            }

            public LoadData(TileType[,] tiles, Vector2 startPosition, Texture2D minimap)
            {
                this.tiles = tiles;
                this.startPosition = startPosition;
                this.minimap = minimap;
            }
        }

        private static readonly Color STARTPOINT_COLOR = new Color(0, 255, 0);
        private static readonly Color FLOOR_COLOR = new Color(255, 255, 255);
        private static readonly Color FLOOR_WITH_KEY_COLOR = new Color(0, 0, 255);
        private static readonly Color DOOR_COLOR = new Color(255, 0, 0);
        private static readonly Color GOAL_COLOR = new Color(255, 255, 0);

        private Model wallModel;
        private Model floorModel;
        private Model ceilingModel;
        private Model floorKeyModel;
        private Model doorModel;

        private string filepath;
        private Size2 tileSize;
        private TileType[,] tiles;
        private Vector2 startPosition;

        private Texture2D minimap;

        public Vector3 StartPosition
        {
            get { return new Vector3((startPosition.X + 0.5f) * tileSize.Width, 0, (startPosition.Y + 0.5f) * tileSize.Height); }
        }
        public Texture2D Minimap
        {
            get { return minimap; }
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
            minimap = loadData.Minimap;
            var importer = new Assimp.AssimpImporter();


            string fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/wall.3ds");
            Assimp.Scene scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            wallModel = new Model(scene, device, content);

            fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/floor.3ds");
            scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            floorModel = new Model(scene, device, content);

            fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/floorKey.3ds");
            scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            floorKeyModel = new Model(scene, device, content);

            fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/ceiling.3ds");
            scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            ceilingModel = new Model(scene, device, content);


            fileName = System.IO.Path.GetFullPath(content.RootDirectory + "/door.3ds");
            scene = importer.ImportFile(fileName, Assimp.PostProcessSteps.MakeLeftHanded);
            doorModel = new Model(scene, device, content);

        }

        public bool intersects(Vector3 playerPosition, Vector2 size)
        {
            Point playerTilePosition = worldToTileCoordinates(playerPosition);

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
                else
                {
                    return true;
                }
            }

            return false;
        }

        public void trigger(Player player, bool moved)
        {
            Point currentTile = moved ? worldToTileCoordinates(player.Position) : worldToTileCoordinates(player.Position + Vector3.Normalize(player.Direction) * Math.Min(tileSize.Width, tileSize.Height));

            if (currentTile.X >= 0 && currentTile.Y >= 0 && currentTile.X <= tiles.GetUpperBound(0) && currentTile.Y <= tiles.GetUpperBound(1))
            {
                TileType type = tiles[currentTile.X, currentTile.Y];
                switch (type)
                {
                    case TileType.Floor_With_Key:
                        player.addKey();
                        tiles[currentTile.X, currentTile.Y] = TileType.Floor;
                        break;
                    case TileType.Door:
                        if (player.hasKey())
                        {
                            tiles[currentTile.X, currentTile.Y] = TileType.Floor;
                        }
                        break;
                    case TileType.Goal:
                        if (moved)
                        {
                            player.Won = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private Point worldToTileCoordinates(Vector3 worldCoordinate)
        {
            return new Point((int)((worldCoordinate.X + tileSize.Width) / tileSize.Width) - 1, (int)((worldCoordinate.Z + tileSize.Height) / tileSize.Height) - 1);
        }

        private static LoadData LoadMapData(GraphicsDevice device, string filepath)
        {
            Texture2D minimap = Texture2D.Load(device, filepath,TextureFlags.ShaderResource,SharpDX.Direct3D11.ResourceUsage.Dynamic);
            TileType[,] tiles = new TileType[minimap.Width, minimap.Height];
            List<Point> startPositions = new List<Point>();
            List<Point> keyPositions = new List<Point>();
            Color[] pixel = minimap.GetData<Color>();

            for (int y = 0; y < minimap.Height; ++y)
            {
                for (int x = 0; x < minimap.Width; ++x)
                {
                    Color currentPixel = pixel[x + y * minimap.Width];
                    currentPixel = new Color(currentPixel.B,currentPixel.G,currentPixel.R);

                    if (currentPixel == FLOOR_COLOR)
                    {
                        tiles[x, y] = TileType.Floor;
                        pixel[x + y * minimap.Width] = Color.White;
                        Console.Out.Write("F");
                    }
                    else if (currentPixel == STARTPOINT_COLOR)
                    {
                        startPositions.Add(new Point(x, y));
                        tiles[x, y] = TileType.Floor;
                        pixel[x + y * minimap.Width] = Color.White;
                        Console.Out.Write("S");
                    }
                    else if (currentPixel == FLOOR_WITH_KEY_COLOR)
                    {
                        keyPositions.Add(new Point(x, y));
                        tiles[x, y] = TileType.Floor;
                        pixel[x + y * minimap.Width] = Color.White;
                        Console.Out.Write("K");
                    }
                    else if (currentPixel == DOOR_COLOR)
                    {
                        tiles[x, y] = TileType.Door;
                        pixel[x + y * minimap.Width] = Color.White;
                        Console.Out.Write("D");
                    }
                    else if (currentPixel == GOAL_COLOR)
                    {
                        tiles[x, y] = TileType.Goal;
                        pixel[x + y * minimap.Width] = Color.White;
                        Console.Out.Write("G");
                    }
                    else
                    {
                        tiles[x, y] = TileType.Wall;
                        pixel[x + y * minimap.Width] = Color.Black;
                        Console.Out.Write("_");
                    }
                }
                Console.Out.WriteLine();
            }

            if (startPositions.Count == 0)
            {
                throw new Exception("no startpoint found!");
            }

            Random r = new Random();
            int keyIndex = r.Next(keyPositions.Count);

            tiles[keyPositions[keyIndex].X, keyPositions[keyIndex].Y] = TileType.Floor_With_Key;
            minimap.SetData<Color>(pixel);
            return new LoadData(tiles, startPositions[r.Next(startPositions.Count)], minimap);
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
            if (player.IsMoving)
            {
                player.Height = (float)Math.Pow(Math.Sin(player.MovingTime.TotalSeconds * 5), 2) * 0.8f + 15;
            }
            else
            {
                player.Height = Math.Max(player.Height - 3f * (float)gameTime.ElapsedGameTime.TotalSeconds, 15);
            }
            var transformCB = effect.ConstantBuffers["Transforms"];
            transformCB.Parameters["worldViewProj"].SetValue(player.Cam.viewProjection);
            transformCB.Parameters["world"].SetValue(ref player.Cam.view);
            Matrix worldInvTr = Helpers.CreateInverseTranspose(ref player.Cam.view);
            transformCB.Parameters["worldInvTranspose"].SetValue(ref worldInvTr);
            transformCB.Parameters["cameraPos"].SetValue(player.Position);

            var lightCB = effect.ConstantBuffers["Lights"];
            lightCB.Parameters["lightDir"].SetValue(new Vector3(0));
            lightCB.Parameters["specularPower"].SetValue(0);
            lightCB.Parameters["dirLightColor"].SetValue(new Vector3(0.0f));
            lightCB.Parameters["numPointLights"].SetValue(1);

            PointLight[] pointLights = new PointLight[1];

            pointLights[0].Set(10.0f, 60.0f + 7.5f + (player.IsMoving ? 2 : 1) * 2 * (float)Math.Pow(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 150), 1) * 2);
            pointLights[0].pos = player.Position;
            //pointLights[0].color = (Vector3.Lerp(Color.White.ToVector3(), Vector3.Lerp(Color.DarkOrange.ToVector3(), Color.Yellow.ToVector3(), (float)(Math.Pow(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 600), 2f)) * 0.5f), 0.5f));
            pointLights[0].color = Color.White.ToVector3() * 0.5f;

            lightCB.Parameters["lights"].SetValue(pointLights);

            Point playerTilePosition = worldToTileCoordinates(player.Position);
            const int DRAWING_RANGE = 5;

            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Wall:
                                Helpers.drawModel(wallModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                    else
                    {
                        Helpers.drawModel(wallModel, graphicsDevice, effect, transformation, player, gameTime);
                    }
                    transformation *= Matrix.Translation(0, 25, 0);
                    Helpers.drawModel(ceilingModel, graphicsDevice, effect, transformation, player, gameTime);
                }
            }
            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Wall:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(wallModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }
            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Floor_With_Key:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(floorKeyModel, graphicsDevice, effect, transformation, player, gameTime);
                                Helpers.drawModel(floorModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }
            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Door:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(doorModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }
            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Floor:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(floorModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }
            for (int x = playerTilePosition.X - DRAWING_RANGE; x <= playerTilePosition.X + DRAWING_RANGE; ++x)
            {
                for (int y = playerTilePosition.Y - DRAWING_RANGE; y <= playerTilePosition.Y + DRAWING_RANGE; ++y)
                {
                    if (x >= 0 && y >= 0 && x <= tiles.GetUpperBound(0) && y <= tiles.GetUpperBound(1))
                    {
                        switch (tiles[x, y])
                        {
                            case TileType.Goal:
                                transformation = Matrix.Translation((x + 0.5f) * tileSize.Width, -player.Height, (y + 0.5f) * tileSize.Height);
                                Helpers.drawModel(floorModel, graphicsDevice, effect, transformation, player, gameTime);
                                break;
                        }
                    }
                }
            }
        }
    }
}
