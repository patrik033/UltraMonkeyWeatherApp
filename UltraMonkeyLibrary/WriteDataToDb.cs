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
        public List<WeatherData> uniques { get; set; }

        public WriteDataToDb()
        {
            uniques = new List<WeatherData>();
        }
        public async Task WriteToDb()
        {
            using (var context = new UltraMonkeyContext())
            {
                if (context.WeatherDatas.Count() == 0)
                {
                    string path = @"..\..\..\..\TempFuktData.csv";
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string headerLine = sr.ReadLine();
                        string line;
                        line = await FilterAndWriteToClass(sr);
                        RemoveDuplicates();
                        AddOpenTime();
                    }
                    SaveToDb(uniques);
                }
                else
                {
                    Console.WriteLine("Database already exists");
                    await Task.Delay(2000);
                }
            }
        }


        /// <summary>
        /// Filtrerar bort dåliga tecken och läser in datan till en klass
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        private async Task<string> FilterAndWriteToClass(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var splitedLine = line.Split(',');
                splitedLine[2].Trim();
                splitedLine[1].Trim();
                float res = CheckForBadCharacters(splitedLine);
                await AddToClass(uniques, splitedLine, res);
            }
            return line;
        }

        

        /*
         lägger till data i kolumnerna opentime & diff

        om nästkommande element har samma plats dvs om det är ute,ute eller inne,inne hoppar den över en rad
        annars kommer den jämföra nuvarande plats och nuvarande plats + 1  med elementen på plats 30 och 31
        alltså om datan är helt korrekt(vilket den inte är) så kommer den lägga till vid +15 minuter. 
        Vi ansåg att vi trots det kommer nära nog med denna lösning istället för att jämföra minut för minut.
        När vi jämförde minut för minut blev variationerna så pass små att det var svårt att se några skillnader om några alls,
        vi valde därför denna lösning
         */
        private void AddOpenTime()
        {
            for (int i = 0; i + 31 < uniques.Count; i += 30)
            {
                if (uniques[i].Location == uniques[i + 1].Location)
                {
                    i++;
                }

                else if (uniques[i].Location == uniques[i + 30].Location && uniques[i + 1].Location == uniques[(i + 1) + 30].Location && uniques[i].Date == uniques[i + 1].Date)
                {
                    //kollar 30 platser framåt, dvs varje kvart om temperaturen har ändrats något
                    if (uniques[i + 30].Temp > uniques[i].Temp && uniques[(i + 1) + 30].Temp < uniques[i + 1].Temp)
                    {
                        float outDiff = uniques[i + 30].Temp - uniques[(i)].Temp;
                        float innDiff = uniques[(i + 1)].Temp - uniques[(i + 30) + 1].Temp;
                        double result = Math.Round((outDiff + innDiff), 1);

                        uniques[(i + 30) + 1].OpenTime = 15;
                        uniques[(i + 30) + 1].Diff = result;
                    }
                }
            }
        }



        /* Vi har använta oss utav två nästlade for loopar då vi ansåg att använda oss utav en kompositnyckel för att sortera
        ut databasen dels tog på tog för lång tid(10 sekunder mot 3-4 minuter(om man har en processor med 16 kärnor :=)  )), dels så var resultaten snarlika i med hur många resultat
        som försvann mellan de olika resultaten(cirka 139000 för kompositnyckel och cirka 140311 på det sätt vi gör nu).
         */
        private void RemoveDuplicates()
        {
            /*
             I den första for loopen här kollar vi igenom om alla properties i de nästkommande raderna är likadana som i,
            om de är det så tas j bort från listan

            Om de har olika tid så bryts den inre loopen
             */
            for (int i = 0; i < uniques.Count; i++)
            {
                for (int j = i + 1; j < uniques.Count; j++)
                {
                    if (uniques[i].Date == uniques[j].Date && uniques[i].Location == uniques[j].Location && uniques[i].Temp == uniques[j].Temp && uniques[i].AirMoisture == uniques[j].AirMoisture)
                    {
                        uniques.RemoveAt(j);
                    }
                    else if (uniques[i].Date != uniques[j].Date)
                        break;
                }
            }


            /*
             Vi valde även att inkludera en till nästlad for loop för att rensa ut om det finns inläsningar som har samma
            plats och tid

            Det hade eventuellt gått att lägga den här koden i den första for loopen, men körtiden var så pass låg att vi ansåg
            att det inte gjorde någonting.
             */

            for (int i = 0; i < uniques.Count; i++)
            {
                for (int j = i + 1; j < uniques.Count; j++)
                {
                    if (uniques[i].Date == uniques[j].Date && uniques[i].Location == uniques[j].Location)
                    {
                        uniques.RemoveAt(j);
                    }
                    else if (uniques[i].Date != uniques[j].Date)
                        break;
                }
            }
        }

        private void SaveToDb(List<WeatherData> uniques)
        {
            using (var context = new UltraMonkeyContext())
            {
                if (context.WeatherDatas.Count() == 0)
                {
                    Console.WriteLine("Creating");
                    context.WeatherDatas.AddRange(uniques);
                    context.SaveChanges();
                    Console.WriteLine("Finished");
                }
                else
                    Console.WriteLine("Database already exists");
            }
        }

        //skriv alla data till en fil
        //bara testad med .csv filer
        private void AddToFile(List<WeatherData> uniques, string pathTest)
        {
            using (var writer = new StreamWriter(pathTest))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(uniques);
            }
        }


        private async Task AddToClass(List<WeatherData> uniques, string[] splitedLine, float res)
        {
            var dates = DateTime.Parse(splitedLine[0]);
            dates.GetDateTimeFormats();
            var location = splitedLine[1];
            var humid = int.Parse(splitedLine[3]);
            int moldIndex = CalculateMoldIndex(res, humid);

            var myData = new WeatherData
            {
                Date = dates,
                Location = location,
                Temp = res,
                AirMoisture = humid,
                MoldIndex = moldIndex,
            };
            uniques.Add(myData);
            await Task.FromResult(uniques);
        }

       
         
        /// <summary>
        /// Konverterar strängen till en float med rätt kultur
        /// </summary>
        /// <param name="splitedLine"></param>
        /// <returns></returns>
        private float CheckForBadCharacters(string[] splitedLine)
        {
            float.TryParse(splitedLine[2], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out float result);
            return result;
        }



        private int CalculateMoldIndex(float temp, int humidity)
        {
            int value = 0;

            if (humidity > 90 && humidity <= 100 && temp >= 10 && temp <= 50)
                value = 7;
            else if (humidity > 80 && humidity <= 90 && temp >= 10 && temp <= 50)
                value = 6;
            else if (humidity > 70 && humidity <= 80 && temp >= 10 && temp <= 50)
                value = 5;
            else if (humidity > 60 && humidity <= 70 && temp >= 10 && temp <= 50)
                value = 4;
            else if (humidity > 50 && humidity <= 60 && temp >= 10 && temp <= 50)
                value = 3;
            else if (humidity > 40 && humidity <= 50 && temp >= 10 && temp <= 50)
                value = 2;
            else if (humidity > 30 && humidity <= 40 && temp >= 10 && temp <= 50)
                value = 1;
            return value;
        }
    }
}

