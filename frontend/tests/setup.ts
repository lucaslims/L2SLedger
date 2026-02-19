import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// Polyfill ResizeObserver for jsdom (used by Radix UI components)
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  } as unknown as typeof ResizeObserver;
}

// Polyfill hasPointerCapture for jsdom (used by Radix UI Select)
if (typeof HTMLElement.prototype.hasPointerCapture === 'undefined') {
  HTMLElement.prototype.hasPointerCapture = () => false;
}
if (typeof HTMLElement.prototype.setPointerCapture === 'undefined') {
  HTMLElement.prototype.setPointerCapture = () => {};
}
if (typeof HTMLElement.prototype.releasePointerCapture === 'undefined') {
  HTMLElement.prototype.releasePointerCapture = () => {};
}

// Polyfill scrollIntoView for jsdom (used by Radix UI Select content)
if (typeof Element.prototype.scrollIntoView === 'undefined') {
  Element.prototype.scrollIntoView = () => {};
}

// Cleanup após cada teste
afterEach(() => {
  cleanup();
});

// Mock environment variables
if (!import.meta.env.VITE_API_BASE_URL) {
  import.meta.env.VITE_API_BASE_URL = 'http://localhost:5000/api/v1';
}

if (!import.meta.env.VITE_FIREBASE_API_KEY) {
  import.meta.env.VITE_FIREBASE_API_KEY = 'test-api-key';
  import.meta.env.VITE_FIREBASE_AUTH_DOMAIN = 'test.firebaseapp.com';
  import.meta.env.VITE_FIREBASE_PROJECT_ID = 'test-project';
  import.meta.env.VITE_FIREBASE_STORAGE_BUCKET = 'test.appspot.com';
  import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID = '123456';
  import.meta.env.VITE_FIREBASE_APP_ID = '1:123456:web:test';
}
