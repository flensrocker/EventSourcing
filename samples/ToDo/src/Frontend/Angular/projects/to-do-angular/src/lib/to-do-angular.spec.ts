import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToDoAngular } from './to-do-angular';

describe('ToDoAngular', () => {
  let component: ToDoAngular;
  let fixture: ComponentFixture<ToDoAngular>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToDoAngular],
    }).compileComponents();

    fixture = TestBed.createComponent(ToDoAngular);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
