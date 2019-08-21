using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CitiesAutoComplete.Models
{
	public class QueryParameters
	{
		/// <summary>
		/// Search text
		/// </summary>
		[BindRequired, MinLength(2)]
		public string q { get; set; }

		/// <summary>
		/// Latitude close to the city you want to find
		/// </summary>
		[Range(24, 83)]
		public double? Latitude { get; set; }

		/// <summary>
		/// Longitude close to the city you want to find
		/// </summary>
		[Range(-141, -52)]
		public double? Longitude { get; set; }
	}

	public class QueryParametersV2 : QueryParameters
	{
		/// <summary>
		/// From result to enable paging
		/// </summary>
		public int? From { get; set; }

		/// <summary>
		/// Number of result in a page
		/// </summary>
		[Range(1, 100)]
		public int? Size { get; set; }
	}
}
