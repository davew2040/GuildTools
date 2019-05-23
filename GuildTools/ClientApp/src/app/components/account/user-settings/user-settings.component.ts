import { Component, OnInit, Inject } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';
import { Router } from '@angular/router';
import { BusyService } from 'app/shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { StoredValuesService } from 'app/shared-services/stored-values';
import { AccountService } from 'app/services/account-service';
import { NotificationService } from 'app/shared-services/notification-service';

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.css']
})
export class UserSettingsComponent implements OnInit {

  public settingsForm: FormGroup;
  public passwordForm: FormGroup;

  public get usernameControlName(): string { return 'username'; }
  public get emailControlName(): string { return 'email'; }

  public get oldPasswordControlName(): string { return 'oldPassword'; }
  public get newPasswordControlName(): string { return 'newPassword'; }
  public get retypeNewPasswordControlName(): string { return 'retypeNewPassword'; }

  settingsErrors: Array<string>;
  passwordErrors: Array<string>;

  constructor(
      public router: Router,
      private auth: AuthService,
      private valuesService: StoredValuesService,
      private accountService: AccountService,
      private busyService: BusyService,
      private notificationService: NotificationService,
      private errorService: ErrorReportingService) {
    this.settingsErrors = [];
    this.passwordErrors = [];

    if (!this.auth.isAuthenticated) {
      this.errorService.reportError('You must be logged in to access this functionality.');
      return;
    }

    this.settingsForm = this.buildSettingsForm();
    this.passwordForm = this.buildPasswordForm();
  }

  ngOnInit() {
  }

  public updateDetails() {
    this.settingsErrors = [];

    if (!this.settingsForm.valid) {
      return;
    }

    const username = this.settingsForm.controls[this.usernameControlName].value;

    this.busyService.setBusy();
    this.accountService.updateDetails(username).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.notificationService.showNotification('User details successfully changed.');
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  public changePassword() {
    this.passwordErrors = [];

    if (!this.passwordForm.valid) {
      return;
    }

    const oldPassword = this.passwordForm.controls[this.oldPasswordControlName].value;
    const newPassword = this.passwordForm.controls[this.newPasswordControlName].value;
    const retypeNewPassword = this.passwordForm.controls[this.retypeNewPasswordControlName].value;

    if (newPassword !== retypeNewPassword) {
      this.passwordErrors.push('New password entries must match.');
      return;
    }

    this.busyService.setBusy();
    this.accountService.changePassword(oldPassword, newPassword).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.notificationService.showNotification('Password successfully changed.');
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      }
    );
  }

  private buildSettingsForm(): FormGroup {
    const formBuilder = new FormBuilder();

    const form = formBuilder.group({
      username: ['', Validators.required],
      email: ['', Validators.required]
    });

    form.controls[this.emailControlName].disable();

    const userDetails = this.valuesService.userDetails;

    form.controls[this.emailControlName].setValue(userDetails.email);
    form.controls[this.usernameControlName].setValue(userDetails.username);

    return form;
  }

  private buildPasswordForm(): FormGroup {
    const formBuilder = new FormBuilder();

    const form = formBuilder.group({
      oldPassword: ['', Validators.required],
      newPassword: ['', Validators.required],
      retypeNewPassword: ['', Validators.required]
    });

    return form;
  }
}
