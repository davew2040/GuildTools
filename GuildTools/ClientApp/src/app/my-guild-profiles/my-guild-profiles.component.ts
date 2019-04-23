import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'
import { DataService } from '../data-services/data-services';
import { BlizzardService } from '../blizzard-services/blizzard-services';
import { GuildProfile } from '../data-services/Models/GuildProfile';
import { GuildMember } from '../data-services/Models/GuildMember';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-my-guild-profiles',
  templateUrl: './my-guild-profiles.component.html',
  styleUrls: ['./my-guild-profiles.component.css']
})
export class MyGuildProfilesComponent implements OnInit {

  constructor(public route: ActivatedRoute, public dataService: DataService, public blizzardService: BlizzardService, private authService: AuthService, private router: Router) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
    });
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }
}
