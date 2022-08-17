import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { appointment } from './appointment';

@Injectable({
  providedIn: 'root'
})
export class ApiPipeService {
  appointmentMock: appointment = {
    id: 123,
    name: "Dienstag",
    timestamp: new Date(),
    participants: [
      {
        "id": 2,
        "name": "Haru",
        "preferedRoles": [
          1,
          2
        ],
        "roles": [
          1,
          2
        ]
      },
      {
        "id": 3,
        "name": "Markus",
        "preferedRoles": [
          1,
          3
        ],
        "roles": [
          1,
          3
        ]
      },
      {
        "id": 4,
        "name": "Statur",
        "preferedRoles": [
          1,
          4
        ],
        "roles": [
          1,
          4
        ]
      },
      {
        "id": 5,
        "name": "Zaratusa",
        "preferedRoles": [
          1,
          5
        ],
        "roles": [
          1,
          5
        ]
      },
      {
        "id": 6,
        "name": "Franzi/Zokora",
        "preferedRoles": [
          1,
          6
        ],
        "roles": [
          1,
          6
        ]
      },
      {
        "id": 7,
        "name": "Ithyphallophobie",
        "preferedRoles": [
          1,
          7
        ],
        "roles": [
          1,
          7
        ]
      },
      {
        "id": 8,
        "name": "Sillox",
        "preferedRoles": [
          1,
          8
        ],
        "roles": [
          1,
          8
        ]
      }
    ]
  };
  appointment$: Observable<appointment> = new Observable((observer)=>{
    observer.next(this.appointmentMock);
  });
  constructor() { }

  public getAppointment(){
    return this.appointment$;
  }

}
