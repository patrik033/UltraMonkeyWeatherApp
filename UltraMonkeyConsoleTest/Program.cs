using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



WriteToEF();


void WriteToEF()
{
    WriteDataToDb write = new WriteDataToDb();
    write.WriteToDb();
}


//AVGtemp();

void AVGtemp()
{
    //Ask for Date 
    Console.WriteLine("Input a date(MM-DD): ");
    var input = DateTime.Parse("2016-" + Console.ReadLine());
    //Search for date in database
    using (var dbcontext = new UltraMonkeyContext())
    {
        var searcher = from d in dbcontext.WeatherDatas
                       where d.Date == input
                       select new
                       {
                           Date = input
                       };
    }
    //Take all with date
    //Count the Average temperature of that date
    //Output Date + AVG temp (Do it as Return type)
}