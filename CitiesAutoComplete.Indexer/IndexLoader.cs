using System;
using System.Collections.Generic;
using System.IO;
using CitiesAutoComplete.Services.Entities;
using Elasticsearch.Net;
using Nest;

namespace CitiesAutoComplete.Indexer
{
	public class IndexLoader
	{
		public static void LoadData()
		{
			var nodes = new List<Uri> { new Uri("http://localhost:9200")};
			var pool = new StaticConnectionPool(nodes);
			var settings = new ConnectionSettings(pool);
			settings.DefaultIndex("city");

#if DEBUG
			//enable to retrieve raw json query
			settings.DisableDirectStreaming(true);
#endif
			var client = new Nest.ElasticClient(settings);

			using (var reader = new StreamReader(@"C:\cities_canada-usa.csv"))
			{
				if (!reader.EndOfStream)
					reader.ReadLine();

				var documents = new List<City>();
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line.Split(';');

					//0 - id

					//1 - name

					//3 - alt_name

					//4 - lat

					//5 - lon

					//8 - country_code
					documents.Add(new City
					{
						Id = int.Parse(values[0]),
						Name = values[1],
						AltName = values[3],
						Country = values[8],
						Location = new Location(double.Parse(values[4]),  double.Parse(values[5])),
						FullText =	string.Concat(values[1], ", ", values[8])
					});
				}

				var	result = client.IndexMany(documents, "city");
			}
		}
	}
}
