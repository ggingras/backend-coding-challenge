using Nest;

namespace CitiesAutoComplete.Services.Entities
{
	public class City
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string AltName { get; set; }

		public string FullText { get; set; }

		[Keyword]
		public string Country { get; set; }


		[GeoPoint]
		public Location Location { get; set; }
	}
}