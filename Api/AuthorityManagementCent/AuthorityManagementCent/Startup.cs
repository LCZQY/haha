﻿using AuthorityManagementCent.Filters;
using AuthorityManagementCent.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuthorityManagementCent
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfileConfiguration());
            });
        }
        //推送是否成功了呢
        public IConfiguration Configuration { get; }
        public MapperConfiguration _mapperConfiguration { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //配置Token
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(o =>
            {
                o.Events = new JwtBearerEvents()
                {
                    //默认是通过Http的Authorization头来获取的
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Query["access_token"];
                        return Task.CompletedTask;
                    }
                };
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, //是否验证Issuer
                    ValidateAudience = true, //是否验证Adience
                    ValidateLifetime = true, //是否验证失效时间
                    ValidateIssuerSigningKey = true, //是否验证SigningKey
                    ValidIssuer = "ZQY", //发放者
                    ValidAudience = "PC", // 来源(那个端)
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZ1456789513"))
                };
            });





            //AutoMapper
            services.AddSingleton<IMapper>(mp => _mapperConfiguration.CreateMapper());

            //数据库连接,注意：一定要加 sslmode=none 
            var connection = Configuration.GetConnectionString("MysqlConnection");
            services.AddDbContext<ModelContext>(options => options.UseMySQL(connection));

            //依赖注入类
            Plugin.AddScopeds(services);

            //注册Swagger文件
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "权限管理系统",
                    Description = "用户登陆请求Token/用户管理/组织管理/权限管理/角色管理",
                    TermsOfService = "None",
                });

                //给Swagger界面新增添加权限界面
                options.OperationFilter<HttpHeaderOperation>(); // 添加httpHeader               
                options.DocInclusionPredicate((docName, description) => true);
                options.IgnoreObsoleteActions();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            services.AddMvc(options =>
            {
                options.Filters.Add<JwtTokenAuthorize>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }


            //启动权限验证
            app.UseAuthentication();

            //启用自定义权限验证
            //app.UseMiddleware<JwtTokenAuth>();

            //启动Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            //启动前台跨域访问
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            app.UseHttpsRedirection();

            //日志配置，为了防止中文乱码
            ConfigureNLog(app, env, loggerFactory);
            app.UseMvc();
        }

        public void ConfigureNLog(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//这是为了防止中文乱码
            loggerFactory.AddNLog();//添加NLog
            env.ConfigureNLog("nlog.config");//读取Nlog配置文件                                             
        }
    }
}
