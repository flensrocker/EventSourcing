import { type IDomainEventVersionable } from '@flensrocker/event-sourcing-core';

import { getToDoTaskRoute, TaskStatus } from './constants';

// TODO create validation schema
export type TaskView_Details = IDomainEventVersionable &
  Readonly<{
    TaskId: string;
    Title: string;
    Status: TaskStatus;
    DismissReason: string | null;
    StartedByUserId: string | null;
  }>;
export const emptyTaskViewDetails: TaskView_Details = {
  TaskId: '',
  Version: 0,
  Title: '',
  Status: TaskStatus.Open,
  DismissReason: null,
  StartedByUserId: null,
};

export const getTaskViewDetailsId = (details: TaskView_Details) => details.TaskId;
export const getTaskViewDetailsRoute = (taskId: string) => getToDoTaskRoute('', taskId);
export const getTaskViewDetailsStreamRoute = (taskId: string) => getToDoTaskRoute('stream', taskId);
