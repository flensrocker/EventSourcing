import { Component, computed, inject, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { busyTracker, commandHandler, versionTracker } from '@flensrocker/event-sourcing-angular';
import { getEventSourceValue } from '@flensrocker/event-sourcing-core';
import { TaskStatus, emptyTaskViewDetails, getTaskViewDetailsId } from '@flensrocker/to-do-core';

import { ToDoTaskClient } from './client';
import { toDoTaskViewDetailsStream } from './views';

@Component({
  selector: 'to-do-task-details',
  imports: [MatButtonModule, MatCardModule, MatProgressBarModule],
  styles: `
    .error {
      color: var(--mat-sys-error);
    }
  `,
  template: `<mat-card>
    <mat-card-header>
      <mat-card-title>{{ title() }}</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      @if (isLoading()) {
        <mat-progress-bar mode="indeterminate" />
      } @else if (detailsError(); as errMsg) {
        <div class="error">ERROR: {{ errMsg }}</div>
      } @else if (isOpened()) {
        opened
      } @else if (isStarted()) {
        started by TODO
      } @else if (isPaused()) {
        paused
      } @else if (isCompleted()) {
        completed
      } @else if (isDismissed()) {
        dismissed with reason &quot;{{ dismissReason() }}&quot;
      }
    </mat-card-content>
    <mat-card-actions>
      <button type="button" matButton [disabled]="start.disabled()" (click)="start.submit()">Start</button>
      <button type="button" matButton [disabled]="pause.disabled()" (click)="pause.submit()">Pause</button>
      <button type="button" matButton [disabled]="complete.disabled()" (click)="complete.submit()">Complete</button>
      <button type="button" matButton [disabled]="dismiss.disabled()" (click)="dismiss.submit()">Dismiss</button>
      <button type="button" matButton [disabled]="reopen.disabled()" (click)="reopen.submit()">Reopen</button>
    </mat-card-actions>
  </mat-card>`,
})
export class ToDoTaskDetails {
  readonly #client = inject(ToDoTaskClient);

  readonly taskId = input.required<string>();

  readonly #detailsStream = toDoTaskViewDetailsStream(this.taskId);
  readonly #details = computed(() => {
    if (!this.#detailsStream.hasValue()) {
      return emptyTaskViewDetails;
    }

    const data = this.#detailsStream.value();
    const details = data == null ? emptyTaskViewDetails : getEventSourceValue(data);
    return details ?? emptyTaskViewDetails;
  });
  protected readonly detailsError = computed(() => {
    const error = this.#detailsStream.error();
    return error?.message;
  });

  readonly #busyTracker = busyTracker();
  readonly #versionTracker = versionTracker(this.taskId, this.#details, getTaskViewDetailsId);

  protected readonly isLoading = computed(
    () => this.#detailsStream.isLoading() || this.#busyTracker.isBusy() || this.#versionTracker.updateExpected(),
  );

  protected readonly title = computed(() => this.#details().Title);
  protected readonly status = computed(() => this.#details().Status);
  protected readonly dismissReason = computed(() => this.#details().DismissReason ?? '');

  protected readonly isOpened = computed(() => this.status() === TaskStatus.Opened);
  protected readonly isStarted = computed(() => this.status() === TaskStatus.Started);
  protected readonly isPaused = computed(() => this.status() === TaskStatus.Paused);
  protected readonly isCompleted = computed(() => this.status() === TaskStatus.Completed);
  protected readonly isDismissed = computed(() => this.status() === TaskStatus.Dismissed);

  protected readonly start = commandHandler({
    // TODO show dialog to select user
    createCommand: () => [this.taskId(), this.#client.start(this.taskId(), { StartedAt: null, ByUserId: null })],
    disabledWhen: () =>
      this.isLoading() || !this.#detailsStream.hasValue() || (this.status() !== TaskStatus.Opened && this.status() !== TaskStatus.Paused),
    busyTracker: this.#busyTracker,
    versionTracker: this.#versionTracker,
  });
  protected readonly pause = commandHandler({
    createCommand: () => [this.taskId(), this.#client.pause(this.taskId(), { PausedAt: null })],
    disabledWhen: () => this.isLoading() || !this.#detailsStream.hasValue() || this.status() !== TaskStatus.Started,
    busyTracker: this.#busyTracker,
    versionTracker: this.#versionTracker,
  });
  protected readonly complete = commandHandler({
    createCommand: () => [this.taskId(), this.#client.complete(this.taskId())],
    disabledWhen: () =>
      this.isLoading() ||
      !this.#detailsStream.hasValue() ||
      (this.status() !== TaskStatus.Opened && this.status() !== TaskStatus.Started && this.status() !== TaskStatus.Paused),
    busyTracker: this.#busyTracker,
    versionTracker: this.#versionTracker,
  });
  protected readonly dismiss = commandHandler({
    // TODO show dialog to enter reason
    createCommand: () => [this.taskId(), this.#client.dismiss(this.taskId(), { Reason: "I don't want to!" })],
    disabledWhen: () =>
      this.isLoading() ||
      !this.#detailsStream.hasValue() ||
      (this.status() !== TaskStatus.Opened && this.status() !== TaskStatus.Started && this.status() !== TaskStatus.Paused),
    busyTracker: this.#busyTracker,
    versionTracker: this.#versionTracker,
  });
  protected readonly reopen = commandHandler({
    createCommand: () => [this.taskId(), this.#client.reopen(this.taskId())],
    disabledWhen: () =>
      this.isLoading() ||
      !this.#detailsStream.hasValue() ||
      this.status() === TaskStatus.Opened ||
      this.status() === TaskStatus.Started ||
      this.status() === TaskStatus.Paused,
    busyTracker: this.#busyTracker,
    versionTracker: this.#versionTracker,
  });
}
