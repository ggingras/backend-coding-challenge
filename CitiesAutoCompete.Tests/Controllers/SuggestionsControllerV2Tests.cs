using System;
using System.Collections.Generic;
using CitiesAutoComplete.Controllers;
using CitiesAutoComplete.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using CitiesAutoComplete;
using CitiesAutoComplete.Services;
using CitiesAutoComplete.Services.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;

namespace CitiesAutoCompete.Tests.Controllers
{
	[TestFixture]
	public class SuggestionsControllerV2Tests
	{
		private SuggestionsControllerV2 _suggestions;
		private Mock<ICitySearchService> _citySearchServiceMock;
		private Mock<ILogger<SuggestionsControllerV2>> _loggerMock;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			var mappings = new MapperConfigurationExpression();
			mappings.AddProfile<SuggestionProfile>();
			Mapper.Reset();
			Mapper.Initialize(mappings);
		}

		[SetUp]
		public void Setup()
		{
			_citySearchServiceMock = new Mock<ICitySearchService>();
			_loggerMock = new Mock<ILogger<SuggestionsControllerV2>>();
			_suggestions = new SuggestionsControllerV2(Mapper.Instance, _citySearchServiceMock.Object, _loggerMock.Object);
		}

		[Test]
		public async Task MissingQParameterShouldReturnError400()
		{
			//Hack : https://alenjalex.github.io/dev/dev/Asp.Net-Core-ModelState-Validation-Using-UnitTest/
			_suggestions.ModelState.AddModelError("q", "The q field is required."); 

			var response = await _suggestions.GetSuggestions(null);
			(response.Result as BadRequestResult).StatusCode.Should().Be(StatusCodes.Status400BadRequest);
		}

		[Test]
		public async Task GivenInternalErrorShouldReturnError500()
		{
			_citySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCriteria>())).Throws<Exception>();
			var p = new QueryParametersV2 { q = "search" };
			var response = await _suggestions.GetSuggestions(p);
			(response.Result as ObjectResult).StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
		}

		[Test]
		public async Task QueryParametersShouldBePassedToCitySearchService()
		{
			SearchCriteria searchCriteria = null;
			_citySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCriteria>())).Callback((SearchCriteria s) => searchCriteria = s);

			var p = new QueryParametersV2 { q = "search", Longitude = 1, Latitude = 2 };
			var response = await _suggestions.GetSuggestions(p);

			searchCriteria.Text.Should().Be(p.q);
			searchCriteria.Longitude.Should().Be(p.Longitude);
			searchCriteria.Latitude.Should().Be(p.Latitude);
		}

		[Test]
		public async Task GivenEmptyLonLatInQueryParametersShouldPassDoubleMinValue()
		{
			SearchCriteria searchCriteria = null;
			_citySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCriteria>())).Callback((SearchCriteria s) => searchCriteria = s);

			var p = new QueryParametersV2 { q = "search" };
			var response = await _suggestions.GetSuggestions(p);

			searchCriteria.Text.Should().Be(p.q);
			searchCriteria.Longitude.Should().Be(double.MinValue);
			searchCriteria.Latitude.Should().Be(double.MinValue);
		}

		[Test]
		public async Task GivenValidSearchShouldConvertElasticResponseToSuggestionModelAndOkResult()
		{
			var validSearchResponse = new Mock<ISearchResponse<City>>();
			validSearchResponse.Setup(x => x.IsValid).Returns(true);
			validSearchResponse.Setup(x => x.MaxScore).Returns(1);

			var hits = new List<IHit<City>>();
			var hit = new Mock<IHit<City>>();
			var montreal = new City {Name = "Montreal", Country = "CA", Location = new Location(1, 2)};
			hit.Setup(x => x.Source).Returns(montreal);
			hits.Add(hit.Object);

			validSearchResponse.Setup(x => x.Hits).Returns(hits);
			_citySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCriteria>())).Returns(Task.FromResult(validSearchResponse.Object));

			var p = new QueryParametersV2 { q = "search" };
			var response = await _suggestions.GetSuggestions(p);

			(response.Result as ObjectResult).StatusCode.Should().Be(StatusCodes.Status200OK);
			var suggestions = (response.Result as OkObjectResult).Value as Suggestion[];
			suggestions.Length.Should().Be(1);
			suggestions[0].Name.Should().Be(string.Concat(montreal.Name, ", ", montreal.Country));
			suggestions[0].Longitude.Should().Be(montreal.Location.Lon);
			suggestions[0].Latitude.Should().Be(montreal.Location.Lat);
		}

		[Test]
		public async Task GivenInValidSearchShouldReturnError500()
		{
			var invalidSearchResponse = new Mock<ISearchResponse<City>>();
			invalidSearchResponse.Setup(x => x.IsValid).Returns(false);
			_citySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<SearchCriteria>())).Returns(Task.FromResult(invalidSearchResponse.Object));

			var p = new QueryParametersV2 { q = "search" };
			var response = await _suggestions.GetSuggestions(p);
			(response.Result as ObjectResult).StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
		}
	}
}
