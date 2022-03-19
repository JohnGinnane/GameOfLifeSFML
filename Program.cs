using System;

namespace GameOfLifeSFML
{
    class Program
    {
        static private GameOfLife game;
        static void Main(string[] args)
        {
            Global.ScreenSize = new SFML.System.Vector2f(800, 600);
            game = new GameOfLife();
            game.run();            
        }
    }
}
