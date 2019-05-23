import { Component, OnInit, Inject } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ResetPasswordModel } from './reset-password.model';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { AccountService } from 'app/services/account-service';
import { BusyService } from 'app/shared-services/busy-service';

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
      private errorService: ErrorReportingService,
      private accountService: AccountService,
      private busyService: BusyService,
      @Inject('BASE_URL')
      private baseUrl: string) {
    this.model = new ResetPasswordModel();
    this.errors = [];
  }

  ngOnInit() {
  }

  resetPassword(): boolean {
    const email = this.model.Email;

    this.busyService.setBusy();
    this.accountService.requestPasswordReset(email).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.emailSent = true;
      },
      error => {
        this.busyService.unsetBusy();
        this.emailSent = false;
        this.errorService.reportApiError(error);
      });

    return false;
  }
}
