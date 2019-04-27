import { Component, OnInit } from '@angular/core';
import { merge } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-view-guild-profile',
  templateUrl: './view-guild-profile.component.html',
  styleUrls: ['./view-guild-profile.component.css']
})
export class ViewGuildProfileComponent implements OnInit {

  constructor(private route: ActivatedRoute) { }

  ngOnInit() {

    merge(this.route.params, this.route.queryParams).subscribe(params => {
      this.updateParams(params);
    });
  }

  private updateParams(params: any): void {
    let x = 42;
  }

}
