using Microsoft.AspNetCore.Mvc;
using MyHotel.GraphQL.Client;
using MyHotel.Models;
using MyHotel.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyHotel.Controllers
{
    [Route("api/[controller]")]
    public class ReservationsController : Controller
    {
        private readonly MyHotelRepository _myHotelRepository;
        private readonly ReservationHttpGraphqlClient _httpGraphqlClient;
        private readonly ReservationGraphqlClient _graphqlClient;

        public ReservationsController(MyHotelRepository myHotelRepository,
            ReservationHttpGraphqlClient httpGraphqlClient,
            ReservationGraphqlClient graphqlClient)
        {
            _myHotelRepository = myHotelRepository;
            _httpGraphqlClient = httpGraphqlClient;
            _graphqlClient = graphqlClient;
        }

        [HttpGet("[action]")]
        public async Task<List<ReservationModel>> List()
        {
            return await _myHotelRepository.GetAll<ReservationModel>();
        }

        [HttpGet("[action]")]
        public async Task<List<ReservationModel>> ListFromGraphql(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.NativeHttp:
                    return await GetViaHttpGraphqlClient(); /* (Way:1) Native Http Client */

                case ClientType.CustomGraphQlClient:
                    return await GetViaCustomGraphqlClient(); /* (Way:2) GraphQl.Client Library */
            }

            throw new NotSupportedException();
        }

        private async Task<List<ReservationModel>> GetViaHttpGraphqlClient()
        {
            var response = await _httpGraphqlClient.GetReservationsAsync();
            response.ThrowExceptionOnError();
            return response.Data.Reservations;
        }

        private async Task<List<ReservationModel>> GetViaCustomGraphqlClient()
        {
            var reservations = await _graphqlClient.GetReservationsAsync();
            return reservations;
        }
    }
}