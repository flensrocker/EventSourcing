import { CommandResult, isObjectOfType, WithoutType, withType, WithType } from '@flensrocker/event-sourcing-core';

import { TaskCommandType, TaskRoute } from './constants';

const getTaskCommandRoute = (taskId: string, command: string) => `${TaskRoute}/${encodeURIComponent(taskId)}/${command}`;

export type TaskCommand_Complete = WithType<typeof TaskCommandType.Complete>;
export type TaskCommand_Complete_Response = CommandResult;
const complete = withType<TaskCommand_Complete>({}, TaskCommandType.Complete);
export const createTaskCommandComplete = () => complete;
export const getTaskCommandCompleteRoute = (taskId: string) => getTaskCommandRoute(taskId, 'complete');

export type TaskCommand_Dismiss = WithType<typeof TaskCommandType.Dismiss> & {
  readonly Reason: string;
};
export type TaskCommand_Dismiss_Response = CommandResult;
export const createTaskCommandDismiss = (cmd: WithoutType<TaskCommand_Dismiss>) => withType(cmd, TaskCommandType.Dismiss);
export const getTaskCommandDismissRoute = (taskId: string) => getTaskCommandRoute(taskId, 'dismiss');

export type TaskCommand_Note = WithType<typeof TaskCommandType.Note> & {
  readonly Title: string;
};
export type TaskCommand_Note_Response = CommandResult & Readonly<{ TaskId: string }>;
export const createTaskCommandNote = (cmd: WithoutType<TaskCommand_Note>) => withType(cmd, TaskCommandType.Note);
export const getTaskCommandNoteRoute = () => `${TaskRoute}/note`;

export type TaskCommand_Reopen = WithType<typeof TaskCommandType.Reopen>;
export type TaskCommand_Reopen_Response = CommandResult;
const reopen = withType<TaskCommand_Reopen>({}, TaskCommandType.Reopen);
export const createTaskCommandReopen = () => reopen;
export const getTaskCommandReopenRoute = (taskId: string) => getTaskCommandRoute(taskId, 'reopen');

export type TaskCommand = TaskCommand_Complete | TaskCommand_Dismiss | TaskCommand_Note | TaskCommand_Reopen;

export const isTaskCommandComplete = (event: TaskCommand) => isObjectOfType<TaskCommand_Complete>(event, TaskCommandType.Complete);
export const isTaskCommandDismiss = (event: TaskCommand) => isObjectOfType<TaskCommand_Dismiss>(event, TaskCommandType.Dismiss);
export const isTaskCommandNote = (event: TaskCommand) => isObjectOfType<TaskCommand_Note>(event, TaskCommandType.Note);
export const isTaskCommandReopen = (event: TaskCommand) => isObjectOfType<TaskCommand_Reopen>(event, TaskCommandType.Reopen);
