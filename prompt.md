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

1. Przeczytaj orbito-frontend\.agent\feature_list.json - znajdź pierwszy blok z "passes": false
2. Przeczytaj orbito-frontend\.agent\claude-progress.txt - kontekst poprzednich sesji
3. Otwórz Frontend_Prompts.md i znajdź sekcję dla tego bloku (szukaj BLOCK_START: X.X)
4. Wykonaj DOKŁADNIE kroki z promptu

PO zakończeniu bloku:

1. npm run typecheck && npm run lint
2. Zmień "passes": true w feature_list.json dla ukończonego bloku
3. Dodaj wpis do claude-progress.txt
4. Zaktualizuj checklistę w pliku Frontend_Prompts.md
5. Git commit

Zacznij od sprawdzenia stanu projektu.
