import { Component, Inject } from '@angular/core';
import { AuthService } from './auth/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { BlizzardService } from './blizzard-services/blizzard-services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';

  constructor(
      private http: HttpClient,
      @Inject('BASE_URL') private baseUrl: string,
      public auth: AuthService,
      public router: Router,
      public blizzardService : BlizzardService) {
    this.auth.handleAuthentication();
  }

  ngOnInit(): void {
  }
}
