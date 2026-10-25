import { Component, computed } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';

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
  template: `<mat-list>
      @if (isLoading()) {
        <mat-progress-bar mode="indeterminate" />
      } @else if (listError(); as errMsg) {
        <mat-list-item class="error">ERROR: {{ errMsg }}</mat-list-item>
      } @else {
        @for (item of itemList(); track item.TaskId) {
          <to-do-task-list-item [item]="item" />
        } @empty {
          <mat-list-item>No tasks found.</mat-list-item>
        }
      }
    </mat-list>
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

  protected reload() {
    this.#listQuery.reload();
  }
}
