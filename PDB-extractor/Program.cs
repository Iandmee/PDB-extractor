
using PdbExtractor;
using System;
using System.Runtime.InteropServices;
using System.Text;
/* # Assumptions: DWORD=4h, WORD=2h, Short.length = WORD , Data directories: 16 */

internal class Program
{

    private static void Main(string[] args)
    {
        Printer.printPdbInfo(args);
    }
}