import 'zone.js';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
  { teardown: { destroyAfterEach: false } }
);

/**
 * Jasmine → Jest compatibility shim.
 * Spec files use Jasmine's `spyOn(obj, method).and.returnValue(...)` API.
 * Jest provides jest.spyOn but without the `.and.*` helpers, so we add them.
 */
(global as any).spyOn = (obj: any, method: string) => {
  const spy = jest.spyOn(obj, method as any);
  const jestSpy = spy as any;
  jestSpy.and = {
    returnValue(val: any) { spy.mockReturnValue(val); return jestSpy; },
    returnValues(...vals: any[]) { spy.mockReturnValueOnce(vals[0]); return jestSpy; },
    callFake(fn: (...args: any[]) => any) { spy.mockImplementation(fn); return jestSpy; },
    stub() { spy.mockImplementation(() => undefined); return jestSpy; },
    callThrough() { spy.mockRestore(); return jestSpy; },
    throwError(err: any) {
      spy.mockImplementation(() => { throw err instanceof Error ? err : new Error(err); });
      return jestSpy;
    },
  };
  return jestSpy;
};
