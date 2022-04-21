namespace TemperatureSensor.WebUI.Model
{
    public class HouseIndicator
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public decimal TemperatureData { get; set; }

        public decimal Humidity { get; set; }
    }
}