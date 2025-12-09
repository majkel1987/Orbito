---

## Pytania

**1. Jakie konkretnie były problemy z API?**
- Czy Orval wygenerował hooki poprawnie?
- Czy problem był w konfiguracji axios/fetch?
- Czy backend zwraca `Result<T>` a frontend tego nie obsługiwał?

**2. Gdzie były mockowane dane?**
- W hookach (np. `useClients` zwracał hardcoded array)?
- W komponentach (dane "na sztywno")?
- Czy agent w ogóle nie wywoływał API?

**3. Co z auth?**
- Czy token JWT był poprawnie przechowywany?
- Czy był wysyłany w headerach do API?
- Czy NextAuth poprawnie komunikował się z backendem?

**4. Jak wygląda teraz działający kod?**
- Możesz wrzucić przykład poprawnej konfiguracji API?
- Jak wygląda działający login flow?

---

## Co chcę zrobić

Na podstawie Twoich odpowiedzi zaktualizuję:

1. **Prompty** - dodam sekcję "🚫 ZAKAZY" (no mocks, no hardcoded data)
2. **Checklisty** - "Weryfikacja API" przed oznaczeniem bloku jako done
3. **feature_list.json** - dodam pole `apiEndpoints` do każdego bloku
4. **claude-progress.txt** - sekcja "LESSONS LEARNED" z konkretnymi błędami

Opisz mi problemy które napotkałeś, a przygotuję ulepszone materiały 🛠️
