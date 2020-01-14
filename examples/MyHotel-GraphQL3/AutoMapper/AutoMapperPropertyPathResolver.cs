using AutoMapper;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFrameworkCore.DynamicLinq.Resolvers;

namespace MyHotel.AutoMapper
{
    public class AutoMapperPropertyPathResolver : IPropertyPathResolver
    {
        private readonly ICollection<MappingItem> _mappings = new List<MappingItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapperPropertyPathResolver"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public AutoMapperPropertyPathResolver([NotNull] IMapper mapper)
        {
            // Guard.NotNull(mapper, nameof(mapper));

            Init(mapper);
        }

        private void Init(IMapper mapper)
        {
            var allTypeMaps = mapper.ConfigurationProvider.GetAllTypeMaps();
            foreach (var typeMap in allTypeMaps)
            {
                foreach (var propertyMap in typeMap.PropertyMaps.Where(pm => pm.CustomMapExpression == null))
                {
                    string sourcePropertyPath = propertyMap.SourceMember.Name;

                    _mappings.Add(new MappingItem
                    {
                        SourceType = propertyMap.SourceType,
                        SourcePropertyPath = sourcePropertyPath,
                        DestinationType = typeMap.DestinationType,
                        DestinationPropertyPath = propertyMap.DestinationName
                    });
                }

                foreach (var propertyMap in typeMap.PropertyMaps.Where(pm => pm.CustomMapExpression != null))
                {
                    string body = propertyMap.CustomMapExpression.Body.ToString();
                    string tag = propertyMap.CustomMapExpression.Parameters[0].Name;
                    string sourcePropertyPath = body.Replace($"{tag}.", string.Empty);

                    _mappings.Add(new MappingItem
                    {
                        SourceType = propertyMap.SourceType,
                        SourcePropertyPath = sourcePropertyPath,
                        DestinationType = typeMap.DestinationType,
                        DestinationPropertyPath = propertyMap.DestinationName
                    });
                }
            }

            foreach (var mapping in _mappings.ToArray())
            {
                _mappings.Add(new MappingItem
                {
                    SourceType = mapping.DestinationType,
                    SourcePropertyPath = mapping.DestinationPropertyPath,
                    DestinationType = mapping.SourceType,
                    DestinationPropertyPath = mapping.SourcePropertyPath
                });
            }
        }

        public string Resolve(Type sourceType, string sourcePropertyPath, Type destinationType)
        {
            var destination = _mappings.FirstOrDefault(m => m.SourceType == sourceType && m.SourcePropertyPath == sourcePropertyPath && (m.DestinationType == destinationType || destinationType == null));

            return destination != null ? destination.DestinationPropertyPath : sourcePropertyPath;
        }
    }
}