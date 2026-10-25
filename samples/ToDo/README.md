# The ToDo Feature Module

The main purpose of this feature module is to show case the Event-Sourcing framework.
We all know to-do lists to some extend, so I don't have to explain a lot.
And it can be complex enough to have room for "fancy stuff".

The ToDo module manages "tasks" and their transitions between different states like "opened" up to "completed".

In the future there may be "boards" and "teams" for more advanced visualization.

## The Domain

A task has a simple title, which describes the task to do.

A task can be in one of the following states:

- `Opened`: this is the "to do" state
- `Started`: someone started the work on this task ("doing")
- `Paused`: the work is paused (still "doing")
- `Completed`: the work is "done"
- `Dismissed`: a decision was made to not do the work ("don't")

```mermaid
stateDiagram-v2
    state Task {
        Opened
        Started
        Paused
        Completed
        Dismissed

        [*] --> Opened: open
        Opened --> Opened: reopen
        Opened --> Started: start
        Opened --> Completed: complete
        Opened --> Dismissed: dismiss
        Started --> Started: start
        Started --> Paused: pause
        Started --> Completed: complete
        Started --> Dismissed: dismiss
        Paused --> Started: start
        Paused --> Paused: pause
        Paused --> Completed: complete
        Paused --> Dismissed: dismiss
        Completed --> Opened: reopen
        Completed --> Completed: complete
        Dismissed --> Opened: reopen
        Dismissed --> Dismissed: dismiss with same reason
        Dismissed --> [*]
        Completed --> [*]
    }
```

The task transitions between states on applying a command to the task.
Either an error or a success will be returned.

- `NotFound`: if the task does not exist
- `Forbidden`: if access to the task is denied
- `BadRequest`: if the command is not allowed in the current state of the task
- `InternalError`: if an unexpected error happens
- `Success`: on a successful transition a list of domain events is returned which describes the transition;
  the list can be empty if the transition doesn't change the state at all
