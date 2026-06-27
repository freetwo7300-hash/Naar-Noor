# Testing Framework Remediation - Tasks

**Specification:** Testing Framework Remediation  
**Total Tasks:** 48  
**Estimated Duration:** 6 weeks (40-54 hours)

---

## PHASE 1: CRITICAL PATH - CI/CD FOUNDATION (Week 1)

### Section 1: Coverage Configuration (4-6 hours)

- [ ] 1.1 Create coverlet.runsettings configuration file
  - _Create_ `coverlet.runsettings` in repo root
  - _Configure_ Cobertura format output
  - _Set_ layer thresholds: Domain 85%, App 82%, Infra 78%, API 80%
  - _Test_ coverage collection with `dotnet test`
  - _Verify_ coverage.cobertura.xml generated correctly
  - _Requirements: 1.1_

- [ ] 1.2 Create ReportGenerator configuration
  - _Create_ script to invoke ReportGenerator
  - _Configure_ HTML report generation
  - _Set_ output directory to `/coverage-reports/`
  - _Test_ report generation with sample coverage file
  - _Verify_ HTML dashboard loads with drill-down capability
  - _Requirements: 1.3_

- [ ] 1.3 Create coverage validation script
  - _Create_ `scripts/validate-coverage.sh` (Linux/Mac) or `.ps1` (Windows)
  - _Implement_ per-layer threshold checking
  - _Output_ detailed coverage gap report if threshold not met
  - _Test_ with coverage files from test runs
  - _Verify_ script correctly fails when coverage insufficient
  - _Requirements: 1.1, 1.3_

- [ ] 1.4 Test coverage measurement end-to-end
  - _Run_ `dotnet test` with coverage configuration
  - _Verify_ all layer coverage files generated
  - _Generate_ HTML report with ReportGenerator
  - _Validate_ coverage above thresholds
  - _Document_ commands in TESTING.md quick start
  - _Requirements: 1.1_

### Section 2: GitHub Actions Workflow (6-8 hours)

- [ ] 2.1 Create backend test workflow job
  - _Create_ `.github/workflows/tests.yml`
  - _Add_ backend test job with steps:
    - Checkout code
    - Setup .NET 8.0
    - Restore dependencies
    - Build projects
    - Run tests with coverage
  - _Configure_ timeout and retry logic
  - _Test_ workflow manually on feature branch
  - _Requirements: 2.1_

- [ ] 2.2 Create frontend test workflow job
  - _Add_ frontend test job to `tests.yml`
  - _Add_ steps:
    - Setup Node.js
    - Install dependencies (`npm ci`)
    - Run Jasmine tests
    - Collect Istanbul coverage
  - _Configure_ timeout handling
  - _Test_ workflow on feature branch
  - _Requirements: 2.1_

- [ ] 2.3 Create coverage gating workflow job
  - _Add_ coverage gate job to `tests.yml`
  - _Implement_ logic:
    - Check backend coverage against thresholds
    - Check frontend coverage against thresholds
    - Fail if any layer below threshold
    - Generate detailed comparison report
  - _Test_ with insufficient coverage to verify failure
  - _Requirements: 2.2_

- [ ] 2.4 Create artifact collection
  - _Add_ artifact upload steps to workflow:
    - Upload test results (TRX files)
    - Upload coverage reports (XML + HTML)
    - Upload coverage badges
  - _Configure_ artifact retention (30 days)
  - _Test_ artifact download from workflow run
  - _Requirements: 2.1_

- [ ] 2.5 Create PR comment workflow
  - _Add_ step to comment on PR with results
  - _Include_ coverage summary per layer
  - _Include_ pass/fail status
  - _Include_ link to detailed coverage report
  - _Test_ on sample PR
  - _Requirements: 2.2_

### Section 3: Pre-commit Hooks (2-3 hours)

- [ ] 3.1 Configure Husky pre-commit hooks
  - _Install_ Husky: `npm install husky --save-dev`
  - _Initialize_ Husky: `npx husky install`
  - _Create_ `.husky/pre-commit` hook script
  - _Implement_ to run affected tests
  - _Test_ by attempting commit with failing test
  - _Verify_ commit blocked with error message
  - _Requirements: 2.3_

- [ ] 3.2 Create .husky directory structure
  - _Create_ `.husky/.gitignore` to exclude hook artifacts
  - _Create_ `install` hook for local setup
  - _Create_ `post-merge` hook for dependency updates
  - _Test_ fresh clone still has hooks working
  - _Requirements: 2.3_

- [ ] 3.3 Document hook setup in README
  - _Update_ main README with post-clone setup
  - _Add_ command: `npm run husky:install`
  - _Explain_ what hooks do
  - _Document_ bypass: `git commit --no-verify`
  - _Requirements: 2.3_

### Section 4: Property Test Extraction (6 hours)

- [ ] 4.1 Extract Property 7: CRUD Round-Trip into dedicated file
  - _Create_ `api-server/tests/NaarNoor.Infrastructure.Tests/Repositories/RepositoryCrudPropertyTests.cs`
  - _Move_ CRUD operation tests from ReferentialIntegrityPropertyTests
  - _Create_ test data generators for various entity states
  - _Implement_ 5-7 property test methods:
    - Create operation with valid data
    - Read operation returns correct entity
    - Update operation persists changes
    - Delete operation removes entity
    - Round-trip creates then reads correctly
  - _Test_ all property methods pass with 100 iterations
  - _Requirements: 3.1_

- [ ] 4.2 Extract Property 8: Query Pagination into dedicated file
  - _Create_ `api-server/tests/NaarNoor.Application.Tests/Common/QueryPaginationPropertyTests.cs`
  - _Move_ pagination tests from GetReservationsQueryHandlerPropertyTests
  - _Implement_ 5-7 property test methods:
    - Offset calculation correctness
    - Page size handling
    - Total count accuracy
    - Boundary conditions (empty, single page, multiple pages)
    - Sort order preservation
  - _Test_ all property methods pass with 100 iterations
  - _Requirements: 3.1_

- [ ] 4.3 Extract Property 9: Transaction Atomicity into dedicated file
  - _Create_ `api-server/tests/NaarNoor.Infrastructure.Tests/Persistence/TransactionAtomicityPropertyTests.cs`
  - _Move_ transaction tests from ReferentialIntegrityPropertyTests
  - _Implement_ 5-7 property test methods:
    - All-or-nothing behavior (commit all or rollback all)
    - Rollback on error within transaction
    - Isolation level enforcement
    - Multiple concurrent transactions
    - Nested transaction handling
  - _Test_ all property methods pass with 100 iterations
  - _Requirements: 3.1_

- [ ] 4.4 Update existing test files after extraction
  - _Remove_ extracted tests from ReferentialIntegrityPropertyTests
  - _Remove_ extracted tests from GetReservationsQueryHandlerPropertyTests
  - _Update_ remaining test data generators
  - _Verify_ no duplicate test logic remains
  - _Run_ full test suite to ensure no regressions
  - _Requirements: 3.1_

- [ ] 4.5 Verify extraction and run tests
  - _Run_ new dedicated test files in isolation
  - _Run_ full test suite to verify no breaking changes
  - _Verify_ coverage metrics still meet targets
  - _Update_ test documentation with new file locations
  - _Requirements: 3.1_

---

## PHASE 2: FRONTEND TESTING FRAMEWORK (Weeks 3-4)

### Section 5: Karma/Jasmine Configuration (6-8 hours)

- [ ] 5.1 Install Karma and Jasmine dependencies
  - _Install_ packages: `npm install --save-dev karma karma-jasmine karma-chrome-launcher karma-coverage`
  - _Install_ Istanbul/nyc: `npm install --save-dev istanbul karma-istanbul-reporter`
  - _Verify_ package.json updated with versions
  - _Create_ `karma.conf.js` configuration file
  - _Requirements: 4.1_

- [ ] 5.2 Create karma.conf.js with coverage settings
  - _Configure_ Jasmine framework
  - _Configure_ Chrome headless browser
  - _Configure_ coverage reporter:
    - Format: html, lcovonly, text-summary, cobertura
    - Output: `./coverage/`
  - _Set_ coverage thresholds:
    - Services: 80%
    - Components: 75%
  - _Configure_ test patterns: `src/**/*.spec.ts`
  - _Requirements: 4.1_

- [ ] 5.3 Create sample Jasmine test for verification
  - _Create_ `src/app/services/tests/sample.service.spec.ts`
  - _Write_ 3-4 basic Jasmine tests
  - _Include_ TestBed setup for DI
  - _Include_ HttpClientTestingModule setup
  - _Run_ `npm test` to verify Karma works
  - _Verify_ coverage report generated
  - _Requirements: 4.1_

- [ ] 5.4 Configure npm test scripts
  - _Add_ `"test"` script: runs Karma in watch mode
  - _Add_ `"test:ci"` script: runs Karma once with coverage
  - _Add_ `"coverage:check"` script: validates thresholds
  - _Test_ each script runs correctly
  - _Requirements: 4.1_

### Section 6: Angular Service Tests (8-10 hours)

- [ ] 6.1 Create ReservationService tests (Property 12: HTTP Communication)
  - _Create_ `src/app/services/tests/reservation.service.spec.ts`
  - _Implement_ 5-6 test methods:
    - HTTP GET request correct URL
    - HTTP POST request correct payload
    - Error handling returns error response
    - Retry logic on transient failure
    - Request timeout handling
  - _Use_ HttpClientTestingModule for mocking
  - _Achieve_ 85%+ line coverage
  - _Requirements: 4.1_

- [ ] 6.2 Create OrderService tests (Property 12-13: HTTP & Error Handling)
  - _Create_ `src/app/services/tests/order.service.spec.ts`
  - _Implement_ 5-6 test methods:
    - Create order request format
    - Update order PATCH/PUT handling
    - List orders pagination parameters
    - Error response parsing
    - Network error handling
  - _Use_ HttpClientTestingModule
  - _Achieve_ 85%+ line coverage
  - _Requirements: 4.1_

- [ ] 6.3 Create MenuService tests with caching (Property 14: Caching Consistency)
  - _Create_ `src/app/services/tests/menu.service.spec.ts`
  - _Implement_ 5-6 test methods:
    - First call fetches from HTTP
    - Second call returns cached data
    - Cache invalidation clears data
    - Multiple subscribers share cache
    - Concurrent requests use single HTTP call
  - _Implement_ caching in service if not present
  - _Use_ rxjs shareReplay operator
  - _Achieve_ 85%+ line coverage
  - _Requirements: 4.1_

- [ ] 6.4 Create base class for service tests
  - _Create_ `src/app/services/tests/service-test.base.ts`
  - _Implement_ common TestBed setup
  - _Implement_ HttpTestingController helper methods
  - _Implement_ mock data generators
  - _Implement_ common assertion helpers
  - _Requirements: 4.1_

- [ ] 6.5 Verify service tests coverage and pass
  - _Run_ `npm run test:ci`
  - _Verify_ all service tests pass
  - _Verify_ coverage meets 80% threshold
  - _Generate_ coverage report
  - _Requirements: 4.1_

### Section 7: Angular Component Tests (8 hours)

- [ ] 7.1 Create component test base class
  - _Create_ `src/app/components/tests/component-test.base.ts`
  - _Implement_ TestBed setup for components
  - _Implement_ common DOM query helpers
  - _Implement_ form interaction helpers
  - _Implement_ change detection helpers
  - _Implement_ mock service providers
  - _Requirements: 4.2_

- [ ] 7.2 Create ReservationFormComponent tests (Property 15: State Management)
  - _Create_ `src/app/components/reservation-form/reservation-form.spec.ts`
  - _Implement_ 3-4 test methods:
    - Component initialization state
    - Form input binding
    - Form submission triggers service
    - Validation error display
  - _Use_ component test base class
  - _Achieve_ 80%+ line coverage
  - _Requirements: 4.2_

- [ ] 7.3 Create MenuListComponent tests
  - _Create_ `src/app/components/menu-list/menu-list.spec.ts`
  - _Implement_ 3-4 test methods:
    - List renders menu items
    - Filter applies correctly
    - Item selection emits output
    - Loading/empty states
  - _Use_ component test base class
  - _Achieve_ 80%+ line coverage
  - _Requirements: 4.2_

- [ ] 7.4 Create OrderDisplayComponent tests
  - _Create_ `src/app/components/order-display/order-display.spec.ts`
  - _Implement_ 3-4 test methods:
    - Order details display correctly
    - Status updates reflect in UI
    - Actions (cancel, modify) work
    - Error state displays message
  - _Use_ component test base class
  - _Achieve_ 80%+ line coverage
  - _Requirements: 4.2_

- [ ] 7.5 Verify component tests coverage and pass
  - _Run_ `npm run test:ci`
  - _Verify_ all component tests pass
  - _Verify_ coverage meets 75% component threshold
  - _Generate_ coverage report
  - _Requirements: 4.2_

### Section 8: E2E Tests with Cypress (8 hours)

- [ ] 8.1 Create Cypress page objects directory and structure
  - _Create_ `cypress/support/page-objects/` directory
  - _Create_ `LoginPage.ts` with login interactions
  - _Create_ `ReservationPage.ts` with reservation form
  - _Create_ `OrderPage.ts` with order placement
  - _Create_ `MenuPage.ts` with menu interactions
  - _Implement_ common selectors and methods
  - _Requirements: 4.3_

- [ ] 8.2 Create authentication E2E tests
  - _Create_ `cypress/e2e/auth.cy.ts`
  - _Implement_ test scenarios:
    - User can login with valid credentials
    - User cannot login with invalid credentials
    - User can logout
    - Session persists on refresh
  - _Use_ page objects for interactions
  - _Requirements: 4.3_

- [ ] 8.3 Create reservation workflow E2E tests
  - _Create_ `cypress/e2e/reservations.cy.ts`
  - _Implement_ test scenarios:
    - User can view available times
    - User can select date/time
    - User can enter guest details
    - Confirmation displays reservation number
  - _Use_ ReservationPage page object
  - _Requirements: 4.3_

- [ ] 8.4 Create order workflow E2E tests
  - _Create_ `cypress/e2e/orders.cy.ts`
  - _Implement_ test scenarios:
    - User can browse menu
    - User can add items to order
    - User can modify order quantity
    - User can place order
    - Order confirmation displays
  - _Use_ MenuPage and OrderPage page objects
  - _Requirements: 4.3_

- [ ] 8.5 Create additional E2E scenarios
  - _Create_ `cypress/e2e/menu-search.cy.ts` for search/filter
  - _Create_ `cypress/e2e/reviews.cy.ts` for review submission
  - _Create_ `cypress/e2e/navigation.cy.ts` for page navigation
  - _Implement_ 2-3 scenarios per file
  - _Use_ appropriate page objects
  - _Requirements: 4.3_

- [ ] 8.6 Configure Cypress for CI/CD
  - _Update_ `cypress.config.ts` for headless mode
  - _Configure_ reporter for CI/CD
  - _Add_ npm script: `"e2e"` runs Cypress headless
  - _Add_ npm script: `"e2e:open"` opens Cypress UI
  - _Test_ Cypress runs in headless mode
  - _Requirements: 4.3_

- [ ] 8.7 Integrate E2E tests into GitHub Actions
  - _Add_ E2E test job to `.github/workflows/tests.yml`
  - _Configure_ job to run after backend/frontend tests pass
  - _Upload_ Cypress videos/screenshots on failure
  - _Test_ workflow runs E2E tests
  - _Requirements: 4.3_

---

## PHASE 3: BACKEND PROPERTY TESTS (Week 5)

### Section 9: API Security Properties (6-8 hours)

- [ ] 9.1 Create API exception mapping property tests (Property 11)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/ExceptionMappingPropertyTests.cs`
  - _Implement_ 3-5 property test methods:
    - DomainException maps to 400 BadRequest
    - ValidationException maps to 422 UnprocessableEntity
    - NotFoundException maps to 404 NotFound
    - UnauthorizedException maps to 401 Unauthorized
    - Exception message formatted consistently
  - _Use_ InlineData and Theory for various exceptions
  - _Requirements: 3.2_

- [ ] 9.2 Create input validation property tests (Property 16)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/InputValidationPropertyTests.cs`
  - _Implement_ 3-5 property test methods:
    - Required fields validated (not empty/null)
    - Email format validated
    - Phone number format validated
    - Numeric fields validated (positive, range)
    - String length validated (min/max)
  - _Use_ invalid input generators
  - _Requirements: 3.2_

- [ ] 9.3 Create authorization enforcement property tests (Property 17)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/AuthorizationPropertyTests.cs`
  - _Implement_ 3-5 property test methods:
    - Unauthorized requests denied (401)
    - Insufficient permissions denied (403)
    - Admin endpoints restricted to admin role
    - Customer endpoints restricted to customer role
    - Claims verified on protected endpoints
  - _Use_ mock principals with various claims
  - _Requirements: 3.2_

- [ ] 9.4 Create injection attack prevention property tests (Property 19)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/InjectionAttackPropertyTests.cs`
  - _Implement_ 3-5 property test methods:
    - SQL injection patterns rejected
    - XSS script tags escaped/removed
    - Command injection patterns rejected
    - LDAP injection patterns rejected
    - Encoded attacks detected
  - _Use_ common injection payloads
  - _Requirements: 3.2_

- [ ] 9.5 Create sensitive data protection property tests (Property 20)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/SensitiveDataPropertyTests.cs`
  - _Implement_ 3-5 property test methods:
    - Passwords never in response bodies
    - Passwords never in logs
    - PII fields encrypted in transit (HTTPS)
    - Security headers present (CSP, X-Frame-Options, etc.)
    - Error messages don't leak internal details
  - _Use_ response inspection
  - _Requirements: 3.2_

### Section 10: Performance SLA Properties (2-3 hours)

- [ ] 10.1 Create query performance SLA property tests (Property 21)
  - _Create_ `api-server/tests/NaarNoor.Application.Tests/PerformancePropertyTests.cs`
  - _Implement_ 2-3 property test methods:
    - GetReservations query executes < 100ms
    - GetMenuItems query executes < 100ms
    - GetOrders query executes < 100ms
  - _Use_ Stopwatch for timing measurement
  - _Use_ large dataset generators (1000+ records)
  - _Requirements: 3.3_

- [ ] 10.2 Create API endpoint response time property tests (Property 22)
  - _Create_ `api-server/tests/NaarNoor.API.Tests/EndpointPerformancePropertyTests.cs`
  - _Implement_ 2-3 property test methods:
    - GET /api/reservations responds < 500ms
    - POST /api/reservations responds < 500ms
    - GET /api/menu responds < 500ms
  - _Use_ full HTTP request/response cycle
  - _Use_ realistic payload sizes
  - _Requirements: 3.3_

- [ ] 10.3 Verify all property tests pass
  - _Run_ full test suite: `dotnet test`
  - _Verify_ all new property tests pass with 100 iterations
  - _Verify_ coverage still meets thresholds
  - _Requirements: 3.2, 3.3_

---

## PHASE 4: DOCUMENTATION (Week 6)

### Section 11: Testing Documentation (8-10 hours)

- [ ] 11.1 Create TESTING.md quick start guide
  - _Create_ `/docs/TESTING.md`
  - _Include_ sections:
    - Running tests locally (backend and frontend)
    - Understanding test output
    - Common test commands
    - Test directory structure
    - Debugging test failures
    - Coverage reports interpretation
  - _Minimum_ 60 lines with code examples
  - _Requirements: 5.1_

- [ ] 11.2 Create property-based testing tutorial
  - _Create_ `/docs/TESTING_PROPERTIES.md`
  - _Include_ sections:
    - Property testing concepts and benefits
    - FsCheck generators and combinators
    - Writing first property test (tutorial)
    - Common property testing patterns
    - Debugging failing properties
    - Property test best practices
  - _Include_ at least 3 working code examples
  - _Minimum_ 80 lines
  - _Requirements: 5.2_

- [ ] 11.3 Create mocking and fixtures documentation
  - _Create_ `/docs/TESTING_MOCKING.md`
  - _Include_ sections:
    - Mock factory patterns in codebase
    - Builder pattern for test data
    - Database seeding for integration tests
    - Async test patterns and gotchas
    - Common mock configurations
    - Using InlineData and MemberData
  - _Include_ code examples from actual codebase
  - _Minimum_ 70 lines
  - _Requirements: 5.3_

- [ ] 11.4 Create coverage interpretation guide
  - _Create_ `/docs/TESTING_COVERAGE.md`
  - _Include_ sections:
    - Reading coverage reports (HTML dashboard)
    - Coverage goals by layer (why 85% vs 75%)
    - Improving coverage (techniques and gotchas)
    - Understanding excluded files
    - Coverage trends and alerts
    - Coverage false positives
  - _Include_ screenshots/descriptions of reports
  - _Minimum_ 50 lines
  - _Requirements: 5.4_

- [ ] 11.5 Create troubleshooting guide
  - _Create_ `/docs/TESTING_TROUBLESHOOTING.md`
  - _Include_ sections:
    - Common test failures and solutions (10+ items)
    - Debugging async/await tests
    - Mock configuration issues
    - Flaky test patterns and prevention
    - Test performance issues
    - Database test isolation problems
    - Logging in tests
  - _Minimum_ 60 lines
  - _Requirements: 5.5_

- [ ] 11.6 Update README with testing section
  - _Update_ main `/README.md`
  - _Add_ "Testing" section with:
    - Link to `/docs/TESTING.md` quick start
    - Common test commands
    - Running tests before commit
    - CI/CD testing gate explanation
  - _Add_ badges for coverage status
  - _Requirements: 5.1_

- [ ] 11.7 Create API test documentation
  - _Create_ `api-server/tests/README.md`
  - _Include_:
    - Test project organization
    - Layer-specific testing patterns
    - Running tests from Visual Studio
    - Running tests from command line
    - Debugging failing tests
    - Coverage thresholds per project
  - _Minimum_ 40 lines
  - _Requirements: 5.1_

- [ ] 11.8 Create frontend test documentation
  - _Create_ `src/README.md` or `cypress/README.md`
  - _Include_:
    - Service test patterns
    - Component test patterns
    - E2E test patterns with page objects
    - Running tests locally
    - Running tests in CI
    - Debugging test failures
  - _Minimum_ 40 lines
  - _Requirements: 5.1_

- [ ] 11.9 Review and validate all documentation
  - _Read_ all 8 documentation files
  - _Verify_ code examples compile/run correctly
  - _Verify_ links point to correct files
  - _Verify_ terminology consistent across docs
  - _Verify_ new developer can follow docs end-to-end
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 11.10 Commit all documentation
  - _Stage_ all documentation files
  - _Commit_ with message: "docs: add comprehensive testing documentation"
  - _Push_ to main branch
  - _Requirements: 5.1-5.5_

---

## PHASE 5: VALIDATION & CLEANUP

### Section 12: Final Validation (2-3 hours)

- [ ] 12.1 Run full test suite
  - _Execute_ `dotnet test` from repo root
  - _Verify_ all 125+ backend tests pass
  - _Verify_ all backend properties pass with 100 iterations
  - _Verify_ coverage meets all thresholds
  - _Verify_ no warnings or errors
  - _Requirements: 3.1, 3.2, 3.3_

- [ ] 12.2 Run full frontend test suite
  - _Execute_ `npm run test:ci` from frontend root
  - _Verify_ all 35+ frontend tests pass
  - _Verify_ coverage meets 80%+ services, 75%+ components
  - _Verify_ E2E tests run in headless mode
  - _Requirements: 4.1, 4.2, 4.3_

- [ ] 12.3 Verify CI/CD pipeline works end-to-end
  - _Create_ feature branch
  - _Make_ small test change
  - _Push_ to GitHub
  - _Verify_ Actions workflow runs
  - _Verify_ all jobs pass
  - _Verify_ PR can merge
  - _Requirements: 2.1, 2.2, 2.3_

- [ ] 12.4 Test pre-commit hooks
  - _Create_ local change that fails test
  - _Attempt_ `git commit`
  - _Verify_ hook prevents commit
  - _Fix_ test
  - _Verify_ commit succeeds
  - _Requirements: 2.3_

- [ ] 12.5 Verify coverage reporting and artifacts
  - _Check_ GitHub Actions workflow artifacts
  - _Verify_ coverage reports uploaded
  - _Verify_ coverage badge generated
  - _Verify_ HTML reports accessible
  - _Requirements: 2.1_

- [ ] 12.6 Update remediation spec completion
  - _Mark_ all tasks complete
  - _Create_ final summary commit
  - _Document_ completion in spec metadata
  - _Requirements: All_

---

## SUCCESS CRITERIA CHECKLIST

### By End of Week 1
- [ ] ✅ Coverage configuration working locally
- [ ] ✅ GitHub Actions workflow executing tests
- [ ] ✅ Pre-commit hooks preventing bad commits
- [ ] ✅ Properties 7-9 in dedicated files
- [ ] ✅ All backend tests passing

### By End of Week 2
- [ ] ✅ Coverage gating enforced
- [ ] ✅ CI/CD runs in <10 minutes
- [ ] ✅ Coverage reports generating
- [ ] ✅ First PR blocked due to low coverage (verification)

### By End of Week 4
- [ ] ✅ Karma/Jasmine configured
- [ ] ✅ 15+ service tests passing
- [ ] ✅ 10+ component tests passing
- [ ] ✅ Frontend coverage gating in CI/CD

### By End of Week 6
- [ ] ✅ All 22 properties implemented
- [ ] ✅ All 5 documentation guides completed
- [ ] ✅ New developer can write test from guides
- [ ] ✅ All CI/CD gates active and enforced
- [ ] ✅ Zero known infrastructure gaps

### Production Ready
- [ ] ✅ 100% property test passing
- [ ] ✅ All layers meeting coverage thresholds
- [ ] ✅ CI/CD blocking inadequate submissions
- [ ] ✅ Pre-commit hooks deployed
- [ ] ✅ Team documentation complete

---

## NOTES

- Property tests use FsCheck (backend) with minimum 100 iterations
- All documentation must be reviewed by team lead before merge
- UI tests use page object pattern for maintainability
- Coverage thresholds are hard gates (PRs cannot merge if below)
- Pre-commit hooks can be bypassed with `--no-verify` but discouraged

