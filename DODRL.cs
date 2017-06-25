using System;
using System.Collections.Generic;

namespace dodrl {

    public interface IOverlapHandler {
        int Overlap(int player, int tile);
    }
    
    public class OverlapNull : IOverlapHandler {
        public int Overlap(int player, int tile) {
            return player;
        }
    }
    
    public class OverlapTreasure : IOverlapHandler {
        public int Overlap(int player, int tile) {
            int gold = Player.GetGold(player);
            return Player.SetGold(player, gold + tile * 16);
        }
    }

    internal class DODRL {

        private const int PLAYER_ID = 128;
        private const int WALKABLE_ID = 512;
        
        public static void Main(string[] args) {
            Dictionary<int, int> movedt = new Dictionary<int, int> {
                { 1, -8 },
                { 2, 1 },
                { 3, 8 },
                { 4, -1 },
            };

            IOverlapHandler[] overlaps = {
                new OverlapNull(), 
                new OverlapTreasure(),
                new OverlapNull(), 
                new OverlapNull(), 
                new OverlapNull(), 
                new OverlapNull(), 
                new OverlapNull(), 
                new OverlapNull(), 
            };
            
            Dictionary<ConsoleKey, int> keys = new Dictionary<ConsoleKey, int> {
                { ConsoleKey.UpArrow,    1 },
                { ConsoleKey.RightArrow, 2 },
                { ConsoleKey.DownArrow,  3 },
                { ConsoleKey.LeftArrow,  4 },
                { ConsoleKey.W,          1 },
                { ConsoleKey.D,          2 },
                { ConsoleKey.S,          3 },
                { ConsoleKey.A,          4 },
            };
            
            Dictionary<int, string> tiles = new Dictionary<int, string> {
                { 1,   "*" },
                { 2,   "x" },
                { 3,   "X" },
                { 128, "@" },
                { 256, "." },
                { 512, "#" },
            };

            // Order by lowest value in group range
            int[] groups = { 0, 1, 8, 64, 128, 256, 512 };
            // 0          : empty
            // 1 > 8      : treasures
            // 8 > 64     : loot
            // 64 > 128   : enemies
            // 128        : player
            // 129 > 256  : friends
            // 256 > 512  : walkable tiles
            // 512 > 768  : non walkables
            // 768 > 1024 : map links
            
            // lay out maps and entities sequentially
            // look up map link from map, subtract 768
            // map 0 has link 769 to map 1 (769 - 768 = 1)
            // map 1 has link 768 back to map 0
            // spawning player would require the knowledge of where
            // the player came from
            
            int[] map = {
                512, 512, 512, 512, 512, 512, 512, 512,
                512, 256, 256, 256, 256, 256, 256, 512,
                512, 256, 512, 512, 256, 512, 256, 512,
                512, 256, 512, 512, 256, 512, 256, 256,
                512, 256, 256, 512, 256, 512, 512, 512,
                512, 512, 256, 512, 256, 256, 256, 512,
                512, 256, 256, 256, 256, 256, 256, 512,
                512, 512, 512, 512, 512, 512, 512, 512,
            };
            int[] entities = {
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 1, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 2, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
            };

            int[] buffer = new int[map.Length];
            int mapwidth = 8;
            int ppos = 31;
            int player = 0;
            ConsoleKey key;

            player = Player.SetHealth(player, 100);
            
            do {
                Console.Clear();
                for (int i = 0; i < map.Length; i += 8) {
                    int m0 = map[i];
                    int m1 = map[i+1];
                    int m2 = map[i+2];
                    int m3 = map[i+3];
                    int m4 = map[i+4];
                    int m5 = map[i+5];
                    int m6 = map[i+6];
                    int m7 = map[i+7];
                    buffer[i] = m0;
                    buffer[i+1] = m1;
                    buffer[i+2] = m2;
                    buffer[i+3] = m3;
                    buffer[i+4] = m4;
                    buffer[i+5] = m5;
                    buffer[i+6] = m6;
                    buffer[i+7] = m7;
                }
                for (int i = 0; i < entities.Length; ++i) {
                    if (entities[i] == 0) {
                        continue;
                    }
                    buffer[i] = entities[i];
                }
                buffer[ppos] = PLAYER_ID;
                
                string mapline = "";
                for (int i = 0, n = 1; i < buffer.Length; ++i, ++n) {
                    mapline += RenderTile(buffer[i], tiles);
                    if (n % mapwidth != 0) {
                        continue;
                    }
                    Console.WriteLine(mapline);
                    mapline = "";
                }

                Console.WriteLine("Health: "+Player.GetHealth(player)+" Gold: "+Player.GetGold(player));
                
                key = Console.ReadKey(false).Key;

                int input = GetPlayerInput(key, keys);
                int dt = GetNextPos(movedt, input);
                int next = Clamp(ppos + dt, 0, map.Length - 1, ppos);
                int walkable = InRange(map[next], 0, WALKABLE_ID);
                dt *= walkable;
                next = ppos + dt;
                int ent = entities[next];
                IOverlapHandler ohandler = GetOverlapHandler(groups, overlaps, ent);
                player = Overlap(ohandler, player, ent);
                entities[next] = 0;
                ppos = next;

            } while (key != ConsoleKey.Q);
        }

        private static IOverlapHandler GetOverlapHandler(int[] groups, IOverlapHandler[] overlaps, int ntile) {
            int g = -1;
            for (int i = 0; i < groups.Length; ++i) {
                if (ntile >= groups[i]) {
                    continue;
                }
                g = i - 1;
                break;
            }

            return g >= 0 ? overlaps[g] : null;
        }

        private static int Overlap(IOverlapHandler handler, int player, int ntile) {
            return handler?.Overlap(player, ntile) ?? player;
        }

        // oobval = Out Of Bounds Value
        private static int Clamp(int value, int lower, int upper, int oobval) {
            return value < lower ? oobval : value > upper ? oobval : value;
        }

        // oobval = Out Of Bounds Value
        private static int InRange(int value, int lower, int upper) {
            return value > lower && value < upper ? 1 : 0;
        }

        private static int GetNextPos(IReadOnlyDictionary<int, int> movedeltas, int mov) {
            int dt = 0;
            movedeltas.TryGetValue(mov, out dt);
            return dt;
        }

        private static int GetPlayerInput(ConsoleKey key, Dictionary<ConsoleKey, int> keys) {
            int input;
            keys.TryGetValue(key, out input);
            return input;
        }
        
        private static string RenderTile(int n, Dictionary<int, string> tiles) {
            string tile = "_";
            tiles.TryGetValue(n, out tile);
            return tile;
        }
    }

    public static class Player {

        private const int MASK_GOLD = 16383;
        private const int MASK_HEALTH = 127;

        public static int GetGold(int player) {
            return MaskBits(player, MASK_GOLD, 7);
        }
        
        public static int SetGold(int player, int value) {
            return SetBits(player, value, MASK_GOLD, 7);
        }

        public static int GetHealth(int player) {
            return MaskBits(player, MASK_HEALTH, 0);
        }

        public static int SetHealth(int player, int value) {
            return SetBits(player, value, MASK_HEALTH, 0);
        }

        public static int SetBits(int data, int value, int mask, int offset) {
            return (data & ~(mask << offset)) | ((value & mask) << offset);
        }

        public static int MaskBits(int data, int mask, int offset) {
            return (data & mask << offset) >> offset;
        }

        public static string GetBits(int n) {
            return Convert.ToString(n, 2).PadLeft(32, '0');
        }
    }
}
