using System;

namespace Volumetric_Terrain_Monogame_Testbed.WinDX
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
