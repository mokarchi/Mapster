using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Reflection;

namespace Mapster.DependencyInjection.Tests
{
    [TestClass]
    public class EnhancedDITests
    {
        [TestMethod]
        public void AddMapster_WithOptions_ShouldRegisterServiceMapper()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMapster(options =>
            {
                options.UseServiceMapper = true;
                options.ConfigureAction = config =>
                {
                    config.NewConfig<Source, Destination>()
                        .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            mapper.ShouldNotBeNull();
            mapper.ShouldBeOfType<ServiceMapper>();

            var source = new Source { FirstName = "John", LastName = "Doe" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("John Doe");
        }

        [TestMethod]
        public void AddMapster_WithConfigureAction_ShouldApplyConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMapster(config =>
            {
                config.NewConfig<Source, Destination>()
                    .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            var source = new Source { FirstName = "Jane", LastName = "Smith" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Jane Smith");
        }

        [TestMethod]
        public void AddMapsterWithConfig_ShouldUseExistingConfig()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new TypeAdapterConfig();
            config.NewConfig<Source, Destination>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            // Act
            services.AddMapsterWithConfig(config);

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            var source = new Source { FirstName = "Bob", LastName = "Johnson" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Bob Johnson");
        }

        [TestMethod]
        public void AddMapsterFrozen_ShouldFreezeConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMapsterFrozen(config =>
            {
                config.NewConfig<Source, Destination>()
                    .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            var source = new Source { FirstName = "Alice", LastName = "Williams" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Alice Williams");
        }

        [TestMethod]
        public void AddMapsterModule_ShouldRegisterModule()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMapsterModule<TestMapsterModule>();

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            var source = new Source { FirstName = "Module", LastName = "Test" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Module Test");
        }

        [TestMethod]
        public void ScanMapster_ShouldScanAssemblies()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.ScanMapster(Assembly.GetExecutingAssembly());

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            mapper.ShouldNotBeNull();
            mapper.ShouldBeOfType<ServiceMapper>();
        }

        [TestMethod]
        public void AddMapster_BackwardCompatibility_ShouldStillWork()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new TypeAdapterConfig();
            config.NewConfig<Source, Destination>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
            services.AddSingleton(config);

            // Act
            services.AddMapster();

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            mapper.ShouldNotBeNull();
            // Note: Without config parameter, it uses default Mapper, not ServiceMapper
            mapper.ShouldBeOfType<Mapper>();
        }

        [TestMethod]
        public void AddMapster_WithBuildActions_ShouldExecuteActions()
        {
            // Arrange
            var services = new ServiceCollection();
            var beforeCompileCalled = false;
            var afterCompileCalled = false;

            // Act
            services.AddMapster(
                config =>
                {
                    config.NewConfig<Source, Destination>()
                        .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
                },
                actions =>
                {
                    actions.OnBeforeCompile = _ => beforeCompileCalled = true;
                    actions.OnAfterCompile = _ => afterCompileCalled = true;
                });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert
            beforeCompileCalled.ShouldBeTrue();
            afterCompileCalled.ShouldBeTrue();

            var source = new Source { FirstName = "Test", LastName = "User" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Test User");
        }

        [TestMethod]
        public void AddMapster_WithPrecompile_ShouldPrecompileTypePairs()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMapster(options =>
            {
                options.ConfigureAction = config =>
                {
                    config.NewConfig<Source, Destination>()
                        .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
                };
                options.TypePairsToPrecompile.Add((typeof(Source), typeof(Destination)));
            });

            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Assert - Should work without errors
            var source = new Source { FirstName = "Pre", LastName = "Compiled" };
            var destination = mapper.Map<Source, Destination>(source);
            destination.FullName.ShouldBe("Pre Compiled");
        }
    }

    // Test classes
    public class Source
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Destination
    {
        public string FullName { get; set; }
    }

    // Test module
    public class TestMapsterModule : IMapsterModule
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Source, Destination>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
        }
    }

    // Test register class
    public class TestRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Source, Destination>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
        }
    }

    // Test IMapFrom implementation
    public class DestinationWithMapFrom : IMapFrom<Source>
    {
        public string FullName { get; set; }

        public void ConfigureMapping(TypeAdapterConfig config)
        {
            config.NewConfig<Source, DestinationWithMapFrom>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
        }
    }
}