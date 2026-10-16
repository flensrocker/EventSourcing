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

export type TaskEvent_Reopened = WithType<typeof TaskEventType.Reopened>;
const reopened = withType<TaskEvent_Reopened>({}, TaskEventType.Reopened);
export const createTaskEventReopened = () => reopened;

export type TaskEvent = TaskEvent_Completed | TaskEvent_Dismissed | TaskEvent_Noted | TaskEvent_Reopened;

export const isTaskEventCompleted = (event: TaskEvent) => isObjectOfType<TaskEvent_Completed>(event, TaskEventType.Completed);
export const isTaskEventDismissed = (event: TaskEvent) => isObjectOfType<TaskEvent_Dismissed>(event, TaskEventType.Dismissed);
export const isTaskEventNoted = (event: TaskEvent) => isObjectOfType<TaskEvent_Noted>(event, TaskEventType.Noted);
export const isTaskEventReopened = (event: TaskEvent) => isObjectOfType<TaskEvent_Reopened>(event, TaskEventType.Reopened);
