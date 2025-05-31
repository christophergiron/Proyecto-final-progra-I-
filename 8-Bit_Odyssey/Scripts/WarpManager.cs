using JumpMan;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled.Renderers;


namespace Bit_Odyssey.Scripts
{
    public static class WarpManager
    {
        public static bool CheckWarpTriggers(
            Player player,
            List<TiledMapObject> warpZones,
            ContentManager content,
            GraphicsDevice graphicsDevice,
            out TiledMap newMap,
            out TiledMapRenderer newRenderer,
            out List<Rectangle> newColliders,
            out List<TiledMapObject> newWarps,
            out Vector2? spawnPosition
        )
        {
            newMap = null;
            newRenderer = null;
            newColliders = null;
            newWarps = null;
            spawnPosition = null;

            KeyboardState keyboard = Keyboard.GetState();

            foreach (var warp in warpZones)
            {
                // Leer propiedades del warp
                string direction = warp.Properties.TryGetValue("direction", out var dirProp)
                    ? dirProp.ToString().ToLower()
                    : "down";

                string target = warp.Properties.TryGetValue("warpTarget", out var targetProp)
                    ? targetProp.ToString()
                    : null;

                string spawnId = warp.Properties.TryGetValue("spawnId", out var spawnIdProp)
                    ? spawnIdProp.ToString()
                    : null;

                int expandX = 4;
                int expandY = 4;

                if (direction == "left" || direction == "right")
                    expandX = 12;

                if (direction == "up" || direction == "down")
                    expandY = 12;

                Rectangle warpRect = new Rectangle(
                    (int)warp.Position.X - expandX,
                    (int)(warp.Position.Y - warp.Size.Height) - expandY,
                    (int)warp.Size.Width + 2 * expandX,
                    (int)warp.Size.Height + 2 * expandY
                );

                if (!player.Hitbox.Intersects(warpRect))
                    continue;

                // Validar entrada
                bool shouldWarp = direction switch
                {
                    "down" => keyboard.IsKeyDown(Keys.Down) && player.IsOnGround,
                    "up" => keyboard.IsKeyDown(Keys.Up),
                    "left" => keyboard.IsKeyDown(Keys.Left),
                    "right" => keyboard.IsKeyDown(Keys.Right),
                    _ => false
                };

                if (!shouldWarp || string.IsNullOrEmpty(target))
                    continue;

                try
                {
                    
                    newMap = content.Load<TiledMap>(target.Replace("\\", "/"));
                    newRenderer = new TiledMapRenderer(graphicsDevice, newMap);

                    // Detecta segun el mapa
                    if (target.Contains("Underground") || target.ToLower().Contains("underground"))
                    {
                        Music.PlayMusicUnderGroud();
                    }
                    else
                    {
                        Music.PlayMusicOverWorld();
                    }

                    
                    // Cargar colisiones
                    newColliders = new List<Rectangle>();
                    var layer = newMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
                    for (int y = 0; y < layer.Height; y++)
                        for (int x = 0; x < layer.Width; x++)
                        {
                            var tile = layer.GetTile((ushort)x, (ushort)y);
                            if (!tile.IsBlank)
                            {
                                newColliders.Add(new Rectangle(
                                    x * newMap.TileWidth,
                                    y * newMap.TileHeight,
                                    newMap.TileWidth,
                                    newMap.TileHeight));
                            }
                        }

                    // Cargar warps nuevos
                    var warpLayer = newMap.GetLayer<TiledMapObjectLayer>("Warps");
                    newWarps = warpLayer?.Objects?.ToList() ?? new List<TiledMapObject>();

                    // Buscar punto de aparición
                    Vector2 finalSpawn = new Vector2(100, 300);
                    var spawnLayer = newMap.GetLayer<TiledMapObjectLayer>("SpawnPoints");

                    if (!string.IsNullOrEmpty(spawnId) && spawnLayer != null)
                    {
                        var targetSpawn = spawnLayer.Objects.FirstOrDefault(o =>
                            o.Properties.TryGetValue("spawnId", out var prop) &&
                            prop.ToString() == spawnId);

                        if (targetSpawn != null)
                            finalSpawn = new Vector2(targetSpawn.Position.X, targetSpawn.Position.Y - targetSpawn.Size.Height);
                    }

                    spawnPosition = finalSpawn;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Warp Error] {ex.Message}");
                    return false;
                }
            }

            return false;
        }
    }
}