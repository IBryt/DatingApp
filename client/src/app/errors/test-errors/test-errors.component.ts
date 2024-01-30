import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { environment } from 'src/environment/environment';

const observerOrNext = {
  next: (response: any) => console.log(response),
  error: (error:any) => console.log(error),
};

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent {
  baseUrl = environment.apiUrl;
  validationErrors: string[] = [];

  constructor(
    private http: HttpClient,
  ) { }

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe(observerOrNext);
  }

  get400Error() {
    this.http.get(this.baseUrl + 'buggy/bad-request').subscribe(observerOrNext);
  }

  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error').subscribe(observerOrNext);
  }

  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth').subscribe(observerOrNext);
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', {}).subscribe({
      next: (response: any) => console.log(response),
      error: (error:any) =>{ 
        console.log(error)
        this.validationErrors = error
      },
    });
  }
}
