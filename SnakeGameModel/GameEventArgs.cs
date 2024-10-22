using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.SnakeGame.Model
{
    public class GameEventArgs
    {
        public int x;
        public int y;
        public Color color;

        public int score;
        public string? gameovertext;

        public string? text;

        public int speed;
        public GameEventArgs(int x, int y, Color color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
        public GameEventArgs(int score, string? gameovertext)
        {
            this.score = score;
            this.gameovertext = gameovertext;
        }

        public GameEventArgs(int score)
        {
            this.score = score;
        }
        public GameEventArgs(int x,int y,string text)
        {
            this.x = x;
            this.y = y;
            this.text = text;
        }
    }
}
