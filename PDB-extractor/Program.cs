
using PdbExtractor;
using System;
using System.Runtime.InteropServices;
using System.Text;
/* # Assumptions: QWORD=4h, DWORD=2h , WORD=1h, Short.length = DWORD , Data directories: 16 */

internal class Program
{

    private static void Main(string[] args)
    {
        Printer.printPdbInfo(args);
    }
}