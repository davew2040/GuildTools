import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { AccountService } from 'app/services/account-service';
import { RoutePaths } from 'app/data/route-paths';

enum ConfirmationState {
  Waiting,
  Confirmed,
  Failed
}

@Component({
  selector: 'app-confirm-registration',
  templateUrl: './confirm-registration.component.html',
  styleUrls: ['./confirm-registration.component.scss']
})
export class ConfirmRegistrationComponent implements OnInit {

  public confirmationState = ConfirmationState.Waiting;
  private userId: string;
  private token: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountService) { }

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

    this.accountService.confirmEmail(this.userId, this.token)
      .subscribe(
        success => {
          this.confirmationState = ConfirmationState.Confirmed;
        },
        error => {
          this.confirmationState = ConfirmationState.Failed;
        }
      )
  }

  public get isWaiting(): boolean {
    return this.confirmationState === ConfirmationState.Waiting;
  }

  public get isConfirmed(): boolean {
    return this.confirmationState === ConfirmationState.Confirmed;
  }

  public get isFailed(): boolean {
    return this.confirmationState === ConfirmationState.Failed;
  }

  public navigateToLogin(): void {
    this.router.navigate([RoutePaths.Login]);
  }
}
