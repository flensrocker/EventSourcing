import { TaskStatus } from './constants';

export type TaskView_Details = Readonly<{
  Version: number;
  Title: string;
  Status: TaskStatus;
  DismissReason: string | null;
}>;
