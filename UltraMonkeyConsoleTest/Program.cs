using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyLibrary;



WriteToEF();


void WriteToEF()
{
    WriteDataToDb write = new WriteDataToDb();
    write.WriteToDb();
}



