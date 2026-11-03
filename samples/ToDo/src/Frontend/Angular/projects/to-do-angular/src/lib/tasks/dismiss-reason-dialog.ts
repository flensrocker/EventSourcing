import { Component, inject, signal } from '@angular/core';
import { FormField, FormRoot, form, required, maxLength } from '@angular/forms/signals';

import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { WithoutType } from '@flensrocker/event-sourcing-core';
import { TaskCommand_Dismiss, TaskDismissReasonMaxLength } from '@flensrocker/to-do-core';

export type ToDoTaskDismissReasonDialogResult = WithoutType<TaskCommand_Dismiss> | null;

@Component({
  selector: 'to-do-task-dismiss-reason-dialog',
  imports: [FormField, FormRoot, MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule],
  template: `<form [formRoot]="dismissForm">
    <h2 mat-dialog-title>Dismiss Task</h2>
    <mat-dialog-content>
      <mat-form-field>
        <mat-label>Reason</mat-label>
        <input type="text" matInput [formField]="dismissForm.Reason" />
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions>
      <button type="submit" matButton="filled">Dismiss</button>
      <button type="button" matButton="outlined" [matDialogClose]="null">Cancel</button>
    </mat-dialog-actions>
  </form>`,
})
export class ToDoTaskDismissReasonDialog {
  readonly #dialogRef = inject<MatDialogRef<ToDoTaskDismissReasonDialog, ToDoTaskDismissReasonDialogResult>>(MatDialogRef);

  readonly #dismissModel = signal<NonNullable<ToDoTaskDismissReasonDialogResult>>({
    Reason: '',
  });

  protected readonly dismissForm = form(
    this.#dismissModel,
    (schemaPath) => {
      required(schemaPath.Reason, { message: 'The reason is required.' });
      maxLength(schemaPath.Reason, TaskDismissReasonMaxLength, { message: 'The reason is too long.' });
    },
    {
      submission: {
        action: (field) => {
          this.#dialogRef.close(field().value());
          return Promise.resolve();
        },
      },
    },
  );
}
