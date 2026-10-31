import { Component, viewChild } from '@angular/core';
import { outputToObservable, toObservable, toSignal } from '@angular/core/rxjs-interop';

import { switchMap } from 'rxjs';

import { ToDoTaskViewDetails } from './tasks/view-details';
import { ToDoTaskViewList } from './tasks/view-list';

@Component({
  imports: [ToDoTaskViewDetails, ToDoTaskViewList],
  selector: 'to-do-angular',
  styles: ``,
  template: `<h2>Task-List</h2>
    <to-do-task-view-list #taskList />

    <h2>Task-Details</h2>

    @for (taskId of selectedTaskIds(); track $index) {
      <to-do-task-view-details [taskId]="taskId" />
    }`,
})
export class ToDoAngular {
  protected readonly taskList = viewChild.required<ToDoTaskViewList>('taskList');
  protected readonly selectedTaskIds = toSignal(
    toObservable(this.taskList).pipe(switchMap((taskList) => outputToObservable(taskList.selectedTaskIds))),
    { initialValue: [] },
  );
}
