using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetricsAgent.DB;
using MetricsAgent.Jobs;
using MetricsAgent.DB.Entities;
using AutoMapper;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using System.Net.Http;

namespace MetricsAgent
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MetricsAgent", Version = "v1" });
            });

            var mapperConfig = new MapperConfiguration(mp => mp.AddProfile(new MapperProfile()));
            var mapper = mapperConfig.CreateMapper();

            services.AddSingleton(mapper);

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IDBRepository<CpuMetricsEntity>, DBRepository<CpuMetricsEntity>>();
            services.AddScoped<IDBRepository<DotNetMetricsEntity>, DBRepository<DotNetMetricsEntity>>();
            services.AddScoped<IDBRepository<HddMetricsEntity>, DBRepository<HddMetricsEntity>>();
            services.AddScoped<IDBRepository<NetworkMetricsEntity>, DBRepository<NetworkMetricsEntity>>();
            services.AddScoped<IDBRepository<RamMetricsEntity>, DBRepository<RamMetricsEntity>>();
            
            // ���������� ��������
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            // ���������� ������
            services.AddSingleton<Job>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(Job),
                cronExpression: "0/5 * * * * ?"));
            services.AddHostedService<QuartzHostedService>();

            // ���������� Http-client
            services.AddTransient<IStartupFilter, HttpClientManager>();
            //services.AddTransient<IHttpClientFactory>();
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetricsAgent v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}