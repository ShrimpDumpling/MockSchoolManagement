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
            //ע��swagger������
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "MockSchoolManagement API", 
                    Description= "ΪMockSchoolManagementϵͳ�����һ���򵥵�ASP.NET CORE WEB API ʾ��",
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
            //�Զ�ע��
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
                options.SignIn.RequireConfirmedEmail = true;//ǿ����֤������¼

                options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                //����ԭ�е����䴦�����

                // 10�ε�¼ʧ���Ժ������30���ӵĹ���
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.ConfigureApplicationCookie(o => {
                o.ExpireTimeSpan = TimeSpan.FromDays(5);//cookie���ʱ
                o.SlidingExpiration = true;
            });
            services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromHours(3));//�Զ���������Ч��



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
                .AddRazorRuntimeCompilation()//������߲��Ե�ʱ����ע�͵�
                .AddXmlSerializerFormatters();
            


            //services.AddLogging();
            //services.AddControllersWithViews();

            //��Ȩ����
            services.AddAuthorization(options =>
            {
                //���Խ��������Ȩ
                options.AddPolicy("CreateRolePolicy",
                  policy => policy.RequireClaim("Create Role","True"));

                //funcʵ���Զ�����Ȩ
                options.AddPolicy("EditRolePolicy",
                    policy => policy.RequireAssertion(context => AuthorizaAccess(context)));

                //���Խ�Ͻ�ɫ��Ȩ
                options.AddPolicy("AdminRolePolicy",
                   policy => policy.RequireRole("Admin", "Super Admin"));

                ////���Խ�϶����ɫ������Ȩ
                //options.AddPolicy("SuperAdminPolicy", policy =>
                //  policy.RequireRole("Super Admin"));

                //�Զ�����Ȩ�������
                options.AddPolicy("CustomEditRolePolicy", policy =>
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

            });

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            


            //services.ConfigureApplicationCookie(options =>
            //{
            //    //�ܾ�����
            //    options.AccessDeniedPath = new PathString("/Admin/AccessDeied");

            //    //�޸ĵ�¼��ַ��·��
            //    //   options.LoginPath = new PathString("/Admin/Login");  
            //    //�޸�ע����ַ��·��
            //    //   options.LogoutPath = new PathString("/Admin/LogOut");

            //    //ͳһϵͳȫ�ֵ�Cookie����
            //    options.Cookie.Name = "MockSchoolCookieName";
            //    // ��¼�û�Cookie����Ч�� 
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);//60����
            //    //�Ƿ��Cookie���û�������ʱ�䡣
            //    options.SlidingExpiration = true;

            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILogger<Startup> logger)
        {
            //app.UseDataInitializer(); //��ʼ���������ݵķ��������Ե�ʱ��ʹ��



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
                //��������������swagget�м��
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
                });
            }

            app.UseCookiePolicy();
            app.UseAuthentication();//��Ȩ
            app.UseAuthorization();//��֤

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
