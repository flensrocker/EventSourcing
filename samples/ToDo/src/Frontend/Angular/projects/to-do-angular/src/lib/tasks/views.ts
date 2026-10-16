import { httpResource } from '@angular/common/http';
import { Signal, Injector } from '@angular/core';

import { getTaskViewDetailsRoute, TaskView_Details } from '@flensrocker/to-do-core';

export const toDoTaskViewDetails = (taskId: Signal<string | null | undefined>, injector?: Injector) =>
  httpResource<TaskView_Details>(
    () => {
      const tId = taskId();
      return tId == null ? undefined : getTaskViewDetailsRoute(tId);
    },
    // TODO use parse with validation schema
    { injector },
  );

// TODO add "stream" endpoint with Server Sent Events
