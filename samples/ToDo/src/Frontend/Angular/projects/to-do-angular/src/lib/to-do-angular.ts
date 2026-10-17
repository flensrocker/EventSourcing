import { JsonPipe } from '@angular/common';
import { Component, signal } from '@angular/core';

import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatProgressBar } from '@angular/material/progress-bar';

import { toDoTaskViewDetailsStream } from './tasks/views';

@Component({
  imports: [JsonPipe, MatFormField, MatInput, MatLabel, MatProgressBar],
  selector: 'to-do-angular',
  styles: ``,
  template: `<mat-form-field>
      <mat-label>Task-Id</mat-label>
      <input type="text" matInput [value]="taskId()" (input)="taskId.set($event.data)" />
    </mat-form-field>
    @if (taskDetails.isLoading()) {
      <mat-progress-bar mode="indeterminate" />
    } @else {
      @let details = taskDetails.value();
      @if (details) {
        <div>{{ details | json }}</div>
      }
    }`,
})
export class ToDoAngular {
  protected readonly taskId = signal<string | null>('');

  protected readonly taskDetails = toDoTaskViewDetailsStream(this.taskId);
}
