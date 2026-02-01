import {
  signInWithEmailAndPassword,
  createUserWithEmailAndPassword,
  signOut,
  sendEmailVerification,
  User as FirebaseUser,
} from 'firebase/auth';
import { auth } from './config';

/**
 * Helper functions para Firebase Auth
 */

export async function signInWithEmail(email: string, password: string): Promise<FirebaseUser> {
  const userCredential = await signInWithEmailAndPassword(auth, email, password);
  return userCredential.user;
}

export async function signUpWithEmail(email: string, password: string): Promise<FirebaseUser> {
  const userCredential = await createUserWithEmailAndPassword(auth, email, password);
  return userCredential.user;
}

export async function signOutUser(): Promise<void> {
  await signOut(auth);
}

export async function sendVerificationEmail(user: FirebaseUser): Promise<void> {
  await sendEmailVerification(user);
}

export async function getIdToken(user: FirebaseUser): Promise<string> {
  return await user.getIdToken();
}

export function isEmailVerified(user: FirebaseUser): boolean {
  return user.emailVerified;
}
