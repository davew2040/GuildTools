import { Component, OnInit, Inject } from '@angular/core';
import { RegisterUserModel } from './register-user.model';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';

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
      @Inject('BASE_URL') private baseUrl: string) {
    this.model = new RegisterUserModel();
    this.errors = new Array<string>();
  }

  onSubmit(): void {
    this.errors = this.getSubmissionErrors();

    if (this.errors.length !== 0) {
      return;
    }

    let credentials :ServiceCredentials = new ServiceCredentials();

    credentials.Username = this.model.Username;
    credentials.Password = this.model.Password;
    credentials.Email = this.model.Email;

    this.http.post(this.baseUrl + 'api/account/register', credentials).subscribe(
      success => {
        this.auth.loggedIn(success);
        this.router.navigate(['/']);
      },
      error => { console.error(error); });
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
