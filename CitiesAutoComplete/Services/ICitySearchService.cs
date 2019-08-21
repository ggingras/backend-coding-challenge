using System.Threading.Tasks;
using CitiesAutoComplete.Services.Entities;
using Nest;

namespace CitiesAutoComplete.Services
{
	public interface ICitySearchService
	{
		Task<ISearchResponse<City>> SearchAsync(SearchCriteria criteria);
	}
}