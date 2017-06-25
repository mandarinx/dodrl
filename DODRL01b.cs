using System;

namespace dodrl {

    public class DODRL01b {
        
        public static void Mainx(string[] args) {
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
            
            int curpos = 31;
            int gold = 0;
            ConsoleKey key;
            
            Console.Clear();

            do {
                // Render
                string mapline = "";
                for (int i = 0, n = 1; i < map.Length; ++i, ++n) {
                    int val = map[i];
                    switch (val) {
                        case 0:
                            mapline += ".";
                            break;
                        case 1:
                            mapline += "#";
                            break;
                        case 2:
                            mapline += "X";
                            break;
                        case 9:
                            mapline += "@";
                            break;
                        default:
                            mapline += " ";
                            break;
                    }
                    
                    if (n % 8 == 0) {
                        Console.WriteLine(mapline);
                        mapline = "";
                    }
                }
                
                // Inventory
                Console.WriteLine("Gold "+gold.ToString("000"));

                // Wait for input
                key = Console.ReadKey(false).Key;

                // Default to current position
                int nextpos = curpos;

                if (key == ConsoleKey.UpArrow) {
                    nextpos = curpos - 8;
                }
                if (key == ConsoleKey.RightArrow) {
                    nextpos = curpos + 1;
                }
                if (key == ConsoleKey.DownArrow) {
                    nextpos = curpos + 8;
                }
                if (key == ConsoleKey.LeftArrow) {
                    nextpos = curpos - 1;
                }

                // Prevent player from moving outside the level
                if (nextpos > map.Length - 1) {
                    nextpos = curpos;
                }
                if (nextpos < 0) {
                    nextpos = curpos;
                }
                
                // Collision detection
                if (map[nextpos] != 0 && map[nextpos] != 2) {
                    nextpos = curpos;
                }

                // Handle pickups
                if (map[nextpos] == 2) {
                    gold += 1;
                }
                
                // Move the player
                map[curpos] = 0;
                map[nextpos] = 9;
                curpos = nextpos;
                
                Console.Clear();

            } while (key != ConsoleKey.Q);
        }
    }
}
