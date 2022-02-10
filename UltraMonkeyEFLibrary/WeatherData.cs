using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMonkeyEFLibrary
{
    public class WeatherData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public float Temp { get; set; }
        public int AirMoisture { get; set; }
        public int? MoldIndex { get; set; }
        public int? OpenTime { get; set; }
    }
}
