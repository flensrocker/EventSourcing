import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import { Observable, throwError } from 'rxjs';

import { WithoutType } from '@flensrocker/event-sourcing-core';
import {
  createTaskCommandComplete,
  createTaskCommandDismiss,
  createTaskCommandOpen,
  createTaskCommandReopen,
  getTaskCommandCompleteRoute,
  getTaskCommandDismissRoute,
  getTaskCommandOpenRoute,
  getTaskCommandReopenRoute,
  isToDoTaskIdValid,
  TaskCommand_Complete_Response,
  TaskCommand_Dismiss,
  TaskCommand_Dismiss_Response,
  TaskCommand_Open,
  TaskCommand_Open_Response,
  TaskCommand_Reopen_Response,
} from '@flensrocker/to-do-core';

@Service()
export class ToDoTaskClient {
  readonly #http = inject(HttpClient);

  #validate(taskId: string): Observable<never> | null {
    return isToDoTaskIdValid(taskId) ? null : throwError(() => new Error('taskId is required'));
  }

  complete(taskId: string): Observable<TaskCommand_Complete_Response> {
    return (
      this.#validate(taskId) ??
      this.#http.put<TaskCommand_Complete_Response>(getTaskCommandCompleteRoute(taskId), createTaskCommandComplete(), {
        keepalive: true,
      })
    );
  }

  dismiss(taskId: string, command: WithoutType<TaskCommand_Dismiss>): Observable<TaskCommand_Dismiss_Response> {
    return (
      this.#validate(taskId) ??
      this.#http.put<TaskCommand_Dismiss_Response>(getTaskCommandDismissRoute(taskId), createTaskCommandDismiss(command), {
        keepalive: true,
      })
    );
  }

  open(command: WithoutType<TaskCommand_Open>): Observable<TaskCommand_Open_Response> {
    return this.#http.post<TaskCommand_Open_Response>(getTaskCommandOpenRoute(), createTaskCommandOpen(command), {
      keepalive: true,
    });
  }

  reopen(taskId: string): Observable<TaskCommand_Reopen_Response> {
    return (
      this.#validate(taskId) ??
      this.#http.put<TaskCommand_Reopen_Response>(getTaskCommandReopenRoute(taskId), createTaskCommandReopen(), {
        keepalive: true,
      })
    );
  }
}
