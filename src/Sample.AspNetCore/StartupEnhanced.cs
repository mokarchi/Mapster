using ExpressionDebugger;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.AspNetCore.Controllers;
using Sample.AspNetCore.Models;
using System.Linq.Expressions;
using System.Reflection;
#if NET6_0
using Hellang.Middleware.ProblemDetails;
#endif

namespace Sample.AspNetCore
{
    /// <summary>
    /// Example Startup class demonstrating the enhanced Mapster DI configuration.
    /// This is an alternative to the default Startup.cs showing the new features.
    /// </summary>
    public class StartupEnhanced
    {
        public StartupEnhanced(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(opts => opts.EnableEndpointRouting = false)
                .AddOData(options => options.Select().Filter().OrderBy())
                .AddNewtonsoftJson();
            services.AddDbContext<SchoolContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Register NameFormatter for use in mappings
            services.AddSingleton<NameFormatter>();

            // NEW: Enhanced Mapster configuration with multiple options
            services.AddMapster(options =>
            {
                // Scan assemblies for IRegister and IMapFrom implementations
                options.AssembliesToScan.Add(Assembly.GetExecutingAssembly());

                // Use ServiceMapper for DI support (default: true)
                options.UseServiceMapper = true;

                // Configure inline mappings
                options.ConfigureAction = config =>
                {
                    // Set custom compiler for debugging
                    config.Compiler = exp => exp.CompileWithDebugInfo(
                        new ExpressionCompilationOptions
                        {
                            EmitFile = true,
                            ThrowOnFailedCompilation = true
                        });

                    // Configure mappings with DI
                    config.NewConfig<Enrollment, EnrollmentDto>()
                        .AfterMappingAsync(async dto =>
                        {
                            var context = MapContext.Current.GetService<SchoolContext>();
                            var course = await context.Courses.FindAsync(dto.CourseID);
                            if (course != null)
                                dto.CourseTitle = course.Title;
                            var student = await context.Students.FindAsync(dto.StudentID);
                            if (student != null)
                                dto.StudentName = MapContext.Current
                                    .GetService<NameFormatter>()
                                    .Format(student.FirstMidName, student.LastName);
                        });

                    config.NewConfig<Student, StudentDto>()
                        .Map(dest => dest.Name,
                             src => MapContext.Current
                                .GetService<NameFormatter>()
                                .Format(src.FirstMidName, src.LastName));

                    config.NewConfig<Course, CourseDto>()
                        .Map(dest => dest.CourseIDDto, src => src.CourseID)
                        .Map(dest => dest.CreditsDto, src => src.Credits)
                        .Map(dest => dest.TitleDto, src => src.Title)
                        .Map(dest => dest.EnrollmentsDto, src => src.Enrollments);
                };

                // Precompile hot paths for better performance
                options.TypePairsToPrecompile.Add((typeof(Student), typeof(StudentDto)));
                options.TypePairsToPrecompile.Add((typeof(Course), typeof(CourseDto)));
                options.TypePairsToPrecompile.Add((typeof(Enrollment), typeof(EnrollmentDto)));
            });

            // Alternative: Simple frozen configuration for production
            // services.AddMapsterFrozen(config =>
            // {
            //     config.NewConfig<Student, StudentDto>();
            //     config.NewConfig<Course, CourseDto>();
            // });

            // Alternative: Using assembly scanning only
            // services.ScanMapster(Assembly.GetExecutingAssembly());

            services.AddProblemDetails();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#if NET6_0
            app.UseProblemDetails();
#endif
            app.UseRouting();
            app.UseAuthorization();
            app.UseMvc();
        }
    }
}