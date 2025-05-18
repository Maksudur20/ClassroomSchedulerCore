using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClassroomSchedulerCore.Areas.Identity.Data;
using ClassroomSchedulerCore.Data;
using ClassroomSchedulerCore.Models;

[assembly: HostingStartup(typeof(ClassroomSchedulerCore.Areas.Identity.IdentityHostingStartup))]
namespace ClassroomSchedulerCore.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                // Identity is already configured in Program.cs, so we don't need to add it here
            });
        }
    }
}
