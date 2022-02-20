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
        //TODO få funktionen att fungera med vinter också!
        public async Task<string> LoopForAutumn()
        {
            int temp = 0;
            string returnValue = "";
            bool keepRunning = true;
            List<WeatherData> list = new List<WeatherData>();
            list = await FirstDayForAutumn(list, temp);
            while (keepRunning)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Temp > 0 && list[i].Temp < 10 && list[0].Temp > list[1].Temp && list[1].Temp > list[2].Temp && list[2].Temp > list[3].Temp && list[3].Temp > list[4].Temp)
                    {
                        returnValue = $"Första höstdagen inföll den: {list[0].Date.Year} {list[0].Date.Month} {list[0].Date.Day}";
                        keepRunning = false;
                        break;
                    }
                    if (list.Count < 5)
                    {
                        returnValue = "Hösten inträffade inte";
                        keepRunning = false;
                        break;
                    }
                    else
                    {
                        list.Clear();
                        temp++;
                        list = await FirstDayForAutumn(list, temp);
                    }
                }
            }
            return await Task.FromResult(returnValue);
        }



        private async Task<List<WeatherData>> FirstDayForAutumn(List<WeatherData> temp, int skip)
        {
            using (var context = new UltraMonkeyContext())
            {

                List<WeatherData> temps = new List<WeatherData>();
                var newList = context.WeatherDatas.GroupBy(x => new
                {
                    x.Date.Date,
                    x.Location
                }).Select(g => new
                {
                    Date = g.Key,
                    AVG = g.Average(x => x.Temp),
                    Loc = g.Key.Location

                }).Where(x => x.Loc == "Ute").OrderBy(x => x.Date.Date).Skip(skip).Take(5);
                foreach (var item in newList)
                {
                    WeatherData Rubin = new WeatherData()
                    {
                        Date = item.Date.Date,
                        Location = item.Loc,
                        Temp = item.AVG
                    };
                    temps.Add(Rubin);
                }



                return await Task.FromResult(temps);
            }
        }
    }
}
