using Application.Implementations;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services
                .AddTransient<ITaskBL, TaskBL>()
                .AddTransient<IAuthenticationBL, AuthenticationBL>();
        }    
    }
}
