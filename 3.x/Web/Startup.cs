using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web
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
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
               .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    //��Ȩ�˷����ַ
                    options.Authority = "http://localhost:5000/";
                    //�Ƿ�https����
                    options.RequireHttpsMetadata = false;
                    //�ͻ���ID����
                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClaimActions.MapJsonKey("website", "website");
                    //ȡ����Ȩ���� ���ص�ַ
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRemoteFailure = context =>
                        {
                            //��ת�������ַ
                            context.Response.Redirect("http://localhost:5002/Home/Error");
                            context.HandleResponse();
                            return Task.FromResult(0);
                        }
                    };
                    //���ص�����
                    options.ResponseType = "code id_token";
                    //����Զ����û���Ϣ
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    //�Ƿ�洢token
                    options.SaveTokens = true;
                    //���������ڴ����ƶ˵���յ�id_token������ʶ�󣬴�������Ƿ�Ӧת���û���Ϣ�˵�
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //��������api��Χ
                    options.Scope.Add("api");
                    //����claims��Ĭ�Ϲ��˵�������������м�����˵�nbf��amr, ��nbf��amr�ӱ����˵��������Ƴ�������ʹ�������������ʽ:
                    options.ClaimActions.Remove("nbf");
                    options.ClaimActions.Remove("amr");
                    //ɾ��ĳЩClaims
                    options.ClaimActions.DeleteClaim("sid");
                    options.ClaimActions.DeleteClaim("idp");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
            //���Cookie
            app.UseCookiePolicy();
            //����û���֤�м��
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
