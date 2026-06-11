using Libraryapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Libraryapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container. 
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<LibraryContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/Login";
                });

            var app = builder.Build();

            // Automatic Database Creation & Seeding
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
                context.Database.EnsureCreated();

                if (!context.Students.Any() && !context.Books.Any())
                {
                    var student1 = new Student { Name = "Rahul", Branch = "CSE" };
                    var student2 = new Student { Name = "Aman", Branch = "IT" };
                    var student3 = new Student { Name = "Sneha", Branch = "ECE" };
                    var student4 = new Student { Name = "Priya", Branch = "CSE" };
                    var student5 = new Student { Name = "Karan", Branch = "ME" };

                    context.Students.AddRange(student1, student2, student3, student4, student5);

                    var book1 = new Book { Title = "DBMS Basics", Publisher = "Pearson", Genre = "Education", IsAvailable = true };
                    var book2 = new Book { Title = "C# in Depth", Publisher = "Manning", Genre = "Programming", IsAvailable = true };
                    var book3 = new Book { Title = "Data Structures", Publisher = "OReilly", Genre = "Education", IsAvailable = true };
                    var book4 = new Book { Title = "Clean Code", Publisher = "Prentice Hall", Genre = "Programming", IsAvailable = true };
                    var book5 = new Book { Title = "AI Intro", Publisher = "Springer", Genre = "AI", IsAvailable = true };

                    context.Books.AddRange(book1, book2, book3, book4, book5);

                    context.SaveChanges();

                    if (!context.BorrowRecords.Any())
                    {
                        var record = new BorrowRecord
                        {
                            StudentId = student1.StudentId,
                            BookId = book2.BookId,
                            BorrowDate = DateTime.Now.AddDays(-1),
                            ReturnDate = DateTime.Now
                        };
                        context.BorrowRecords.Add(record);
                        context.SaveChanges();
                    }
                }
            }

            // Configure the HTTP request pipeline. 
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.MapControllers();

            app.Run();

        }
    }
}
