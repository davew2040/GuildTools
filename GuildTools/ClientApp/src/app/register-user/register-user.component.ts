import { Component, OnInit, Inject } from '@angular/core';
import { RegisterUserModel } from './register-user.model';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { AccountService, RegistrationModel } from '../services/account-service';
import { BusyService } from '../shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';

@Component({
  selector: 'app-register-user',
  templateUrl: './register-user.component.html',
  styleUrls: ['./register-user.component.css']
})
export class RegisterUserComponent implements OnInit {

  public model: RegisterUserModel;
  public errors: string[];

  constructor(
      private http: HttpClient,
      public auth: AuthService,
      public router: Router,
      private accountService: AccountService,
      private busyService: BusyService,
      private errorService: ErrorReportingService) {
    this.model = new RegisterUserModel();
    this.errors = new Array<string>();
  }

  onSubmit(): void {
    this.errors = this.getSubmissionErrors();

    if (this.errors.length !== 0) {
      return;
    }

    const credentials = new RegistrationModel();

    credentials.username = this.model.Username;
    credentials.password = this.model.Password;
    credentials.email = this.model.Email;

    this.busyService.setBusy();

    this.accountService.register(credentials).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.auth.processLogin(success);
        this.router.navigate(['/']);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  validateEmail(email: string): boolean {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
  }

  ngOnInit() {
  }

  private getSubmissionErrors() : Array<string> {
    let errors = new Array<string>();

    if (!this.validateEmail(this.model.Email)) {
      errors.push("Email address format is invalid.");
    }

    if (this.model.Password !== this.model.VerifyPassword) {
      errors.push("Password and Password Confirmation must match.");
    }

    return errors;
  }

  get diagnostic() { return JSON.stringify(this.model); }
}

class ServiceCredentials {
  Username: string;
  Password: string;
  Email: string;
}
