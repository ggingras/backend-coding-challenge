using CitiesAutoComplete.Models;
using CitiesAutoComplete.Services.Entities;
using Nest;
using Profile = AutoMapper.Profile;

namespace CitiesAutoComplete
{
	public class SuggestionProfile : Profile
	{
		public SuggestionProfile()
		{
			CreateMap<IHit<City>, Suggestion>()
				.ForMember(c => c.Longitude, o => o.MapFrom(m => m.Source.Location.Lon))
				.ForMember(c => c.Latitude, o => o.MapFrom(m => m.Source.Location.Lat))
				.ForMember(c => c.Name, o => o.MapFrom(m => m.Source.Name));
		}
	}
}
