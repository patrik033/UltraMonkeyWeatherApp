using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class OpenTime
    {
        public  async Task<List<string>> OrderByTime(List<string> temp)
        {
            using (var context = new UltraMonkeyContext())
            {
                string items = "";
                var newList = context.WeatherDatas.GroupBy(x => new
                {
                    x.Date.Date,
                }).Select(g => new
                {
                    Date = g.Key,
                    AVG = g.Sum(x => x.OpenTime),


                }).OrderByDescending(x => x.AVG);
                foreach (var item in newList)
                {
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, {item.AVG}";
                    temp.Add(items);
                }
                return await Task.FromResult(temp);
            }
        }


        public async Task<List<string>> OrderByDiff(List<string> temp)
        {
            using (var context = new UltraMonkeyContext())
            {
                string items = "";
                var newList = context.WeatherDatas.GroupBy(x => new
                {
                    x.Date.Date,
                }).Select(g => new
                {
                    Date = g.Key,
                    AVG = g.Average(x => x.Diff),


                }).OrderByDescending(x => x.AVG);
                foreach (var item in newList)
                {
                    items = $"{item.Date.Date.Year}-{item.Date.Date.Month}-{item.Date.Date.Day}, {item.AVG}";
                    temp.Add(items);
                }
                return await Task.FromResult(temp);
            }
        }
    }
}
