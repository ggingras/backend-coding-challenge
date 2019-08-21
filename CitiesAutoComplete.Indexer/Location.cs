namespace CitiesAutoComplete.Services.Entities
{
	public class Location
	{
		public Location(double lat, double lon)
		{
			Lat = lat;
			Lon = lon;
		}

		public double Lat { get; set; }
		public double Lon { get; set; }
	}
}