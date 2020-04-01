using System;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
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
