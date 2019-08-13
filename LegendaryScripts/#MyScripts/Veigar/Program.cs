namespace EnsoulSharp.Veigar
{
    using System;
    using EnsoulSharp.SDK;

    internal class Program
    {
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Veigar")
                return;
            Veigar.OnLoad();
             Chat.Print("Death is coming Veigar");
        }
    }
}
