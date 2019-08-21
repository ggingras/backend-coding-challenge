using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitiesAutoComplete.Services.Entities;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;

namespace CitiesAutoComplete.Services
{
	public class CitySearchService : ICitySearchService
	{
		private readonly IElasticClient _client;

		public CitySearchService(IElasticClient client)
		{
			_client = client;
		}

		public CitySearchService(IConfiguration config)
		{
			var uris = config.GetSection("Elastic").Get<string[]>();

			var nodes = new List<Uri>();
			foreach (var u in uris)
				nodes.Add(new Uri(u));

			var pool = new StaticConnectionPool(nodes);
			var settings = new ConnectionSettings(pool);

#if DEBUG
			//enable to retrieve raw json query
			settings.DisableDirectStreaming(true);
#endif
			_client = new Nest.ElasticClient(settings);
		}

		public async Task<ISearchResponse<City>> SearchAsync(SearchCriteria criteria)
		{
			var result = await _client.SearchAsync<City>(s =>
				{
					s.From(criteria.SearchFrom);
					s.TrackScores();
					s.TrackTotalHits();
					if (criteria.SearchSize != int.MinValue)
						s.Size(criteria.SearchSize);
					s.Index(criteria.IndexName);
					
					s.Sort(sort => sort.Descending(SortSpecialField.Score).Ascending("name.raw"));
					s.Query(q => SearchQuery(q, criteria));
#if DEBUG
					return s.Explain(true);
				}
#else
					return s;
				}
#endif
			);

			return result;
		}

		private QueryContainer SearchQuery(QueryContainerDescriptor<City> p, SearchCriteria criteria)
		{
			if (criteria.Longitude != double.MinValue && criteria.Latitude > double.MinValue)
			{
				return p.FunctionScore(fu => fu.Query(q => q.Bool(b => b.Must(s => s.MultiMatch(m => m.Fields(f => f.Fields("name")).Query(criteria.Text)))))
					.ScoreMode(FunctionScoreMode.Sum)
					.Functions(f => f.Weight(w => w.Weight(1).Filter(fi => fi.Bool(b => b.Must(s => s.MultiMatch(m => m.Fields(fields => fields.Fields("name")).Query(criteria.Text))))))
								     .LinearGeoLocation(g => g.Field("location").Scale(Distance.Kilometers(50)).Origin(new GeoLocation(criteria.Latitude, criteria.Longitude)))));
			}

			return p.Bool(b =>  b.Must(s => s.MultiMatch(m => m.Fields(f => f.Fields("name")).Query(criteria.Text))));
		}
	}
}
