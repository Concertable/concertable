---
name: data-tables
description: TanStack Table behind one shared DataTable component — features supply columns and rows and never touch useReactTable, flexRender or a row model themselves, row models past the core one are opt-in because each costs work on every render, client-side sorting and pagination are dropped wherever the server already does it (otherwise the table sorts one page and calls it sorted), column definitions live at module scope or memoized so the table does not lose its state to a new column identity each render, cells render rather than fetch or decide, and a list of non-comparable rows is not a table at all. Use when adding a table, adding sorting/filtering/pagination to one, writing a ColumnDef or cell renderer, or reviewing a component that builds its own table instance.
---

# data-tables

The standard is `../../standards/react/TABLES.md`, shipped in this plugin. Read it and follow it; this skill only routes to it.
