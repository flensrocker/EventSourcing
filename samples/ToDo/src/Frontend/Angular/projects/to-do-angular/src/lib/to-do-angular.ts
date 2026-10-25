import { Component, computed, signal } from '@angular/core';

import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

import { isToDoTaskIdValid } from '@flensrocker/to-do-core';

import { ToDoTaskDetails } from './tasks/to-do-task-details';
import { ToDoTaskList } from './tasks/to-do-task-list';

@Component({
  imports: [MatFormField, MatInput, MatLabel, ToDoTaskDetails, ToDoTaskList],
  selector: 'to-do-angular',
  styles: ``,
  template: `<h2>Task-List</h2>
    <to-do-task-list />

    <h2>Task-Details</h2>
    <div>
      <mat-form-field>
        <mat-label>Task-Id</mat-label>
        <input type="text" matInput [value]="taskId()" (change)="taskId.set($event.target.value)" />
      </mat-form-field>
      @if (isValidTaskId()) {
        <to-do-task-details [taskId]="taskId()" />
      }
    </div>`,
})
export class ToDoAngular {
  protected readonly taskId = signal<string>('');

  protected readonly isValidTaskId = computed(() => isToDoTaskIdValid(this.taskId()));
}
