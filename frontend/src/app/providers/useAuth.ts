import { useContext } from 'react';
import { AuthContext } from './AuthProvider';

/**
 * Hook para acessar contexto de autenticação
 * 
 * Deve ser usado dentro de AuthProvider
 */
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
