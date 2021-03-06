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

namespace MockSchoolManagement
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
            services.RegisterAssemblyPublicNonGenericClasses()
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
            //.AddCookie(p => p.SlidingExpiration = true)

            //services.AddAuthorization();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;//??????????????????

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                //??????????????????????

                // 10????????????????????30??????????
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.ConfigureApplicationCookie(o => {
                o.ExpireTimeSpan = TimeSpan.FromDays(5);//cookie????????
                o.SlidingExpiration = true;
            });
            services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromHours(3));//????????????????



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
                .AddRazorRuntimeCompilation()//??????????????????????????
                .AddXmlSerializerFormatters();
            


            //services.AddLogging();
            //services.AddControllersWithViews();

            //????????
            services.AddAuthorization(options =>
            {
                //????????????????
                options.AddPolicy("CreateRolePolicy",
                  policy => policy.RequireClaim("Create Role","True"));

                //func??????????????
                options.AddPolicy("EditRolePolicy",
                    policy => policy.RequireAssertion(context => AuthorizaAccess(context)));

                //????????????????
                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("Admin", "Super Admin"));

                ////????????????????????????
                //options.AddPolicy("SuperAdminPolicy", policy =>
                //  policy.RequireRole("Super Admin"));

                //??????????????????
                options.AddPolicy("CustomEditRolePolicy", policy =>
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

            });

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            


            //services.ConfigureApplicationCookie(options =>
            //{
            //    //????????
            //    options.AccessDeniedPath = new PathString("/Admin/AccessDeied");

            //    //??????????????????
            //    //   options.LoginPath = new PathString("/Admin/Login");  
            //    //??????????????????
            //    //   options.LogoutPath = new PathString("/Admin/LogOut");

            //    //??????????????Cookie????
            //    options.Cookie.Name = "MockSchoolCookieName";
            //    // ????????Cookie???????? 
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);//60????
            //    //??????Cookie??????????????????
            //    options.SlidingExpiration = true;

            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
        {
            app.UseDataInitializer(); //????????????????????????????????????



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

            app.UseCookiePolicy();
            app.UseAuthentication();//????
            app.UseAuthorization();//????

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
            //app.UseMvcWithDefaultRoute();
            //app.UseRouting();
            //app.UseEndpoints(endpoiints =>
            //{
            //    endpoiints.MapControllers();
            //});




            //app.UseRouting();
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //     {
            //         context.Response.ContentType = "text/plain;charset=UTF-8";
            //         //var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            //         //await context.Response.WriteAsync(processName);
            //         await context.Response.WriteAsync(Configuration["MyKey"].ToString());
            //     });
            //});

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //}

            //app.UseStaticFiles();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapRazorPages();
            //});
        }

        public bool AuthorizaAccess(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole("Admin") && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
                || context.User.IsInRole("Super Admin");
        }
    }
}
