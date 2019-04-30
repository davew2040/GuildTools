import { Component, Inject } from '@angular/core';
import { AuthService } from './auth/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { BusyService } from './shared-services/busy-service';
import { BusyDirective } from './directives/busy.directive'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'app';

  constructor(
      private http: HttpClient,
      @Inject('BASE_URL') private baseUrl: string,
      public auth: AuthService,
      public router: Router,
      public busyService: BusyService) {
    this.auth.appInitialization();
  }

  ngOnInit(): void {
  }
}
