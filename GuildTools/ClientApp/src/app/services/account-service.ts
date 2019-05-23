import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { RegistrationDetails, LoginCredentials, ConfirmEmail, LoginResponse, ILoginResponse, ChangePassword, UpdateUserDetails } from './ServiceTypes/service-types';
import { map, share } from 'rxjs/operators';
import { AuthService } from 'app/auth/auth.service';
import { StoredValuesService } from 'app/shared-services/stored-values';

class Urls {
  public static get changePasswordUrl() {
    return `api/account/changePassword`;
  }

  public static get updateUserDetailsUrl() {
    return `api/account/updateUserDetails`;
  }
}

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

  public changePassword(oldPassword: string, newPassword: string): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new ChangePassword();

    input.oldPassword = oldPassword;
    input.newPassword = newPassword;

    return this.http.post(this.baseUrl + Urls.changePasswordUrl, input, { headers: headers });
  }

  public updateDetails(username: string): Observable<Object> {
    const headers = this.getAuthorizeHeader();

    const input = new UpdateUserDetails();

    input.username = username;

    return this.http.post(this.baseUrl + Urls.updateUserDetailsUrl, input, { headers: headers });
  }

  private getResetUrl(baseUrl: string) {
    return `${baseUrl}/api/account/resetpasswordtoken`;
  }

  private getAuthorizeHeader(): HttpHeaders {
    const accessToken = localStorage.getItem(StoredValuesService.AccessTokenKey);

    const authBearer = 'Bearer ' + accessToken;

    const headers = new HttpHeaders().append('Authorization', authBearer);

    return headers;
  }
}
