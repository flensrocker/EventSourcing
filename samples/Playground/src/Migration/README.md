# Migrations

This project handles all the migrations associated with the product "Playground".

## Generate EF Migrations

Example for the "ToDo" feature module.

```sh
dotnet ef migrations add ToDoInit --context ToDoDbContext --project ../../../ToDo/src/Persistence.PostgreSQL
```
