import { Component, OnInit, Inject } from '@angular/core';
import { LoginModel } from './login.model';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { RoutePaths } from '../data/route-paths';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  model: LoginModel;
  errors: Array<string>;

  constructor(
      public auth: AuthService,
      public router: Router,
      @Inject('BASE_URL') private baseUrl: string) {
    this.model = new LoginModel();
    this.errors = [];
  }

  ngOnInit() {
  }

  onSubmit(): boolean {

    this.auth.logIn(this.model.Email, this.model.Password).subscribe(
      success => {
        this.router.navigate(['/']);
      },
      error => {
        this.errors = [error.error];
      });

    return false;
  }

  forgotPassword(): void {
    this.router.navigate(['/' + RoutePaths.ResetPassword]);
  }
}
