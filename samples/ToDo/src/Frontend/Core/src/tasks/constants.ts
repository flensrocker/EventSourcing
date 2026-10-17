import * as ToDoConstants from '../constants';

export const TaskTitleMaxLength = 100;
export const TaskDismissReasonMaxLength = 200;

export const enum TaskStatus {
  Open = 0,
  Completed = 1,
  Dismissed = 2,
}

export const isToDoTaskIdValid = (taskId: string | null | undefined): taskId is string =>
  taskId != null && typeof taskId === 'string' && taskId.length > 0;

export const TaskRoute = `${ToDoConstants.BaseRoute}/task`;
export function getToDoTaskRoute(): string;
export function getToDoTaskRoute(suffix: string): string;
export function getToDoTaskRoute(suffix: string, taskId: string): string;
export function getToDoTaskRoute(suffix?: string, taskId?: string): string {
  if (!isToDoTaskIdValid(taskId)) {
    if (suffix == null || suffix === '') {
      return TaskRoute;
    }

    return `${TaskRoute}/${suffix}`;
  }

  if (suffix == null || suffix === '') {
    return `${TaskRoute}/${encodeURIComponent(taskId)}`;
  }

  return `${TaskRoute}/${encodeURIComponent(taskId)}/${suffix}`;
}

export const TaskCommandType_Base = `${ToDoConstants.Module}_TaskCommand`;
export type TaskCommandType_Base = typeof TaskCommandType_Base;

export const TaskCommandType = {
  Complete: `${TaskCommandType_Base}_Complete`,
  Dismiss: `${TaskCommandType_Base}_Dismiss`,
  Note: `${TaskCommandType_Base}_Note`,
  Reopen: `${TaskCommandType_Base}_Reopen`,
} as const;

export const TaskEventType_Base = `${ToDoConstants.Module}_TaskEvent`;
export type TaskEventType_Base = typeof TaskEventType_Base;

export const TaskEventType = {
  Completed: `${TaskEventType_Base}_Completed`,
  Dismissed: `${TaskEventType_Base}_Dismissed`,
  Noted: `${TaskEventType_Base}_Noted`,
  Reopened: `${TaskEventType_Base}_Reopened`,
} as const;

export const TaskViewType_Base = `${ToDoConstants.Module}_TaskView`;
export type TaskViewType_Base = typeof TaskViewType_Base;

export const TaskViewType = {
  Base: `${ToDoConstants.Module}_TaskView`,
  DailyStats: `${TaskViewType_Base}_DailyStats`,
  Details: `${TaskViewType_Base}_Details`,
  ListItem: `${TaskViewType_Base}_ListItem`,
} as const;
