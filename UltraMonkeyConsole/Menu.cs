using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;
using UltraMonkeyLibrary;

namespace UltraMonkeyConsole
{
    public class Menu
    {
        public Temps temp { get; set; }
        public Seasons seasons { get; set; }
        public Humid humid { get; set; }
        public Mold mold { get; set; }
        public OpenTime openTime { get; set; }

        public Menu()
        {
            temp = new Temps();
            seasons = new Seasons();
            humid = new Humid();
            mold = new Mold();
            openTime = new OpenTime();
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
        async Task<string> PromptDateList(string roomType, List<string> myList)
        {
            //Kallar på Query 
            List<string> myDates = new List<string>();
            myDates = await Dates(roomType, myDates);
            //Var queryn kommer adderas i Addchoices
            var menu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Datumlista")
                .PageSize(10)
                .MoreChoicesText("Scrolla ner")
                .AddChoices(myDates));

            return menu;
        }
        void SpectreMenu(List <string> menuChoice)
        {
            var menu = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                               .Title("Datumlista")
                               .PageSize(10)
                               .MoreChoicesText("Tryck på Enter för att återvända till huvudmenyn")
                               .AddChoices(menuChoice));
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

        public async Task Run()
        {
            Console.Title = "VäderData";
            WriteDataToDb write = new WriteDataToDb();
            await write.WriteToDb();
            
            bool loop = true;
            
            List<string> dateList = new List<string>();
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
                        List<string> myList = new List<string>();
                        string roomType = PromptMetod();
                        string testRun = await PromptDateList(roomType, myList);
                        string output = AVGtemp(roomType, testRun);
                        AnsiConsole.WriteLine(output);
                        Console.ReadKey();
                        break;
                    //Klar
                    case '2':
                        List<string> list = new List<string>();
                        string tempRoom = PromptMetod();
                        list = await temp.ReturnResult(false, tempRoom);
                        SpectreMenu(list);
                        break;
                    //Klar
                    case '3':
                        List<string> list2 = new List<string>();
                        string humidRoom = PromptMetod();
                        list2 = await humid.ReturnResult(true, humidRoom);
                        SpectreMenu(list2);
                        break;

                    case '4':
                        List<string> list3 = new List<string>();
                        string moldRoom = PromptMetod();
                        list3 = await mold.ReturnResult(true, moldRoom);
                        SpectreMenu(list3);
                        break;

                    case '5':
                        List<WeatherData> autumnList = new List<WeatherData>();
                        string finalValue = await seasons.LoopForAutumn();
                        Console.WriteLine(finalValue);
                        Console.ReadKey();
                        break;

                    case '6': //Metrologisk vinter
                        List<WeatherData> winterList = new List<WeatherData>();
                        string finalWinterValue = await seasons.LoopForWinter();
                        Console.WriteLine(finalWinterValue);
                        Console.ReadKey();
                        break;

                    
                    case '7': //Öppettider för balkonger
                        List<string> openList = new List<string>();
                        openList = await openTime.OrderByTime(openList);
                        SpectreMenu(openList);
                        break;
                    
                    case '8': //Tempskillnader
                        List<string> diffList = new List<string>();
                        string order = PromptOrder();
                        if (order == "DESC")
                        {
                            diffList = await openTime.OrderByDiff(diffList);
                        }
                        else
                        {
                            diffList = await openTime.OrderByDiffAsc(diffList);
                        }
                        SpectreMenu(diffList);
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

    }
}
