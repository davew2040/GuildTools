import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GuildStatsComponent } from './guild-stats.component';

describe('GuildStatsComponent', () => {
  let component: GuildStatsComponent;
  let fixture: ComponentFixture<GuildStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GuildStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GuildStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
