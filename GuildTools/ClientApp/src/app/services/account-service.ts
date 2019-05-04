import { Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { RegistrationDetails, LoginCredentials, ConfirmEmail, LoginResponse, ILoginResponse } from './ServiceTypes/service-types';
import { map, share } from 'rxjs/operators';

@Injectable()
export class AccountService {

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {

  }
  public login(username: string, password: string): Observable<LoginResponse> {

    const serviceCredentials = new LoginCredentials();

    serviceCredentials.email = username;
    serviceCredentials.password = password;

    return this.http.post(this.baseUrl + 'api/account/login', serviceCredentials)
      .pipe(
        map(response => new LoginResponse(response as ILoginResponse)),
        share()
      );
  }


  public confirmEmail(userId: string, token: string): Observable<Object> {

    const input = new ConfirmEmail();

    input.token = token;
    input.userId = userId;

    return this.http.post(this.baseUrl + 'api/account/confirmEmail', input);
  }

  public register(inputModel: RegistrationDetails): Observable<Object> {

    return this.http.post(this.baseUrl + 'api/account/register', inputModel);
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
