using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using MyHotel.Models;

namespace MyHotel.GraphQL.Client
{
    /*This class is created to use GraphQl.Client library*/
    public class ReservationGraphqlClient
    {
        private readonly GraphQLHttpClient _client;

        public ReservationGraphqlClient(GraphQLHttpClient client)
        {
            _client = client;
        }
        public class ReservationsType
        {
            public List<ReservationModel> Reservations { get; set; }
        }
        public async Task<List<ReservationModel>> GetReservationsAsync()
        {
            var query = new GraphQLRequest
            {
                Query = @"
query reservation {
  reservations {
    checkinDate
    guest  {
      name
    }
    room {
      name,
      number
    }
  }
}
"
            };

            var response = await _client.SendQueryAsync<ReservationsType>(query);
            if (response.Errors != null && response.Errors.Any())
            {
                throw new ApplicationException(response.Errors[0].Message);
            }

            var reservations = response.Data?.Reservations;
            return reservations;
        }
    }
}
