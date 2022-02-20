using CsvHelper;
using System.Globalization;
using System.Linq;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;
using Spectre.Console;

Temps temp = new Temps();
Seasons seasons = new Seasons();
Humid humid = new Humid();
Mold mold = new Mold();
//OpenTime openTime = new OpenTime();
bool loop = true;
List<string> myDates = new List<string>();
myDates = await Dates("Inne", myDates);

await Run(temp, seasons, humid, mold, loop,myDates);
async Task WriteToEF()
{
    //laddar databasen med allt på filen
    WriteDataToDb write = new WriteDataToDb();
    await write.WriteToDb();



    Temps temp = new Temps();
    Seasons seasons = new Seasons();
    Humid humid = new Humid();
    Mold mold = new Mold();
    OpenTime openTime = new OpenTime();


    //skriver ut opentime per dag
    List<string> openList = new List<string>();
    openList = await openTime.OrderByTime(openList);
    foreach (var item in openList)
    {
        Console.WriteLine($"{item}");
    }

    Console.WriteLine();


    //skriver ut genomsnitts diff per dag
    List<string> diffList = new List<string>();
    diffList = await openTime.OrderByDiff(diffList);
    foreach (var item in diffList)
    {
        Console.WriteLine($"{item}");
    }

    Console.WriteLine();

    //skriver ut genomsnittstemperaturen på vald plats i vald ordning
    List<string> list = new List<string>();
    list = await temp.ReturnResult(true, "Inne");
    foreach (var item in list)
        Console.WriteLine(item);

    Console.WriteLine();
    //skriver ut torrast/kallast
    Console.WriteLine();
    List<string> list2 = new List<string>();
    list2 = await humid.ReturnResult(true, "Inne");
    foreach (var item in list2)
        Console.WriteLine(item);

    Console.WriteLine();
    //skriver ut moldindex
    Console.WriteLine();
    List<string> list3 = new List<string>();
    list3 = await mold.ReturnResult(true, "Ute");
    foreach (var item in list3)
        Console.WriteLine(item);

    Console.WriteLine();



    //Skriver ut första höstdagen
    List<WeatherData> autumnList = new List<WeatherData>();
    string finalValue = await seasons.LoopForAutumn();
    Console.WriteLine(finalValue);


}

string PromptOrder()
{
    var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Visa väderdata?")
        .PageSize(10)
        .MoreChoicesText("")
        .AddChoices(new[]
        {
            "1. Fallande",
            "2. Stigande"
        }));

    if (menu == "1. Fallande")
    {
        menu = "DESC";
    }
    else
    {
        menu = "ASC";
    }
    return menu;

}
string PromptMetod()
{
    var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Visa väderdata?")
        .PageSize(10)
        .MoreChoicesText("")
        .AddChoices(new[]
        {
            "1. Inne",
            "2. Ute"
        }));

    if (menu == "1. Inne")
    {
        menu = "Inne";
    }
    else
    {
        menu = "Ute";
    }
    return menu;

}
string PromptDateList(string roomType,List <string> myList)
{
    //Kallar på Query 
    //Var queryn kommer adderas i Addchoices


    var menu = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Visa väderdata?")
        .PageSize(10)
        .MoreChoicesText("Scrolla ner")
        .AddChoices(myList));
        
return menu;
}

string AVGtemp(string locationPlace, string date)
{
    //Ask for Date 
    //Console.WriteLine("Input a date(MM-DD): ");
    var input = DateOnly.Parse(date);
    string output = "";

    //Search for date in database
    using (var dbcontext = new UltraMonkeyContext())
    {
        var searcher = from d in dbcontext.WeatherDatas
                       where d.Date.Day == input.Day && d.Date.Month == input.Month && d.Location == locationPlace
                       group d by new
                       {
                           d.Date.Date,
                           d.Location
                       } into g
                       select new
                       {
                           Date = g.Key.Date,
                           Temp = g.Average(x => x.Temp),
                           Loc = g.Key.Location,
                       };

        foreach (var a in searcher)
        {
            output = $"{a.Date.Year}-{a.Date.Month}-{a.Date.Day} {Math.Round(a.Temp, 1)} {a.Loc} ";
        }
    }
    return output;
}

async Task Run(Temps temp, Seasons seasons, Humid humid, Mold mold, bool loop,List <string> myList)
{
    //WriteDataToDb write = new WriteDataToDb();
    //await write.WriteToDb();
    //await Task.Delay(2000);
    while (loop)
    {
        Console.Clear();
        var menu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Visa väderdata")
                .PageSize(10)
                .MoreChoicesText("Scrolla för fler alternativ")
                .AddChoices(new[] {
            "1. Medeltemp för valt datum",
            "2. Sortering av varmast/kallast dag",
            "3. Sortering av torrast/fuktigast dag",
            "4. Sortering av minst/högst risk för mögel",
            "5. Datum för metreologisk höst",
            "6. Datum för metreologisk vinter",
            "7. Öppettider av balkongdörr",
            "8. Temperaturskillnader vid öppen dörr",
            "Q. Avsluta program"
                }));
        switch (menu[0])
        {
            //Klar
            case '1':
                string roomType = PromptMetod();
                string testRun = PromptDateList(roomType,myList);
                string output = AVGtemp(roomType,testRun);
                AnsiConsole.WriteLine(output);
                Console.ReadKey();
                break;
            //Klar
            case '2':
                List<string> list = new List<string>();
                string tempRoom = PromptMetod();
                list = await temp.ReturnResult(false, tempRoom);
                foreach (var item in list)
                    Console.WriteLine(item);
                Console.ReadKey();
                break;
            //Klar
            case '3':
                List<string> list2 = new List<string>();
                string humidRoom = PromptMetod();
                list2 = await humid.ReturnResult(true, humidRoom);
                foreach (var item in list2)
                    Console.WriteLine(item);
                Console.ReadKey();
                break;

            case '4':
                Console.WriteLine();
                List<string> list3 = new List<string>();
                string moldRoom = PromptMetod();
                list3 = await mold.ReturnResult(true, moldRoom);
                foreach (var item in list3)
                    Console.WriteLine(item);
                Console.ReadKey();
                break;

            case '5':
                List<WeatherData> autumnList = new List<WeatherData>();
                string finalValue = await seasons.LoopForAutumn();
                Console.WriteLine(finalValue);
                Console.ReadKey();
                break;

            case '6': //Metrologisk vinter
                break;

            case '7': //Öppettider för balkonger
                OpenTime openTime = new OpenTime();
                List<string> openList = new List<string>();
                openList = await openTime.OrderByTime(openList);
                foreach (var item in openList)
                {
                    AnsiConsole.WriteLine($"{item} Minuter");
                }
                Console.ReadKey();
                break;

            case '8': //Tempskillnader
                OpenTime openTimeDiff = new OpenTime();
                List<string> diffList = new List<string>();
                string order = PromptOrder();
                if (order == "DESC")
                {
                    diffList = await openTimeDiff.OrderByDiff(diffList);
                    foreach (var item in diffList)
                    {
                        AnsiConsole.WriteLine($"{item}");
                    }
                }
                else
                {
                    diffList = await openTimeDiff.OrderByDiffAsc(diffList);
                    foreach (var item in diffList)
                    {
                        AnsiConsole.WriteLine($"{item}");
                    }
                }
                Console.ReadKey();
                break;

            case 'Q':
                loop = false;
                break;

            default:

                break;
        }
    }
}






async Task<List<string>> Dates(string roomType, List<string> temp)
{
    using (var context = new UltraMonkeyContext())
    {
        string items = "";
        var newList = context.WeatherDatas.GroupBy(x => new
        {
            x.Date.Date,
            x.Location
        }).Select(g => new
        {
            Date = g.Key,
            AVG = g.Average(x => x.Temp),
            Loc = g.Key.Location

        }).Where(x => x.Loc == roomType).OrderBy(x => x.Date.Date);
        foreach (var item in newList)
        {
            items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}";
            temp.Add(items);
        }
        return await Task.FromResult(temp);
    }
}
