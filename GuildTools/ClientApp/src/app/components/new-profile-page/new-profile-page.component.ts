import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutePaths } from 'app/data/route-paths';

@Component({
  selector: 'app-new-profile-page',
  templateUrl: './new-profile-page.component.html',
  styleUrls: ['./new-profile-page.component.css']
})
export class NewProfilePageComponent implements OnInit {
  constructor(private router: Router) { }

  ngOnInit() {
  }

  public profileCreated(profileId): void{
    this.router.navigate([`/${RoutePaths.ViewProfile}/${profileId}`]);
  }
}
