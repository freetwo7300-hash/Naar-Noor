# Testing Framework Remediation - Design

**Version:** 1.0  
**Date:** June 27, 2026

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    CI/CD Pipeline (GitHub Actions)          │
├─────────────────────────────────────────────────────────────┤
│  Trigger: PR/Push → Run Tests → Measure Coverage → Gate     │
│  Result: Merge blocked if coverage below threshold           │
└─────────────────────────────────────────────────────────────┘
          │                    │                   │
          ▼                    ▼                   ▼
    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
    │   Backend    │    │   Frontend   │    │  Pre-commit  │
    │   Tests      │    │   Tests      │    │   Hooks      │
    │              │    │              │    │              │
    │ - xUnit      │    │ - Jasmine    │    │ - Husky      │
    │ - FsCheck    │    │ - Karma      │    │ - Jest       │
    │ - Moq        │    │ - Cypress    │    │              │
    │ - 73 props   │    │ - Services   │    │ Runs locally │
    │ - Coverlet   │    │ - Components │    │ before push  │
    │              │    │ - E2E        │    │              │
    └──────────────┘    └──────────────┘    └──────────────┘
          │                    │
          └────────┬───────────┘
                   ▼
        ┌──────────────────────┐
        │ Coverage Reports &   │
        │ Metrics Dashboard    │
        │                      │
        │ - HTML reports       │
        │ - Trend tracking     │
        │ - Threshold alerts   │
        └──────────────────────┘
```

---

## Layer Architecture

### Backend - .NET Testing Stack

```
Domain Layer (85% threshold)
├── Entity State Validation (Property 1)
├── Value Object Immutability (Property 2)
├── State Transitions (Property 3)
└── Domain Invariants (Property 4)
    Files: 4 test projects, 51 property tests

Application Layer (82% threshold)
├── Command Handler Processing (Property 5)
├── Query Result Filtering (Property 6)
├── Query Pagination (Property 8)
├── Backend exception mapping (Property 11)
├── Input Validation (Property 16)
├── Authorization (Property 17)
└── Security properties (19, 20)
    Files: 3 test projects, 25+ property tests

Infrastructure Layer (78% threshold)
├── CRUD Round-Trip (Property 7)
├── Referential Integrity (Property 10)
├── Transaction Atomicity (Property 9)
└── Performance SLAs (Properties 21-22)
    Files: 2 test projects, 15+ property tests

API Layer (80% threshold)
├── Exception Mapping (Property 11)
├── Response validation (Property 20)
└── Security headers (Property 20)
    Files: 1 test project, 5+ property tests
```

### Frontend - Angular Testing Stack

```
Service Layer (80% threshold)
├── HTTP Communication (Property 12)
├── Error Handling (Property 13)
└── Caching Consistency (Property 14)
    Files: 5+ service .spec.ts, 15+ tests
    Framework: Jasmine + Karma

Component Layer (75% threshold)
├── Component State Management (Property 15)
├── Form validation
├── User interactions
└── Lifecycle hooks
    Files: 8+ component .spec.ts, 10+ tests
    Framework: Jasmine + TestBed

E2E Layer
├── Authentication workflows
├── Reservation creation
├── Order placement
├── Menu search
└── Review submission
    Files: Cypress E2E tests, 10+ scenarios
    Framework: Cypress page objects
```

---

## Coverage Configuration Design

### Backend Coverage (Coverlet)

```xml
<!-- coverlet.runsettings -->
<RunSettings>
  <InProcessDataCollector assemblyQualifiedName="Coverlet.Collector...">
    <Configuration>
      <Threshold>78</Threshold>
      <ThresholdType>line</ThresholdType>
      <ThresholdStat>minimum</ThresholdStat>
      <UseSourceLink>true</UseSourceLink>
      <Format>cobertura</Format>
      <ExcludeByFile>
        <!-- Exclude migrations, generated code -->
        **/bin/**/*
        **/obj/**/*
      </ExcludeByFile>
    </Configuration>
  </InProcessDataCollector>
</RunSettings>
```

**Per-Layer Configuration:**
```
Domain.Tests:      --threshold=85 --thresholdType=line
Application.Tests: --threshold=82 --thresholdType=line
Infrastructure.Tests: --threshold=78 --thresholdType=line
API.Tests:         --threshold=80 --thresholdType=line
```

### Frontend Coverage (Karma/Istanbul)

```javascript
// karma.conf.js
module.exports = function(config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: {
      jasmine: {
        random: false
      },
      clearContext: false
    },
    jasmineHtmlReporter: {
      suppressAll: true
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'lcovonly' },
        { type: 'text-summary' },
        { type: 'cobertura' }
      ],
      check: {
        global: {
          statements: 75,
          branches: 75,
          functions: 75,
          lines: 75
        },
        each: {
          statements: 70,
          branches: 70,
          functions: 70,
          lines: 70
        }
      }
    }
  });
};
```

---

## CI/CD Pipeline Design

### GitHub Actions Workflow

```yaml
# .github/workflows/tests.yml
name: Tests & Coverage

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore api-server/NaarNoor.sln
      
      - name: Build
        run: dotnet build api-server/NaarNoor.sln --no-restore
      
      - name: Run tests with coverage
        run: dotnet test api-server/NaarNoor.sln \
          --no-build \
          --collect:"XPlat Code Coverage" \
          --settings:coverlet.runsettings
      
      - name: Check coverage thresholds
        run: ./scripts/validate-coverage.sh
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.cobertura.xml

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run tests with coverage
        run: npm run test:ci
      
      - name: Check coverage thresholds
        run: npm run coverage:check
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/cobertura-coverage.xml

  coverage-gate:
    needs: [backend-tests, frontend-tests]
    runs-on: ubuntu-latest
    if: always()
    steps:
      - name: Check if coverage gates passed
        run: |
          if [ "${{ needs.backend-tests.result }}" != "success" ] || \
             [ "${{ needs.frontend-tests.result }}" != "success" ]; then
            echo "Coverage gates failed"
            exit 1
          fi
      
      - name: Comment on PR with results
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: '✅ All coverage gates passed'
            })
```

### Pre-commit Hooks (Husky)

```bash
#!/bin/sh
# .husky/pre-commit

echo "Running pre-commit tests..."

# Run affected backend tests
dotnet test --filter "Category=UnitTest" --no-build 2>/dev/null
if [ $? -ne 0 ]; then
  echo "❌ Backend tests failed"
  exit 1
fi

# Run affected frontend tests
npm run test:affected 2>/dev/null
if [ $? -ne 0 ]; then
  echo "❌ Frontend tests failed"
  exit 1
fi

echo "✅ Pre-commit tests passed"
exit 0
```

---

## Property Test Organization

### Before (Current - Embedded)
```
ReferentialIntegrityPropertyTests.cs
├── Property 7: CRUD Round-Trip ⚠️ Embedded
├── Property 9: Transaction Atomicity ⚠️ Embedded
├── Property 10: Referential Integrity ✅

GetReservationsQueryHandlerPropertyTests.cs
├── Property 6: Query Filtering ✅
├── Property 8: Query Pagination ⚠️ Embedded
```

### After (Remediated - Dedicated)
```
RepositoryCrudPropertyTests.cs
└── Property 7: CRUD Round-Trip ✅ Dedicated
    ├── Create operation (2 tests)
    ├── Read operation (2 tests)
    ├── Update operation (2 tests)
    └── Delete operation (1 test)

QueryPaginationPropertyTests.cs
└── Property 8: Query Pagination ✅ Dedicated
    ├── Offset calculation (2 tests)
    ├── Page size handling (2 tests)
    ├── Total count (1 test)
    ├── Boundary conditions (2 tests)

TransactionAtomicityPropertyTests.cs
└── Property 9: Transaction Atomicity ✅ Dedicated
    ├── All-or-nothing behavior (2 tests)
    ├── Rollback on error (2 tests)
    ├── Isolation levels (2 tests)
    └── Concurrent transactions (1 test)

ReferentialIntegrityPropertyTests.cs
└── Property 10: Referential Integrity ✅ (unchanged)
    ├── Cascade delete (3 tests)
    ├── Foreign key constraints (3 tests)
    └── Orphaned prevention (3 tests)
```

---

## Data Flow

### Coverage Measurement Flow
```
Test Execution
  ↓
Coverlet (Backend) / Istanbul (Frontend) Collects Coverage
  ↓
Coverage Reports Generated (.cobertura.xml)
  ↓
ReportGenerator Creates HTML Reports
  ↓
Coverage Threshold Validator Checks Against Targets
  ↓
[Below Threshold] → Fail CI, Comment on PR
[Meets Threshold] → Pass CI, Store Artifacts
  ↓
Dashboard Aggregates Metrics & Trends
```

### Test Execution Flow (CI/CD)
```
PR Opened / Push to Branch
  ↓
GitHub Actions Triggered
  ↓
Parallel Jobs:
├─ Backend Tests → Coverage Collection → Threshold Check
├─ Frontend Tests → Coverage Collection → Threshold Check
└─ E2E Tests (if applicable)
  ↓
All Pass? → Gate Allows Merge
Any Fails? → Block with Actionable Message
```

---

## Testing Patterns - Standardized

### Pattern 1: FsCheck Property Tests (Backend)
```csharp
[Property(MaxTest = 100)]
public Property PropertyName_Condition_ExpectedResult(GeneratorType input)
{
    // Arrange
    var data = GenerateTestData(input);
    
    // Act
    var result = MethodUnderTest(data);
    
    // Assert
    return result.ShouldBe(expectedValue).ToProperty();
}
```

### Pattern 2: xUnit Theory Tests (Backend Parameterized)
```csharp
[Theory]
[MemberData(nameof(GetValidInputs))]
public void MethodName_Condition_ExpectedResult(TestData data)
{
    // Arrange
    
    // Act
    
    // Assert
}
```

### Pattern 3: Jasmine Property Tests (Frontend)
```typescript
it('Property: should satisfy condition for all inputs', () => {
  // Generate multiple random inputs
  const inputs = generateRandomInputs(100);
  
  inputs.forEach(input => {
    // Arrange
    
    // Act
    const result = methodUnderTest(input);
    
    // Assert
    expect(result).toBe(expectedValue);
  });
});
```

### Pattern 4: E2E Page Object Pattern (Cypress)
```typescript
class ReservationPage {
  visit() { cy.visit('/reservations'); }
  fillForm(data) { /* interact with elements */ }
  submit() { cy.get('[data-cy=submit]').click(); }
  getSuccessMessage() { return cy.get('[data-cy=success]'); }
}

describe('Reservation Flow', () => {
  it('should create reservation successfully', () => {
    const page = new ReservationPage();
    page.visit();
    page.fillForm(validData);
    page.submit();
    page.getSuccessMessage().should('be.visible');
  });
});
```

---

## Documentation Structure

```
/docs/
├── TESTING.md                  (Quick start guide)
├── TESTING_PROPERTIES.md       (Property testing tutorial)
├── TESTING_MOCKING.md          (Mock/fixture patterns)
├── TESTING_COVERAGE.md         (Coverage interpretation)
└── TESTING_TROUBLESHOOTING.md  (Common issues & solutions)

api-server/tests/
├── README.md                   (Test structure overview)
└── [Test projects...]

src/app/
├── services/tests/             (Service test examples)
└── components/tests/           (Component test examples)

cypress/
├── e2e/                        (E2E test scenarios)
├── support/
│   └── page-objects/           (Page object definitions)
└── README.md                   (E2E testing guide)
```

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Backend Coverage | 80%+ all layers | Coverlet reports |
| Frontend Coverage | 75%+ components, 80%+ services | Karma/Istanbul reports |
| Test Execution Time | <10 min CI, <5 min local | GitHub Actions logs |
| CI/CD Pass Rate | 95%+ | GitHub workflow status |
| Property Test Pass Rate | 100% | xUnit/Jasmine output |
| Documentation Completeness | 5/5 guides | File checklist |
| Team Adoption | 100% new tests use patterns | Code review metrics |

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| CI/CD timeout | Parallel job execution, test filtering |
| Flaky tests | Fixed seed in tests, async patterns, retry logic |
| Coverage regression | Gating prevents merge if coverage decreases |
| Team knowledge gaps | Comprehensive documentation + examples |
| Tool incompatibility | Use standard, well-supported tools (Coverlet, Karma, Cypress) |

