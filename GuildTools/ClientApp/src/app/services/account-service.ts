import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { RegistrationCredentials, LoginCredentials } from './ServiceTypes/service-types';

@Injectable()
export class AccountService {

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {

  }

  public login(username: string, password: string): Observable<Object> {
    const serviceCredentials = new LoginCredentials();

    serviceCredentials.email = username;
    serviceCredentials.password = password;

    return this.http.post(this.baseUrl + 'api/account/login', serviceCredentials);
  }

  public register(inputModel: RegistrationModel): Observable<Object> {

    const serviceCredentials = new RegistrationCredentials();

    serviceCredentials.email = inputModel.email;
    serviceCredentials.password = inputModel.password;
    serviceCredentials.username = inputModel.username;

    return this.http.post(this.baseUrl + 'api/account/register', serviceCredentials);
  }


  public requestPasswordReset(email: string): Observable<Object> {
    return this.http.get(this.baseUrl + `api/account/resetPassword?emailAddress=${email}`);
  }


  public postPasswordResetWithToken(userId: string, token: string, newPassword: string): Observable<Object> {
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
