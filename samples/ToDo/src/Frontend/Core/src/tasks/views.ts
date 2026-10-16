import { getToDoTaskRoute, TaskStatus } from './constants';

// TODO create validation schema
export type TaskView_Details = Readonly<{
  Version: number;
  Title: string;
  Status: TaskStatus;
  DismissReason: string | null;
}>;
export const getTaskViewDetailsRoute = (taskId: string) => getToDoTaskRoute('', taskId);
