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
        public void WriteToDb()
        {
            List<WeatherData> testData = new List<WeatherData>();
            var uniques = testData.DistinctBy(x => x.Date).DistinctBy(d => d.Temp).DistinctBy(c => c.Location).ToList();

            using (StreamReader sr = new StreamReader(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\TempFuktData.csv"))
            {
                string headerLine = sr.ReadLine();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var splitedLine = line.Split(',');
                    splitedLine[2].Trim();
                    CheckForBadCharacters(splitedLine);
                    splitedLine[2].Trim();
                    AddToClass(uniques, splitedLine);
                }
            }
            //using (var context = new UltraMonkeyContext())
            //{
            //    context.WeatherDatas.AddRange(uniques);
            //    context.SaveChanges();
            //}

            using (var writer = new StreamWriter(@"C:\Users\patri\source\repos\UltraMonkeyWeatherApp\testtext.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }

            Console.WriteLine("klar");
            Console.ReadLine();
        }

        private static void AddToClass(List<WeatherData> uniques, string[] splitedLine)
        {
            var dates = DateTime.Parse(splitedLine[0]);
            dates.GetDateTimeFormats();
            var location = splitedLine[1];
            float temp = float.Parse(splitedLine[2]);
            var humid = int.Parse(splitedLine[3]);

            

            var myData = new WeatherData
            {
                Date = dates,
                Location = location,
                Temp = temp,
                AirMoisture = humid
            };
            uniques.Add(myData);
        }

        private static void CheckForBadCharacters(string[] splitedLine)
        {
            if (splitedLine[2].Contains('.'))
            {
                splitedLine[2] = splitedLine[2].Replace('.', ',');
            }

            if (splitedLine[2].Contains('−'))
            {
                splitedLine[2] = splitedLine[2].Replace('−', '-');
            }

            float myTemp = float.Parse(splitedLine[2]);




            if (splitedLine[2].Contains('^') || splitedLine[2].Contains('’') || splitedLine[2].Contains('â'))
            {
                for (int i = 0; i < splitedLine[2].Length; i++)
                {
                    if (char.IsDigit(splitedLine[2][i]))
                    {
                        splitedLine[2] = splitedLine[2].Substring(i);
                    }
                }
            }
        }

        private void CalculateMoldIndex(float temp, int humidity)
        {
            List<double> squares = new List<double>();


            double sq = Math.Round(Math.Sqrt((Math.Pow(temp * 2, 2) + Math.Pow(humidity * 2, 2) + 70) - (0.092 * temp) + (1.092 * humidity)), 3);
            squares.Add(sq);

            Console.WriteLine("\n\n");
            //squares.Sort();
            foreach (var item in squares)
            {
                Console.WriteLine(Math.Round(item, 3));
            }
        }
    }
}
