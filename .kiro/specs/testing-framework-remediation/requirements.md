# Testing Framework Remediation - Requirements

**Version:** 1.0  
**Date:** June 27, 2026  
**Status:** Ready for Implementation

---

## Overview

The NaarNoor testing framework audit identified 42% completion with excellent property-based tests but critical gaps in CI/CD infrastructure, frontend testing, and documentation. This spec remediates all gaps to achieve production-ready quality gates and 100% test coverage across backend and frontend.

**Audit Reference:** `/AUDIT_REPORT.md`, `/ACTION_PLAN.md`

---

## Business Requirements

### 1. Coverage Measurement & Enforcement

**Req 1.1:** Backend coverage must be measured on every build
- Requirement: Coverlet configuration file with layer-specific thresholds
- Target Thresholds:
  - Domain: 85% line coverage
  - Application: 82% line coverage
  - Infrastructure: 78% line coverage
  - API: 80% line coverage
- Success Criteria: `dotnet test` produces coverage.cobertura.xml with per-layer metrics

**Req 1.2:** Frontend coverage must be measured on every test run
- Requirement: Karma/Istanbul configuration for Angular tests
- Target Thresholds:
  - Services: 80% line coverage
  - Components: 75% line coverage
- Success Criteria: `npm test` produces coverage reports with per-layer metrics

**Req 1.3:** Coverage reports must be human-readable
- Requirement: HTML report generation with trend tracking
- Success Criteria: ReportGenerator creates `/coverage-reports/index.html` with drill-down to file level

---

### 2. CI/CD Pipeline & Gating

**Req 2.1:** Automated testing on every pull request
- Requirement: GitHub Actions workflow triggers on PR/push
- Components:
  - Backend test execution with coverage collection
  - Frontend test execution with coverage collection
  - Coverage threshold validation
  - Artifact collection (test results, coverage reports)
- Success Criteria: PR cannot merge if tests fail or coverage below threshold

**Req 2.2:** Coverage enforcement blocks low-quality merges
- Requirement: CI/CD workflow must reject PRs with insufficient coverage
- Logic:
  - Measure coverage per layer
  - Compare against Req 1.1 and 1.2 thresholds
  - Fail workflow if any layer below threshold
  - Provide detailed report showing gap size
- Success Criteria: PR fails with actionable message when coverage insufficient

**Req 2.3:** Pre-commit hooks prevent local commits with failing tests
- Requirement: Husky hooks run tests before git commit
- Behavior:
  - Hook runs affected tests for modified files
  - Prevents commit if tests fail
  - Shows actionable error messages
  - Can be bypassed with `--no-verify` for admin access
- Success Criteria: `git commit` fails if unit tests fail

---

### 3. Property-Based Test Completeness

**Req 3.1:** Properties 7-9 must be in dedicated test files
- Property 7: Repository CRUD Round-Trip Operations
  - File: `RepositoryCrudPropertyTests.cs`
  - Tests: Create, Read, Update, Delete operations
  - Count: 5-7 test methods
  
- Property 8: Query Pagination Correctness
  - File: `QueryPaginationPropertyTests.cs`
  - Tests: Offset, page size, total count calculations
  - Count: 5-7 test methods
  
- Property 9: Transaction Atomicity
  - File: `TransactionAtomicityPropertyTests.cs`
  - Tests: All-or-nothing semantics, rollback on error
  - Count: 5-7 test methods

- Success Criteria: All properties pass with 100 FsCheck iterations minimum

**Req 3.2:** Backend security properties must be implemented
- Property 11: API Exception Mapping (status codes, messages)
- Property 16: Input Validation Completeness (all controller inputs validated)
- Property 17: Authorization Enforcement (policy-based access control)
- Property 19: Injection Attack Prevention (SQL injection, XSS, command injection)
- Property 20: Sensitive Data Protection (no passwords in responses, secure headers)
- Success Criteria: All 5 properties with 3+ test methods each

**Req 3.3:** Performance SLA properties must be implemented
- Property 21: Query Performance SLA (queries execute < 100ms average)
- Property 22: API Endpoint Response Time (endpoints respond < 500ms average)
- Success Criteria: Properties measure 95th percentile performance

---

### 4. Frontend Testing Framework

**Req 4.1:** Angular service tests with property-based verification
- Requirement: Jasmine tests for HTTP, error handling, caching
- Coverage:
  - Property 12: HTTP Communication Reliability
  - Property 13: Service Error Handling
  - Property 14: Response Caching Consistency
- Success Criteria: 15+ service test methods, 80% line coverage

**Req 4.2:** Angular component tests with state management verification
- Requirement: Jasmine tests for component initialization, input/output binding, lifecycle
- Coverage:
  - Property 15: Component State Management
  - Tests: ReservationForm, MenuList, OrderDisplay components
- Success Criteria: 10+ component test methods, 75% line coverage

**Req 4.3:** E2E tests with Cypress page objects
- Requirement: Critical user workflows tested end-to-end
- Coverage:
  - Authentication flows (login, logout, session management)
  - Reservation creation workflow
  - Order placement workflow
  - Menu search and filtering
  - Review submission
- Success Criteria: 10+ E2E scenarios, 100% workflow coverage

---

### 5. Documentation & Knowledge Transfer

**Req 5.1:** Testing overview guide for new team members
- File: `/docs/TESTING.md`
- Content:
  - Quick start (running tests locally)
  - Test directory structure
  - Test naming conventions
  - Common test commands
- Success Criteria: New developer can run full test suite in <5 minutes from guide

**Req 5.2:** Property-based testing tutorial
- File: `/docs/TESTING_PROPERTIES.md`
- Content:
  - Property testing concepts and benefits
  - FsCheck generator patterns
  - How to write a new property test
  - Common property test patterns
  - Troubleshooting failing properties
- Success Criteria: Developer can write new property test independently

**Req 5.3:** Mock and fixture patterns documentation
- File: `/docs/TESTING_MOCKING.md`
- Content:
  - Mock factory patterns
  - Builder patterns for test data
  - Database seeding for tests
  - Async test patterns
  - Common mock configurations
- Success Criteria: Developer understands all mocking approaches used in codebase

**Req 5.4:** Coverage interpretation guide
- File: `/docs/TESTING_COVERAGE.md`
- Content:
  - Interpreting coverage reports
  - Coverage goals by layer
  - Improving coverage metrics
  - Understanding exclusions
  - Coverage trends and alerts
- Success Criteria: Developer understands layer-specific coverage targets

**Req 5.5:** Testing troubleshooting guide
- File: `/docs/TESTING_TROUBLESHOOTING.md`
- Content:
  - Common test failures and solutions
  - Debugging async tests
  - Mock configuration issues
  - Flaky test patterns and fixes
  - Performance issues in test runs
- Success Criteria: Developer can diagnose and fix 80% of common test failures

---

## Success Criteria

### Week 1 Checkpoint
- ✅ Coverage measurement configured and working locally
- ✅ GitHub Actions workflow executing tests on PR/push
- ✅ Pre-commit hooks preventing commits with failing tests
- ✅ Properties 7-9 extracted into dedicated test files
- ✅ All backend tests passing with coverage reports

### Week 2 Checkpoint
- ✅ Coverage thresholds enforced in CI/CD
- ✅ Test suite executes in <10 minutes on GitHub Actions
- ✅ Coverage trends dashboard visible
- ✅ First PR blocked due to low coverage (successful gate test)

### Week 4 Checkpoint
- ✅ Karma/Jasmine configured and working locally
- ✅ 15+ service tests passing (80% coverage)
- ✅ 10+ component tests passing (75% coverage)
- ✅ Frontend tests integrated into CI/CD
- ✅ Frontend coverage gating enforced

### Week 6 Checkpoint
- ✅ All 22 property tests implemented and passing
- ✅ All documentation files complete and reviewed
- ✅ New team member can write test independently
- ✅ All CI/CD gates active and enforced
- ✅ Zero known test infrastructure gaps

### Production Ready
- ✅ 100% property test passing (all 22)
- ✅ All layers meeting coverage thresholds
- ✅ CI/CD blocking inadequate submissions
- ✅ Pre-commit hooks active on all machines
- ✅ Team documentation available and current
- ✅ Zero production support incidents related to test infrastructure

---

## Constraints & Assumptions

### Constraints
- Must use existing tooling (Coverlet, xUnit, Karma, Jasmine, Cypress)
- Must not modify existing test logic (only reorganize/add)
- CI/CD must complete in <15 minutes per PR
- Documentation must be markdown-based
- Frontend tests must run in <5 minutes

### Assumptions
- Team has access to GitHub Actions (no additional infrastructure)
- NuGet packages for Coverlet/xUnit already available
- npm packages (Karma, Jasmine) can be installed
- Cypress can be installed in CI environment
- Team familiar with basic Git/GitHub workflow

---

## Out of Scope

- Refactoring existing test code logic
- Adding new business features or test coverage beyond audit findings
- Changing test frameworks (FsCheck, xUnit, Jasmine)
- Infrastructure provisioning or DevOps setup beyond CI/CD config
- Performance optimization of test execution (beyond parallel runs)

---

## Dependencies

### External
- GitHub (PR/workflow capabilities)
- npm registry (Karma, Jasmine, Istanbul packages)
- NuGet registry (Coverlet packages)

### Internal
- Existing test projects structure
- Existing Domain/Application/Infrastructure projects
- Existing Angular application structure

