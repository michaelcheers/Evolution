using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution
{
    public class FoodSource
    {
        public Point location;
        public Direction direction;
        public Game1 game;

        public void Update()
        {
            switch (Game1.rnd.Next(3))
            {
                case 0:
                    location = Cell.WrapPoint(game.foodGrid, location + Cell.Pointify(direction));
                    break;
                case 1:
                    break;
                case 2:
                    direction = (Direction)Game1.rnd.Next(4);
                    break;
            }
            if (game.foodGrid[location.X, location.Y] < 254)
                game.foodGrid[location.X, location.Y]++;
        }
    }

}
