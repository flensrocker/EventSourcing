import { Component, inject, signal } from '@angular/core';
import { FormField, FormRoot, form, required, maxLength } from '@angular/forms/signals';

import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { WithoutType } from '@flensrocker/event-sourcing-core';
import { TaskCommand_ChangeTitle, TaskTitleMaxLength } from '@flensrocker/to-do-core';

export type ToDoTaskChangeTitleDialogData = WithoutType<TaskCommand_ChangeTitle>;
export type ToDoTaskChangeTitleDialogResult = WithoutType<TaskCommand_ChangeTitle> | null;

@Component({
  selector: 'to-do-task-change-title-dialog',
  imports: [FormField, FormRoot, MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule],
  template: `<form [formRoot]="changeTitleForm">
    <h2 mat-dialog-title>Change Title</h2>
    <mat-dialog-content>
      <mat-form-field>
        <mat-label>Title</mat-label>
        <input type="text" matInput [formField]="changeTitleForm.Title" />
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions>
      <button type="submit" matButton="filled">Change</button>
      <button type="button" matButton="outlined" [matDialogClose]="null">Cancel</button>
    </mat-dialog-actions>
  </form>`,
})
export class ToDoTaskChangeTitleDialog {
  readonly #dialogRef = inject<MatDialogRef<ToDoTaskChangeTitleDialog, ToDoTaskChangeTitleDialogResult>>(MatDialogRef);
  readonly #dialogData = inject<ToDoTaskChangeTitleDialogData>(MAT_DIALOG_DATA);

  readonly #changeTitleModel = signal<NonNullable<ToDoTaskChangeTitleDialogResult>>({
    Title: this.#dialogData.Title,
  });

  protected readonly changeTitleForm = form(
    this.#changeTitleModel,
    (schemaPath) => {
      required(schemaPath.Title, { message: 'The title is required.' });
      maxLength(schemaPath.Title, TaskTitleMaxLength, { message: 'The title is too long.' });
    },
    {
      submission: {
        action: (field) => {
          this.#dialogRef.close(field().dirty() ? field().value() : null);
          return Promise.resolve();
        },
      },
    },
  );
}
