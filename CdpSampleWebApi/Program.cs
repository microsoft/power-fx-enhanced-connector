// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace CdpSampleWebApi
{
    public partial class Program
    {
        public static DateTime StartupTime { get; private set; }

        public static void Main(string[] args)
        {
            Program.StartupTime = DateTime.UtcNow;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(mvcOptions =>
            {
                mvcOptions.Filters.Add(new ExceptionHandler());
            });

            // The provider factory is the critical piece to 
            // determine which datasource we have. 
            
            //builder.Services.AddSingleton<ITableProviderFactory, DatabricksTableProviderFactory>();
            builder.Services.AddSingleton<ITableProviderFactory, TrivialTableProviderFactory>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
