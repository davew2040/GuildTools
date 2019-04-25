import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class AccountService {

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {

  }

  public login(username: string, password: string): Observable<Object> {
    let credentials = {
      "email": username,
      "password": password
    };

    return this.http.post(this.baseUrl + 'api/account/login', credentials);
  }

  public register(model: RegistrationModel): Observable<Object> {
    return this.http.post(this.baseUrl + 'api/account/register', model);
  }

  public postPasswordReset(userId: string, token: string, newPassword: string): Observable<Object> {
    return this.http.post(this.getResetUrl(this.baseUrl),
      {
        userId: userId,
        token: token,
        newPassword: newPassword
      }
    );
  }

  private getResetUrl(baseUrl: string) {
    return `${baseUrl}/api/account/resetpasswordtoken`;
  }
}

export class RegistrationModel {
  public username: string;
  public password: string;
  public email: string;
}
