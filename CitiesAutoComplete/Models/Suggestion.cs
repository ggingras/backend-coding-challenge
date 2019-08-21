using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesAutoComplete.Models
{
	/// <summary>
	/// A suggestion result with Name, Latitude, Longitude and Score fields
	/// </summary>
	public class Suggestion
	{
		/// <summary>
		/// The name of the city
		/// **City**, Province/State, Country
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The latitude of the city
		/// </summary>
		public double Latitude { get; set; }

		/// <summary>
		/// The longitude of the city
		/// </summary>
		public double Longitude { get; set; }

		/// <summary>
		/// The relevance score according to the search
		/// **Between 0 and 1**
		/// </summary>
		public double Score { get; set; }
	}
}
