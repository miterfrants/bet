using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Homo.Api;
using Homo.Bet.Api;

namespace Homo.Bet.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Console.WriteLine(env.EnvironmentName);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"secrets.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        readonly string AllowSpecificOrigins = "";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Homo.Bet.Api.AppSettings appSettings = new Homo.Bet.Api.AppSettings();
            Configuration.GetSection("Config").Bind(appSettings);
            services.Configure<Homo.Bet.Api.AppSettings>(Configuration.GetSection("Config"));

            // setup CROS if config file includ CROS section
            IConfigurationSection CROSSection = Configuration.GetSection("CROS");

            string stringCrossOrigins = Configuration.GetSection("CROS").GetValue<string>("Origin");
            if (stringCrossOrigins != null)
            {
                string[] crossOrigins = stringCrossOrigins.Split(",");
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(crossOrigins));

                services.AddCors(options =>
                {
                    options.AddPolicy(AllowSpecificOrigins,
                        builder =>
                        {
                            builder.WithOrigins(crossOrigins)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .SetPreflightMaxAge(TimeSpan.FromSeconds(600));
                        });
                });
            }
            CultureInfo currentCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            services.AddSingleton<ErrorMessageLocalizer>(new ErrorMessageLocalizer(appSettings.Common.LocalizationResourcesPath));
            services.AddSingleton<CommonLocalizer>(new CommonLocalizer(appSettings.Common.LocalizationResourcesPath));
            services.AddSingleton<ValidationLocalizer>(new ValidationLocalizer(appSettings.Common.LocalizationResourcesPath));
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            var secrets = (Homo.Bet.Api.Secrets)appSettings.Secrets;
            services.AddDbContext<BargainingChipDBContext>(options => options.UseMySql(secrets.DBConnectionString, serverVersion));
            services.AddControllers();
            services.AddCronJob<GitHubIssuesNotificationCronJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 0,8 * * 1-5";
            });
            services.AddCronJob<GitHubAutoCommentViolationCronJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"* * * * *";
            });
            services.AddCronJob<RenewCoinLog>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 0 * * 0";
            });
            services.AddCronJob<BetCoinNotification>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 9 * * 3";
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Api Doc", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                        },
                        new string[] { }
                        }
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "dev")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "homo auth api v1"));
            }

            var supportedCultures = new[] {
                new CultureInfo("zh-TW"),
                new CultureInfo("en-US"),
                new CultureInfo("fr"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("zh-TW"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider{QueryStringKey= "culture"}
                }
            });

            String apiPrefix = Configuration.GetSection("Config").GetSection("Common").GetValue<string>("ApiPrefix");
            app.UseCors(AllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UsePathBase(new PathString($"/{apiPrefix}"));
            app.UseRouting();
            app.UseMiddleware(typeof(AuthApiErrorHandlingMiddleware));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}