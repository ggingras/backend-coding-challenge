
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitiesAutoComplete.Services;
using CitiesAutoComplete.Services.Entities;
using FluentAssertions;
using Moq;
using Nest;
using NUnit.Framework;

namespace CitiesAutoCompete.Tests.Services
{
	[TestFixture]
	public class CitySearchServiceTests
	{
		private CitySearchService _citySearchService;
		private Mock<IElasticClient> _clientMock;

		[SetUp]
		public void Setup()
		{
			_clientMock = new Mock<IElasticClient>();
			_citySearchService = new CitySearchService(_clientMock.Object);
		}

		[Test]
		public async Task NestSearchShouldBeInvokedOnce()
		{
			var searchCriteria = new SearchCriteria();

			var result = await _citySearchService.SearchAsync(searchCriteria);

			_clientMock.Verify(x => x.SearchAsync(It.IsAny<System.Func<SearchDescriptor<City>, ISearchRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Test]
		public async Task GivenOneDocumentFoundShouldReturnOneResult()
		{
			var searchCriteria = new SearchCriteria();

			var elasticResult = new Mock<ISearchResponse<City>>();
			elasticResult.Setup(x => x.Total).Returns(1);
			elasticResult.Setup(x => x.Documents).Returns(new List<City> { new City() });
			_clientMock.Setup(x => x.SearchAsync(It.IsAny<System.Func<SearchDescriptor<City>, ISearchRequest>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(elasticResult.Object));

			var result = await _citySearchService.SearchAsync(searchCriteria);

			result.Documents.Count.Should().Be(1);
			result.Total.Should().Be(1);
		}

		[Test]
		public async Task GivenZeroDocumentFoundShouldReturnEmptyResult()
		{
			var searchCriteria = new SearchCriteria();

			var elasticResult = new Mock<ISearchResponse<City>>();
			elasticResult.Setup(x => x.Total).Returns(0);
			elasticResult.Setup(x => x.Documents).Returns(new List<City>());
			_clientMock.Setup(x => x.SearchAsync(It.IsAny<System.Func<SearchDescriptor<City>, ISearchRequest>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(elasticResult.Object));

			var result = await _citySearchService.SearchAsync(searchCriteria);

			result.Documents.Count.Should().Be(0);
			result.Total.Should().Be(0);
		}
	}
}
