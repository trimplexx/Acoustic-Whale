using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM_Audio_Player.Music
{
    public class Tracks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public string Time { get; set; }

        public Tracks(int I, string Ti, string Auth, string Alb, string Pa, string Tim) 
        {
            Id = I;
            Title = Ti;
            Author = Auth;
            Album = Alb;
            Path = Pa;
            Time = Tim;
        }
    }
}
