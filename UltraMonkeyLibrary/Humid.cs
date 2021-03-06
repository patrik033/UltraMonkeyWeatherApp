using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class Humid : Types
    {
        public override async Task<List<string>> ReturnResult(bool orderBy, string roomType)
        {
            List<string> temp = new List<string>();
            if (orderBy)
            {
                temp = await OrderByType(roomType, temp);
                return await Task.FromResult(temp);
            }
            else
            {
                temp = await OrderByDescendingType(roomType, temp);
                return await Task.FromResult(temp);
            }
        }

        protected override async Task<List<string>> OrderByDescendingType(string roomType, List<string> temp)
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
                    AVG = g.Average(x => x.AirMoisture),
                    Loc = g.Key.Location
                }).Where(x => x.Loc == roomType).OrderByDescending(x => x.AVG).ToList();
                foreach (var item in newList)
                {
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, Average: {Math.Round((float)item.AVG, 1)}";
                    temp.Add(items);
                }
                return await Task.FromResult(temp);
            }
        }

        protected override async Task<List<string>> OrderByType(string roomType, List<string> temp)
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
                    AVG = g.Average(x => x.AirMoisture),
                    Loc = g.Key.Location

                }).Where(x => x.Loc == roomType).OrderBy(x => x.AVG);
                foreach (var item in newList)
                {
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, {item.Loc} Average: {Math.Round((float)item.AVG, 1)}";
                    temp.Add(items);
                }
                return await Task.FromResult(temp);
            }
        }




    }
}
