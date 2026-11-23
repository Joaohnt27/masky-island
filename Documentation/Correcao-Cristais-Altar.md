# Cristais permanentes no altar

Os cristais colocados eram filhos do painel do enigma. Fechar o painel ocultava também as ilustrações, embora o GameManager ainda guardasse os encaixes preenchidos.

## Alterações
- `Assets/Scripts/AltarCrystalDisplay.cs` e `.meta` (novos): exibição dos cristais independente do painel, alinhada ao altar e baseada em `IsCrystalSocketFilled`.
- `Assets/Scripts/AltarPuzzleLayout.cs`: cria a exibição usando as mesmas ilustrações e posições existentes. As imagens não interceptam cliques.
- `Assets/Scripts/MiningUI.cs`: inicializa a exibição mesmo quando o painel começa fechado.
- `Tests/GameplayValidation/AltarPuzzleValidation.cs`: verifica fechamento, retorno à caverna e limpeza dos encaixes ao iniciar nova partida.

Não há configuração manual no Inspector nem assets faltantes. A persistência usa o GameManager existente e vale durante a partida, inclusive entre cenas.

## Validação
21 verificações aprovadas na cópia isolada Unity 6, incluindo fechamento do painel, retorno à caverna e reset de nova partida. Compilação sem erros e captura do retorno à caverna revisada visualmente. Resultados: Documentation/QuestJournal/AltarPersistenceResults.txt. O índice de busca do Editor registrou sua exceção conhecida, separada dos testes de gameplay.

