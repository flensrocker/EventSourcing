import { type IDomainEventVersionable } from '@flensrocker/event-sourcing-core';
import { getToDoTaskRoute, TaskStatus } from './constants';

// TODO create validation schema
export type TaskView_Details = IDomainEventVersionable &
  Readonly<{
    Title: string;
    Status: TaskStatus;
    DismissReason: string | null;
  }>;
export const getTaskViewDetailsRoute = (taskId: string) => getToDoTaskRoute('', taskId);
export const getTaskViewDetailsStreamRoute = (taskId: string) => getToDoTaskRoute('stream', taskId);
