namespace EnsoulSharp.Ryze
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
            if (ObjectManager.Player.CharacterName != "Ryze")
                return;
            Ryze.OnLoad();
            Chat.Print("DeathGod Ryze");
        }
    }
}
