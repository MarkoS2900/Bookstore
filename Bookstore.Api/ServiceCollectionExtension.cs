using Bookstore.Api.Repositories;
using Bookstore.Api.Repositories.Interfaces;
using Bookstore.Api.Services;
using Bookstore.Api.Services.Interfaces;

namespace Bookstore.Api
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddBookstoreServices(this IServiceCollection services)
        {
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IAuthorService, AuthorService>();
            
            return services;
        }
    }
}
