import { initializeApp } from 'firebase/app';
import { getAuth } from 'firebase/auth';
import { getEnv } from '@/shared/lib/env';

const firebaseConfig = {
  apiKey: getEnv('VITE_FIREBASE_API_KEY'),
  authDomain: getEnv('VITE_FIREBASE_AUTH_DOMAIN'),
  projectId: getEnv('VITE_FIREBASE_PROJECT_ID'),
  storageBucket: getEnv('VITE_FIREBASE_STORAGE_BUCKET'),
  messagingSenderId: getEnv('VITE_FIREBASE_MESSAGING_SENDER_ID'),
  appId: getEnv('VITE_FIREBASE_APP_ID'),
};

// Validar configuração
if (!firebaseConfig.apiKey) {
  throw new Error('VITE_FIREBASE_API_KEY não configurada no .env');
}

if (!firebaseConfig.authDomain) {
  throw new Error('VITE_FIREBASE_AUTH_DOMAIN não configurada no .env');
}
if (!firebaseConfig.projectId) {
  throw new Error('VITE_FIREBASE_PROJECT_ID não configurada no .env');
}

// Initialize Firebase
const app = initializeApp(firebaseConfig);

// Initialize Firebase Authentication
export const auth = getAuth(app);

export default app;
