import { Component, OnInit, Inject } from '@angular/core';
import { LoginModel } from './login.model';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { RoutePaths } from '../data/route-paths';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';

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
      private busyService: BusyService,
      private errorService: ErrorReportingService) {
    this.model = new LoginModel();
    this.errors = [];
  }

  ngOnInit() {
  }

  onSubmit(): boolean {

    this.busyService.setBusy();
    this.auth.logIn(this.model.Email, this.model.Password).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.router.navigate(['/']);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });

    return false;
  }

  forgotPassword(): void {
    this.router.navigate(['/' + RoutePaths.ResetPassword]);
  }
}
