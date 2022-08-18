import { HttpClient } from '@angular/common/http';
import { Injectable, Inject } from '@angular/core';
import { Observable } from 'rxjs';
import { appointment } from './appointment';
import { lineUp } from './lineUp';

@Injectable({
  providedIn: 'root',
})
export class ApiPipeService {
  appointment$: Observable<appointment> = new Observable((observer) => {
    this.http.get<appointment[]>(this.baseUrl + 'raid/appointments').subscribe(
      (result) => {
        observer.next(result[0]);
      },
      (error) => console.error(error)
    );
  });

  constructor(
    private http: HttpClient,
    @Inject('WEBAPI_BASE_URL') private baseUrl: string
  ) {}

  public getAppointment() {
    return this.appointment$;
  }

  public postLineUp(lineUp: lineUp) {
    this.http.post(this.baseUrl + 'raid/lineUp', lineUp).subscribe(
      (result) => console.info('line up posted'),
      (error) => console.error(error)
    );
  }
}
