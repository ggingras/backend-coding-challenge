namespace CitiesAutoComplete.Services.Entities
{
	public class SearchCriteria
	{
		public int SearchFrom { get; set; }

		public int SearchSize { get; set; } = 5;

		public string IndexName { get; set; }

		public string Text { get; set; }

		public double Longitude { get; set; }

		public double Latitude { get; set; }
	}
}