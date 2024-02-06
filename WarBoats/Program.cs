// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using WarBoats;
using System.Diagnostics;
using System.Media;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using System.Reflection;

Console.OutputEncoding = Encoding.Unicode;
Console.CursorVisible = false;

Artist artist = new Artist(10, 10, 3, 3);

static void TitleScreen(Artist artist)
{

    Console.Clear();
    artist.DrawBorder();
    artist.DrawSweetGraphics();
    artist.WritePrompt("     PRESS THE ANY KEY", "       FOR NEW GAME");
    Console.ReadKey(true);
}

static int GetDificultyOption(Artist artist)
{
    while (true)
    {
        artist.ClearPromptArea();
        artist.WritePrompt("CHOOSE A DIFICULTY LEVEL", "EASY:1   MEDIUM:2   HARD:3");
        string selection = Console.ReadKey(true).KeyChar.ToString().ToUpper();

        if (Regex.Match(selection, "[E,M,H,1,2,3]").Success)
        {
            return selection switch
            {
                "E" or "1" => 1,
                "M" or "2" => 2,
                "H" or "3" => 2, // should return 3 if/when I finish the third difficulty
                _ => 1
            };
        }
        artist.ClearPromptArea();
        artist.WritePrompt("INVALID SELECTION");
        Thread.Sleep(1500);

    }
}


TitleScreen(artist);
bool playing = true;
while (playing)
{
    int difficulty = GetDificultyOption(artist);
    Game game = new Game(artist, 10, 10, difficulty);
    game.StartNewGame();

    playing = false;
    artist.ClearPromptArea();
    artist.WritePrompt("PRESS ENTER TO PLAY AGAIN");
    if (Console.ReadKey(true).Key == ConsoleKey.Enter) { playing = true; }
}

Console.CursorVisible = true;

