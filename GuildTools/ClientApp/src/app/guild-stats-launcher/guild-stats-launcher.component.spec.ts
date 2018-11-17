import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GuildStatsLauncherComponent } from './guild-stats-launcher.component';

describe('GuildStatsLauncherComponent', () => {
  let component: GuildStatsLauncherComponent;
  let fixture: ComponentFixture<GuildStatsLauncherComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GuildStatsLauncherComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GuildStatsLauncherComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
