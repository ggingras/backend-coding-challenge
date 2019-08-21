# Coveo Backend Coding Challenge
(inspired by https://github.com/busbud/coding-challenge-backend-c)

## Requirements

Design a REST API endpoint that provides auto-complete suggestions for large cities.

- The endpoint is exposed at `/suggestions`
- The partial (or complete) search term is passed as a querystring parameter `q`
- The caller's location can optionally be supplied via querystring parameters `latitude` and `longitude` to help improve relative scores
- The endpoint returns a JSON response with an array of scored suggested matches
    - The suggestions are sorted by descending score
    - Each suggestion has a score between 0 and 1 (inclusive) indicating confidence in the suggestion (1 is most confident)
    - Each suggestion has a name which can be used to disambiguate between similarly named locations
    - Each suggestion has a latitude and longitude

## "The rules"

- *You can use the language and technology of your choosing.* It's OK to try something new (tell us if you do), but feel free to use something you're comfortable with. We don't care if you use something we don't; the goal here is not to validate your knowledge of a particular technology.
- End result should be deployed on a public Cloud (Heroku, AWS etc. all have free tiers you can use).

## Advice

- **Try to design and implement your solution as you would do for real production code**. Show us how you create clean, maintainable code that does awesome stuff. Build something that we'd be happy to contribute to. This is not a programming contest where dirty hacks win the game.
- Documentation and maintainability are a plus, and don't you forget those unit tests.
- We don’t want to know if you can do exactly as asked (or everybody would have the same result). We want to know what **you** bring to the table when working on a project, what is your secret sauce. More features? Best solution? Thinking outside the box?

## Can I use a database?

If you wish, it's OK to use external systems such as a database, an Elastic index, etc. in your solution. But this is certainly not required to complete the basic requirements of the challenge. Keep in mind that **our goal here is to see some code of yours**; if you only implement a thin API on top of a DB we won't have much to look at.

Our advice is that if you choose to use an external search system, you had better be doing something really truly awesome with it.

## Sample responses

These responses are meant to provide guidance. The exact values can vary based on the data source and scoring algorithm

**Near match**

    GET /suggestions?q=Londo&latitude=43.70011&longitude=-79.4163

```json
{
  "suggestions": [
    {
      "name": "London, ON, Canada",
      "latitude": "42.98339",
      "longitude": "-81.23304",
      "score": 0.9
    },
    {
      "name": "London, OH, USA",
      "latitude": "39.88645",
      "longitude": "-83.44825",
      "score": 0.5
    },
    {
      "name": "London, KY, USA",
      "latitude": "37.12898",
      "longitude": "-84.08326",
      "score": 0.5
    },
    {
      "name": "Londontowne, MD, USA",
      "latitude": "38.93345",
      "longitude": "-76.54941",
      "score": 0.3
    }
  ]
}
```

**No match**

    GET /suggestions?q=SomeRandomCityInTheMiddleOfNowhere

```json
{
  "suggestions": []
}
```

## References

- Geonames provides city lists Canada and the USA http://download.geonames.org/export/dump/readme.txt

## Getting Started

Begin by forking this repo and cloning your fork. GitHub has apps for [Mac](http://mac.github.com/) and
[Windows](http://windows.github.com/) that make this easier.


## Implementation details

- I think the project is developed using the best practice guidelines (Dependency injections, api versioning, unit tests, Api documentation, Async wait pattern to better scalability, ...)
- For the tech stack, I used Asp.Net Core 2.2 / C# as programming language. For the Full-Text Search engine I used ElasticSearch 7.3. I chose ElasticSearch, since I have been using it on a weekly basis for more than two years.
- For versioning I used header implementation (api-version=2.0) instead of route implementation. I think it is respecting more the OpenClose principle which state Open for extension, but close for modification.
- The documentation of my API is provided with Swashbuckle / Swagger, which implement the OpenApi 3.0 specification. The documentation is the default route on the website and can be accessed at "/index.html".
- I implemented two versions of the SuggestionsController, to highlight the use of versioning in Api. Version 2 adds paging support to Api.
- My solution is quite simple with only 3 projects:
	- CitiesAutoComplete Web Api itSelf
	- CitiesAutoComplete.Test for unit tests
	- CitiesAutoComplete.Indexer Command line projet used to upload .csv file into ElasticSearch
  
  In a real implementation, I would probably create a project itself for the CitySearchService and I would have used Logstash instead of command-line app to load the data in Elastic

- I choose to deploy to Azure, since it is fully integrated with Visual Studio
	- Api: http://40.121.71.9/api/suggestions
	- Swagger doc: http://40.121.71.9/index.html
	
## ElasticSearch Implementation details

- I used the template pattern for automatic index creation when loading data. You can look at the configuration in Elastic\cityTermplate.json
- To resume the template:
	- I setup 1 primary shard with no replica (since only 1 Elastic node)
	- I normalize and filter the data when indexing, this allow for example to ignore casing and accent
	- I used a NGram analyzer so it allows full-text search on part of the word only. In my case 3 letters minimum
- For the search itself, I implemented two searches. The first one when no location is provided does a simple multi-match query search. The second case is when a location is provided. In this scenario, I used a function score to enable linear geo-location based on provided location.
- You will see that I only used simple search configuration, but in production environment we could much more. For example:
	- Add a synonym file
	- Track what people are searching for and optimize the result, according to past search
	- Add fuzzy search so it can find easily misspelled word
	- Apply filtering by country code
	- Apply boosting on perfect match
	- ...
 - Note: as Google Maps does, I only return the 5 most relevant results. This, of course, is a matter of choice and is easy to change.

## Known issues

- ElasticSearch: The final implementation has a tweak to get the score between 0 and 1, since Lucene used TF-IDF has the scoring algorithm and does not implement a normalisation function for that
- ElasticSearch: because of lack of resources, I only deploy one Elastic node. In a production environment, I would normally deploy at least 3 nodes.
- Security: I deployed my site using HTTP, since I did not have any certificate available for HTTPS. In a production environment HTTPS would be the way to go.
