import { CommandResult, isObjectOfType, WithoutType, withType, WithType } from '@flensrocker/event-sourcing-core';

import { getToDoTaskRoute, TaskCommandType } from './constants';

// TODO create validation schema for command types
export type TaskCommand_Complete = WithType<typeof TaskCommandType.Complete>;
export type TaskCommand_Complete_Response = CommandResult;
const complete = withType<TaskCommand_Complete>({}, TaskCommandType.Complete);
export const createTaskCommandComplete = () => complete;
export const getTaskCommandCompleteRoute = (taskId: string) => getToDoTaskRoute('complete', taskId);

export type TaskCommand_Dismiss = WithType<typeof TaskCommandType.Dismiss> & {
  readonly Reason: string;
};
export type TaskCommand_Dismiss_Response = CommandResult;
export const createTaskCommandDismiss = (cmd: WithoutType<TaskCommand_Dismiss>) => withType(cmd, TaskCommandType.Dismiss);
export const getTaskCommandDismissRoute = (taskId: string) => getToDoTaskRoute('dismiss', taskId);

export type TaskCommand_Note = WithType<typeof TaskCommandType.Note> & {
  readonly Title: string;
};
export type TaskCommand_Note_Response = CommandResult & Readonly<{ TaskId: string }>;
export const createTaskCommandNote = (cmd: WithoutType<TaskCommand_Note>) => withType(cmd, TaskCommandType.Note);
export const getTaskCommandNoteRoute = () => getToDoTaskRoute('note');

export type TaskCommand_Pause = WithType<typeof TaskCommandType.Pause> & {
  readonly PausedAt: string | null;
};
export type TaskCommand_Pause_Response = CommandResult;
export const createTaskCommandPause = (cmd: WithoutType<TaskCommand_Pause>) => withType(cmd, TaskCommandType.Pause);
export const getTaskCommandPauseRoute = (taskId: string) => getToDoTaskRoute('pause', taskId);

export type TaskCommand_Reopen = WithType<typeof TaskCommandType.Reopen>;
export type TaskCommand_Reopen_Response = CommandResult;
const reopen = withType<TaskCommand_Reopen>({}, TaskCommandType.Reopen);
export const createTaskCommandReopen = () => reopen;
export const getTaskCommandReopenRoute = (taskId: string) => getToDoTaskRoute('reopen', taskId);

export type TaskCommand_Start = WithType<typeof TaskCommandType.Start> & {
  readonly StartedAt: string | null;
  readonly ByUserId: string | null;
};
export type TaskCommand_Start_Response = CommandResult;
export const createTaskCommandStart = (cmd: WithoutType<TaskCommand_Start>) => withType(cmd, TaskCommandType.Start);
export const getTaskCommandStartRoute = (taskId: string) => getToDoTaskRoute('Start', taskId);

export type TaskCommand =
  TaskCommand_Complete | TaskCommand_Dismiss | TaskCommand_Note | TaskCommand_Pause | TaskCommand_Reopen | TaskCommand_Start;

export const isTaskCommandComplete = (event: TaskCommand) => isObjectOfType<TaskCommand_Complete>(event, TaskCommandType.Complete);
export const isTaskCommandDismiss = (event: TaskCommand) => isObjectOfType<TaskCommand_Dismiss>(event, TaskCommandType.Dismiss);
export const isTaskCommandNote = (event: TaskCommand) => isObjectOfType<TaskCommand_Note>(event, TaskCommandType.Note);
export const isTaskCommandPause = (event: TaskCommand) => isObjectOfType<TaskCommand_Pause>(event, TaskCommandType.Pause);
export const isTaskCommandReopen = (event: TaskCommand) => isObjectOfType<TaskCommand_Reopen>(event, TaskCommandType.Reopen);
export const isTaskCommandStart = (event: TaskCommand) => isObjectOfType<TaskCommand_Start>(event, TaskCommandType.Start);
