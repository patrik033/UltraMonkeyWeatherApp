using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class Seasons
    {
        public async Task<string> AutumnRes()
        {
            List<WeatherData> temp = new List<WeatherData>();
            temp = await Autumn();
            string res = "";

            if(temp.Count < 5)
                res = "Det var en mild höst";
            else
                res = $"Första höstdagen inföll den: {temp[0].Date.Year}-{temp[0].Date.Month}-{temp[0].Date.Day}";

            return await Task.FromResult(res);  
        }


        public async Task<string> WinterRes()
        {
            List<WeatherData> temp = new List<WeatherData>();
            temp = await Winter();
            string res = "";
           
            if (temp.Count < 5)
                res = "Det var en mild vinter";
            else
                res = $"Första Vinterdagen inföll den: {temp[0].Date.Year}-{temp[0].Date.Month}-{temp[0].Date.Day}";

            return await Task.FromResult(res);
        }

        private async Task<List<WeatherData>> Autumn()
        {
            List<WeatherData> temp = new List<WeatherData>();
            using (var context = new UltraMonkeyContext())
            {
                var autummn = context.WeatherDatas.Where(x => x.Location == "Ute")
                        .GroupBy(x => new{x.Date.Date, x.Location})
                        .Select(g => new{Date = g.Key, AVG = g.Average(g => g.Temp)
                        })
                        .OrderBy(x => x.Date.Date.Month)
                        .ThenBy(x => x.Date.Date.Day)
                        .Where(x => x.AVG > 0 && x.AVG < 10)
                        .Take(5)
                        .ToList();
                foreach (var item in autummn)
                {
                    WeatherData Rubin = new WeatherData()
                    {
                        Date = item.Date.Date,
                        Temp = item.AVG
                    };
                    temp.Add(Rubin);
                }
                return await Task.FromResult(temp);
            };
        }


        private async Task<List<WeatherData>> Winter()
        {
            List<WeatherData> temp = new List<WeatherData>();
            using (var context = new UltraMonkeyContext())
            {
                var autummn = context.WeatherDatas.Where(x => x.Location == "Ute")
                        .GroupBy(x => new{x.Date.Date, x.Location})
                        .Select(g => new{Date = g.Key, AVG = g.Average(g => g.Temp)
                        })
                        .OrderBy(x => x.Date.Date.Month)
                        .ThenBy(x => x.Date.Date.Day)
                        .Where(x => x.AVG < 0)
                        .Take(5)
                        .ToList();
                foreach (var item in autummn)
                {
                    WeatherData Rubin = new WeatherData()
                    {
                        Date = item.Date.Date,
                        Temp = item.AVG
                    };
                    temp.Add(Rubin);
                }
                return await Task.FromResult(temp);
            };
        }
    }
}
