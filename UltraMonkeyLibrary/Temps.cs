using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class Temps
    {
        public List<string> OrderTemps(bool orderBy, string roomType)
        {
            List<string> temp = new List<string>();
            string items = "";
            if (orderBy)
            {
                return OrderByTemp(roomType, temp, ref items);
            }
            else
            {
                return OrderByDescendingTemp(roomType, temp, ref items);
            }
        }

        private List<string> OrderByDescendingTemp(string roomType, List<string> temp, ref string items)
        {
            using (var context = new UltraMonkeyContext())
            {

                var newList = context.WeatherDatas.GroupBy(x => new
                {
                    x.Date.Date,
                    x.Location
                }).Select(g => new
                {
                    Date = g.Key,
                    AVG = g.Average(x => x.Temp),
                    Loc = g.Key.Location
                }).Where(x => x.Loc == roomType).OrderByDescending(x => x.AVG).ToList();
                foreach (var item in newList)
                {
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, Average: {item.AVG}";
                    temp.Add(items);
                }
                return temp;
            }

        }

        private List<string> OrderByTemp(string roomType, List<string> temp, ref string items)
        {


            using (var context = new UltraMonkeyContext())
            {
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
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, {item.Loc} Average: {Math.Round(item.AVG, 1)}";
                    temp.Add(items);
                }
                return temp;
            }
        }
    }
}
