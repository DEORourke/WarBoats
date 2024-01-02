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

Console.OutputEncoding = Encoding.Unicode;
Console.CursorVisible = false;

// System.Threading.Thread.Sleep(2000);

 Game game = new Game();

 game.TitleScreen();
 game.RunGame();
// game.TestFun2();
// game.TestFun();


/*
int mask = 0b00001111;          // for abs 1        // for abs 10
Console.WriteLine("E1 is " + (20 >> 4) + " and " + (20 & mask));
Console.WriteLine("E2 is " + (36 >> 4) + " and " + (36 & mask));
Console.WriteLine("F1 is " + (21 >> 4) + " and " + (21 & mask));
*/

Console.ReadKey(true);
Console.CursorVisible = true;

