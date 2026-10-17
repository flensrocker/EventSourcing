import { httpResource } from '@angular/common/http';
import { Signal, Injector } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { of } from 'rxjs';

import { connectEventSource, eventSourceParseJson } from '@flensrocker/event-sourcing-core';
import { getTaskViewDetailsRoute, getTaskViewDetailsStreamRoute, isToDoTaskIdValid, TaskView_Details } from '@flensrocker/to-do-core';

export const toDoTaskViewDetails = (taskId: Signal<string | null | undefined>, injector?: Injector) =>
  httpResource<TaskView_Details>(
    () => {
      const tId = taskId();
      return isToDoTaskIdValid(tId) ? getTaskViewDetailsRoute(tId) : undefined;
    },
    // TODO use parse with validation schema
    { injector },
  );

export const toDoTaskViewDetailsStream = (taskId: Signal<string | null | undefined>, injector?: Injector) =>
  rxResource({
    params: taskId,
    stream: ({ params: taskId }) =>
      isToDoTaskIdValid(taskId)
        ? connectEventSource<TaskView_Details>(
            getTaskViewDetailsStreamRoute(taskId),
            { withCredentials: true },
            { parseFn: eventSourceParseJson },
          )
        : of(undefined),
    injector,
  });
