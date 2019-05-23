import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { ResetPasswordWithTokenModel } from './reset-password-token.model';
import { AuthService } from '../../../auth/auth.service';
import { AccountService } from '../../../services/account-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { BusyService } from 'app/shared-services/busy-service';

@Component({
  selector: 'app-reset-password-token',
  templateUrl: './reset-password-token.component.html',
  styleUrls: ['./reset-password-token.component.css']
})
export class ResetPasswordWithTokenComponent implements OnInit {

  public model: ResetPasswordWithTokenModel;
  public errors: Array<string>;
  private userId: string;
  private token: string;
  private passwordChangedSuccessfully = false;

  constructor(
      public auth: AuthService,
      public router: Router,
      public route: ActivatedRoute,
      private accountService: AccountService,
      private errorService: ErrorReportingService,
      private busyService: BusyService) {
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
      this.errors.push('Passwords do not match!');

      return false;
    }

    this.busyService.setBusy();
    this.accountService.postPasswordResetWithToken(this.userId, this.token, this.model.Password).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.auth.logOut();
        this.passwordChangedSuccessfully = true;
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportError(error);
      });

    return false;
  }
}
