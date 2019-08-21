using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CitiesAutoComplete.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CitiesAutoComplete
{
	public class Startup
	{
		private readonly ILogger<Startup> _logger;
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration, ILogger<Startup> logger)
		{
			_logger = logger;
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<IISServerOptions>(options =>
			{
				options.AutomaticAuthentication = false;
			});

			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			services.AddScoped<ICitySearchService, CitySearchService>();

			services.AddMvc(setupAction =>
			{
				//setupAction.EnableEndpointRouting = false;

				var jsonOutputFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
				if (jsonOutputFormatter != null)
				{
					// remove text/json as it isn't the approved media type
					// for working with JSON at API level
					if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
						jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
				}

			}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = actionContext =>
				{
					var actionExecutingContext =
						actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

					// if there are modelstate errors & all keys were correctly
					// found/parsed we're dealing with validation errors
					if (actionContext.ModelState.ErrorCount > 0
					    && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
					{
						return new UnprocessableEntityObjectResult(actionContext.ModelState);
					}

					// if one of the keys wasn't correctly found / couldn't be parsed
					// we're dealing with null/unparsable input
					return new BadRequestObjectResult(actionContext.ModelState);
				};
			});

			services.AddVersionedApiExplorer(setupAction =>
			{
				setupAction.GroupNameFormat = "'v'VV";
			});

			services.AddApiVersioning(setupAction =>
			{
				setupAction.UseApiBehavior = false;
				setupAction.AssumeDefaultVersionWhenUnspecified = true;
				setupAction.DefaultApiVersion = new ApiVersion(1, 0);
				setupAction.ReportApiVersions = true;
				setupAction.ApiVersionReader = new HeaderApiVersionReader("api-version");
			});

			var apiVersionDescriptionProvider =
			   services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

			services.AddSwaggerGen(setupAction =>
			{
				foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
				{
					setupAction.SwaggerDoc(
						$"CitiesAutoCompleteSpecification{description.GroupName}",
						new Microsoft.OpenApi.Models.OpenApiInfo
						{
							Title = "Cities Auto-Complete API",
							Version = description.ApiVersion.ToString(),
							Description = "Through this API you can search for cities within Canada and USA.",
							Contact = new Microsoft.OpenApi.Models.OpenApiContact()
							{
								Email = "gaetan.gingras@usherbrooke.ca",
								Name = "Gaetan Gingras",
							},
							License = new Microsoft.OpenApi.Models.OpenApiLicense()
							{
								Name = "MIT License",
								Url = new Uri("https://opensource.org/licenses/MIT")
							}
						});
				}

				setupAction.DocInclusionPredicate((documentName, apiDescription) =>
				{
					var actionApiVersionModel = apiDescription.ActionDescriptor
					.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

					if (actionApiVersionModel == null)
						return true;

					if (actionApiVersionModel.DeclaredApiVersions.Any())
						return actionApiVersionModel.DeclaredApiVersions.Any(v => $"CitiesAutoCompleteSpecificationv{v.ToString()}" == documentName);

					return actionApiVersionModel.ImplementedApiVersions.Any(v => $"CitiesAutoCompleteSpecificationv{v.ToString()}" == documentName);
				});

				var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

				setupAction.IncludeXmlComments(xmlCommentsFullPath);
			});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			if (env.IsDevelopment())
			{
				_logger.LogInformation("In Development environment");
				app.UseDeveloperExceptionPage();
			}

			app.UseSwagger();

			app.UseSwaggerUI(setupAction =>
			{
				foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
				{
					setupAction.SwaggerEndpoint($"/swagger/CitiesAutoCompleteSpecification{description.GroupName}/swagger.json",
						description.GroupName.ToUpperInvariant());
				}

				setupAction.RoutePrefix = "";

				setupAction.DefaultModelExpandDepth(2);
				setupAction.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
				setupAction.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
				setupAction.EnableDeepLinking();
				setupAction.DisplayOperationId();
			});

			app.UseStaticFiles();

			app.UseMvc();
		}
	}
}
