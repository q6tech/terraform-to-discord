using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TerraformToDiscord
{
    public class Program
    {
        public static string Version => typeof(Program).Assembly.GetName().Version!.ToString();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(builder => { builder.UseStartup<Startup>(); });
        }
    }
}
