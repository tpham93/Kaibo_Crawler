using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
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

        private string filepath;
        private Size2 tileSize;
        private TileType[,] tiles;
        private Vector2 startPosition;


        public Map(string filepath, Size2 tileSize)
        {
            this.filepath = filepath;
            this.tileSize = tileSize;
        }

        public void LoadContent(GraphicsDevice device)
        {
            LoadData loadData = LoadMapData(device, filepath);
            tiles = loadData.Tiles;
            startPosition = loadData.StartPosition;
        }

        public bool intersects(Vector3 playerPosition, Vector2 size)
        {
            Vector2 playerTilePosition = worldToTileCoordinates(playerPosition);
            Rectangle playerRect = new Rectangle((int)playerPosition.X, (int)playerPosition.Y, (int)size.X, (int)size.Y);
            for (int x = (int)playerTilePosition.X - 1; x < (int)playerTilePosition.X + 2; ++x)
            {
                for (int y = (int)playerTilePosition.X - 1; y < (int)playerTilePosition.X + 2; ++y)
                {
                    TileType type = tiles[x, y];
                    switch (type)
                    {
                        case TileType.Wall:
                        case TileType.Door:
                            Rectangle tileRect = new Rectangle(x*tileSize.Width, y*tileSize.Height, tileSize.Width, tileSize.Height);
                            if (playerRect.Intersects(tileRect))
                            {
                                return true;
                            }
                            break;
                        default:
                            break;
                    }
                }
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

        private Vector2 worldToTileCoordinates(Vector3 worldCoordinate)
        {
            return new Vector2((int)(worldCoordinate.X) / tileSize.Width, (int)(worldCoordinate.Z) / tileSize.Height);
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

        public void Draw()
        {

        }
    }
}
