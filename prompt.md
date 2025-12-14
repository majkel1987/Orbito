Twórz i sprawdzaj kod kod pod kątem następujacych kryteriów:

🔒 SECURITY: luki bezpieczeństwa, izolacja tenant (tenant context), SQL injection, XSS, autoryzacja, walidacja inputów
🐛 BUGS: logika biznesowa, edge cases, null handling, race conditions, exception handling
✨ QUALITY: SOLID, DRY, async/await usage, dependency injection, kod testowany, naming conventions
⚡ PERFORMANCE: N+1 queries, EF tracking, memory leaks, connection pooling, caching
✅ COMPLETENESS: brakujące funkcje, error responses, logging, rollback scenarios

Wskaż krytyczne problemy i sugestie poprawy.

---

## 📋 Oryginalne Instrukcje (dla referencji)

Act as IT Architect and Professional Frontend (React) and Backend (.NET) Develolper
Odczytaj pliki @Frontend_Plan.md, @Frontend_Implement_Plan.md, @README.md, @README_FRONTEND.md
Dodatkowo przejżyj pliki z /READMEs
Wprowadź punkt 1.2 z planu implementacji:

#### 1.2 Auth Context & Provider

Stosuj sie do wszystkich zasad z cursor rules
Frontend ma prawidłowo komunikować się z backendem dlatego zawsze sprawdzaj jak dana funkcjonalność wygląda w backendzie
Zaktualizuj wszystkie pliki z planem po zakończeniu tej implenentacji

Stosuj się do @RULES.md
Wszystkie nowe pliki README zapisuj w /READMEs
Nie twórz nowych plików Readme za każdym razem tylko aktualizuj już istniejące.

---

Act as IT Architect and Professional Frontend (React) and Backend (.NET) Develolper
Odczytaj pliki @Frontend_Plan.md, @Frontend_Implement_Plan.md, @README.md, @README_FRONTEND.md
Dodatkowo przejrzyj pliki z /READMEs

Nie mogę poprawnie zarejestrować Providera, w pliku @issues.md wkleiłem błędy z konsoli a tutaj wklejam wszystkie logi z zapliakcji.

Zadanie:
Zidentyfikuj błąd.
Napraw błędy

---

Act as Professional Frontend (React) and Backend (.NET) Develolper
Odczytaj pliki @Frontend_Plan.md, @Frontend_Implement_Plan.md, @README.md, @README_FRONTEND.md
Dodatkowo przejrzyj pliki z /READMEs

Problem:
Nie mogę poprawnie zalogować Użytkownika.
Po zalogowaniu nie jestem przenoszony do dashboardu, ręczne wywołanie strony również nie działa poprawnie.
Rejestracja w backend i frontend działa prawidłowo.
Logowanie w backend również działa poprawnie.

Zadanie:
Zidentyfikuj wszystkie komponenty i inne funkcje które są bezpośrednio powiązane z logowaniem i przekierowywaniem użytkownika do dashboardu.
Usuń te pliki i stwórz funkcjonalność logowania kompletnie od nowa.
Pamiętaj, żeby używać TypeScript.
Niech formularz logowania będzie modalem który pojawia się na wyblurowanym ekranie strony głównej lub jak to będzie później zamodelowane strony MVP - analogicznie do formularza rejestracji.

Stosuj się do wszystkich zasad z cursor rules
Frontend ma prawidłowo komunikować się z backendem dlatego zawsze sprawdzaj jak dana funkcjonalność wygląda w backendzie
Zaktualizuj wszystkie pliki z planem po zakończeniu tej implenentacji

---

Pracujesz nad projektem Orbito Frontend.
PRZED rozpoczęciem pracy:

🚨 Przeczytaj .agent/API_RULES.md - KRYTYCZNE reguły dotyczące API (OBOWIĄZKOWE!)
Przeczytaj .agent/feature_list.json - znajdź pierwszy blok z "passes": false
Przeczytaj .agent/claude-progress.txt - kontekst poprzednich sesji i KNOWN BUGS
Sprawdź apiEndpoints i requiredHooks dla tego bloku w feature_list.json
Otwórz Frontend_Prompts.md i znajdź sekcję dla tego bloku (szukaj BLOCK_START: X.X)
Wykonaj DOKŁADNIE kroki z promptu

PODCZAS pracy:

🚫 NIGDY nie używaj hardcoded data (0, [], "placeholder")
🚫 NIGDY nie twórz mock funkcji z console.log("TODO")
✅ ZAWSZE używaj hooków z @/core/api/generated/
✅ ZAWSZE obsługuj isLoading, error, empty states

PO zakończeniu bloku:

npm run typecheck && npm run lint
🔍 WERYFIKACJA W DEVTOOLS:

Network tab: Czy widzę requesty do /api/? Status 200? Header Authorization: Bearer?
Console: Brak TypeError, brak 401
UI: Dane są PRAWDZIWE (nie "0", nie puste listy)

Zmień "passes": true w feature_list.json dla ukończonego bloku
Dodaj wpis do claude-progress.txt
Zaktualizuj checklistę w pliku Frontend_Prompts.md
Git commit: feat(scope): description

Jeśli endpoint nie istnieje:
NIE MOCKUJ! Zamiast tego:

Powiedz że endpoint nie istnieje w Swagger
Zaproponuj opcje (dodać backend, użyć innego endpointu, pominąć)
Poczekaj na decyzję

Zacznij od sprawdzenia stanu projektu i przeczytania API_RULES.md.
