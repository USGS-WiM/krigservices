﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using KrigAgent;
using KrigServices.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using WiM.Services.Resources;
using WiM.Services.Analytics;
using WiM.Utilities.ServiceAgent;

namespace KrigServices
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
            if (env.IsDevelopment()) {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
        }//end startup       

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //add functionality to inject IOptions<T>
            services.AddOptions();
            services.AddScoped<IAnalyticsAgent, WiM.Utilities.ServiceAgent.GoogleAnalyticsAgent>((gaa) => new GoogleAnalyticsAgent(Configuration["AnalyticsKey"]));
            services.Configure<ProjectionSettings>(Configuration.GetSection("ProjectionSettings"));
            services.Configure<APIConfigSettings>(Configuration.GetSection("APIConfigSettings"));

            services.AddScoped<IKrigAgent, Krig>((ctx)=> {
                var data = Configuration.GetSection("KrigResources").GetChildren()
                                .Select(item => new KeyValuePair<string, string>(item.Key, item.Value))
                                .ToDictionary(x => x.Key, x => x.Value);
                return new Krig(data);
            });

            services.AddCors(options => {
                options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin()
                                                                 .AllowAnyMethod()
                                                                 .AllowAnyHeader()
                                                                 .AllowCredentials());
            });

            services.AddMvc(options => { options.RespectBrowserAcceptHeader = true; })
                                .AddXmlSerializerFormatters()
                                .AddXmlDataContractSeria‌​lizerFormatters()
                                .AddJsonOptions(options => loadJsonOptions(options));
        }     

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            // global policy - assign here or on each controller
            app.UseCors("CorsPolicy");
            app.UseMvc();
        }

        #region Helper Methods
        private void loadJsonOptions(MvcJsonOptions options)
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
            options.SerializerSettings.TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple;
            options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
        }
        #endregion
    }
}
