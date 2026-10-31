import * as ToDoConstants from '../constants';

export const TaskTitleMaxLength = 100;
export const TaskDismissReasonMaxLength = 200;

export const TaskStatus = {
  Opened: 0,
  Completed: 1,
  Dismissed: 2,
  Started: 3,
  Paused: 4,
} as const;
export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus];

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
  ChangeTitle: `${TaskCommandType_Base}_ChangeTitle`,
  Complete: `${TaskCommandType_Base}_Complete`,
  Dismiss: `${TaskCommandType_Base}_Dismiss`,
  Open: `${TaskCommandType_Base}_Open`,
  Pause: `${TaskCommandType_Base}_Pause`,
  Reopen: `${TaskCommandType_Base}_Reopen`,
  Start: `${TaskCommandType_Base}_Start`,
} as const;

export const TaskEventType_Base = `${ToDoConstants.Module}_TaskEvent`;
export type TaskEventType_Base = typeof TaskEventType_Base;

export const TaskEventType = {
  Completed: `${TaskEventType_Base}_Completed`,
  Dismissed: `${TaskEventType_Base}_Dismissed`,
  Opened: `${TaskEventType_Base}_Opened`,
  Paused: `${TaskEventType_Base}_Paused`,
  Reopened: `${TaskEventType_Base}_Reopened`,
  Started: `${TaskEventType_Base}_Started`,
  TitleChanged: `${TaskEventType_Base}_TitleChanged`,
} as const;

export const TaskViewType_Base = `${ToDoConstants.Module}_TaskView`;
export type TaskViewType_Base = typeof TaskViewType_Base;

export const TaskViewType = {
  Base: `${ToDoConstants.Module}_TaskView`,
  DailyStats: `${TaskViewType_Base}_DailyStats`,
  Details: `${TaskViewType_Base}_Details`,
  ListItem: `${TaskViewType_Base}_ListItem`,
} as const;
