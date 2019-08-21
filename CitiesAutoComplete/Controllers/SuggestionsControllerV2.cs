using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CitiesAutoComplete.Models;
using CitiesAutoComplete.Services;
using CitiesAutoComplete.Services.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CitiesAutoComplete.Controllers
{
	/// <summary>
	/// Suggestions enable full-text search on North-American cities
	/// Version 2 enables paging result with parameter from and size
	/// </summary>
	[Route("api/[controller]")]
	[Produces("application/json")]
	[ApiVersion("2.0")]
	[ApiController]
	[ControllerName("Suggestions")]
	public class SuggestionsControllerV2: ControllerBase
	{
		private readonly ICitySearchService _citySearchService;
		private readonly IMapper _mapper;
		private readonly ILogger _logger;

		public SuggestionsControllerV2(IMapper mapper, ICitySearchService citySearchService, ILogger<SuggestionsControllerV2> logger)
		{
			_logger = logger;
			_mapper = mapper;
			_citySearchService = citySearchService;
		}

		/// <summary>
		/// Get a paged list of suggestions
		/// </summary>
		/// <returns>An ActionResult of type IEnumerable of Suggestion</returns>
		[HttpGet]
		[MapToApiVersion("2.0")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<Suggestion>>> GetSuggestions([FromQuery]QueryParametersV2 parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			try
			{
				var criteria = new SearchCriteria
				{
					Text = parameters.q,
					Longitude = parameters.Longitude ?? double.MinValue,
					Latitude = parameters.Latitude ?? double.MinValue,
					SearchFrom = parameters.From ?? 0,
					SearchSize = parameters.Size ?? 5,
				};
				var result = await _citySearchService.SearchAsync(criteria);

				if (result.IsValid)
				{
					var suggestions = _mapper.Map<Suggestion[]>(result.Hits);

					//Tweak score to fall between 0 and 1 since not supported by Lucene and ElasticSearch
					foreach (var suggestion in suggestions)
						suggestion.Score = suggestion.Score / result.MaxScore;

					return Ok(suggestions);
				}
				else
					_logger.Log(LogLevel.Error, result.DebugInformation);
			}
			catch (Exception e)
			{
				_logger.Log(LogLevel.Error, e, "Exception in GetSuggestions");
			}

			return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
		}
	}
}
