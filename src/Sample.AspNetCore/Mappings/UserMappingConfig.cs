using Mapster;
using Sample.AspNetCore.Models;
using System;

namespace Sample.AspNetCore.Mappings
{
    /// <summary>
    /// Example of using IRegister pattern for mapping configuration.
    /// This will be automatically discovered when using assembly scanning.
    /// </summary>
    public class UserMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserDto>()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}")
                .Map(dest => dest.Age, src => DateTime.Now.Year - src.BirthYear);
        }
    }
}