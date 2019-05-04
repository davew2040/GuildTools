import { Component, OnInit, Inject } from '@angular/core';
import { RegisterUserModel } from './register-user.model';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../auth/auth.service';
import { Router } from '@angular/router';
import { BusyService } from '../../../shared-services/busy-service';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';
import { NotificationService } from 'app/shared-services/notification-service';
import { MatDialog } from '@angular/material/dialog';
import { FormGroup, Validators, FormControl, FormBuilder, AbstractControl } from '@angular/forms';
import { FindPlayerDialogComponent } from 'app/dialogs/find-player-dialog.component/find-player-dialog.component';
import { SelectedPlayer } from 'app/models/selected-player';
import { RegistrationDetails } from 'app/services/ServiceTypes/service-types';
import { AccountService } from 'app/services/account-service';

@Component({
  selector: 'app-register-user',
  templateUrl: './register-user.component.html',
  styleUrls: ['./register-user.component.css']
})
export class RegisterUserComponent implements OnInit {

  public errors: string[];
  public selectedPlayer: SelectedPlayer;
  public registrationForm = this.buildForm();

  constructor(
      private http: HttpClient,
      public auth: AuthService,
      public router: Router,
      private dialog: MatDialog,
      private accountService: AccountService,
      private busyService: BusyService,
      private errorService: ErrorReportingService,
      private notificationService: NotificationService) {
    this.errors = new Array<string>();
  }

  ngOnInit() {
  }

  public get usernameControlName(): string { return 'userName'; }
  public get emailControlName(): string { return 'email'; }
  public get passwordControlName(): string { return 'password'; }
  public get passwordRepeatControlName(): string { return 'passwordRepeat'; }
  public get playerNameControlName(): string { return 'playerName'; }

  onSubmit(): void {
    this.errors = this.getSubmissionErrors();

    if (this.errors.length !== 0) {
      return;
    }

    const registrationModel = new RegistrationDetails();

    registrationModel.username = this.registrationForm.controls[this.usernameControlName].value;
    registrationModel.password = this.registrationForm.controls[this.passwordControlName].value;
    registrationModel.email = this.registrationForm.controls[this.emailControlName].value;
    registrationModel.playerName = this.selectedPlayer.name;
    registrationModel.playerRealm = this.selectedPlayer.realm;
    registrationModel.playerRegion = this.selectedPlayer.region.Name;
    registrationModel.guildName = this.selectedPlayer.guildName;
    registrationModel.guildRealm = this.selectedPlayer.guildRealm;

    this.busyService.setBusy();

    this.accountService.register(registrationModel).subscribe(
      success => {
        this.busyService.unsetBusy();
        this.notificationService.showNotification(`A confirmation email has been sent to ${registrationModel.email}.`);
      },
      error => {
        this.busyService.unsetBusy();
        this.errorService.reportApiError(error);
      });
  }

  public findCharacter(): boolean {
    const dialogRef = this.dialog.open(FindPlayerDialogComponent, {
      disableClose: true,
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }

      this.selectedPlayer = result;

      const formattedPlayer = `${this.selectedPlayer.name} - ${this.selectedPlayer.realm}`;
      this.playerNameFormControl.setValue(formattedPlayer);
      this.registrationForm.markAsPristine();
    });

    return false;
  }

  private validateEmail(email: string): boolean {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
  }

  private buildForm(): FormGroup {
    const formBuilder = new FormBuilder();

    const form = formBuilder.group({
      userName: ['', Validators.required],
      email: ['', Validators.required],
      password: ['', Validators.required],
      passwordRepeat: ['', Validators.required],
      playerName: new FormControl('', Validators.required),
    });

    form.controls[this.playerNameControlName].disable();

    return form;
  }

  private get playerNameFormControl(): AbstractControl {
    return this.registrationForm.controls[this.playerNameControlName];
  }

  private get getEmailFormControl(): AbstractControl {
    return this.registrationForm.controls[this.emailControlName];
  }

  private get getPasswordFormControl(): AbstractControl {
    return this.registrationForm.controls[this.passwordControlName];
  }

  private get getPasswordRepeatFormControl(): AbstractControl {
    return this.registrationForm.controls[this.passwordRepeatControlName];
  }

  private getSubmissionErrors(): Array<string> {
    const errors = new Array<string>();

    if (!this.validateEmail(this.registrationForm.controls[this.emailControlName].value)) {
      errors.push('Email address format is invalid.');
    }

    if (this.registrationForm.controls[this.passwordControlName].value
      !== this.registrationForm.controls[this.passwordRepeatControlName].value) {
      errors.push('Password and Password Confirmation must match.');
    }

    return errors;
  }

  private get diagnostic() { return JSON.stringify(this.registrationForm); }
}
