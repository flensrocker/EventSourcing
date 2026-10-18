import { Component, computed, signal } from '@angular/core';

import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

import { ToDoTaskDetails } from './tasks/to-do-task-details';
import { isToDoTaskIdValid } from '@flensrocker/to-do-core';

@Component({
  imports: [MatFormField, MatInput, MatLabel, ToDoTaskDetails],
  selector: 'to-do-angular',
  styles: ``,
  template: `<mat-form-field>
      <mat-label>Task-Id</mat-label>
      <input type="text" matInput [value]="taskId()" (change)="taskId.set($event.target.value)" />
    </mat-form-field>
    @if (isValidTaskId()) {
      <to-do-task-details [taskId]="taskId()" />
    }`,
})
export class ToDoAngular {
  protected readonly taskId = signal<string>('');

  protected readonly isValidTaskId = computed(() => isToDoTaskIdValid(this.taskId()));
}
