namespace Othello
{
    public static class DiscExtensions
    {
        public static Disc Opponent(this Disc disc)
        {
            if (disc == Disc.Black)
            {
                return Disc.White;
            }
            else if (disc == Disc.White)
            {
                return Disc.Black;
            }
            else
            {
                return Disc.None;
            }
        }
    }
}