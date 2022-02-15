using CsvHelper;
using System.Globalization;
using UltraMonkeyConsoleTest;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;



WriteToEF();

async Task WriteToEF()
{
    //laddar databasen med allt på filen
    WriteDataToDb write = new WriteDataToDb();
    await write.WriteToDb();
    Temps temp = new Temps();
    Seasons seasons = new Seasons();


    //skriver ut genomsnittstemperaturen på vald plats i vald ordning
    List<string> list = new List<string>();
    list = await temp.ReturnResult(true, "Inne");

    foreach (var item in list)
        Console.WriteLine(item);



    //Skriver ut första höstdagen
    List<WeatherData> autumnList = new List<WeatherData>();
    string finalValue = await seasons.LoopForAutumn();
    Console.WriteLine(finalValue);

   
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