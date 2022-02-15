using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



//WriteToEF();


void WriteToEF()
{
    WriteDataToDb write = new WriteDataToDb();
    write.WriteToDb();
}


AVGtemp();

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
                       orderby d.Date
                       select new
                       {
                           temp = d.Temp,
                           date = d.Date
                       };
        
        foreach (var a in searcher)
        {
            Console.WriteLine(a.date);
        }
        Console.ReadKey();
    }
    
    //Take all with date
    //Count the Average temperature of that date
    //Output Date + AVG temp (Do it as Return type)
}