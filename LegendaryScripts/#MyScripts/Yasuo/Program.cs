namespace EnsoulSharp.Yasuo
{
    using EnsoulSharp.SDK;

    public class Program
    {
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }
        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Yasuo")
            {
                return;
            }
            Chat.Print("DeathGodX " + ObjectManager.Player.CharacterName + " Loaded <font color='#1dff00'>by DeathGodX</font>");
            Yasuo.OnLoad();
        }
    }
}
