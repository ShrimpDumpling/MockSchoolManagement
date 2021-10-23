using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;
using MockSchoolManagement.DataRepositories;
using Microsoft.EntityFrameworkCore.SqlServer;
using MockSchoolManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MockSchoolManagement.CustomerMiddlewares;
using Microsoft.AspNetCore.Authorization;
using MockSchoolManagement.Security;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using MockSchoolManagement.Security.CustomTokenProvider;
using Microsoft.AspNetCore.DataProtection;
using MockSchoolManagement.Application.Students;
using MockSchoolManagement.Infrastructure.Data;
using MockSchoolManagement.Application.Courses;
using MockSchoolManagement.Application.Teachers;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using NetCore.AutoRegisterDi;
using Swashbuckle.AspNetCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using MockSchoolManagement.Application.Dtos;

namespace MockSchoolManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //注册swagger生成器
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "MockSchoolManagement API", 
                    Description= "为MockSchoolManagement系统，添加一个简单的ASP.NET CORE WEB API 示例",
                    Version = "v1",
                    //TermsOfService=new Uri("https://github.com/ShrimpDumpling"),
                    Contact=new OpenApiContact
                    {
                        Name="JerryHuang",
                        Email="JerryHuang@outlook.jp",
                        Url=new Uri("https://github.com/ShrimpDumpling")
                    },
                    License=new OpenApiLicense
                    {
                        Name= "The MIT License",
                        Url=new Uri("https://github.com/aws/mit-0")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });

            services.AddHttpContextAccessor();
            services.AddScoped<IStudentRepository, SQLStudentRepositry>();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>
                ("CustomEmailConfirmation");

            services.AddSingleton<DataProtectionPurposeStrings>();
            services.AddTransient(typeof(IRepository<,>), typeof(Repository<,>));


            //services.AddScoped<IStudentService, StudentService>();
            //services.AddScoped<ICourseService,CourseService>();
            //自动注入
            var assembliesToScan = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.GetAssembly(typeof(PagedResultDto<>)),
            };
            services.RegisterAssemblyPublicNonGenericClasses(assembliesToScan)
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);



            services.AddAuthentication()
                .AddMicrosoftAccount(microsoleOptions =>
                {
                    microsoleOptions.ClientId = "3d802b3b-4296-450c-a278-65962830718f";
                    microsoleOptions.ClientSecret = "o19c0r1t-Bx84_O7N0-.7NVurPHcnbM~C_";

                    microsoleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    //microsoleOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    //microsoleOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];

                });

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;//强制验证邮箱后登录

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                //覆盖原有的邮箱处理规则

                // 10次登录失败以后会锁定30分钟的规则
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.ConfigureApplicationCookie(o => {
                o.ExpireTimeSpan = TimeSpan.FromDays(5);//cookie活动超时
                o.SlidingExpiration = true;
            });
            services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromHours(3));//自定义令牌有效期



            //services.AddRazorPages();
            services.AddDbContextPool<AppDbContext>(
                options => 
                options.UseSqlServer(Configuration.GetConnectionString("MockStudentDBConnection")));

            services.AddMvc(a=> {
                a.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder()
                                              .RequireAuthenticatedUser()
                                              .Build();
                a.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddRazorRuntimeCompilation()//部署或者测试的时候请注释掉
                .AddXmlSerializerFormatters();
            


            //services.AddLogging();
            //services.AddControllersWithViews();

            //授权策略
            services.AddAuthorization(options =>
            {
                //策略结合声明授权
                options.AddPolicy("CreateRolePolicy",
                  policy => policy.RequireClaim("Create Role","True"));

                //func实现自定义授权
                options.AddPolicy("EditRolePolicy",
                    policy => policy.RequireAssertion(context => AuthorizaAccess(context)));

                //策略结合角色授权
                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("Admin", "Super Admin"));

                ////策略结合多个角色进行授权
                //options.AddPolicy("SuperAdminPolicy", policy =>
                //  policy.RequireRole("Super Admin"));

                //自定义授权处理程序
                options.AddPolicy("CustomEditRolePolicy", policy =>
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

            });

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            


            //services.ConfigureApplicationCookie(options =>
            //{
            //    //拒绝访问
            //    options.AccessDeniedPath = new PathString("/Admin/AccessDeied");

            //    //修改登录地址的路由
            //    //   options.LoginPath = new PathString("/Admin/Login");  
            //    //修改注销地址的路由
            //    //   options.LogoutPath = new PathString("/Admin/LogOut");

            //    //统一系统全局的Cookie名称
            //    options.Cookie.Name = "MockSchoolCookieName";
            //    // 登录用户Cookie的有效期 
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);//60分钟
            //    //是否对Cookie启用滑动过期时间。
            //    options.SlidingExpiration = true;

            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
        {
            //app.UseDataInitializer(); //初始化种子数据的方法，调试的时候使用



            if (env.IsDevelopment())
            {
                var options = new DeveloperExceptionPageOptions();
                options.SourceCodeLineCount = 40;
                app.UseDeveloperExceptionPage(options);
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Error");
            }


           


            //app.UseDirectoryBrowser();
            //app.UseDefaultFiles();
            //app.UseStaticFiles();
            app.UseFileServer();

            if (env.IsDevelopment())
            {
                //开发环境下启用swagget中间件
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
                });
            }

            app.UseCookiePolicy();
            app.UseAuthentication();//授权
            app.UseAuthorization();//验证

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
        }

        public bool AuthorizaAccess(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole("Admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
                || context.User.IsInRole("Super Admin");
        }
    }
}
