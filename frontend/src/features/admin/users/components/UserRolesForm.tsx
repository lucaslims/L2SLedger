import { useState } from 'react';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Button } from '@/shared/components/ui/button';
import { useUpdateUserRoles } from '../hooks/useUpdateUserRoles';
import { ROLES } from '@/shared/lib/utils/constants';
import type { UserRole } from '@/shared/types/common.types';

const ROLE_LABELS: Record<string, string> = {
  [ROLES.ADMIN]: 'Admin — Acesso total ao sistema',
  [ROLES.LEITURA]: 'Leitura — Apenas visualização',
  [ROLES.FINANCEIRO]: 'Financeiro — Pode criar e editar',
};

interface UserRolesFormProps {
  userId: string;
  currentRoles: UserRole[];
  onSuccess?: () => void;
}

export function UserRolesForm({ userId, currentRoles, onSuccess }: UserRolesFormProps) {
  const [selectedRoles, setSelectedRoles] = useState<UserRole[]>(currentRoles);
  const { mutate: updateRoles, isPending } = useUpdateUserRoles();

  const handleToggleRole = (role: UserRole) => {
    setSelectedRoles((prev) =>
      prev.includes(role)
        ? prev.filter((r) => r !== role)
        : [...prev, role]
    );
  };

  const handleSubmit = () => {
    if (selectedRoles.length === 0) return;

    updateRoles(
      { userId, roles: selectedRoles },
      { onSuccess }
    );
  };

  const hasChanges =
    selectedRoles.length !== currentRoles.length ||
    selectedRoles.some((r) => !currentRoles.includes(r));

  return (
    <div className="space-y-4">
      <div className="space-y-3">
        {Object.values(ROLES).map((role) => (
          <div key={role} className="flex items-center space-x-3">
            <Checkbox
              id={`role-${role}`}
              checked={selectedRoles.includes(role)}
              onCheckedChange={() => handleToggleRole(role)}
            />
            <label
              htmlFor={`role-${role}`}
              className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
            >
              {ROLE_LABELS[role] || role}
            </label>
          </div>
        ))}
      </div>

      {selectedRoles.length === 0 && (
        <p className="text-sm text-destructive">
          O usuário deve ter pelo menos uma role.
        </p>
      )}

      <Button
        onClick={handleSubmit}
        disabled={isPending || !hasChanges || selectedRoles.length === 0}
      >
        {isPending ? 'Salvando...' : 'Salvar Roles'}
      </Button>
    </div>
  );
}
