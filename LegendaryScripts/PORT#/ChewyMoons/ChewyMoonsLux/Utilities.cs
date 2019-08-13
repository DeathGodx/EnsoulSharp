#region



#endregion

using EnsoulSharp;

namespace ChewyMoonsLux
{
    internal class Utilities
    {
        public static void PrintChat(string msg)
        {
            Chat.Print(
                "<font color=\"#6699ff\"><b>ChewyMoon's Lux:</b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }
    }
}