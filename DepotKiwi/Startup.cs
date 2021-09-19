using System;
using System.IO;
using DepotKiwi.Db;
using DepotKiwi.Middleware;
using DepotKiwi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DepotKiwi {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            
            services.AddSingleton<IDatabaseSettings>(x => x.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<DatabaseContext>();
            
            var storage = Environment.GetEnvironmentVariable("DEPOTKIWI_STORAGE");

            storage ??= Path.Join(AppContext.BaseDirectory, "storage");
            
            services.AddSingleton(new RepositoryService(storage));
            
            services.AddCors(o => o.AddPolicy("Any", builder =>  {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
            }
            
            app.UseCors("Any");

            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseMiddleware<NotFoundMiddleware>();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}