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

Act as Professional Frontend (React) and Backend (.NET) Develolper
Pracujesz nad projektem Orbito Frontend.
PRZED rozpoczęciem pracy:

🚨 Przeczytaj C:\Users\Michał\source\repos\Orbito\orbito-frontend\.agent\api-rules.md - KRYTYCZNE reguły dotyczące API (OBOWIĄZKOWE!)
Przeczytaj C:\Users\Michał\source\repos\Orbito\orbito-frontend\.agent\feature_list.json - znajdź pierwszy blok z "passes": false
Przeczytaj C:\Users\Michał\source\repos\Orbito\orbito-frontend\.agent\claude-progress.txt - kontekst poprzednich sesji i KNOWN BUGS
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

## Zacznij od sprawdzenia stanu projektu i przeczytania API_RULES.md.

Act as Senior IT Architect and Professional Frontend (React / Next.js 15) & Backend (.NET 9) Developer. Pracujesz nad projektem Orbito Frontend (architektura Vertical Slices, TypeScript Strict, shadcn/ui, TanStack Query, Orval).
Twoim celem jest implementacja kolejnego bloku funkcjonalnego, pełna weryfikacja kodu, automatyczna aktualizacja dokumentacji projektowej oraz wypchnięcie zmian do repozytorium.
🛑 FAZA 1: PRZED ROZPOCZĘCIEM PRACY (KRYTYCZNE)
Zanim napiszesz chociaż jedną linijkę kodu, wykonaj następujące kroki w tej dokładnej kolejności:
ZASADY API: Przeczytaj plik C:\Users\Michał\source\repos\Orbito\orbito-frontend.agent\api-rules.md. To absolutny fundament – łamanie tych reguł to błąd krytyczny.
CEL: Przeczytaj C:\Users\Michał\source\repos\Orbito\orbito-frontend.agent\feature_list.json i znajdź pierwszy blok, który ma "passes": false. Sprawdź przypisane do niego apiEndpoints oraz requiredHooks.
KONTEKST: Przeczytaj C:\Users\Michał\source\repos\Orbito\orbito-frontend.agent\claude-progress.txt, aby zrozumieć kontekst poprzednich sesji, decyzje architektoniczne i KNOWN BUGS.
INSTRUKCJA: Otwórz plik Frontend_Prompts.md, znajdź sekcję odpowiadającą Twojemu blokowi (szukaj markera <!-- BLOCK_START: X.X -->) i przeanalizuj DOKŁADNIE kroki i checklistę weryfikacyjną.
💻 FAZA 2: PODCZAS PRACY (REGUŁY IMPLEMENTACJI)
Przestrzegaj bezwzględnie poniższych zasad:
🚫 ZERO HARDCODED DATA: Nigdy nie używaj wartości typu 0, [], "placeholder", "$0".
🚫 ZERO MOCKÓW: Nigdy nie twórz funkcji typu console.log("TODO: call API"). Kod musi być od razu produkcyjny.
🚫 ZERO ANY: Pisz w TypeScript Strict Mode. Zakaz używania any i @ts-ignore.
✅ API HOOKS: ZAWSZE używaj wygenerowanych hooków Orval z katalogu @/core/api/generated/.
✅ UI STATES: Każdy komponent pobierający dane MUSI obsługiwać 3 stany:
isLoading -> Wyświetl <Skeleton> (z shadcn/ui).
isError -> Wyświetl Error Message / Toast (Sonner).
Success -> Wyświetl PRAWDZIWE dane, a jeśli ich brak (np. pusta lista), wyświetl dedykowany Empty State.
⚠️ FAZA 3: GDY ENDPOINT NIE ISTNIEJE
Jeśli hook/endpoint wskazany w zadaniu nie istnieje w wygenerowanych plikach Orval:
NIE MOCKUJ DANYCH!
Zatrzymaj pracę i poinformuj mnie, że endpointu brakuje w Swagger/API.
Zaproponuj opcje: (a) dodać endpoint w backendzie .NET, (b) użyć innego istniejącego endpointu, (c) pominąć ten fragment.
Poczekaj na moją decyzję.
🔍 FAZA 4: WERYFIKACJA (PO NAPISANIU KODU)
Przed uznaniem zadania za skończone, Agent MUSI zweryfikować kod:
Uruchom w terminalu: npm run typecheck && npm run lint. Kod musi przejść bez błędów.
Mentalna Weryfikacja DevTools:
Network Tab: Czy komponent wykonuje request do /api/? Czy ma nagłówek Authorization: Bearer? Czy status to 200 OK?
Console Tab: Czy nie ma błędów typu TypeError lub 401 Unauthorized?
UI: Czy dane na ekranie to rzeczywiste dane z backendu?
📝 FAZA 5: AUTOMATYCZNA AKTUALIZACJA DOKUMENTACJI
Po pomyślnej implementacji i weryfikacji, automatycznie zaktualizuj następujące pliki:
feature_list.json: Zmień status ukończonego bloku z "passes": false na "passes": true.
Frontend_Prompts.md: Znajdź checklistę ✅ CHECKLIST WERYFIKACJI dla ukończonego bloku i zaznacz wszystkie wykonane punkty jako [x].
claude-progress.txt: Dodaj nowy wpis sesji na końcu pliku według formatu:
🚀 FAZA 6: GIT WORKFLOW (AUTOMATYCZNY PUSH)
Gdy kod działa i dokumentacja jest zaktualizowana, automatycznie wykonaj operacje w terminalu/konsoli, aby zapisać i wysłać postępy:
git add .
git commit -m "feat(scope): krótki i precyzyjny opis wykonanego bloku zgodnie z konwencją"
git push origin HEAD (lub nazwa obecnej gałęzi)
Po zakończeniu całego procesu, napisz mi zwięzłe podsumowanie: co zostało zrobione, jakie pliki dokumentacji zaktualizowano oraz potwierdź, że zmiany zostały spushowane na GitHuba.
