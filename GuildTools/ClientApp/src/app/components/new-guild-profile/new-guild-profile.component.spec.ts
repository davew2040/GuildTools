import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FindGuildComponent, NewGuildProfileComponent } from './new-guild-profile.component';

describe('FindGuildComponent', () => {
  let component: NewGuildProfileComponent;
  let fixture: ComponentFixture<NewGuildProfileComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FindGuildComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FindGuildComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
