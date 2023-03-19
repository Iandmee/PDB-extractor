
using PdbExtractor;
using System;
using System.Runtime.InteropServices;
using System.Text;
/* # Assumptions: QWORD=8h, DWORD=4h , WORD=2h, Short.length = DWORD */

internal class Program
{

    private static void Main(string[] args)
    {
        Printer.printPdbInfo(args);
    }
}