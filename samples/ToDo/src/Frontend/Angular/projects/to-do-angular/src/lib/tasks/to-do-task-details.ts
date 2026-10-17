import { Component, computed, inject, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { finalize } from 'rxjs';

import { busyTracker, versionTracker } from '@flensrocker/event-sourcing-angular';
import { getEventSourceValue } from '@flensrocker/event-sourcing-core';
import { TaskStatus, emptyTaskViewDetails } from '@flensrocker/to-do-core';

import { ToDoTaskClient } from './client';
import { toDoTaskViewDetailsStream } from './views';

@Component({
  selector: 'to-do-task-details',
  imports: [MatButtonModule, MatCardModule, MatProgressBarModule],
  template: `<mat-card>
    <mat-card-header>
      <mat-card-title>{{ title() }}</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      @if (isLoading()) {
        <mat-progress-bar mode="indeterminate" />
      } @else if (isOpen()) {
        open
      } @else if (isCompleted()) {
        completed
      } @else if (isDismissed()) {
        dismissed with reason &quot;{{ dismissReason() }}&quot;
      }
    </mat-card-content>
    <mat-card-actions>
      <button type="button" matButton [disabled]="completeDisabled()" (click)="complete()">Complete</button>
      <button type="button" matButton [disabled]="dismissDisabled()" (click)="dismiss()">Dismiss</button>
      <button type="button" matButton [disabled]="reopenDisabled()" (click)="reopen()">Reopen</button>
    </mat-card-actions>
  </mat-card>`,
})
export class ToDoTaskDetails {
  readonly #client = inject(ToDoTaskClient);

  readonly taskId = input.required<string>();

  readonly #detailsStream = toDoTaskViewDetailsStream(this.taskId);
  readonly #details = computed(() => {
    const data = this.#detailsStream.value();
    const details = data == null ? emptyTaskViewDetails : getEventSourceValue(data);
    return details ?? emptyTaskViewDetails;
  });

  readonly #versionTracker = versionTracker(this.taskId, this.#details);
  readonly #busyTracker = busyTracker();

  protected readonly isLoading = computed(
    () => this.#detailsStream.isLoading() || this.#busyTracker.isBusy() || this.#versionTracker.updateExpected(),
  );

  protected readonly title = computed(() => this.#details().Title);
  protected readonly status = computed(() => this.#details().Status);
  protected readonly dismissReason = computed(() => this.#details().DismissReason ?? '');

  protected readonly isOpen = computed(() => this.status() === TaskStatus.Open);
  protected readonly isCompleted = computed(() => this.status() === TaskStatus.Completed);
  protected readonly isDismissed = computed(() => this.status() === TaskStatus.Dismissed);

  protected readonly completeDisabled = computed(() => this.isLoading() || this.status() !== TaskStatus.Open);
  protected readonly dismissDisabled = computed(() => this.isLoading() || this.status() !== TaskStatus.Open);
  protected readonly reopenDisabled = computed(() => this.isLoading() || this.status() === TaskStatus.Open);

  protected complete() {
    const taskId = this.taskId();

    // TODO error handling
    const stopSubmit = this.#busyTracker.start();
    this.#client
      .complete(taskId)
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }

  protected dismiss() {
    const taskId = this.taskId();

    // TODO show dialog to enter reason
    const Reason = "I don't want to";

    // TODO error handling
    const stopSubmit = this.#busyTracker.start();
    this.#client
      .dismiss(taskId, { Reason })
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }

  protected reopen() {
    const taskId = this.taskId();

    // TODO error handling
    const stopSubmit = this.#busyTracker.start();
    this.#client
      .reopen(taskId)
      .pipe(finalize(stopSubmit))
      .subscribe({
        next: (response) => {
          this.#versionTracker.setExpectedVersion(taskId, response.Version);
        },
      });
  }
}
