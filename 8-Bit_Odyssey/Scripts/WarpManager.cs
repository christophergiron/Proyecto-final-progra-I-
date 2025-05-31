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
              Rectangle warpRect = new Rectangle(
                  (int)warp.Position.X,
                  (int)(warp.Position.Y - warp.Size.Height),
                  (int)warp.Size.Width,
                  (int)warp.Size.Height
              );

              if (!player.Hitbox.Intersects(warpRect))
                  continue;

              string target = warp.Properties.TryGetValue("warpTarget", out var targetProp) ? targetProp.ToString() : null;
              string direction = warp.Properties.TryGetValue("direction", out var dirProp) ? dirProp.ToString().ToLower() : "down";
              string spawnId = warp.Properties.TryGetValue("spawnId", out var spawnIdProp) ? spawnIdProp.ToString() : null;

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
                  // Cargar el nuevo mapa
                  newMap = content.Load<TiledMap>(target.Replace("\\", "/"));
                  newRenderer = new TiledMapRenderer(graphicsDevice, newMap);

                  // Leer colisiones
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

                  // Warps del nuevo mapa
                  var warpLayer = newMap.GetLayer<TiledMapObjectLayer>("Warps");
                  newWarps = warpLayer?.Objects?.ToList() ?? new List<TiledMapObject>();

                  // Punto de aparición
                  Vector2 finalSpawn = new Vector2(100, 300); // Por defecto
                  var spawnLayer = newMap.GetLayer<TiledMapObjectLayer>("SpawnPoints");
                  if (!string.IsNullOrEmpty(spawnId) && spawnLayer != null)
                  {
                      var targetSpawn = spawnLayer.Objects.FirstOrDefault(o =>
                          o.Properties.TryGetValue("spawnId", out var prop) && prop.ToString() == spawnId);

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