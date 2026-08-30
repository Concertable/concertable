---
name: composition-testing
description: Concertable's host composition-validation tier — every executable .NET host enables strict service-provider validation (`ValidateOnBuild`/`ValidateScopes` via `UseStrictServiceProviderValidation`, or `StrictDistributedApplication` for Aspire AppHosts), and each host's `*.CompositionTests` project builds the real production registration graph without starting the host so `ValidateComposition` proves the roots `ValidateOnBuild` cannot — dynamically-created framework roots, uninvoked factories, keyed services, closed generic consumers, hosted services and activation roots (controllers, Razor Pages, isolated Functions, middleware). Registration must stay side-effect-free, and `AppHostCompositionTests.Inventory_AllExecutableProjectsDeclareCoverageOrExclusion` fails any executable host lacking strict validation and a composition suite or a declared `CompositionValidationExclusion`. Use when adding or changing an executable host or its DI registrations, writing or reviewing a composition test, or adding a new deployable project.
---

# Host composition validation

A fourth test tier beside unit, integration and E2E: it builds each executable host's **real production
registration graph without starting it** or any external infrastructure, and proves the graph resolves.
Tests that execute requests, business operations or infrastructure belong in an integration or E2E project,
never here.

## Every executable host enables strict service-provider validation

Every executable .NET host enables `ValidateOnBuild` and `ValidateScopes` through
`UseStrictServiceProviderValidation`, `ServiceProviderValidation.CreateFactory`, or the equivalent
`AddServiceDefaults` path. Aspire does not expose the underlying service-provider factory through its public
builder API, so AppHosts use `StrictDistributedApplication`: it requires the Development environment in which
Aspire enables strict default-container validation and rejects any other environment.

## A `*.CompositionTests` project proves what `ValidateOnBuild` cannot

`ValidateOnBuild` cannot prove roots created dynamically by a framework, factories that have not been invoked,
unused keyed registrations, or open generics without a closed consumer. The owning `*.CompositionTests` project
therefore builds each real production registration path without starting the host, then `ValidateComposition`
resolves application-owned descriptors (including factories and keyed services), closed generic consumers and
registered handler definitions, hosted services, and framework activation roots discovered as controllers,
Razor Pages, isolated Functions and middleware.

**Composition registration must remain side-effect-free.** A registration that connects to infrastructure while
the graph is built or resolved must move that work behind its runtime boundary — otherwise building the graph
to validate it would reach live infrastructure.

## Every executable host is covered or explicitly excluded

`AppHostCompositionTests.Inventory_AllExecutableProjectsDeclareCoverageOrExclusion` discovers executable
projects mechanically. Each discovered host must enable strict provider validation and appear in a real
composition suite, or declare `CompositionValidationExclusion` with the architectural reason and the explicit
substitute validation mechanism. An executable host that does neither fails the inventory.

Each service owns and carries its own composition project, so the coverage survives the carve into separate
repos; the umbrella AppHost suite owns only the umbrella host and the repository-wide inventory.
