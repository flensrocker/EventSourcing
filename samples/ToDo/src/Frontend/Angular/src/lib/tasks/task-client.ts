import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import { Observable } from 'rxjs';

import { WithoutType } from '@flensrocker/event-sourcing-core';
import {
  createTaskCommandComplete,
  createTaskCommandDismiss,
  createTaskCommandNote,
  createTaskCommandReopen,
  getTaskCommandCompleteRoute,
  getTaskCommandDismissRoute,
  getTaskCommandNoteRoute,
  getTaskCommandReopenRoute,
  TaskCommand_Complete_Response,
  TaskCommand_Dismiss,
  TaskCommand_Dismiss_Response,
  TaskCommand_Note,
  TaskCommand_Note_Response,
  TaskCommand_Reopen_Response,
} from '@flensrocker/to-do-core';

@Service()
export class ToDoTaskClient {
  readonly #http = inject(HttpClient);

  complete(taskId: string): Observable<TaskCommand_Complete_Response> {
    return this.#http.put<TaskCommand_Complete_Response>(getTaskCommandCompleteRoute(taskId), createTaskCommandComplete());
  }

  dismiss(taskId: string, command: WithoutType<TaskCommand_Dismiss>): Observable<TaskCommand_Dismiss_Response> {
    return this.#http.put<TaskCommand_Dismiss_Response>(getTaskCommandDismissRoute(taskId), createTaskCommandDismiss(command));
  }

  note(command: WithoutType<TaskCommand_Note>): Observable<TaskCommand_Note_Response> {
    return this.#http.post<TaskCommand_Note_Response>(getTaskCommandNoteRoute(), createTaskCommandNote(command));
  }

  reopen(taskId: string): Observable<TaskCommand_Reopen_Response> {
    return this.#http.put<TaskCommand_Reopen_Response>(getTaskCommandReopenRoute(taskId), createTaskCommandReopen());
  }

  // TODO views & queries
}
