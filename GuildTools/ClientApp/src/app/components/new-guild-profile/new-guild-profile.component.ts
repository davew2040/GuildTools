import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { BlizzardRegionDefinition, BlizzardRealms } from '../../data/blizzard-realms';
import { BusyService } from '../../shared-services/busy-service';
import { DataService } from '../../services/data-services';
import { Realm } from '../../services/ServiceTypes/service-types';
import { FormControl, FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Observable, Subject, combineLatest } from 'rxjs';
import { startWith, map, filter, tap } from 'rxjs/operators';
import { SelectedGuild } from '../../models/selected-guild';
import { MatDialog } from '@angular/material';
import { FindGuildDialogComponent } from '../../dialogs/find-guild-dialog.component/find-guild-dialog.component';
import { ErrorReportingService } from 'app/shared-services/error-reporting-service';

@Component({
  selector: 'app-new-guild-profile',
  templateUrl: './new-guild-profile.component.html',
  styleUrls: ['./new-guild-profile.component.css']
})
export class NewGuildProfileComponent implements OnInit {

  @Output() created = new EventEmitter();

  public newProfileForm = this.buildForm();
  public selectedGuild: SelectedGuild;
  public errors: Array<string> = [];

  constructor(
    public busyService: BusyService,
    private dataService: DataService,
    private dialog: MatDialog,
    private errorService: ErrorReportingService) {

  }

  ngOnInit() {
  }

  public get guildDisplay(): string {
    if (!this.guildFormControl.value) {
      return '';
    }
    else {
      return this.guildFormControl.value;
    }
  }

  public findGuild(): boolean {
    const dialogRef = this.dialog.open(FindGuildDialogComponent, {
      disableClose: true,
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }

      this.selectedGuild = result;

      const formattedGuild = `${this.selectedGuild.name} - ${this.selectedGuild.realm}`;
      this.guildFormControl.setValue(formattedGuild);
      this.newProfileForm.markAsPristine();
    });

    return false;
  }

  public get canSubmit(): boolean {
    if (!this.newProfileForm || !this.newProfileForm.controls) {
      return false;
    }

    return this.newProfileForm.valid && this.guildFormControl.value;
  }

  public createProfile(): void {
    this.errors = [];

    if (this.newProfileForm.errors) {
      this.errors.concat(Object.keys(this.newProfileForm.errors).map(k => this.newProfileForm.errors[k]));
    }

    if (!this.selectedGuild) {
      this.errors.push('Must have a selected guild.');
    }

    if (this.errors.length > 0) {
      return;
    }

    this.busyService.setBusy();

    this.dataService.createNewGuildProfile(
        this.newProfileForm.controls['profileName'].value,
        this.selectedGuild.name,
        this.selectedGuild.realm,
        this.selectedGuild.region)
      .subscribe(
        success => {
          this.created.emit();
          this.busyService.unsetBusy();
        },
        error => {
          this.busyService.unsetBusy();
          this.errorService.reportApiError(error);
        });
  }

  private buildForm(): FormGroup {
    const formBuilder = new FormBuilder();

    const form = formBuilder.group({
      profileName: ['', Validators.required],
      guildName: new FormControl('', Validators.required)
    });

    form.controls['guildName'].disable();

    return form;
  }

  private get guildFormControl(): AbstractControl {
    if (!this.newProfileForm.controls) {
      return null;
    }

    return this.newProfileForm.controls['guildName'];
  }
}
