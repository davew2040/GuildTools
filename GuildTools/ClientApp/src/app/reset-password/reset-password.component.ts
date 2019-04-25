import { Component, OnInit, Inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ResetPasswordModel } from './reset-password.model';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {

  model: ResetPasswordModel;
  errors: Array<string>;
  emailSent: boolean;

  constructor(
      private http: HttpClient,
      public router: Router,
      @Inject('BASE_URL')
      private baseUrl: string) {
    this.model = new ResetPasswordModel();
    this.errors = [];
  }

  ngOnInit() {
  }

  resetPassword(): boolean {
    var email = this.model.Email;

    this.http.get(this.baseUrl + `api/account/resetPassword?emailAddress=${email}`).subscribe(
      success => {
        this.emailSent = true;
      },
      error => {
        this.emailSent = false;
        alert('Could not reset password!');
      });

    return false;
  }
}
