using System;

namespace MyHotel.AutoMapper
{
    public class MappingItem
    {
        public Type SourceType { get; set; }

        public string SourcePropertyPath { get; set; }

        public Type DestinationType { get; set; }

        public string DestinationPropertyPath { get; set; }
    }
}
