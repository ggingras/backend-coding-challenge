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
	/// </summary>
	[Route("api/[controller]")]
	[Produces("application/json")]
	[ApiVersion("1.0")]
	[ApiController]
	public class SuggestionsController : ControllerBase
	{
		private readonly ICitySearchService _citySearchService;
		private readonly IMapper _mapper;
		private readonly ILogger _logger;

		public SuggestionsController(IMapper mapper, ICitySearchService citySearchService, ILogger<SuggestionsController> logger)
		{
			_logger = logger;
			_mapper = mapper;
			_citySearchService = citySearchService;
		}

		/// <summary>
		/// Get the top 5 suggestions ordered by relevance
		/// </summary>
		/// <returns>An ActionResult of type IEnumerable of Suggestion</returns>
		[HttpGet]
		[MapToApiVersion("1.0")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<Suggestion>>> GetSuggestions([FromQuery]QueryParameters parameters)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			try
			{
				var criteria = new SearchCriteria
				{
					Text =	parameters.q,
					Longitude = parameters.Longitude ?? double.MinValue,
					Latitude = parameters.Latitude ?? double.MinValue
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
