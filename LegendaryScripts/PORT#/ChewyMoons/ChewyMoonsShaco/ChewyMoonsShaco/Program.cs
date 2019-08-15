
#region

#endregion

using EnsoulSharp.SDK;

namespace ChewyMoonsShaco
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += ChewyMoonShaco.OnGameLoad;
        }
    }
}