import { ConfirmEmailForm } from "@/features/clients/components/ConfirmEmailForm";

export default async function ConfirmEmailPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;

  if (!token) {
    return (
      <div className="mx-auto max-w-md space-y-4 text-center">
        <h1 className="text-2xl font-bold">Aktywacja konta</h1>
        <p className="text-destructive">
          Brak tokena zaproszenia w linku. Sprawdź czy użyłeś poprawnego linku
          z emaila zaproszenia.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-md space-y-6">
      <div className="space-y-2">
        <h1 className="text-2xl font-bold">Aktywacja konta</h1>
        <p className="text-muted-foreground">
          Ustaw hasło, aby aktywować swoje konto w portalu klienta.
        </p>
      </div>
      <ConfirmEmailForm token={token} />
    </div>
  );
}
