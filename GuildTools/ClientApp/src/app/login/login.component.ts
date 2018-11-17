import { Component, OnInit, Inject } from '@angular/core';
import { LoginModel } from './login.model';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  model: LoginModel;
  errors: Array<string>;

  constructor(
      private http: HttpClient,
      public auth: AuthService,
      public router: Router,
      @Inject('BASE_URL') private baseUrl: string) {
    this.model = new LoginModel();
    this.errors = [];
  }

  ngOnInit() {
  }

  onSubmit(): boolean {
    var credentials = {
      "email": this.model.Email,
      "password": this.model.Password
    };

    this.http.post(this.baseUrl + 'api/account/login', credentials).subscribe(
      success => {
        this.auth.loggedIn(success);
        this.router.navigate(['/']);
      },
      error => {
        this.errors = [error];
      });

    return false;
  }
}
