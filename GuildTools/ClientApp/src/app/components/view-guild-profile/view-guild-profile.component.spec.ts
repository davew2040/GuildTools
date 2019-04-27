import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewGuildProfileComponent } from './view-guild-profile.component';

describe('ViewGuildProfileComponent', () => {
  let component: ViewGuildProfileComponent;
  let fixture: ComponentFixture<ViewGuildProfileComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewGuildProfileComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewGuildProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
