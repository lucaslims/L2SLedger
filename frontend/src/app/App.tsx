import { Providers } from './providers';
import { AppRoutes } from './routes';
import { Toaster } from 'sonner';
import '@/shared/styles/globals.css';

/**
 * App Component
 * 
 * Root da aplicação
 */
function App() {
  return (
    <Providers>
      <AppRoutes />
      <Toaster position="bottom-right" richColors closeButton />
    </Providers>
  );
}

export default App;
