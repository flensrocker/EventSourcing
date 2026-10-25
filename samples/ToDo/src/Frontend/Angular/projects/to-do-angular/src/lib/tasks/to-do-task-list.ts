import { Component, computed, viewChild } from '@angular/core';
import { outputFromObservable, outputToObservable, toObservable } from '@angular/core/rxjs-interop';

import { MatButtonModule } from '@angular/material/button';
import { MatListModule, MatSelectionList } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { map, of, switchMap } from 'rxjs';

import { TaskQueryRequest_List } from '@flensrocker/to-do-core';

import { toDoTaskQueryList } from './queries';
import { ToDoTaskListItem } from './to-do-task-list-item';

@Component({
  selector: 'to-do-task-list',
  imports: [MatButtonModule, MatListModule, MatProgressBarModule, ToDoTaskListItem],
  styles: `
    .error {
      color: var(--mat-sys-error);
    }
  `,
  template: `@if (isLoading()) {
      <mat-progress-bar mode="indeterminate" />
    } @else if (listError(); as errMsg) {
      <div class="error">ERROR: {{ errMsg }}</div>
    } @else if (itemList().length === 0) {
      <div>No tasks found.</div>
    } @else {
      <mat-selection-list #tasks [multiple]="false">
        @for (item of itemList(); track item.TaskId) {
          <mat-list-option [value]="item" togglePosition="before">
            <to-do-task-list-item [item]="item" />
          </mat-list-option>
        }
      </mat-selection-list>
    }
    <div>
      <button type="button" matButton [disabled]="isLoading()" (click)="reload()">Reload</button>
    </div>`,
})
export class ToDoTaskList {
  readonly #listRequest = computed((): TaskQueryRequest_List => ({ TenantId: null }));
  readonly #listQuery = toDoTaskQueryList(this.#listRequest);

  protected readonly isLoading = computed(() => this.#listQuery.isLoading());
  protected readonly listError = computed(() => {
    const error = this.#listQuery.error();
    return error?.message;
  });
  protected readonly itemList = computed(() => (this.#listQuery.hasValue() ? this.#listQuery.value().ItemList : []));

  protected readonly selectionList = viewChild<MatSelectionList>('tasks');
  readonly selectedTaskIds = outputFromObservable(
    toObservable(this.selectionList).pipe(
      switchMap((list) =>
        list == null
          ? of([])
          : outputToObservable(list.selectionChange).pipe(map((_) => list.selectedOptions.selected.map((options) => options.value.TaskId))),
      ),
    ),
  );

  protected reload() {
    this.#listQuery.reload();
  }
}
