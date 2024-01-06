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


 Game game = new Game();

 game.TitleScreen();
 game.RunGame();


Console.ReadKey(true);
Console.CursorVisible = true;

