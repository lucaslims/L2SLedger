import { Button } from '@/shared/components/ui/button';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { Plus, Download } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

/**
 * QuickActions
 * 
 * Card com atalhos para ações frequentes.
 * Apenas navegação — sem lógica de negócio.
 */
export function QuickActions() {
  const navigate = useNavigate();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ações Rápidas</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-2">
        <Button
          onClick={() => navigate(`${ROUTES.TRANSACTIONS}/new`)}
          className="justify-start"
        >
          <Plus className="mr-2 h-4 w-4" />
          Nova Transação
        </Button>
        <Button
          variant="outline"
          className="justify-start"
          disabled
          title="Disponível em breve"
        >
          <Download className="mr-2 h-4 w-4" />
          Exportar Dados
        </Button>
      </CardContent>
    </Card>
  );
}
