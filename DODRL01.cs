using System;

namespace dodrl {

    public class DODRL01 {
        
        public DODRL01() {
            int[] map = {
                1, 1, 1, 1, 1, 1, 1, 1,
                1, 0, 0, 0, 0, 0, 0, 1,
                1, 0, 1, 1, 0, 1, 0, 1,
                1, 0, 1, 1, 0, 1, 0, 9,
                1, 0, 0, 1, 0, 1, 1, 1,
                1, 1, 0, 1, 0, 0, 0, 1,
                1, 0, 0, 0, 0, 2, 0, 1,
                1, 1, 1, 1, 1, 1, 1, 1,
            };
            
            int mw = 8;
            int ppos = 31;
            int score = 0;
            ConsoleKey key;
            
            Console.Clear();

            do {
                string mapline = "";
                for (int i = 0, n = 1; i < map.Length; ++i, ++n) {
                    mapline += GetTileValue(map[i]);
                    if (n % mw == 0) {
                        Console.WriteLine(mapline);
                        mapline = "";
                    }
                }

                key = Console.ReadKey(false).Key;

                int pmov = GetPlayerInput(key);
                int next = GetNextPos(ppos, pmov, mw);
                next = StayWithinMap(map, ppos, next);
                int ntile = GetTile(map, next);

                switch (ntile) {
                    case 0:
                        map = MovePlayer(map, ppos, next);
                        ppos = next;
                        break;
                    case 2:
                        score += 1;
                        map = MovePlayer(map, ppos, next);
                        ppos = next;
                        break;
                }
                
                Console.Clear();

            } while (key != ConsoleKey.Q);
        }

        private static int StayWithinMap(int[] map, int pos, int next) {
            if (next > map.Length - 1) {
                return pos;
            }
            if (next < 0) {
                return pos;
            }
            return next;
        }

        private static int[] MovePlayer(int[] map, int prev, int next) {
            map[prev] = 0;
            map[next] = 9;
            return map;
        }

        private static int GetNextPos(int pos, int mov, int mw) {
            switch (mov) {
                case 1:  return pos - mw;
                case 2:  return pos + 1;
                case 3:  return pos + mw;
                case 4:  return pos - 1;
                default: return pos;
            }
        }

        private static int GetPlayerInput(ConsoleKey key) {
            switch (key) {
                case ConsoleKey.UpArrow:    return 1;
                case ConsoleKey.RightArrow: return 2;
                case ConsoleKey.DownArrow:  return 3;
                case ConsoleKey.LeftArrow:  return 4;
                default:                    return 0;
            }
        }

        private static int GetTile(int[] map, int n) {
            return map[n];
        }
        
        private static string GetTileValue(int n) {
            switch (n) {
                case 0:  return ".";
                case 1:  return "#";
                case 2:  return "X";
                case 9:  return "@";
                default: return " ";
            }
        }
    }
}
