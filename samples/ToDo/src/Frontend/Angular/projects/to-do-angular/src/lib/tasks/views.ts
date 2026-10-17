import { httpResource } from '@angular/common/http';
import { Signal, Injector } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { connectEventSource, eventSourceParseJson } from '@flensrocker/event-sourcing-core';

import { getTaskViewDetailsRoute, getTaskViewDetailsStreamRoute, TaskView_Details } from '@flensrocker/to-do-core';
import { of } from 'rxjs';

export const toDoTaskViewDetails = (taskId: Signal<string | null | undefined>, injector?: Injector) =>
  httpResource<TaskView_Details>(
    () => {
      const tId = taskId();
      return tId == null ? undefined : getTaskViewDetailsRoute(tId);
    },
    // TODO use parse with validation schema
    { injector },
  );

export const toDoTaskViewDetailsStream = (taskId: Signal<string | null | undefined>, injector?: Injector) =>
  rxResource({
    params: taskId,
    stream: ({ params: taskId }) =>
      taskId == null
        ? of(undefined)
        : connectEventSource<TaskView_Details>(
            getTaskViewDetailsStreamRoute(taskId),
            { withCredentials: true },
            { parseFn: eventSourceParseJson },
          ),
    injector,
  });
