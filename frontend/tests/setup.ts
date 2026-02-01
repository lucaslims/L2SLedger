import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

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
