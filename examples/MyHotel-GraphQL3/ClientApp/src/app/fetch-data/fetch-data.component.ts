import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public reservations: Reservation[];
  http: HttpClient;
  baseUrl: string;
  fetchSource: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;
  }

  //////////////////////////   (1)   ////////////////////////////
  fetchFromRestApi(): any {
    this.http.get<Reservation[]>(this.baseUrl + 'api/Reservations/List').subscribe(result => {
      this.reservations = result;
      this.fetchSource = "(1) Old School RestAPI";
    }, error => console.error(error));
  }

  //////////////////////////   (2a)   ////////////////////////////
  fetchFromGraphQlClientViaNativeHttp(): any {
    this.http.get<Reservation[]>(this.baseUrl + 'api/Reservations/ListFromGraphql?clientType=0').subscribe(result => {
      this.reservations = result;
      this.fetchSource = "(2a) .NET Native Http GraphQL Client";
    },
      error => console.error(error));
  }

  //////////////////////////   (2b)   ////////////////////////////
  fetchFromGraphQlClientViaCustomGraphQl(): any {
    this.http.get<Reservation[]>(this.baseUrl + 'api/Reservations/ListFromGraphql?clientType=1').subscribe(result => {
      this.reservations = result;
      this.fetchSource = "(2b) .NET CustomGraphQl Client";
    },
      error => console.error(error));
  }

  //////////////////////////   (3)   ////////////////////////////
  fetchDirectlyFromGraphQl(): any {
    const query = `?query=
                    {
                      reservations {
                        checkinDate
                        guest  {
                          name
                        }
                        room {
                          number
                          name
                        }
                      }
                    }`;

    this.http.get<any>(this.baseUrl + 'graphql/' + query).subscribe(result => {
      this.reservations = result.data.reservations;
      this.fetchSource = "(3) Directly From GraphQL";
    },
      error => console.error(error));
  }


  //////////////////////////   (4)   ////////////////////////////
  fetchUsingVanillaJs(): any {
    fetch('/graphql', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: JSON.stringify({
        query: 'query reservation {reservations {checkinDate guest  {name} room {number name}}}'
      })
    })
      .then(r => r.json())
      .then(response => {
        this.reservations = response.data.reservations;
        this.fetchSource = "(4) Using Vanilla JS";
      });
  }

  //////////////////////////   (5)   ////////////////////////////
  fetchUsingApolloClient(): any {
    /*Apollo is declared in typings.d.ts file for Angular to be compiled */

    var client = new Apollo.lib.ApolloClient(
      {
        networkInterface: Apollo.lib.createNetworkInterface({
          uri: "/graphql"
        })
      });

    const query = Apollo.gql`
query reservation {
  reservations {
    checkinDate
    guest  {
      name
    }
    room {
      number
      name
    }
  }
}`;

    client.query({
      query: query
    }).then(result => {
      this.reservations = result.data.reservations;
      this.fetchSource = "(5) Using Apollo Client";
    });

  }

}

interface Room {
  id: number;
  number: number;
  name: string;
  status: string;
  allowedSmoking: boolean;
}

interface Guest {
  id: number;
  name: string;
  registerDate: Date;
}

interface Reservation {
  checkinDate: Date;
  checkoutDate: Date;
  room: Room;
  guest: Guest;
}

