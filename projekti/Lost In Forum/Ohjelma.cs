#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

#endregion
namespace Lost_In_Forum;
/// <summary>
/// The main class.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        using var game = new Lost_In_Forum();
        game.Run();
    }
}
