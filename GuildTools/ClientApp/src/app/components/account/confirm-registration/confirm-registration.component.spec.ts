import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmRegistrationComponent } from './confirm-registration.component';

describe('ConfirmRegistrationComponent', () => {
  let component: ConfirmRegistrationComponent;
  let fixture: ComponentFixture<ConfirmRegistrationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConfirmRegistrationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
