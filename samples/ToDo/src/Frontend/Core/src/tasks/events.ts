import { isObjectOfType, WithoutType, WithType, withType } from '@flensrocker/event-sourcing-core';

import { TaskEventType } from './constants';

// TODO create validation schema for event types
export type TaskEvent_Completed = WithType<typeof TaskEventType.Completed>;
const completed = withType<TaskEvent_Completed>({}, TaskEventType.Completed);
export const createTaskEventCompleted = () => completed;

export type TaskEvent_Dismissed = WithType<typeof TaskEventType.Dismissed> & {
  readonly Reason: string;
};
export const createTaskEventDismissed = (cmd: WithoutType<TaskEvent_Dismissed>) => withType(cmd, TaskEventType.Dismissed);

export type TaskEvent_Noted = WithType<typeof TaskEventType.Noted> & {
  readonly CreatedAt: string;
  readonly Title: string;
};
export const createTaskEventNoted = (cmd: WithoutType<TaskEvent_Noted>) => withType(cmd, TaskEventType.Noted);

export type TaskEvent_Paused = WithType<typeof TaskEventType.Paused> & {
  readonly PausedAt: string;
};
export const createTaskEventPaused = (cmd: WithoutType<TaskEvent_Paused>) => withType(cmd, TaskEventType.Paused);

export type TaskEvent_Reopened = WithType<typeof TaskEventType.Reopened>;
const reopened = withType<TaskEvent_Reopened>({}, TaskEventType.Reopened);
export const createTaskEventReopened = () => reopened;

export type TaskEvent_Started = WithType<typeof TaskEventType.Started> & {
  readonly StartedAt: string;
  readonly ByUserId: string | null;
};
export const createTaskEventStarted = (cmd: WithoutType<TaskEvent_Started>) => withType(cmd, TaskEventType.Started);

export type TaskEvent =
  TaskEvent_Completed | TaskEvent_Dismissed | TaskEvent_Noted | TaskEvent_Paused | TaskEvent_Reopened | TaskEvent_Started;

export const isTaskEventCompleted = (event: TaskEvent) => isObjectOfType<TaskEvent_Completed>(event, TaskEventType.Completed);
export const isTaskEventDismissed = (event: TaskEvent) => isObjectOfType<TaskEvent_Dismissed>(event, TaskEventType.Dismissed);
export const isTaskEventNoted = (event: TaskEvent) => isObjectOfType<TaskEvent_Noted>(event, TaskEventType.Noted);
export const isTaskEventPaused = (event: TaskEvent) => isObjectOfType<TaskEvent_Paused>(event, TaskEventType.Paused);
export const isTaskEventReopened = (event: TaskEvent) => isObjectOfType<TaskEvent_Reopened>(event, TaskEventType.Reopened);
export const isTaskEventStarted = (event: TaskEvent) => isObjectOfType<TaskEvent_Started>(event, TaskEventType.Started);
