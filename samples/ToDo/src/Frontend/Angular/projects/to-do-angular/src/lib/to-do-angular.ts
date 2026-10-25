import { Component, viewChild } from '@angular/core';
import { outputToObservable, toObservable, toSignal } from '@angular/core/rxjs-interop';

import { switchMap } from 'rxjs';

import { ToDoTaskDetails } from './tasks/to-do-task-details';
import { ToDoTaskList } from './tasks/to-do-task-list';

@Component({
  imports: [ToDoTaskDetails, ToDoTaskList],
  selector: 'to-do-angular',
  styles: ``,
  template: `<h2>Task-List</h2>
    <to-do-task-list #taskList />

    <h2>Task-Details</h2>

    @for (taskId of selectedTaskIds(); track $index) {
      <to-do-task-details [taskId]="taskId" />
    }`,
})
export class ToDoAngular {
  protected readonly taskList = viewChild.required<ToDoTaskList>('taskList');
  protected readonly selectedTaskIds = toSignal(
    toObservable(this.taskList).pipe(switchMap((taskList) => outputToObservable(taskList.selectedTaskIds))),
    { initialValue: [] },
  );
}
