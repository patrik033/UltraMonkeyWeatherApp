using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMonkeyEFLibrary;

namespace UltraMonkeyLibrary
{
    public class WriteDataToDb
    {
        string path = @"C:\Users\zn_19\Downloads\TempFuktData.csv";
        string pathTest = @"C:\Users\zn_19\Downloads\testtext.csv";
        public async Task WriteToDb()
        {
            List<WeatherData> testData = new List<WeatherData>();
            var uniques = testData.DistinctBy(x => x.Date).DistinctBy(d => d.Temp).DistinctBy(c => c.Location).ToList();

            using (StreamReader sr = new StreamReader(path))
            {
                string headerLine = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var splitedLine = line.Split(',');
                    splitedLine[2].Trim();
                    splitedLine[1].Trim();
                    CheckForBadCharacters(splitedLine);
                    splitedLine[2].Trim();
                    AddToClass(uniques, splitedLine);


                }
            }


            //List<string> list = new List<string>();
            //list = await Counter(uniques);

            //foreach (var item in list)
            //{
            //    Console.WriteLine(item.ToString());
            //}
            //Console.WriteLine("klar");

            await SaveToDb(uniques);

            //AddToFile(uniques, pathTest);


            Console.ReadLine();
        }

        private async Task<List<string>> Counter(List<WeatherData> uniques)
        {
            List<string> list = new List<string>();
            List<WeatherData> weatherDatas = new List<WeatherData>();

            int step = 0;
            for (int i = 0; i < uniques.Count; i++)
            {
                weatherDatas = AddNew(uniques, step);
                foreach (var item in weatherDatas)
                {
                    list.Add(item.ToString());
                    step += 50;
                }
            }
            return await Task.FromResult(list);
        }

        private List<WeatherData> AddNew(List<WeatherData> data, int skip)
        {
            List<WeatherData> temp = new List<WeatherData>();

            var myList = data.Select(x => new
            {
                Dat = x.Date,
                Tmp = x.Temp,
                Loc = x.Location
            }).Where(l => l.Loc == "Ute" || l.Loc == "Inne").Skip(skip).Take(4);

            foreach (var item in myList)
            {
                WeatherData Rubin = new WeatherData()
                {
                    Date = item.Dat,
                    Location = item.Loc,
                    Temp = item.Tmp
                };

                temp.Add(Rubin);
            }
            return temp;
        }

        private async Task SaveToDb(List<WeatherData> uniques)
        {
            using (var context = new UltraMonkeyContext())
            {
                    Console.WriteLine("Creating");
                    context.WeatherDatas.AddRange(uniques);
                    context.SaveChanges();
                    
            }
        }


        private void AddToFile(List<WeatherData> uniques, string pathTest)
        {
            using (var writer = new StreamWriter(pathTest))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }

        private void AddToClass(List<WeatherData> uniques, string[] splitedLine)
        {
            var dates = DateTime.Parse(splitedLine[0]);
            dates.GetDateTimeFormats();
            var location = splitedLine[1];
            float temp = float.Parse(splitedLine[2]);
            var humid = int.Parse(splitedLine[3]);
            int moldIndex = CalculateMoldIndex(temp, humid);

            var myData = new WeatherData
            {
                Date = dates,
                Location = location,
                Temp = temp,
                AirMoisture = humid,
                MoldIndex = moldIndex
            };
            uniques.Add(myData);
        }

        private void CheckForBadCharacters(string[] splitedLine)
        {
            if (splitedLine[2].Contains('.'))
            {
                splitedLine[2] = splitedLine[2].Replace('.', ',');
            }

            if (splitedLine[2].Contains('−'))
            {
                splitedLine[2] = splitedLine[2].Replace('−', '-');
            }



            for (int i = 0; i < splitedLine[2].Length; i++)
            {
                if (char.IsDigit(splitedLine[2][i]))
                {
                    splitedLine[2] = splitedLine[2].Substring(i);
                    break;
                }
            }
        }

        private int CalculateMoldIndex(float temp, int humidity)
        {
            int value = 0;

            if (humidity > 90 && humidity <= 100 && temp >= 10 && temp <= 50)
                value = 3;
            if (humidity > 80 && humidity <= 90 && temp >= 10 && temp <= 50)
                value = 2;
            if (humidity >= 73 && humidity <= 80 && temp >= 10 && temp <= 50)
                value = 1;
            if (temp > 50 || humidity < 73 && temp < 10 || temp < 10)
                value = 0;
            return value;
        }
    }
}
