# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

A Turkish banking/accounting demo app (bankacılık muhasebe) with three layers:

- **`DB/`** — Oracle schema: tables, triggers, and PL/SQL packages. This is where the actual business logic lives.
- **`Backend/BankAPI/`** — ASP.NET Core (net10.0) Web API that is a thin pass-through to Oracle stored procedures.
- **`Frontend/`** — Angular 18 standalone-component SPA (admin panel style UI).

The backend does **not** contain business rules — validation, ID generation, balance updates, and audit history are all done in the database layer (triggers + PL/SQL packages under `PKG_MUSTERI`, `PKG_HESAP`, `PKG_HESAPHAREKET`, `PKG_DASHBOARD`). When changing behavior (e.g. account balance calculation, customer validation), look in `DB/03_Procedures.sql` and `DB/02_Triggers.sql` first, not just the C# service.

Layer-specific commands and architecture notes live in each layer's own `CLAUDE.md`: `Backend/CLAUDE.md`, `Frontend/CLAUDE.md`, `DB/CLAUDE.md`.
