import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NewProfilePageComponent } from './new-profile-page.component';

describe('NewProfilePageComponent', () => {
  let component: NewProfilePageComponent;
  let fixture: ComponentFixture<NewProfilePageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NewProfilePageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewProfilePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
