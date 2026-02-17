import { Providers } from './providers';
import { AppRoutes } from './routes';
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
    </Providers>
  );
}

export default App;
