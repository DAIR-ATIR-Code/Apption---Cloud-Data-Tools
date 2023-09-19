
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using DataTools;
using ElectronNET.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net.Mime;
//using System.Net.Mime;
using System.Threading.Tasks;
using WebAppMaterialize.App.Services.Interfaces;
using WebAppMaterialize.Server.Services;

namespace WebAppMaterialize.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds the Server-Side Blazor services, and those registered by the app project's startup.
            //TODOVERIFY services.AddServerSideBlazor<App.Startup>();
            //TODO: Load from JSON configuration file
            services.AddSingleton<Preferences>(new Preferences());
            services.AddSingleton<IUIService, UIService>();
            services.AddSession();
            services.AddMvc();

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    //TODOVERIFYWasmMediaTypeNames.Application.Wasm,
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();

            // Use component registrations and static files from the app project.
            //TODOVERIFY app.UseServerSideBlazor<App.Startup>();
            Task.Run(async () => await Electron.WindowManager.CreateWindowAsync());

        }
    }
}

