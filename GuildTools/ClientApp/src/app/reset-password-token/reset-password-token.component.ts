import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { ResetPasswordWithTokenModel } from './reset-password-token.model';
import { AuthService } from '../auth/auth.service';
import { AccountService } from '../services/account-service';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-reset-password-token',
  templateUrl: './reset-password-token.component.html',
  styleUrls: ['./reset-password-token.component.css']
})
export class ResetPasswordWithTokenComponent implements OnInit {

  private model: ResetPasswordWithTokenModel;
  private errors: Array<string>;
  private userId: string;
  private token: string;
  private passwordChangedSuccessfully = false;

  constructor(
      public auth: AuthService,
      public router: Router,
      public route: ActivatedRoute,
      private accountService: AccountService) {
    this.model = new ResetPasswordWithTokenModel();
    this.errors = [];
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params: ParamMap) => {
      this.userId = params['userId'];
      this.token = params['token'];
    });

    this.route.paramMap.subscribe((params: ParamMap) => {
      const userId = params.get('userId');
      if (userId) {
        this.userId = userId;
      }

      const token = params.get('token');
      if (token) {
        this.token = token;
      }
    });
  }

  resetPassword(): boolean {
    this.errors = [];

    this.model.Password = this.model.Password.trim();
    this.model.VerifyPassword = this.model.VerifyPassword.trim();

    if (this.model.Password !== this.model.VerifyPassword) {
      this.errors.push("Passwords do not match!");

      return false;
    }

    this.accountService.postPasswordReset(this.userId, this.token, this.model.Password).subscribe(
      success => {
        this.auth.logOut();
        this.passwordChangedSuccessfully = true;
      },
      error => {
        this.errors = error.error;
      });

    return false;
  }
}
