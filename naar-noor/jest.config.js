/** @type {import('jest').Config} */
module.exports = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  testEnvironment: 'jsdom',
  testMatch: ['**/src/**/*.spec.ts'],
  moduleFileExtensions: ['ts', 'js', 'mjs', 'html', 'json'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  transformIgnorePatterns: [
    'node_modules/(?!(@angular|rxjs|zone.js)/)',
  ],
  collectCoverage: true,
  coverageDirectory: '../frontend-coverage',
  coverageReporters: ['text', 'lcov', 'cobertura', 'json'],
  collectCoverageFrom: [
    'src/app/services/**/*.ts',
    'src/app/components/**/*.ts',
    '!src/app/**/*.spec.ts',
    '!src/app/**/*.d.ts',
    '!src/app/**/tests/*.base.ts',
    '!src/app/**/tests/component-test.base.ts',
    '!src/app/**/tests/service-test.base.ts',
  ],
  coverageThresholds: {
    global: {
      statements: 75,
      branches: 65,
      functions: 75,
      lines: 75,
    },
  },
  globals: {
    'ts-jest': {
      tsconfig: '<rootDir>/tsconfig.spec.json',
      stringifyContentPathRegex: '\\.html$',
      diagnostics: false,
    },
  },
};
